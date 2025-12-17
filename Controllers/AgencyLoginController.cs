using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Helpers;
using Nass.Models;
using System.Text.Json;

namespace Nass.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyLoginController : ControllerBase
    {
        private readonly NassadContext _context;
        private readonly JwtService _jwt;
       

        public AgencyLoginController(NassadContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost]
        // Example: AgencyLoginController
        [HttpPost]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] JsonElement data)
        {
            try
            {
                if (!data.TryGetProperty("tenat", out var tenatElem) ||
                    !data.TryGetProperty("password", out var passwordElem) ||
                    !data.TryGetProperty("userType", out var userTypeElem))
                {
                    return BadRequest(new { success = false, message = "Username, password or userType missing" });
                }

                string tenat = tenatElem.GetString()!;
                string password = passwordElem.GetString()!;
                string userType = userTypeElem.GetString()!;

                if (userType == "Customer")
                {
                    var customer = await _context.Customers
                        .FirstOrDefaultAsync(c => c.CustomerTenet == tenat && c.CustomerPassword == password);

                    if (customer != null)
                    {
                        var token = _jwt.GenerateToken(tenat, "Customer");
                        return Ok(new { success = true, message = $"Customer {tenat} login successful", token, tenat, role = "Customer" });
                    }
                    return Unauthorized(new { success = false, message = "Invalid Customer credentials" });
                }
                else if (userType == "Agency")
                {
                    var agency = await _context.Agencies
                        .FirstOrDefaultAsync(a => a.AgencyTenet == tenat && a.AgencyPassword == password);

                    if (agency != null)
                    {
                        var token = _jwt.GenerateToken(tenat, "Agency");
                        return Ok(new { success = true, message = $"Agency {tenat} login successful", token, tenat, role = "Agency" });
                    }
                    return Unauthorized(new { success = false, message = "Invalid Agency credentials" });
                }

                return BadRequest(new { success = false, message = "Invalid userType" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }
}
