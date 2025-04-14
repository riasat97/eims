using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EIMS.Server.Data;
using EIMS.Shared.Models;

namespace EIMS.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StorageLocationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<StorageLocationsController> _logger;

    public StorageLocationsController(
        ApplicationDbContext context,
        ILogger<StorageLocationsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/StorageLocations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StorageLocation>>> GetStorageLocations()
    {
        return await _context.StorageLocations.ToListAsync();
    }

    // GET: api/StorageLocations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<StorageLocation>> GetStorageLocation(int id)
    {
        var storageLocation = await _context.StorageLocations
            .Include(s => s.StoredParts)
            .Include(s => s.ChildLocations)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (storageLocation == null)
        {
            return NotFound();
        }

        return storageLocation;
    }

    // POST: api/StorageLocations
    [HttpPost]
    public async Task<ActionResult<StorageLocation>> CreateStorageLocation(StorageLocation location)
    {
        try
        {
            _logger.LogInformation("Creating new storage location: {LocationName}", location.Name);

            // Validate that the name is unique
            if (await _context.StorageLocations.AnyAsync(l => l.Name == location.Name))
            {
                return BadRequest($"A storage location with the name '{location.Name}' already exists.");
            }

            // Set creation dates
            location.Created = DateTime.UtcNow;
            location.LastModified = DateTime.UtcNow;
            
            // Initialize metadata if null
            location.Metadata ??= new Dictionary<string, string>();

            // Add to database
            _context.StorageLocations.Add(location);
            
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully created storage location with ID: {LocationId}", location.Id);
                return CreatedAtAction(nameof(GetStorageLocation), new { id = location.Id }, location);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error creating storage location: {LocationName}", location.Name);
                return StatusCode(500, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating storage location: {LocationName}", location.Name);
            return StatusCode(500, $"An error occurred while creating the storage location: {ex.Message}");
        }
    }

    // PUT: api/StorageLocations/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStorageLocation(int id, StorageLocation location)
    {
        if (id != location.Id)
        {
            return BadRequest();
        }

        // Update the LastModified timestamp
        location.LastModified = DateTime.UtcNow;

        _context.Entry(location).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StorageLocationExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // PUT: api/StorageLocations/5/assignPart/3
    [HttpPut("{locationId}/assignPart/{partId}")]
    public async Task<IActionResult> AssignPartToLocation(int locationId, int partId)
    {
        var location = await _context.StorageLocations.FindAsync(locationId);
        if (location == null)
        {
            return NotFound($"Storage location with ID {locationId} not found");
        }

        var part = await _context.Parts.FindAsync(partId);
        if (part == null)
        {
            return NotFound($"Part with ID {partId} not found");
        }

        // Update the part's storage location
        part.StorageLocationId = locationId;
        
        // Update the LastModified timestamp on the location
        location.LastModified = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
            return Ok();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error assigning part {PartId} to location {LocationId}", partId, locationId);
            return StatusCode(500, $"An error occurred while assigning the part: {ex.Message}");
        }
    }

    private bool StorageLocationExists(int id)
    {
        return _context.StorageLocations.Any(e => e.Id == id);
    }
} 