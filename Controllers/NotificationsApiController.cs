using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Models;

namespace Nass.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsApiController : ControllerBase
    {
        private readonly NassadContext _context;

        public NotificationsApiController(NassadContext context)
        {
            _context = context;
        }

        // =========================
        // GET: All unconfirmed transactions (blind orders)
        // =========================
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _context.Transactions
                .Where(t => t.TransStatus == 0) // only unconfirmed
                .Select(t => new
                {
                    t.Trans_id,
                    t.Trans_date,
                    t.Trans_categories,
                    t.Trans_description,
                    t.TransStatus,
                    Customer = new
                    {
                        t.Customer.CustomerId,
                        t.Customer.CustomerName
                    },
                    AgencyId = t.Agency_id,
                    AgencyTenet = t.Agency_tenat
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // =========================
        // CONFIRM: first agency wins
        // =========================
        [HttpPut("confirm/{transactionId}")]
        public async Task<IActionResult> Confirm(int transactionId, [FromQuery] string tenant)
        {
            var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.AgencyTenet == tenant);
            if (agency == null)
                return BadRequest(new { message = "Invalid tenant" });

            // Use a transaction to avoid race conditions
            using var dbTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transaction = await _context.Transactions
                    .FirstOrDefaultAsync(t => t.Trans_id == transactionId);

                if (transaction == null)
                    return BadRequest(new { message = "Transaction not found" });

                if (transaction.TransStatus == 1)
                {
                    // Already confirmed by another agency
                    return BadRequest(new { message = "Transaction already confirmed by another agency" });
                }

                // 1️⃣ Mark transaction as confirmed
                transaction.TransStatus = 1;
                transaction.Agency_id = agency.AgencyId;
                transaction.Agency_tenat = agency.AgencyTenet;
                transaction.trans_recived_date = DateTime.UtcNow;

                // 2️⃣ Update NotificationRecipients
                var recipients = await _context.NotificationRecipients
                    .Where(nr => nr.Trans_id == transactionId)
                    .ToListAsync();

                foreach (var r in recipients)
                {
                    if (r.AgencyId == agency.AgencyId)
                    {
                        r.Status = "Confirmed";
                        r.IsRead = true;
                        r.ReadAt = DateTime.UtcNow;
                    }
                    else
                    {
                        r.Status = "Rejected";
                    }
                }

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return Ok(new { message = $"Transaction confirmed by {agency.AgencyName}" });
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return StatusCode(500, new { message = "Server error", detail = ex.Message });
            }
        }

        // =========================
        // REJECT: logs rejection only, does not block others
        // =========================
        [HttpPut("reject/{transactionId}")]
        public async Task<IActionResult> Reject(int transactionId, [FromQuery] string tenant)
        {
            var agency = await _context.Agencies.FirstOrDefaultAsync(a => a.AgencyTenet == tenant);
            if (agency == null)
                return BadRequest(new { message = "Invalid tenant" });

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t => t.Trans_id == transactionId);

            if (transaction == null)
                return BadRequest(new { message = "Transaction not found" });

            // Find recipient row for this agency
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(nr => nr.Trans_id == transactionId && nr.AgencyId == agency.AgencyId);

            if (recipient == null)
                return BadRequest(new { message = "Recipient not found" });

            // Only update this agency's row
            recipient.Status = "Rejected";
            recipient.IsRead = true;
            recipient.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Transaction rejected by {agency.AgencyName}" });
        }

    }
}
