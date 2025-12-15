using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nass.Data;
using Nass.Models;

namespace Nass.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesApiController : ControllerBase
    {
        private readonly NassadContext _context;

        public CategoriesApiController(NassadContext context)
        {
            _context = context;
        }

        // =========================
        // GET ALL (Active only)
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();

            return Ok(categories);
        }

        // =========================
        // GET BY ID
        // =========================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id && c.IsActive);

            if (category == null)
                return NotFound();

            return Ok(category);
        }

        // =========================
        // POST (Create)
        // =========================
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            category.IsActive = true;
            category.CreatedDate = DateTime.UtcNow;

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.CategoryId },
                category
            );
        }

        // =========================
        // PUT (Update)
        // =========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Category category)
        {
            if (id != category.CategoryId)
                return BadRequest("Category ID mismatch");

            var existingCategory = await _context.Categories.FindAsync(id);
            if (existingCategory == null || !existingCategory.IsActive)
                return NotFound();

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.IsActive = category.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // =========================
        // DELETE (Soft Delete)
        // =========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> SoftDelete(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null || !category.IsActive)
                return NotFound();

            category.IsActive = false;

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
