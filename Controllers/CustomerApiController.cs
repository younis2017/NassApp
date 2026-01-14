using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Domain.Entities;

namespace Nass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersApiController : ControllerBase
    {
        private readonly NassadContext _context;

        public CustomersApiController(NassadContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var customers = await _context.Customers.ToListAsync();
            return Ok(customers);
        }

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _context.Customers
                .Include(c => c.Transactions)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
                return NotFound();

            return Ok(customer);
        }

        // =========================
        // SEARCH BY NAME
        // =========================
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            var customers = await _context.Customers
                .Where(c => c.CustomerName.Contains(name))
                .ToListAsync();

            return Ok(customers);
        }

        // =========================
        // POST (Create)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            customer.CustomerJoinedDate = DateTime.UtcNow;
            customer.CustomerUid = Guid.NewGuid();

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.CustomerId },
                customer
            );
        }

        // =========================
        // PUT (Update)
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Customer customer)
        {
            if (id != customer.CustomerId)
                return BadRequest("Customer ID mismatch");

            var existingCustomer = await _context.Customers.FindAsync(id);
            if (existingCustomer == null)
                return NotFound();

            existingCustomer.CustomerName = customer.CustomerName;
            existingCustomer.CustomerPhone = customer.CustomerPhone;
            existingCustomer.CustomerEmail = customer.CustomerEmail;
            existingCustomer.CustomerAddress = customer.CustomerAddress;
            existingCustomer.CustomerLocation = customer.CustomerLocation;
            existingCustomer.CustomerTaxId = customer.CustomerTaxId;
            existingCustomer.CustomerTenet = customer.CustomerTenet;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // DELETE (Hard Delete)
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
                return NotFound();

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // LOGIN (Basic – Demo)
        // =========================
        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.CustomerUsername == username &&
                    c.CustomerPassword == password);

            if (customer == null)
                return Unauthorized("Invalid credentials");

            return Ok(new
            {
                customer.CustomerId,
                customer.CustomerName,
                customer.CustomerEmail,
                customer.CustomerUid
            });
        }

        // CustomersKPI Controller
        [HttpGet("customer")]
        public async Task<IActionResult> GetCustomerDashboard([FromQuery] string tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant))
                return BadRequest("Tenant is required");

            var today = DateTime.Today;

            var baseQuery = _context.Transactions
                .Where(t => t.Customer.CustomerTenet == tenant);

            return Ok(new
            {
                totalOrders = await baseQuery.CountAsync(),

                newOrdersToday = await baseQuery
                    .Where(t => t.Trans_date >= today)
                    .CountAsync(),

                pendingOrders = await baseQuery
                    .Where(t => t.TransStatus == 0)
                    .CountAsync(),

                confirmedOrders = await baseQuery
                    .Where(t => t.TransStatus == 1)
                    .CountAsync()
            });
        }

    }
}
