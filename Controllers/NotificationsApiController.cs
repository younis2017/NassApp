using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Models;
using Nass.Services.Email;
using Nass.Services.SMS;

namespace Nass.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsApiController : ControllerBase
    {
        private readonly NassadContext _context;
        private readonly IEmailService<EmailService> _emailService;
        private readonly TwilioSmsService _sms;
        public NotificationsApiController(NassadContext context,
            IEmailService<EmailService> emailService,
            TwilioSmsService sms)
        {
            _context = context;
            _emailService = emailService;
            _sms = sms;
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

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                // Use a transaction inside the strategy
                await using var dbTransaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var transaction = await _context.Transactions
                        .FirstOrDefaultAsync(t => t.Trans_id == transactionId);

                    if (transaction == null)
                        return BadRequest(new { message = "Transaction not found" });

                    if (transaction.TransStatus == 1)
                        return BadRequest(new { message = "Transaction already confirmed by another agency" });

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
                    // Load customer for email
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerId == transaction.Customer_id);
                    if (customer != null && !string.IsNullOrWhiteSpace(customer.CustomerEmail))
                    {
                        string confirmEmailBody = $@"
<!DOCTYPE html>
<html>
<head>
<style>
body {{ font-family: Arial; background:#f4f6f8; padding:20px; }}
.container {{ max-width:650px; background:#fff; margin:auto; border-radius:8px; overflow:hidden; }}
.header {{ background:#28a745; color:#fff; padding:20px; text-align:center; }}
.content {{ padding:20px; color:#000; }}
.box {{ background:#f9f9f9; padding:15px; border-radius:5px; margin:15px 0; }}
.footer {{ background:#28a745; color:#fff; text-align:center; padding:15px; font-size:12px; }}
</style>
</head>
<body>

<div class='container'>
<div class='header'>
<h2>🎉 Your Order Is Confirmed</h2>
<p>NASS Advertising & Designing</p>
</div>

<div class='content'>
<p>Dear <strong>{customer.CustomerName}</strong>,</p>

<p>Good news! Your order has been <strong>confirmed</strong> and assigned to one of our partner agencies.</p>

<div class='box'>
<h3>🏢 Agency Details</h3>
<p><strong>Name:</strong> {agency.AgencyName}</p>
<p><strong>Email:</strong> {agency.AgencyEmail}</p>
<p><strong>Phone:</strong> {agency.AgencyPhone}</p>
</div>

<div class='box'>
<h3>📦 Order Details</h3>
<p><strong>Service:</strong> {transaction.Trans_categories}</p>
<p><strong>Description:</strong><br />{transaction.Trans_description}</p>
<p><strong>Confirmed On:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm}</p>
</div>

<p>The agency will contact you shortly to proceed.</p>

<p>Thank you for choosing <strong>NASS Advertising & Designing</strong>.</p>
</div>

<div class='footer'>
support@nassad.ca | +1 (647) 913-1282<br/>
© {DateTime.UtcNow.Year} Nassad
</div>
</div>

</body>
</html>";

                        await _emailService.SendAsync(
                            customer.CustomerEmail,
                            "🎉 Your Order Has Been Confirmed – Nassad",
                            confirmEmailBody
                        );
                    }


                    await dbTransaction.CommitAsync();

                    return Ok(new { message = $"Transaction confirmed by {agency.AgencyName}" });
                }
                catch (Exception ex)
                {
                    await dbTransaction.RollbackAsync();
                    return StatusCode(500, new { message = "Server error", detail = ex.Message });
                }
            });
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
