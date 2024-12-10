namespace InvoiceApp.Dto
{
    public class DtoClientCreateRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Adress { get; set; }
        public string City { get; set; }
        public int PostCode { get; set; }
        public string Country { get; set; }
        public object Customers { get; internal set; }
        public object Invoices { get; internal set; }
        public object Items { get; internal set; }
        public object InvoiceItems { get; internal set; }
    }
}
