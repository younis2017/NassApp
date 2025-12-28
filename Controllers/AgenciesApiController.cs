using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Models;

namespace Nass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgenciesApiController : ControllerBase
    {
        private readonly NassadContext _context;

        public AgenciesApiController(NassadContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var agencies = await _context.Agencies.ToListAsync();
            return Ok(agencies);
        }

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var agency = await _context.Agencies
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.AgencyId == id);

            if (agency == null)
                return NotFound();

            return Ok(agency);
        }

        // =========================
        // SEARCH BY NAME
        // =========================
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            var agencies = await _context.Agencies
                .Where(a => a.AgencyName.Contains(name))
                .ToListAsync();

            return Ok(agencies);
        }

        // =========================
        // POST (Create)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create(Agencies agency)
        {
            agency.AgencyJoinedDate = DateTime.UtcNow;
            agency.AgencyUid = Guid.NewGuid();

            _context.Agencies.Add(agency);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = agency.AgencyId },
                agency);
        }

        // =========================
        // PUT (Update)
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Agencies agency)
        {
            if (id != agency.AgencyId)
                return BadRequest("Agency ID mismatch");

            var existingAgency = await _context.Agencies.FindAsync(id);
            if (existingAgency == null)
                return NotFound();

            existingAgency.AgencyName = agency.AgencyName;
            existingAgency.AgencyPhone = agency.AgencyPhone;
            existingAgency.AgencyEmail = agency.AgencyEmail;
            existingAgency.AgencyWebsite = agency.AgencyWebsite;
            existingAgency.AgencyAddress = agency.AgencyAddress;
            existingAgency.AgencyLocation = agency.AgencyLocation;
            existingAgency.AgencyTaxId = agency.AgencyTaxId;
            existingAgency.AgencyTenet = agency.AgencyTenet;
            existingAgency.AgencyLogo = agency.AgencyLogo;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // DELETE (Hard Delete)
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var agency = await _context.Agencies.FindAsync(id);
            if (agency == null)
                return NotFound();

            _context.Agencies.Remove(agency);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // LOGIN (Basic – Demo Only)
        // =========================
        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var agency = await _context.Agencies
                .FirstOrDefaultAsync(a =>
                    a.AgencyUsername == username &&
                    a.AgencyPassword == password);

            if (agency == null)
                return Unauthorized("Invalid credentials");

            return Ok(new
            {
                agency.AgencyId,
                agency.AgencyName,
                agency.AgencyEmail,
                agency.AgencyUid
            });
        }

        // agency dashboard
        [HttpGet("agency")]
        public async Task<IActionResult> GetAgencyDashboard([FromQuery] string tenant)
        {
            if (string.IsNullOrEmpty(tenant))
                return BadRequest("Tenant is required");

            var today = DateTime.Today;

            var query = _context.Transactions
                .Where(t => t.Agency_tenat == tenant);

            var totalOrders = await query.CountAsync();

            var newOrdersToday = await query
                .Where(t => t.Trans_date >= today)
                .CountAsync();

            var pendingOrders = await query
                .Where(t => t.TransStatus == 0)
                .CountAsync();

            var confirmedOrders = await query
                .Where(t => t.TransStatus == 1)
                .CountAsync();

            return Ok(new
            {
                totalOrders,
                newOrdersToday,
                pendingOrders,
                confirmedOrders
            });
        }
    }
}
