using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;
using System.Text.RegularExpressions;
using Gemicle_Test.Model;

namespace Gemicle_Test.Command
{
    public class CampaignSchedulerCommand
    {
        private List<Campaign> campaigns;
        private List<Customer> customers;
        private string resourcePath;
        private string sentCustomersFilePath;

        public CampaignSchedulerCommand(List<Campaign> campaigns, List<Customer> customers, string resourcePath, string sentCustomersFilePath)
        {
            this.campaigns = campaigns;
            this.customers = customers;
            this.resourcePath = resourcePath;
            this.sentCustomersFilePath = sentCustomersFilePath;
        }

        public async Task ScheduleCampaigns()
        {
            var sendCustomerIds = GetCustomersAlreadySendToday();
            var needToSendCustomers = customers.Where(x => !sendCustomerIds.Contains(x.CustomerId)).ToList();

            var currentTime = DateTime.Now;
            var groupedCampaigns = campaigns
                .GroupBy(c => c.SendTime)
                .Where(g => g.Key <= currentTime.TimeOfDay && g.Key > currentTime.TimeOfDay.Subtract(TimeSpan.FromMinutes(30)))
                .OrderBy(g => g.Key);
            var sentCustomers = new HashSet<int>();

            foreach (var campaignGroup in groupedCampaigns)
            {
                foreach (var campaign in campaignGroup.OrderBy(c => c.Priority))
                {
                    var targetCustomers = needToSendCustomers.Where(campaign.Condition).ToList();

                    foreach (var customer in targetCustomers)
                    {
                        if (!sentCustomers.Contains(customer.CustomerId))
                        {
                            var highestPriorityCampaign = campaigns
                                .Where(c => c.Condition(customer))
                                .OrderBy(c => c.Priority)
                                .FirstOrDefault();

                            if (highestPriorityCampaign != null && highestPriorityCampaign == campaign)
                            {
                                await SendCampaign(highestPriorityCampaign, customer);
                                sentCustomers.Add(customer.CustomerId);
                            }
                        }
                    }
                }
            }
        }


        private async Task SendCampaign(Campaign campaign, Customer customer)
        {
            var templateFilePath = GetTemplateFilePath(campaign.Template);

            if (templateFilePath == null)
            {
                Console.WriteLine($"Template file for {campaign.Template} does not exist. Campaign not sent to {customer.CustomerId}.");
                return;
            }

            SaveScheduledCustomer(customer.CustomerId);

            using (var writer = new StreamWriter(sentCustomersFilePath, true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteField(DateTime.Now);
                csv.WriteField(customer.CustomerId);
                csv.WriteField(campaign.Template);
                csv.WriteField(campaign.CampaignName);
                csv.NextRecord();
            }

            await Task.CompletedTask;
        }

        private IList<int> GetCustomersAlreadySendToday()
        {
            if (!File.Exists(sentCustomersFilePath))
            {
                return new List<int>();
            }

            using (var reader = new StreamReader(sentCustomersFilePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null
            }))
            {
                var records = csv.GetRecords<SendedCustomer>();
                return records.Select(r => r.CustomerId).ToList();
            }
        }

        private void SaveScheduledCustomer(int customerId)
        {
            if (!File.Exists(sentCustomersFilePath))
            {
                using (var writer = new StreamWriter(sentCustomersFilePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<SendedCustomer>();
                    csv.NextRecord();
                }
            }
        }

        private string GetTemplateFilePath(string templateName)
        {
            var templateFiles = Directory.GetFiles(resourcePath);
            var normalizedTemplateName = Regex.Replace(templateName, @"\s+", "").ToLower();

            foreach (var file in templateFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file).ToLower();
                var normalizedFileName = Regex.Replace(fileNameWithoutExtension, @"\s+", "");

                if (normalizedFileName.Contains(normalizedTemplateName))
                {
                    return file;
                }
            }

            return null;
        }
    }

}
