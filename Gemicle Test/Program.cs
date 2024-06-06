using Gemicle_Test.Manager;
class Program
{
    static async Task Main(string[] args)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        var senderManager = new SenderManager();

        var task = senderManager.Start(cancellationToken);

        Console.WriteLine("!!! Press any key to stop sending !!!");
        Console.ReadKey();

        await senderManager.Stop();
        await task;

        Console.WriteLine("Sending stopped.");
    }
}
