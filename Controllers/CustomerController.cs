using InvoiceApp.Data;
using InvoiceApp.Dto;
using InvoiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetCustomers()
        {
            var customers = _context.Customers
                .Include(c => c.Invoices)
                    .ThenInclude(i => i.Items)
                .Select(c => new
                {
                    Id = c.Id,
                    FullName = c.FullName,
                    Email = c.Email,
                    Address = c.Address,
                    City = c.City,
                    Country = c.Country,
                    PostCode = c.PostCode,
                    ClientId = c.ClientId,
                    Invoices = c.Invoices.Select(i => new
                    {
                        i.Id,
                        i.CreatedDate,
                        i.Description,
                        i.PaymentStatus,
                        PaymentTerm = i.PaymentTerm.ToString(),
                        TotalAmount = i.Items.Sum(item => item.Quantity * item.Price), 
                        Items = i.Items.Select(item => new
                        {
                            item.Id,
                            item.Name,
                            item.Description,
                            item.Quantity,
                            item.Price,
                            Total = item.Quantity * item.Price 
                        }).ToList()
                    }).ToList()
                })
                .ToList();

            return Ok(customers);
        }



        [HttpGet("{id}")]
        public ActionResult<Customer> GetCustomer(int id)
        {
            var customer = _context.Customers.FirstOrDefault(c => c.Id == id);

            if (customer is null)
                return NotFound();

            return Ok(customer);
        }

        [HttpGet("{fullname}")]
        public ActionResult<IEnumerable<Customer>> GetCustomersByName(string fullname)
        {
            var customers = _context.Customers.Where(c => c.FullName.Contains(fullname)).ToList();

            if (!customers.Any())
                return NotFound();

            return Ok(customers);
        }

        [HttpPost]
        public ActionResult<Customer> AddCustomer([FromBody] DtoCustomerCreateRequest customerRequest)
        {
            var defaultClient = _context.Client.FirstOrDefault();

            if (defaultClient == null)
            {
                return BadRequest("Varsayılan bir Client bulunamadı.");
            }

            var customer = new Customer
            {
                FullName = customerRequest.FullName,
                Email = customerRequest.Email,
                Address = customerRequest.Address,
                City = customerRequest.City,
                Country = customerRequest.Country,
                PostCode = customerRequest.PostCode,
                ClientId = 3 
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            var response = new
            {
                Message = "Müşteri eklendi.",
                CustomerId = customer.Id,
                FullName = customer.FullName,
                Email = customer.Email,
                Address = customer.Address,
                City = customer.City,
                Country = customer.Country,
                PostCode = customer.PostCode
            };

            return Ok(response);
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteCustomer(int id)
        {
            var customer = _context.Customers.Find(id);
            if (customer is null)
            {
                return NotFound("Silinecek müşteri bulunamadı.");
            }

            _context.Customers.Remove(customer);
            _context.SaveChanges();

            return Ok("Silme işlemi başarılı.");
        }

    }
}