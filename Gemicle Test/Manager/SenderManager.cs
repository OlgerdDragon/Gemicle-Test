using CsvHelper.Configuration;
using CsvHelper;
using Gemicle_Test.Map;
using Gemicle_Test.Model;
using System.Globalization;
using Gemicle_Test.Command;

namespace Gemicle_Test.Manager
{
    public class SenderManager
    {
        private CancellationTokenSource _cancellationTokenSource;

        public async Task Start()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;

            var task = StartInternal(cancellationToken);

            Console.WriteLine("!!! Press any key to stop sending !!!");
            Console.ReadKey();

            await Stop();
            await task;

            Console.WriteLine("Sending stopped.");
        }

        private async Task StartInternal(CancellationToken cancellationToken)
        {
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _cancellationTokenSource.Token;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine("Sending...");
                    await StartSending();
                    await Task.Delay(TimeSpan.FromMinutes(30), token);
                }
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("SenderManager stopped.");
            }
        }

        private Task Stop()
        {
            _cancellationTokenSource?.Cancel();
            return Task.CompletedTask;
        }

        private async Task StartSending()
        {
            string resourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resource");
            Directory.CreateDirectory(resourcePath);

            var customerFilePath = Path.Combine(resourcePath, "customers.csv");
            var sentCustomersFilePath = Path.Combine(resourcePath, $"sends_{DateTime.Now:yyyyMMdd}.csv");

            var customers = ReadCustomersFromCsv(customerFilePath);
            var campaigns = new List<Campaign>
            {
                new Campaign { CampaignName = "Campaign 1", Template = "Template A", Condition = c => c.Gender == "Male", SendTime = new TimeSpan(10, 15, 0), Priority = 1 },
                new Campaign { CampaignName = "Campaign 2", Template = "Template B", Condition = c => c.Age > 45, SendTime = new TimeSpan(10, 5, 0), Priority = 2 },
                new Campaign { CampaignName = "Campaign 3", Template = "Template C", Condition = c => c.City == "New York", SendTime = new TimeSpan(10, 10, 0), Priority = 5 },
                new Campaign { CampaignName = "Campaign 4", Template = "Template A", Condition = c => c.Deposit > 100, SendTime = new TimeSpan(10, 15, 0), Priority = 3 },
                new Campaign { CampaignName = "Campaign 5", Template = "Template C", Condition = c => c.IsNewCustomer, SendTime = new TimeSpan(10, 5, 0), Priority = 4 }
            };

            var scheduler = new CampaignSchedulerCommand(campaigns, customers, resourcePath, sentCustomersFilePath);
            await scheduler.ScheduleCampaigns();
            Console.WriteLine("Campaigns sended successfully.");
        }


        private List<Customer> ReadCustomersFromCsv(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            }))
            {
                csv.Context.RegisterClassMap<CustomerMap>();
                return new List<Customer>(csv.GetRecords<Customer>());
            }
        }
    }
}
