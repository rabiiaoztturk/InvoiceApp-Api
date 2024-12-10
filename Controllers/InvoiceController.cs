using InvoiceApp.Data;
using InvoiceApp.Dto;
using InvoiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Mail;
using System.Net;

namespace InvoiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class InvoiceController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InvoiceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<List<object>> GetInvoices()
        {
            var invoices = _context.Invoices
                .Include(i => i.Item) 
                .Include(i => i.Client)
                .Include(i => i.Customer) 
                .Select(i => new
                {
                    InvoiceId = i.Id,
                    CreatedDate = i.CreatedDate,
                    PaymentStatus = i.PaymentStatus,
                    PaymentTerm = i.PaymentTerm,
                    Description = i.Description,
                    CustomerName = i.Customer.FullName,
                    CustomerAddress = i.Customer.Address,
                    CustomerEmail = i.Customer.Email,
                    CustomerCity = i.Customer.City,
                    CustomerCountry = i.Customer.Country,
                    CustomerPostCode = i.Customer.PostCode,
                    Client = new
                    {
                        Name = i.Client.Name,
                        City = i.Client.City,
                        Country = i.Client.Country,
                        PostCode = i.Client.PostCode,
                        Address = i.Client.Adress
                    },
                    Items = i.Item.Select(item => new
                    {
                        ItemId = item.Id,
                        Name = item.Name,
                        Description = item.Description,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        Total = item.Price * item.Quantity
                    }).ToList(),
                    TotalAmount = i.Item.Sum(item => item.Price * item.Quantity)
                })
                .ToList();

            return Ok(invoices);
        }

        [HttpGet("{id}")]
        public ActionResult<object> GetInvoice(int id)
        {
            var invoice = _context.Invoices
                .Include(i => i.Customer) 
                .Include(i => i.Client)   
                .Include(i => i.Item)   
                .FirstOrDefault(i => i.Id == id);

            if (invoice == null)
                return NotFound();

            var response = new
            {
                InvoiceId = invoice.Id,
                CreatedDate = invoice.CreatedDate,
                PaymentStatus = invoice.PaymentStatus,
                PaymentTerm = invoice.PaymentTerm,
                Description = invoice.Description,
                CustomerName = invoice.Customer.FullName,
                CustomerAddress = invoice.Customer.Address,
                CustomerEmail = invoice.Customer.Email,
                CustomerCity = invoice.Customer.City,
                CustomerCountry = invoice.Customer.Country,
                CustomerPostCode = invoice.Customer.PostCode,
                Client = new
                {
                    Name = invoice.Client.Name,
                    City = invoice.Client.City,
                    Country = invoice.Client.Country,
                    PostCode = invoice.Client.PostCode,
                    Address = invoice.Client.Adress
                },
                Items = invoice.Item.Select(item => new
                {
                    ItemId = item.Id, 
                    Name = item.Name,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Price = item.Price,
                    Total = item.Total
                }).ToList(),
                TotalAmount = invoice.Item.Sum(item => item.Total)
            };

            return Ok(response);
        }


        [HttpPost]
        public ActionResult<object> AddInvoice([FromBody] DtoInvoiceCreateRequest invoiceRequest)
        {

            var invoice = new Invoice
            {
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                PaymentStatus = invoiceRequest.PaymentStatus,
                PaymentTerm = (PaymentTerm)invoiceRequest.PaymentTerm,
                CustomerId = invoiceRequest.CustomerId,
                ClientId = 3,
                Description = invoiceRequest.Description
            };

            var items = _context.Items
                .Where(item => invoiceRequest.ItemIds.Contains(item.Id))
                .ToList();

            foreach (var item in items)
            {
                var index = invoiceRequest.ItemIds.IndexOf(item.Id);
                var quantityToReduce = invoiceRequest.Quantities[index];

                if (item.Quantity >= quantityToReduce)
                {
                    item.Quantity -= quantityToReduce;

                    var invoiceItem = new InvoiceItem
                    {
                        Name = item.Name,
                        Description = item.Description,
                        Quantity = quantityToReduce,
                        Price = (decimal)item.Price,
                        Total = (decimal)(item.Price * quantityToReduce)
                    };

                    invoice.Item.Add(invoiceItem);
                }
                else
                {
                    return BadRequest($"Yetersiz miktar: {item.Name} için mevcut miktar: {item.Quantity}, istenen miktar: {quantityToReduce}.");
                }
            }

            invoice.TotalAmount = (double)invoice.Item.Sum(i => i.Total);
            _context.Invoices.Add(invoice);
            _context.SaveChanges();

            var customer = _context.Customers
                .Where(c => c.Id == invoice.CustomerId)
                .Select(c => new { c.FullName, c.Email })
                .FirstOrDefault();

            if (customer != null && !string.IsNullOrEmpty(customer.Email))
            {
                using (var client = new SmtpClient("smtp.eu.mailgun.org", 587))
                {
                    client.Credentials = new NetworkCredential("postmaster@mail.rabiaozturk.com.tr", "d2be6cb70a6f83e3282f7b3524570c8d - 3724298e-6b432a6c");
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("postmaster@mail.rabiaozturk.com.tr", "InvoiceApp"),
                        Subject = "Fatura Bilgisi",
                        Body = $@"
                <p style='font-family: Arial, sans-serif;'>
                    Merhaba <strong>{customer.FullName}</strong>,
                </p>
                <p style='font-family: Arial, sans-serif;'>
                    {invoice.Description} faturanızı başarıyla oluşturduk.
                </p>
                <p style='font-family: Arial, sans-serif;'>
                    Toplam tutar: <strong>{invoice.TotalAmount} TL</strong>
                </p>
                <p style='font-family: Arial, sans-serif;'>
                    Ödeme yapmanız gereken son tarih: <strong>{invoice.CreatedDate.AddDays((int)invoice.PaymentTerm):dd.MM.yyyy}</strong>
                </p>
                <p style='font-family: Arial, sans-serif;'>
                    İyi günler dileriz,<br>InvoiceApp
                </p>",
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(new MailAddress(customer.Email));

                    client.Send(mailMessage);
                }
            }

            var response = new
            {
                InvoiceId = invoice.Id,
                CreatedDate = invoice.CreatedDate,
                PaymentStatus = invoice.PaymentStatus,
                PaymentTerm = invoice.PaymentTerm,
                Description = invoice.Description,
                CustomerName = customer?.FullName,
                Items = invoice.Item.Select(i => new
                {
                    ItemId = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    Price = i.Price,
                    Total = i.Total
                }).ToList(),
                TotalAmount = invoice.TotalAmount
            };

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, response);
        }


        [HttpPut]
        public ActionResult<object> UpdateInvoice([FromBody] DtoInvoiceUpdateRequest invoiceRequest)
        {
            var invoice = _context.Invoices.FirstOrDefault(i => i.Id == invoiceRequest.Id);
            if (invoice == null)
                return NotFound();

            invoice.CreatedDate = invoiceRequest.CreatedDate;
            invoice.PaymentStatus = invoiceRequest.PaymentStatus;
            invoice.PaymentTerm = (PaymentTerm)invoiceRequest.PaymentTerm;
            invoice.CustomerId = invoiceRequest.CustomerId;
            invoice.Description = invoiceRequest.Description;
            invoice.UpdatedDate = DateTime.Now;

            foreach (var quantityUpdate in invoiceRequest.Item)
            {
                var dbItem = _context.InvoiceItems
                    .FirstOrDefault(x => x.InvoiceId == invoiceRequest.Id && x.Id == quantityUpdate.Id);

                if (dbItem != null)
                {
                    int oldQuantity = dbItem.Quantity;

                    dbItem.Quantity = quantityUpdate.Quantity;
                    dbItem.Total = dbItem.Price * dbItem.Quantity;
                    
                    int quantityDifference = oldQuantity - quantityUpdate.Quantity;

                    var item = _context.Items.FirstOrDefault(p => p.Name == dbItem.Name);
                    if (item != null)
                    {
                        item.Quantity += quantityDifference;
                    }
                }
                else
                {
                    return BadRequest("Item Bulunamadı!");
                }
            }

            _context.SaveChanges();

            var response = new
            {
                InvoiceId = invoice.Id,
                CreatedDate = invoice.CreatedDate,
                UpdatedDate = invoice.UpdatedDate,
                PaymentStatus = invoice.PaymentStatus,
                PaymentTerm = invoice.PaymentTerm,
                Description = invoice.Description,
                CustomerId = invoice.CustomerId,
                Items = _context.InvoiceItems
                    .Where(i => i.InvoiceId == invoice.Id)
                    .Select(i => new
                    {
                        ItemId = i.Id,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        Total = i.Total
                    }).ToList()
            };
            return Ok(response);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteInvoice(int id)
        {
            var invoice = _context.Invoices.Include(i => i.Items).FirstOrDefault(i => i.Id == id);

            if (invoice is null)
                return NotFound();

            _context.Invoices.Remove(invoice);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
