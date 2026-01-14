using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Helpers;
using Nass.Hubs;
using System.Net.Mail;
using System.Text.Json;
using Nass.Domain.Entities;
using Nass.Email;
using Nass.SMS;

namespace Nass.Controllers
    {

    [ApiController]
    [Route("api/transactions")]
    public class TransactionsApiController: ControllerBase
        {
        private readonly NassadContext _context;
        private readonly NotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hub;
        private readonly IEmailService<EmailService> _emailService;
        private readonly TwilioSmsService _sms;
        public TransactionsApiController (
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
        public async Task<IActionResult> GetAll ()
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
        public async Task<IActionResult> Create (Transaction transaction)
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
        public async Task<IActionResult> Accept (int transactionId)
            {
            int agencyId = GetLoggedInAgencyId();

            var transaction = await _context.Transactions
                .FirstOrDefaultAsync(t =>
                    t.Trans_id == transactionId &&
                    t.TransStatus == 0);

            if (transaction == null)
                return BadRequest("Transaction already handled");

            // ✅ Assign agency + confirm
            transaction.TransStatus = 1;
            transaction.Agency_id = agencyId;
            transaction.trans_recived_date = tz;

            // ✅ Save history (winner)
            _context.NotificationRecipients.Add(new NotificationRecipient
                {
                Trans_id = transaction.Trans_id,
                AgencyId = agencyId,
                Status = "Confirmed",
                ReadAt = DateTime.UtcNow
                });

            await _context.SaveChangesAsync();

            // 🔔 Remove from dashboards
            await _hub.Clients.Group("Agencies")
                .SendAsync("TransactionClaimed", transaction.Trans_id);

            // =========================
            // 📧 SEND CONFIRM EMAIL
            // =========================
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerId == transaction.Customer_id);

            var agency = await _context.Agencies
                .FirstOrDefaultAsync(a => transaction.Agency_id == a.AgencyId);

            if (customer != null && agency != null && !string.IsNullOrWhiteSpace(customer.CustomerEmail))
                {
                string emailBody = $@"
<!DOCTYPE html>
<html>
<head>
<style>
body {{ font-family: Arial; background:#f4f6f8; padding:20px; }}
.container {{ max-width:650px; background:#fff; margin:auto; border-radius:8px; }}
.header {{ background:#28a745; color:#fff; padding:20px; text-align:center; }}
.content {{ padding:20px; }}
.box {{ background:#f9f9f9; padding:15px; border-radius:5px; margin:15px 0; }}
.footer {{ background:#28a745; color:#fff; text-align:center; padding:15px; font-size:12px; }}
</style>
</head>
<body>

<div class='container'>
<div class='header'>
<h2>🎉 Your Order Has Been Confirmed</h2>
<p>NASS Advertising & Designing</p>
</div>

<div class='content'>
<p>Dear <strong>{customer.CustomerName}</strong>,</p>

<p>Great news! Your order has been successfully confirmed by one of our trusted agencies.</p>

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
<p><strong>Confirmed On:</strong> {tz:yyyy-MM-dd HH:mm}</p>
</div>

<p>The agency may contact you shortly to proceed with the next steps.</p>

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
                    "🎉 Your Order Is Confirmed – Nassad",
                    emailBody
                );
                }

            return Ok(new { message = "Transaction accepted & email sent" });
            }


        // =========================
        // REJECT TRANSACTION
        // =========================
        [HttpPut("reject/{transactionId}")]
        public async Task<IActionResult> Reject (int transactionId)
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
        public async Task<IActionResult> Delete (int id)
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
        private string GenerateCustomerTenet (Customer customer)
            {
            var name = customer.CustomerName ?? "CUST";
            var letters = new string(name.Where(char.IsLetter).ToArray());
            var last4 = letters.Length >= 4 ? letters[^4..] : letters.PadLeft(4, 'X');
            var last3 = customer.CustomerId.ToString().PadLeft(3, '0');
            return (last4 + last3).ToUpper();
            }

        // TODO: Replace with JWT
        private int GetLoggedInAgencyId ()
            {
            return 1;
            }

        // ===============================
        //  CREATE TRANSACTION FROM PUBLIC FORM and send to all agencies email + sms
        //  1️⃣ Find or Create Customer
        //  2️⃣ Create Transaction
        //  3️⃣ Create Notification
        //  4️⃣ Send to all Agencies (DB)
        //  5️⃣ Email all Agencies / SMS
        //  6️⃣ Email Customer Confirmation
        //  7️⃣ Response
        // ===============================
        [HttpPost("receive-order")]
        public async Task<IActionResult> ReceiveOrder ()
            {
            var form = Request.Form;

            // ===============================
            // 0️⃣ EXTRACT FORM VALUES
            // ===============================
            string customerName = form.TryGetValue("CustomerName", out var name) ? name.ToString() : "";
            string customerPhone = form.TryGetValue("CustomerPhone", out var phone) ? phone.ToString() : "";
            string customerEmail = form.TryGetValue("CustomerEmail", out var email) ? email.ToString() : "";
            string customerAddress = form.TryGetValue("CustomerAddress", out var address) ? address.ToString() : "";
            string transUrl = form.TryGetValue("transUrl", out var location) ? location.ToString() : "";
            // Multi-select categories (from checkboxes)
            var transCategories = form["Trans_categories"].ToList(); // List<string>
            // Optional: join them into a single string if you want to store in DB
            string transCategoryCsv = string.Join(", ", transCategories);
            string transDescription = form.TryGetValue("Trans_description", out var desc) ? desc.ToString() : "";

            if (string.IsNullOrWhiteSpace(customerName))
                return BadRequest("Customer name is required");

            if (string.IsNullOrWhiteSpace(customerPhone))
                return BadRequest("Customer phone is required");

            // ===============================
            // 1️⃣ HANDLE FILE UPLOAD (SECURE)
            // ===============================
            var uploadedFile = Request.Form.Files["TransFile"];

            if (uploadedFile != null && uploadedFile.Length > 0)
                {
                const long MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

                var allowedExtensions = new[]
 {
    // Images
    ".jpg", ".jpeg", ".png",

    // Documents
    ".pdf", ".doc", ".docx", ".zip",

    // Adobe
    ".psd", ".psb", ".ai", ".eps",

    // AutoCAD
    ".dwg", ".dxf"
};

                var allowedMimeTypes = new[]
                {
                 "image/jpeg",
                 "image/png",
                 "application/pdf",
                 "application/msword",
                 "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                 "application/zip"
                 };

                if (uploadedFile.Length > MAX_FILE_SIZE)
                    return BadRequest("File size exceeds 10MB");

                var extension = Path.GetExtension(uploadedFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                    return BadRequest("File type not allowed");

                if (!allowedMimeTypes.Contains(uploadedFile.ContentType))
                    return BadRequest("Invalid file content");

                //var uploadFolder = Path.Combine(
                //    Directory.GetCurrentDirectory(),
                //    "wwwroot",
                //    "uploads"
                //);

                //if (!Directory.Exists(uploadFolder))
                //    Directory.CreateDirectory(uploadFolder);

                //// 🔐 SAFE filename (do NOT trust user filename)
                //var safeFileName = $"{Guid.NewGuid()}{extension}";
                //var savedFilePath = Path.Combine(uploadFolder, safeFileName);

                //using (var stream = new FileStream(savedFilePath, FileMode.Create))
                //    {
                //    await uploadedFile.CopyToAsync(stream);
                //    }

                //transUrl = $"/uploads/{safeFileName}";
                using var httpClient = new HttpClient();

                using var content = new MultipartFormDataContent();
                content.Add(
                    new StreamContent(uploadedFile.OpenReadStream()),
                    "file", // MUST match $_FILES['file']
                    uploadedFile.FileName
                );

                var response = await httpClient.PostAsync(
                    "https://nassad.ca/api/upload.php",
                    content
                );

                if (!response.IsSuccessStatusCode)
                    {
                    var error = await response.Content.ReadAsStringAsync();
                    return BadRequest($"Upload failed: {error}");
                    }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);

                var relativeUrl = result.GetProperty("url").GetString();
                transUrl = $"https://nassad.ca{relativeUrl}";


                }


            // ===============================
            // 2️⃣ FIND OR CREATE CUSTOMER
            // ===============================
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerPhone == customerPhone &&
                                          c.CustomerEmail == customerEmail);

            if (customer != null && customer.CustomerStatus != 0)
                return BadRequest("This customer is inactive. Cannot create order.");

            bool isNewCustomer = false;
            string tenant;
            string password;
            var tz = DateTime.UtcNow;

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
            // 3️⃣ CREATE TRANSACTION
            // ===============================
            var transaction = new Transaction
                {
                Trans_uid = Guid.NewGuid(),
                Trans_date = tz,
                Trans_categories = transCategoryCsv,
                Trans_description = transDescription,
                Trans_url = transUrl,
                TransStatus = 0,
                Customer_id = customer.CustomerId
                };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            // ===============================
            // 4️⃣ CREATE NOTIFICATION
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
            // 5️⃣ SEND TO ALL ACTIVE AGENCIES
            // ===============================
            var agencies = await _context.Agencies.Where(a => a.AgencyStatus == 0 && !string.IsNullOrWhiteSpace(a.AgencyEmail)).ToListAsync();

            foreach (var agency in agencies)
                {
                _context.NotificationRecipients.Add(new NotificationRecipient
                    {
                    NotificationId = notification.NotificationId,
                    Trans_id = transaction.Trans_id,
                    AgencyId = agency.AgencyId,
                    Status = "Pending",
                    IsRead = false
                    });

                // SEND EMAIL TO AGENCY
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

<a class='btn' href='https://www.nassad.ca'>View Order</a>
</div>

<div class='footer'>
support@nassad.ca | +1 (647) 913-1282<br/>
© {tz.Year} Nassad
</div>
</div>

</body>
</html>";
                if (IsValidEmail(agency.AgencyEmail))
                    {
                    await _emailService.SendAsync(
                     agency.AgencyEmail,
                     "🆕 New Order Available – Nassad",
                     agencyEmailBody
                    );
                    }

                }

            await _context.SaveChangesAsync();

            // ===============================
            // 6️⃣ EMAIL CUSTOMER CONFIRMATION
            // ===============================
            string credentialsBlock = isNewCustomer
                ? $@"
<div class='box'>
<h3>🔐 Your Account Details</h3>
<p><strong>Username:</strong> {customer.CustomerUsername}</p>
<p><strong>Password:</strong> {customer.CustomerPassword}</p>
</div>"
                : "";

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

<a class='btn' href='https://www.nassad.ca'>Visit Our Website</a>
</div>

<div class='footer'>
support@nassad.ca | +1 (647) 913-1282<br/>
© {tz.Year} Nassad
</div>
</div>

</body>
</html>";
                if (IsValidEmail(customer.CustomerEmail))
                    {

                    await _emailService.SendAsync(
                      customer.CustomerEmail,
                     "✅ Your Order Has Been Received – Nassad",
                     customerEmailBody
                    );
                    }

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

        private bool IsValidEmail (string? Email)
            {
            if (string.IsNullOrWhiteSpace(Email)) return false;
            try
                {
                var addr = new MailAddress(Email);
                return true;
                }
            catch
                {
                return false;
                }
            }
        }
    }
