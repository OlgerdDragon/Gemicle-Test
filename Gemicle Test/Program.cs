using Gemicle_Test.Manager;
class Program
{
    static async Task Main(string[] args)
    {
        var senderManager = new SenderManager();
        await senderManager.Start();
    }
}
