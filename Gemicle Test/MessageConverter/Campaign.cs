
namespace Gemicle_Test.Model
{
    public static class MessageConverter
    {
        public static string GetSendMessage(Campaign campaign, Customer customer)
            => $"{DateTime.Now}: Sent {campaign.Template} to {customer.CustomerId} ----- {campaign.CampaignName} ";
    }
}
