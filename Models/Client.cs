namespace InvoiceApp.Models
{
    public class Client
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Adress { get; set; }
        public string City { get; set; }
        public int PostCode { get; set; }
        public string Country { get; set; }
        public List<Customer> Customers { get; set; }
        public List<Invoice> Invoices { get; set; }
        public List<Item> Items { get; set; }
        public List<InvoiceItem> InvoiceItems { get; set; }

    }
}
