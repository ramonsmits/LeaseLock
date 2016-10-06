using System;
using System.Configuration;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        Main().GetAwaiter().GetResult();
    }

    static async Task Main()
    {
        using (ILeaseLock leaseLock = new AzureStorageLeaseLock("ramon", Guid.NewGuid().ToString(), TimeSpan.FromSeconds(5), "leases", ConfigurationManager.ConnectionStrings["AzureStorage"].ConnectionString))
        {
            while (true)
            {
                if (await leaseLock.TryClaim())
                    Console.WriteLine("Obey me you twat!");
                else
                    Console.WriteLine("I'm your humble servant, please humiliate me!");

                if (Console.KeyAvailable && Console.ReadKey().Key == ConsoleKey.R)
                {
                    await leaseLock.Release();
                }

                await Task.Delay(1000);
            }
        }
    }
}
