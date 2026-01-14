using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Domain.Entities;
using Nass.Data;
namespace Nass.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly NassadContext _context;
        public OrdersController(NassadContext context)
        {
            _context = context;
        }

        // GET api/orders?tenant=xxx&userType=Agency&page=1&pageSize=10
        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] string tenant,
            [FromQuery] string userType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrEmpty(tenant)) return BadRequest("Tenant is required");

            IQueryable<Transaction> query = _context.Transactions.Include(t => t.Customer);

            if (userType == "Agency")
            {
                query = query.Where(t => t.Agency_tenat == tenant);
            }
            else if (userType == "Customer")
            {
                // Look up customer by tenant string
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerTenet == tenant);

                if (customer == null)
                    return NotFound("Customer not found");

                query = query.Where(t => t.Customer_id == customer.CustomerId);
            }

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = await query
                .OrderByDescending(t => t.Trans_date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.Trans_id,
                    CustomerName = t.Customer.CustomerName,
                    TransDate = t.trans_recived_date,
                    t.TransStatus
                })
                .ToListAsync();

            return Ok(new { items, totalPages });
        }

        // GET api/orders/details/ORD12345?tenant=xxx&userType=Customer
        [HttpGet("details/{transId}")]
        public async Task<IActionResult> GetOrderDetails(
     int transId,
     [FromQuery] string tenant,
     [FromQuery] string userType)
        {
            IQueryable<Transaction> query = _context.Transactions.Include(t => t.Customer);

            if (userType == "Agency")
                query = query.Where(t => t.Trans_id == transId && t.Agency_tenat == tenant);
            else if (userType == "Customer")
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerTenet == tenant);

                if (customer == null)
                    return NotFound("Customer not found");

                query = query.Where(t => t.Trans_id == transId && t.Customer_id == customer.CustomerId);
            }

            var order = await query
                .Select(t => new
                {
                    t.Trans_id,
                    t.Customer.CustomerName,
                    TransDate = t.Trans_date,
                    t.TransStatus,
                    t.Trans_categories,
                    t.Trans_description,
                    t.Trans_url
                })
                .FirstOrDefaultAsync();

            if (order == null) return NotFound();
            return Ok(order);
        }

    }
}
