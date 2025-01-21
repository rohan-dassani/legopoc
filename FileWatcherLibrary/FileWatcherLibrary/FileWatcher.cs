using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcherLibrary
{
    public class FileWatcher
    {
        private readonly FileSystemWatcher watcher;
        private readonly ConcurrentDictionary<string, FileInfo> fileDictionary = new();
        private readonly ConcurrentQueue<FileInfo> fileQueue = new();
        private readonly Timer processingTimer;
        private readonly Func<FileInfo, Task> onFileEvent;
        private readonly Action<Exception> onErrorEvent;

        public FileWatcher(string directoryPath, Func<FileInfo, Task> onFileEvent, Action<Exception> onErrorEvent = null)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));
            }

            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException($"The directory '{directoryPath}' does not exist.");
            }

            this.onFileEvent = onFileEvent ?? throw new ArgumentNullException(nameof(onFileEvent));
            this.onErrorEvent = onErrorEvent;

            watcher = new FileSystemWatcher(directoryPath)
            {
                NotifyFilter = NotifyFilters.Size | NotifyFilters.FileName,
                IncludeSubdirectories = true,
                EnableRaisingEvents = false
            };

            watcher.Filter = "*.*";
            watcher.InternalBufferSize = 16 * 4096; //64Kb
            watcher.Changed += OnChanged;
            watcher.Error += OnError;

            // Process events every 500ms
            processingTimer = new Timer(ProcessEvents, null, Timeout.Infinite, 500);
        }

        public void Start()
        {
            Console.WriteLine($"FileWatcher started for directory: {watcher.Path}");
            watcher.EnableRaisingEvents = true;
            processingTimer.Change(0, 500);
        }

        public void Stop()
        {
            Console.WriteLine($"FileWatcher stopped for directory: {watcher.Path}");
            watcher.EnableRaisingEvents = false;
            processingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var fileInfo = new FileInfo(e.FullPath);
            //Console.WriteLine("###1 - File changed");
            Console.WriteLine($"File changed: {fileInfo.FullName}");

            // Deduplicate and enqueue
            if (fileDictionary.TryAdd(fileInfo.FullName, fileInfo))
            {
                fileQueue.Enqueue(fileInfo);
                Console.WriteLine($"File enqueued: {fileInfo.FullName}");
            }
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            var exception = e.GetException();
            Console.WriteLine($"FileWatcher Error: {exception.Message}");
            onErrorEvent?.Invoke(exception);
        }

        private void ProcessEvents(object state)
        {
            while (fileQueue.TryDequeue(out var fileInfo))
            {
                // Remove from dictionary to allow future events for the same file
                fileDictionary.TryRemove(fileInfo.FullName, out _);
                Console.WriteLine($"Processing file: {fileInfo.FullName}");

                // Process the event asynchronously
                Task.Run(() => onFileEvent(fileInfo))
                    .ContinueWith(t =>
                    {
                        if (t.Exception != null)
                        {
                            Console.WriteLine($"Error processing file {fileInfo.FullName}: {t.Exception.GetBaseException().Message}");
                        }
                        else
                        {
                            Console.WriteLine($"Successfully processed file: {fileInfo.FullName}");
                        }
                    });
            }
        }
    }
}
