using System;
using System.Threading.Tasks;
using FileWatcherLibrary;

public class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: PartnerApp <PartnerId> <DirectoryPath>");
            return;
        }

        string partnerId = args[0];
        string directoryPath = args[1];

        FileWatcherService fileWatcherService = new FileWatcherService();
        Partner partner = new Partner(partnerId, directoryPath, fileWatcherService);

        bool startpartner = partner.Start();
        if(startpartner==false)
        {
            Console.WriteLine($"Could not start FileWatcher for Partner {partnerId} for directory: {directoryPath}");
            return;
        }

        Console.WriteLine("Press Enter to stop...");
        Console.ReadLine();

        bool stoppartner = partner.Stop();

        Console.WriteLine("Partner stopped. Press Enter to exit...");
        Console.ReadLine();
    }
}