using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Helpers;
using Nass.Hubs;
using Nass.Models;
using Nass.Services.Email;
using Nass.Services.SMS;

namespace Nass.Controllers
{

    [ApiController]
    [Route("api/transactions")]
    public class TransactionsApiController : ControllerBase
    {
        private readonly NassadContext _context;
        private readonly NotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IEmailService<EmailService> _emailService;
        private readonly TwilioSmsService _sms;
        public TransactionsApiController(
            NassadContext context,
            NotificationService notificationService,
            IHubContext<NotificationHub> hub,
            IEmailService<EmailService> emailService,
            TwilioSmsService sms)
           

        {
            _context = context;
            _notificationService = notificationService;
            _hub = hub;
            _emailService = emailService;
             _sms = sms;
        }
        //Get DateTime by Eastern Standard
      
            DateTime tz = DateTimeHelper.NowET();
        


        // =========================
        // GET ALL TRANSACTIONS
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
        // CREATE TRANSACTION (BLIND)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == transaction.Customer_id);

            if (customer == null)
                return BadRequest("Customer not found");

            if (string.IsNullOrEmpty(customer.CustomerTenet))
            {
                customer.CustomerTenet = GenerateCustomerTenet(customer);
                await _context.SaveChangesAsync();
            }

            transaction.Trans_uid = Guid.NewGuid();
            transaction.Trans_date = tz;
            transaction.TransStatus = 0;
            transaction.Agency_id = null;            // blind
            transaction.Agency_tenat = customer.CustomerTenet;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // 🔔 Broadcast ONLY (no DB writes here)
            await _notificationService.BroadcastNewTransactionAsync(transaction);

            return Ok(transaction);
        }

        // =========================
        // ACCEPT TRANSACTION
        // =========================
        [HttpPut("accept/{transactionId}")]
        public async Task<IActionResult> Accept(int transactionId)
        {
            int agencyId = GetLoggedInAgencyId();

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Trans_id == transactionId &&
                    t.TransStatus ==0);

            if (transaction == null)
                return BadRequest("Transaction already handled");

            transaction.TransStatus = 1;
            transaction.Agency_id = agencyId;
            transaction.trans_recived_date = DateTime.UtcNow;

            // Save history (winner)
            _context.NotificationRecipients.Add(new NotificationRecipient
            {
                Trans_id = transaction.Trans_id,
                AgencyId = agencyId,
                Status = "Confirmed",
                ReadAt = tz
            });

            await _context.SaveChangesAsync();

            // 🔔 Remove from all dashboards instantly
            await _hub.Clients.Group("Agencies")
                .SendAsync("TransactionClaimed", transaction.Trans_id);

            return Ok(new { message = "Transaction accepted" });
        }

        // =========================
        // REJECT TRANSACTION
        // =========================
        [HttpPut("reject/{transactionId}")]
        public async Task<IActionResult> Reject(int transactionId)
        {
            int agencyId = GetLoggedInAgencyId();

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Trans_id == transactionId &&
                    t.TransStatus == 0);

            if (transaction == null)
                return BadRequest("Transaction already handled");

            _context.NotificationRecipients.Add(new NotificationRecipient
            {
                Trans_id = transaction.Trans_id,
                AgencyId = agencyId,
                Status = "Rejected",
                ReadAt = tz
            });

            await _context.SaveChangesAsync();
            return Ok(new { message = "Transaction rejected" });
        }

        // =========================
        // DELETE
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return NotFound();

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // HELPERS
        // =========================
        private string GenerateCustomerTenet(Customer customer)
        {
            var name = customer.CustomerName ?? "CUST";
            var letters = new string(name.Where(char.IsLetter).ToArray());
            var last4 = letters.Length >= 4 ? letters[^4..] : letters.PadLeft(4, 'X');
            var last3 = customer.CustomerId.ToString().PadLeft(3, '0');
            return (last4 + last3).ToUpper();
        }

        // TODO: Replace with JWT
        private int GetLoggedInAgencyId()
        {
            return 1;
        }

        [HttpPost("receive-order")]
        public async Task<IActionResult> ReceiveOrder()
        {
            var form = Request.Form;

            string customerName = form.TryGetValue("CustomerName", out var name) ? name.ToString() : "";
            string customerPhone = form.TryGetValue("CustomerPhone", out var phone) ? phone.ToString() : "";
            string customerEmail = form.TryGetValue("CustomerEmail", out var email) ? email.ToString() : "";
            string customerAddress = form.TryGetValue("CustomerAddress", out var address) ? address.ToString() : "";
            string transUrl = form.TryGetValue("transUrl", out var location) ? location.ToString() : "";

            string transCategory = form.TryGetValue("Trans_categories", out var cat) ? cat.ToString() : "";
            string transDescription = form.TryGetValue("Trans_description", out var desc) ? desc.ToString() : "";

            if (string.IsNullOrWhiteSpace(customerName))
                return BadRequest("Customer name is required");

            if (string.IsNullOrWhiteSpace(customerPhone))
                return BadRequest("Customer phone is required");

            // ===============================
            // 1️⃣ FIND OR CREATE CUSTOMER
            // ===============================
            var customer = await _context.Customers
    .FirstOrDefaultAsync(c => c.CustomerPhone == customerPhone &&
        c.CustomerEmail == customerEmail);

            // 🔎 NEW: single, global rule — block inactive customers immediately
            if (customer != null && customer.CustomerStatus != 0 )
            
                return BadRequest("This customer is inactive. Cannot create order.");
           

                bool isNewCustomer = false;
                string tenant;
                string password;

                if (customer == null)
                {
                    tenant = TenantGenerator.GenerateTenant(customerName, customerPhone);
                    password = TenantGenerator.GeneratePassword();

                    customer = new Customer
                    {
                        CustomerName = customerName,
                        CustomerPhone = customerPhone,
                        CustomerEmail = customerEmail,
                        CustomerAddress = customerAddress,
                        CustomerLocation = "",
                        CustomerTenet = tenant,
                        CustomerUsername = tenant,
                        CustomerPassword = password,
                        CustomerUid = Guid.NewGuid(),
                        CustomerJoinedDate = tz,
                        CustomerStatus = 0 // active
                    };

                    _context.Customers.Add(customer);
                    await _context.SaveChangesAsync();
                    isNewCustomer = true;
                }
                else
                {
                    tenant = customer.CustomerTenet;
                    password = customer.CustomerPassword;


                }
            
            // ===============================
            // 2️⃣ CREATE TRANSACTION
            // ===============================
            var transaction = new Transaction
            {
                Trans_uid = Guid.NewGuid(),
                Trans_date = tz,
                Trans_categories = transCategory,
                Trans_description = transDescription,
                Trans_url = transUrl,
                TransStatus = 0,
                Customer_id = customer.CustomerId
            };
          
            _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                // ===============================
                // 3️⃣ CREATE NOTIFICATION
                // ===============================
                var notification = new Notification
                {
                    Trans_Id = transaction.Trans_id,
                    Title = "New Blind Order",
                    Message = "A new order is available for agencies",
                    IsPublished = true,
                    CreatedAt = tz      
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // ===============================
                // 4️⃣ SEND TO ALL AGENCIES (DB)
                // ===============================
                var agencies = await _context.Agencies.ToListAsync();
            var activeAgencies = agencies
    .Where(a => a.AgencyStatus == 0 && !string.IsNullOrWhiteSpace(a.AgencyEmail))
    .ToList();
            foreach (var agency in activeAgencies)
                {
           

                _context.NotificationRecipients.Add(new NotificationRecipient
                    {
                        NotificationId = notification.NotificationId,
                        Trans_id = transaction.Trans_id,
                        AgencyId = agency.AgencyId,
                        Status = "Pending",
                        IsRead = false
                    });
                }

                await _context.SaveChangesAsync();

                // ===============================
                // 5️⃣ EMAIL ALL AGENCIES
                // ===============================
                foreach (var agency in activeAgencies)
                {
                if (!string.IsNullOrWhiteSpace(agency.AgencyPhone))
                {
                    try
                    {
                        _sms.SendSms(
                            agency.AgencyPhone,
                            $"There's new Order by NASSAD.ca" +
                            $"Service: {transaction.Trans_categories}. please login and accept the order."
                        );
                    }
                    catch (Exception ex)
                    {
                        // Optional: log but do NOT fail order
                        Console.WriteLine("SMS Error: " + ex.Message);
                    }
                }
                if (string.IsNullOrWhiteSpace(agency.AgencyEmail))
                        continue;

                    string agencyEmailBody = $@"
<!DOCTYPE html>
<html>
<head>
<style>
body {{ font-family: Arial; background:#f4f6f8; padding:20px; }}
.container {{ max-width:650px; background:#fff; margin:auto; border-radius:8px; overflow:hidden; }}
.header {{ background:#ff7a00; color:#fff; padding:20px; text-align:center; }}
.content {{ padding:20px; color:#000; }}
.box {{ background:#f9f9f9; padding:15px; border-radius:5px; margin:15px 0; }}
.footer {{ background:#ff7a00; color:#fff; text-align:center; padding:15px; font-size:12px; }}
.btn {{ background:#ff7a00; color:#fff!important; padding:10px 20px; text-decoration:none; border-radius:5px; }}
</style>
</head>
<body>

<div class='container'>
<div class='header'>
<h2>🆕 New Order Available</h2>
<p>NASS Advertising & Designing</p>
</div>

<div class='content'>
<p>Hello <strong>{agency.AgencyName}</strong>,</p>
<p>A new customer order is now available.</p>

<div class='box'>
<p><strong>Customer:</strong> {customer.CustomerName}</p>
<p><strong>Service:</strong> {transaction.Trans_categories}</p>
<p><strong>Description:</strong><br />{transaction.Trans_description}</p>
<p><strong>Date:</strong> {transaction.Trans_date:yyyy-MM-dd HH:mm}</p>
</div>

<a class='btn' href='https://localhost:7249/home/login'>View Order</a>
</div>

<div class='footer'>
support@nassad.ca | +1 (647) 913-1282<br/>
© {tz.Year} Nassad
</div>
</div>

</body>
</html>";

                    await _emailService.SendAsync(
                        agency.AgencyEmail,
                        "🆕 New Order Available – Nassad",
                        agencyEmailBody
                    );
                }

                // ===============================
                // SEND SINGLE COPY TO SUPPORT
                // ===============================
                string supportEmailBody = $@"
<!DOCTYPE html>
<html>
<head>
<style>
body {{ font-family: Arial; background:#f4f6f8; padding:20px; }}
.container {{ max-width:650px; background:#fff; margin:auto; border-radius:8px; overflow:hidden; }}
.header {{ background:#ff7a00; color:#fff; padding:20px; text-align:center; }}
.content {{ padding:20px; color:#000; }}
.box {{ background:#f9f9f9; padding:15px; border-radius:5px; margin:15px 0; }}
.footer {{ background:#ff7a00; color:#fff; text-align:center; padding:15px; font-size:12px; }}
</style>
</head>
<body>

<div class='container'>
<div class='header'>
<h2>📩 New Order Broadcast</h2>
<p>NASS Advertising & Designing</p>
</div>

<div class='content'>
<p>A new order has been sent to <strong>{agencies.Count}</strong> agencies.</p>

<div class='box'>
<p><strong>Customer:</strong> {customer.CustomerName}</p>
<p><strong>Service:</strong> {transaction.Trans_categories}</p>
<p><strong>Description:</strong><br />{transaction.Trans_description}</p>
<p><strong>Date:</strong> {transaction.Trans_date:yyyy-MM-dd HH:mm}</p>
</div>
</div>

<div class='footer'>
© {tz.Year} Nassad
</div>
</div>

</body>
</html>";
                await _emailService.SendAgency(
                    to: "support@nassad.ca",
                    subject: "📩 New Order Sent to Agencies – Nassad",
                    body: supportEmailBody,
                    bcc: null
                );
            
            // ===============================
            // 6️⃣ EMAIL CUSTOMER CONFIRMATION
            // ===============================
            string credentialsBlock = "";

            if (isNewCustomer)
            {
                credentialsBlock = $@"
    <div class='box'>
        <h3>🔐 Your Account Details</h3>
        <p><strong>Username:</strong> {customer.CustomerUsername}</p>
        <p><strong>Password:</strong> {customer.CustomerPassword}</p>
    </div>";
            }

            if (!string.IsNullOrWhiteSpace(customer.CustomerEmail))
                {
                    string customerEmailBody = $@"
<!DOCTYPE html>
<html>
<head>
<style>
body {{ font-family: Arial; background:#f4f6f8; padding:20px; }}
.container {{ max-width:650px; background:#fff; margin:auto; border-radius:8px; overflow:hidden; }}
.header {{ background:#ff7a00; color:#fff; padding:20px; text-align:center; }}
.content {{ padding:20px; color:#000; }}
.box {{ background:#f9f9f9; padding:15px; border-radius:5px; margin:15px 0; }}
.footer {{ background:#ff7a00; color:#fff; text-align:center; padding:15px; font-size:12px; }}
.btn {{ background:#ff7a00; color:#fff!important; padding:10px 20px; text-decoration:none; border-radius:5px; }}
</style>
</head>
<body>

<div class='container'>
<div class='header'>
<h2>✅ Order Received</h2>
<p>NASS Advertising & Designing</p>
</div>

<div class='content'>
<p>Dear <strong>{customer.CustomerName}</strong>,</p>
<p>Thank you! Your order has been successfully received.</p>
{credentialsBlock}
<div class='box'>
<p><strong>Service:</strong> {transaction.Trans_categories}</p>
<p><strong>Description:</strong><br />{transaction.Trans_description}</p>
<p><strong>Date:</strong> {transaction.Trans_date:yyyy-MM-dd HH:mm}</p>
</div>

<p>Our partner agencies are reviewing your request.</p>

<a class='btn' href='https://nassad.ca'>Visit Our Website</a>
</div>

<div class='footer'>
support@nassad.ca | +1 (647) 913-1282<br/>
© {tz.Year} Nassad
</div>
</div>

</body>
</html>";

                    await _emailService.SendAsync(
                        customer.CustomerEmail,
                        "✅ Your Order Has Been Received – Nassad",
                        customerEmailBody
                    );
                }

                // ===============================
                // 7️⃣ RESPONSE
                // ===============================
                return Ok(new
                {
                    message = "Order submitted successfully",
                    customerId = customer.CustomerId,
                    transactionId = transaction.Trans_id,
                    isNewCustomer
                });
            
        }

        

    }
}
