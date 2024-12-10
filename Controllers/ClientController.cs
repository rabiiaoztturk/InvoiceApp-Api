using InvoiceApp.Data;
using InvoiceApp.Dto;
using InvoiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ClientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetClients()
        {
            var clients = _context.Client
                .Include(c => c.Customers)
                    .ThenInclude(customer => customer.Invoices)
                        .ThenInclude(invoice => invoice.Item)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Adress,
                    c.City,
                    c.PostCode,
                    c.Country,
                    Customers = c.Customers.Select(customer => new
                    {
                        FullName = customer.FullName,
                        Email = customer.Email,
                        Address = customer.Address,
                        City = customer.City,
                        PostCode = customer.PostCode,
                        Country = customer.Country,
                        Invoices = customer.Invoices.Select(invoice => new
                        {
                            invoice.Id,
                            invoice.CreatedDate,
                            invoice.Description,
                            invoice.PaymentStatus,
                            PaymentTerm = invoice.PaymentTerm.ToString(),
                            TotalAmount = invoice.Item.Sum(item => item.Quantity * item.Price),
                            Items = invoice.Item.Select(item => new
                            {
                                item.Id,
                                item.Name,
                                item.Description,
                                item.Quantity,
                                item.Price,
                                Total = item.Quantity * item.Price
                            }).ToList()
                        }).ToList()
                    }).ToList()
                })
                .ToList();

            return Ok(clients);
        }

        [HttpGet("{id}")]
        public ActionResult<Client> GetClient(int id)
        {
            var client = _context.Client.FirstOrDefault(c => c.Id == id);
            if (client == null)
            {
                return NotFound();
            }
            return Ok(client);
        }

        [HttpPut("{id}")]
        public ActionResult<Client> UpdateClient(int id, [FromBody] Client updatedClient)
        {
            var client = _context.Client.FirstOrDefault(c => c.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            client.Name = updatedClient.Name;
            client.Adress = updatedClient.Adress;
            client.City = updatedClient.City;
            client.PostCode = updatedClient.PostCode;
            client.Country = updatedClient.Country;

            _context.SaveChanges();
            return Ok(client);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClient(int id)
        {
            var client = _context.Client.Find(id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Client.Remove(client);
            _context.SaveChanges();
            return NoContent();
        }
    }
}
