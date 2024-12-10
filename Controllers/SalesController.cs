using InvoiceApp.Data;
using InvoiceApp.Dto;
using InvoiceApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class SalesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<List<DtoInvoiceItemResponse>> GetSaleses()
        {
            var sales = _context.InvoiceItems
                .Select(sales => new DtoInvoiceItemResponse
                {
                    Id = sales.Id,
                    Name = sales.Name,
                    Description = sales.Description,
                    Quantity = sales.Quantity,
                    Price = sales.Price,
                    Total = sales.Total,
                    InvoiceId = sales.InvoiceId
                })
                .ToList();

            return Ok(sales);
        }

        [HttpGet("{id}")]
        public ActionResult<DtoInvoiceItemResponse> GetSales(int id)
        {
            var sales = _context.InvoiceItems.FirstOrDefault(x => x.Id == id);

            if (sales is null)
                return NotFound();

            var response = new DtoInvoiceItemResponse
            {
                Id = sales.Id,
                
                Name = sales.Name,
                Description = sales.Description,
                Quantity = sales.Quantity,
                Price = sales.Price,
                Total = sales.Total,
                InvoiceId = sales.InvoiceId
            };

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteSales(int id)
        {
            return BadRequest("Hoop :) satılan ürünü silemezsin faturayı silmen lazım");
        }
    }
}