//using System;
//using System.IO;
//using System.Threading.Tasks;
//using FileWatcherLibrary;

//public class Partner
//{
//    private readonly string partnerId;
//    private readonly string directoryPath;
//    private readonly FileWatcherService fileWatcherService;
//    private readonly JobQueue jobQueue;

//    public Partner(string partnerId, string directoryPath, FileWatcherService fileWatcherService)
//    {
//        this.partnerId = partnerId;
//        this.directoryPath = directoryPath;
//        this.fileWatcherService = fileWatcherService;
//        this.jobQueue = new JobQueue();
//    }

//    public void Start()
//    {
//        bool issuccessful = fileWatcherService.Subscribe(partnerId, directoryPath, OnFileChanged, OnError);
//        if(issuccessful)
//            Console.WriteLine($"Partner {partnerId} started watching directory: {directoryPath}");

//        else
//            Console.WriteLine($"Could not start FileWatcher for Partner {partnerId} for directory: {directoryPath}");
//    }

//    public void Stop()
//    {
//        fileWatcherService.Unsubscribe(partnerId);
//        jobQueue.CompleteAdding();
//        Console.WriteLine($"Partner {partnerId} stopped watching directory: {directoryPath}");
//    }

//    private async Task ProcessFileEvent(FileInfo fileInfo)
//    {
//        // Mock processing by adding some delay
//        await Task.Delay(500);
//        Console.WriteLine($"Partner {partnerId} processed file: {fileInfo.FullName}");
//    }

//    private Task OnFileChanged(FileInfo fileInfo)
//    {
//        Console.WriteLine($"Partner {partnerId} received file event: {fileInfo.FullName}");
//        jobQueue.EnqueueJob(() => ProcessFileEvent(fileInfo));
//        return Task.CompletedTask;
//    }

//    private void OnError(Exception exception)
//    {
//        Console.WriteLine($"Error for partner {partnerId}: {exception.Message}");
//    }
//}

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileWatcherLibrary;

public class Partner
{
    private readonly string partnerId;
    private readonly string directoryPath;
    private readonly FileWatcherService fileWatcherService;
    private readonly JobQueue jobQueue;
    private readonly Timer heartbeatTimer;

    public Partner(string partnerId, string directoryPath, FileWatcherService fileWatcherService)
    {
        this.partnerId = partnerId;
        this.directoryPath = directoryPath;
        this.fileWatcherService = fileWatcherService;
        this.jobQueue = new JobQueue();

        // Send heartbeat every 10 seconds
        heartbeatTimer = new Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    public bool Start()
    {
        try
        {
            fileWatcherService.Subscribe(partnerId, directoryPath, OnFileChanged, OnError);
            Console.WriteLine($"Partner {partnerId} started watching directory: {directoryPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting FileWatcher for Partner {partnerId}: {ex.Message}");
            return false; // Exit the method if an exception occurs
        }
    }

    public bool Stop()
    {
        try
        {
            fileWatcherService.Unsubscribe(partnerId);
            jobQueue.CompleteAdding();
            heartbeatTimer.Dispose();
            Console.WriteLine($"Partner {partnerId} stopped watching directory: {directoryPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping FileWatcher for Partner {partnerId}: {ex.Message}");
            return false; // Exit the method if an exception occurs
        }
    }

    private async Task ProcessFileEvent(FileInfo fileInfo)
    {
        try
        {
            // Mock processing by adding some delay
            await Task.Delay(500);
            Console.WriteLine($"Partner {partnerId} processed file: {fileInfo.FullName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing file event for Partner {partnerId}: {ex.Message}");
            return; // Exit the method if an exception occurs
        }
    }

    private Task OnFileChanged(FileInfo fileInfo)
    {
        try
        {
            Console.WriteLine($"Partner {partnerId} received file event: {fileInfo.FullName}");
            jobQueue.EnqueueJob(() => ProcessFileEvent(fileInfo));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling file change event for Partner {partnerId}: {ex.Message}");
            return Task.CompletedTask; // Exit the method if an exception occurs
        }
        return Task.CompletedTask;
    }

    private void OnError(Exception exception)
    {
        Console.WriteLine($"Error for partner {partnerId}: {exception.Message}");
    }

    private void SendHeartbeat(object state)
    {
        try
        {
            fileWatcherService.Heartbeat(partnerId);
            Console.WriteLine($"Heartbeat sent for partner {partnerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending heartbeat for Partner {partnerId}: {ex.Message}");
            return; // Exit the method if an exception occurs
        }
    }
}