using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

public class JobQueue
{
    private readonly BlockingCollection<Func<Task>> jobQueue = new();
    private readonly Task processingTask;

    public JobQueue()
    {
        processingTask = Task.Run(ProcessQueue);
    }

    public void EnqueueJob(Func<Task> job)
    {
        jobQueue.Add(job);
        Console.WriteLine("Job enqueued.");
    }

    private async Task ProcessQueue()
    {
        foreach (var job in jobQueue.GetConsumingEnumerable())
        {
            try
            {
                Console.WriteLine("Processing job...");
                await job();
                Console.WriteLine("Job processed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Job processing failed: {ex.Message}");
            }
        }
    }

    public void CompleteAdding()
    {
        jobQueue.CompleteAdding();
        Console.WriteLine("Job queue completed adding.");
    }
}