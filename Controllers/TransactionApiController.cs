using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Models;

namespace Nass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsApiController : ControllerBase
    {
        private readonly NassadContext _context;

        public TransactionsApiController(NassadContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Agency)
                .ToListAsync();

            return Ok(transactions);
        }

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var transaction = await _context.Transactions
                .Include(t => t.Customer)
                .Include(t => t.Agency)
                .FirstOrDefaultAsync(t => t.TransId == id);

            if (transaction == null)
                return NotFound();

            return Ok(transaction);
        }

        // =========================
        // GET BY CUSTOMER
        // =========================
        [HttpGet("by-customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.CustomerId == customerId)
                .ToListAsync();

            return Ok(transactions);
        }

        // =========================
        // GET BY AGENCY
        // =========================
        [HttpGet("by-agency/{agencyId}")]
        public async Task<IActionResult> GetByAgency(int agencyId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.AgencyId == agencyId)
                .ToListAsync();

            return Ok(transactions);
        }

        // =========================
        // POST (Create Transaction)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            transaction.TransUid = Guid.NewGuid();
            transaction.TransDate = DateTime.UtcNow;
            transaction.TransStatus = "New";

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = transaction.TransId },
                transaction
            );
        }

        // =========================
        // PUT (Assign Agency)
        // =========================
        [HttpPut("assign-agency/{transId}")]
        public async Task<IActionResult> AssignAgency(int transId, int agencyId)
        {
            var transaction = await _context.Transactions.FindAsync(transId);
            if (transaction == null)
                return NotFound();

            transaction.AgencyId = agencyId;
            transaction.TransRecivedDate = DateTime.UtcNow;
            transaction.TransStatus = "Assigned";

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // PUT (Update Status)
        // =========================
        [HttpPut("status/{transId}")]
        public async Task<IActionResult> UpdateStatus(int transId, string status)
        {
            var transaction = await _context.Transactions.FindAsync(transId);
            if (transaction == null)
                return NotFound();

            transaction.TransStatus = status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // DELETE (Hard Delete)
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
                return NotFound();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
