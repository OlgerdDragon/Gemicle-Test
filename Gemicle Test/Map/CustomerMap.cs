using CsvHelper.Configuration;

namespace Gemicle_Test.Model
{
    public class CustomerMap : ClassMap<Customer>
    {
        public CustomerMap()
        {
            Map(m => m.CustomerId).Name("CUSTOMER_ID");
            Map(m => m.Age).Name("Age");
            Map(m => m.Gender).Name("Gender");
            Map(m => m.City).Name("City");
            Map(m => m.Deposit).Name("Deposit");
            Map(m => m.IsNewCustomer).Name("NewCustomer");
        }
    }
}
