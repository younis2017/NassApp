using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using System.Text.Json.Serialization;

namespace Nass.Controllers
{
    [ApiController]
    [Route("api/settings")]
    public class SettingsApiController : ControllerBase
    {
        private readonly NassadContext _context;

        public SettingsApiController(NassadContext context)
        {
            _context = context;
        }

        // =========================================================
        // GET SETTINGS (LOAD FORM)
        // =========================================================
        [HttpGet("me")]
        public async Task<IActionResult> GetMySettings(string userType, string tenant)
        {
            if (string.IsNullOrEmpty(tenant))
                return BadRequest("Tenant is required");

            if (userType == "Agency")
            {
                var agency = await _context.Agencies
                    .Where(a => a.AgencyTenet == tenant)
                    .Select(a => new
                    {
                        name = a.AgencyName,
                        email = a.AgencyEmail,
                        address = a.AgencyAddress,
                        website = a.AgencyWebsite,
                        phone = a.AgencyPhone,
                        emailNotifications = a.EmailNotifications,
                        darkMode = a.DarkMode
                    })
                    .FirstOrDefaultAsync();

                if (agency == null) return NotFound();
                return Ok(agency);
            }

            if (userType == "Customer")
            {
                var customer = await _context.Customers
                    .Where(c => c.CustomerTenet == tenant)
                    .Select(c => new
                    {
                        name = c.CustomerName,
                        email = c.CustomerEmail,
                        emailNotifications = c.EmailNotifications,
                        darkMode = c.DarkMode
                    })
                    .FirstOrDefaultAsync();

                if (customer == null) return NotFound();
                return Ok(customer);
            }

            return BadRequest("Invalid user type");
        }

        // =========================================================
        // UPDATE PROFILE
        // =========================================================
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileRequest body)
        {
            if (body == null)
                return BadRequest("Invalid request");

            if (body.UserType == "Agency")
            {
                var agency = await _context.Agencies
                    .FirstOrDefaultAsync(a => a.AgencyTenet == body.Tenant);
                if (agency == null) return NotFound();

                agency.AgencyName = body.Name;
                agency.AgencyEmail = body.Email;
                agency.AgencyAddress = body.Address;
                agency.AgencyPhone = body.Phone;
                agency.AgencyWebsite = body.Website;
            }
            else if (body.UserType == "Customer")
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerTenet == body.Tenant);
                if (customer == null) return NotFound();

                customer.CustomerName = body.Name;
                customer.CustomerEmail = body.Email;
                customer.CustomerAddress = body.Address;
                customer.CustomerPhone = body.Phone;
                }
            else
            {
                return BadRequest("Invalid user type");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated successfully" });
        }


        // =========================================================
        // CHANGE PASSWORD
        // =========================================================
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest body)
        {
            if (body == null)
                return BadRequest("Invalid request");

            if (body.UserType == "Agency")
            {
                var agency = await _context.Agencies
                    .FirstOrDefaultAsync(a => a.AgencyTenet == body.Tenant);

                if (agency == null) return NotFound();

                if (agency.AgencyPassword != body.CurrentPassword)
                    return BadRequest("Current password incorrect");

                agency.AgencyPassword = body.NewPassword;
            }
            else if (body.UserType == "Customer")
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerTenet == body.Tenant);

                if (customer == null) return NotFound();

                if (customer.CustomerPassword != body.CurrentPassword)
                    return BadRequest("Current password incorrect");

                customer.CustomerPassword = body.NewPassword;
            }
            else
            {
                return BadRequest("Invalid user type");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Password updated successfully" });
        }



        // =========================================================
        // UPDATE PREFERENCES
        // =========================================================
        [HttpPut("preferences")]
        public async Task<IActionResult> UpdatePreferences([FromBody] PreferencesRequest body)
        {
            if (body == null)
                return BadRequest("Invalid request");

            if (body.UserType == "Agency")
            {
                var agency = await _context.Agencies
                    .FirstOrDefaultAsync(a => a.AgencyTenet == body.Tenant);
                if (agency == null) return NotFound();

                agency.EmailNotifications = body.EmailNotifications;
                agency.DarkMode = body.DarkMode;
            }
            else if (body.UserType == "Customer")
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.CustomerTenet == body.Tenant);
                if (customer == null) return NotFound();

                customer.EmailNotifications = body.EmailNotifications;
                customer.DarkMode = body.DarkMode;
            }
            else
            {
                return BadRequest("Invalid user type");
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Preferences saved successfully" });
        }

    }
}
public class ProfileRequest
{
    [JsonPropertyName("userType")]
    public string UserType { get; set; }

    [JsonPropertyName("tenant")]
    public string Tenant { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

        [JsonPropertyName("address")]
    public string Address { get; set; }

 [JsonPropertyName("phone")]
    public string Phone { get; set; }

   
    [JsonPropertyName("website")]
    public string Website { get; set; }
}

public class ChangePasswordRequest
{
    [JsonPropertyName("userType")]
    public string UserType { get; set; }

    [JsonPropertyName("tenant")]
    public string Tenant { get; set; }

    [JsonPropertyName("currentPassword")]
    public string CurrentPassword { get; set; }

    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; }
}

public class PreferencesRequest
{
    [JsonPropertyName("userType")]
    public string UserType { get; set; }

    [JsonPropertyName("tenant")]
    public string Tenant { get; set; }

    [JsonPropertyName("emailNotifications")]
    public bool EmailNotifications { get; set; }

    [JsonPropertyName("darkMode")]
    public bool DarkMode { get; set; }
}