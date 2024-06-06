
namespace Gemicle_Test.Model
{
    public class Campaign
    {
        public string CampaignName { get; set; }

        public string Template { get; set; }

        public Func<Customer, bool> Condition { get; set; }

        public TimeSpan SendTime { get; set; }

        public int Priority { get; set; }
    }

}
