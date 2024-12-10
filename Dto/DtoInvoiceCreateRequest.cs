using InvoiceApp.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvoiceApp.Dto
{
    public class DtoInvoiceCreateRequest
    {
        public PaymentStatus PaymentStatus { get; set; }
        public int PaymentTerm { get; set; }
        public int CustomerId { get; set; }
        public string Description { get; set; }
        public List<int> ItemIds { get; set; } = new List<int>();
        public List<int> Quantities { get; set; } = new List<int>();
    }
    public class DtoInvoiceUpdateRequest
    {
        public int Id { get; set; }  
        public DateTime CreatedDate { get; set; } =DateTime.Now;
        public PaymentStatus PaymentStatus { get; set; }
        public string Description { get; set; }
        public int PaymentTerm { get; set; }
        public int CustomerId { get; set; }
        public List<InvoiceItemResponse> Item { get; set; }
    }

    public class InvoiceItemResponse
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
    }
}
