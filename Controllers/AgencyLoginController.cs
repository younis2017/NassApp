using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Helpers;
using Nass.Models;
using System;
using System.Threading.Tasks;

namespace Nass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly NassadContext _context;
        private readonly JwtService _jwt;

        public LoginController(NassadContext context, JwtService jwt)
        {
            _context = context;
            _jwt = jwt;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Tenat) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.UserType))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Tenat, password or userType missing"
                });
            }

            if (dto.UserType == "Agency")
            {
                var agency = await _context.Agencies
                    .FirstOrDefaultAsync(a =>
                        a.AgencyTenet == dto.Tenat &&
                        a.AgencyPassword == dto.Password);

                if (agency == null)
                    return Unauthorized(new { success = false, message = "Invalid agency credentials" });

                var token = _jwt.GenerateToken(agency.AgencyUid.ToString(), "Agency");

                return Ok(new
                {
                    success = true,
                    token,
                    role = "Agency",
                    tenat = agency.AgencyTenet,
                    message = "Agency login successful"
                });
            }

            if (dto.UserType == "Customer")
            {
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c =>
                        c.CustomerTenet == dto.Tenat &&
                        c.CustomerPassword == dto.Password);

                if (customer == null)
                    return Unauthorized(new { success = false, message = "Invalid customer credentials" });

                var token = _jwt.GenerateToken(customer.CustomerUid.ToString(), "Customer");

                return Ok(new
                {
                    success = true,
                    token,
                    role = "Customer",
                    tenat = customer.CustomerTenet,
                    message = "Customer login successful"
                });
            }

            return BadRequest(new
            {
                success = false,
                message = "Invalid userType"
            });
        }
    }
    public class LoginRequestDto
    {
        public string Tenat { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty; // Agency | Customer
    }

}
