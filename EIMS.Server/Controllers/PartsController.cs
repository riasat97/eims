using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EIMS.Server.Data;
using EIMS.Shared.Models;

namespace EIMS.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PartsController> _logger;

    public PartsController(ApplicationDbContext context, ILogger<PartsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/Parts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Part>>> GetParts()
    {
        return await _context.Parts.ToListAsync();
    }

    // GET: api/Parts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Part>> GetPart(int id)
    {
        var part = await _context.Parts
            .Include(p => p.Documents)
            .Include(p => p.Substitutes)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (part == null)
        {
            return NotFound();
        }

        return part;
    }

    // POST: api/Parts
    [HttpPost]
    public async Task<ActionResult<Part>> CreatePart(Part part)
    {
        try
        {
            _logger.LogInformation("Creating new part: {PartName}", part.Name);

            // Validate required fields
            if (string.IsNullOrWhiteSpace(part.Name))
            {
                _logger.LogWarning("Part creation failed: Name is required");
                return BadRequest("Part name is required.");
            }

            // Initialize collections if null
            part.Dimensions ??= new Dictionary<string, string>();
            part.TechnicalSpecs ??= new Dictionary<string, string>();
            part.PhysicalSpecs ??= new Dictionary<string, string>();
            part.UsedInProjects ??= new List<string>();
            part.UsedInMetaParts ??= new List<string>();
            part.Documents ??= new List<Document>();
            part.CadKeys ??= new List<string>();
            part.Tags ??= new List<string>();

            // Set creation date
            part.Created = DateTime.UtcNow;

            // Add to database
            _context.Parts.Add(part);
            
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully created part with ID: {PartId}", part.Id);
                return CreatedAtAction(nameof(GetPart), new { id = part.Id }, part);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error creating part: {PartName}", part.Name);
                return StatusCode(500, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating part: {PartName}", part.Name);
            return StatusCode(500, $"An error occurred while creating the part: {ex.Message}");
        }
    }

    // PUT: api/Parts/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePart(int id, Part part)
    {
        if (id != part.Id)
        {
            return BadRequest();
        }

        _context.Entry(part).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!PartExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Parts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePart(int id)
    {
        var part = await _context.Parts.FindAsync(id);
        if (part == null)
        {
            return NotFound();
        }

        _context.Parts.Remove(part);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool PartExists(int id)
    {
        return _context.Parts.Any(e => e.Id == id);
    }
} 