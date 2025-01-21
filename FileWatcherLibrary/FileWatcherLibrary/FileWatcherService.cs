using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;



namespace FileWatcherLibrary
{
    public class FileWatcherService
    {
        private readonly ConcurrentDictionary<string, ConcurrentBag<FileWatcher>> partnerWatchers = new();
        private readonly ConcurrentDictionary<string, DateTime> partnerHeartbeats = new();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(10); // Limit to 10 concurrent requests
        private readonly Timer heartbeatTimer;

        public FileWatcherService()
        {
            // Check heartbeats every 10 seconds
            heartbeatTimer = new Timer(CheckHeartbeats, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        public bool Subscribe(string partnerId, string directoryPath, Func<FileInfo, Task> onFileChanged, Action<Exception> onError)
        {
            var watcher = new FileWatcher(directoryPath, async (fileInfo) =>
            {
                await semaphore.WaitAsync(); // Acquire the semaphore
                try
                {
                    Console.WriteLine($"File event received for partner {partnerId}: {fileInfo.FullName}");
                    await onFileChanged(fileInfo);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Event Processing failed for {partnerId} with exception: {ex.Message}");
                }
                finally
                {
                    semaphore.Release(); // Release the semaphore
                }
            },
            (exception) =>
            {
                // Handle and log the error
                Console.WriteLine($"Error in FileWatcher for partner {partnerId}: {exception.Message}");
                onError?.Invoke(exception);
            });

            if (!partnerWatchers.ContainsKey(partnerId))
            {
                partnerWatchers[partnerId] = new ConcurrentBag<FileWatcher>();
            }

            partnerWatchers[partnerId].Add(watcher);
            watcher.Start();
            Console.WriteLine($"Partner {partnerId} subscribed to directory: {directoryPath}");
            return true;
        }

        public bool Unsubscribe(string partnerId)
        {
            if (partnerWatchers.TryRemove(partnerId, out var watchers))
            {
                foreach (var watcher in watchers)
                {
                    watcher.Stop();
                }
                Console.WriteLine($"Partner {partnerId} unsubscribed from all directories.");
                return true;
            }

            Console.WriteLine($"Partner {partnerId} is not subscribed.");
            return false;
        }

        public void Heartbeat(string partnerId)
        {
            partnerHeartbeats[partnerId] = DateTime.UtcNow;
        }

        private void CheckHeartbeats(object state)
        {
            var now = DateTime.UtcNow;
            foreach (var partnerId in partnerHeartbeats.Keys)
            {
                if (partnerHeartbeats.TryGetValue(partnerId, out var lastHeartbeat))
                {
                    if ((now - lastHeartbeat).TotalSeconds > 30) // 30 seconds timeout
                    {
                        Console.WriteLine($"Partner {partnerId} missed heartbeat. Cleaning up...");
                        Unsubscribe(partnerId);
                        partnerHeartbeats.TryRemove(partnerId, out _);
                    }
                }
            }
        }
    }

    //public class FileWatcherService
    //{
    //    private readonly ConcurrentDictionary<string, FileWatcher> watchers = new();
    //    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(5); // Limit to 5 concurrent requests

    //    public bool Subscribe(string partnerId, string directoryPath, Func<FileInfo, Task> onFileChanged, Action<Exception> onError)
    //    {
    //        if (watchers.ContainsKey(partnerId))
    //        {
    //            Console.WriteLine($"Partner {partnerId} is already subscribed.");
    //            return false;
    //        }

    //        var watcher = new FileWatcher(directoryPath, async (fileInfo) =>
    //        {
    //            await semaphore.WaitAsync(); // Acquire the semaphore
    //            try
    //            {
    //                Console.WriteLine($"File event received for partner {partnerId}: {fileInfo.FullName}");
    //                await onFileChanged(fileInfo);
    //            }
    //            catch (Exception ex)
    //            {
    //                Console.WriteLine($"Event Processing failed for {partnerId} with exception: {ex.Message}");
    //            }
    //            finally
    //            {
    //                semaphore.Release(); // Release the semaphore
    //            }
    //        },
    //        (exception) =>
    //        {
    //            // Handle and log the error
    //            Console.WriteLine($"Error in FileWatcher for partner {partnerId}: {exception.Message}");
    //            onError?.Invoke(exception);
    //        });

    //        if (watchers.TryAdd(partnerId, watcher))
    //        {
    //            watcher.Start();
    //            Console.WriteLine($"Partner {partnerId} subscribed successfully.");
    //            return true;
    //        }

    //        return false;
    //    }

    //    public bool Unsubscribe(string partnerId)
    //    {
    //        if (watchers.TryRemove(partnerId, out var watcher))
    //        {
    //            watcher.Stop();
    //            Console.WriteLine($"Partner {partnerId} unsubscribed successfully.");
    //            return true;
    //        }

    //        Console.WriteLine($"Partner {partnerId} is not subscribed.");
    //        return false;
    //    }
    //}

}
