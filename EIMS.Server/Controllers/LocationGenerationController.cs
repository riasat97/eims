using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EIMS.Server.Services;
using EIMS.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EIMS.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationGenerationController : ControllerBase
    {
        private readonly ILocationGenerationService _locationGenerationService;
        private readonly ILogger<LocationGenerationController> _logger;

        public LocationGenerationController(
            ILocationGenerationService locationGenerationService,
            ILogger<LocationGenerationController> logger)
        {
            _locationGenerationService = locationGenerationService;
            _logger = logger;
        }

        [HttpPost("preview")]
        public async Task<ActionResult<LocationGenerationPreview>> Preview(LocationGenerationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var preview = await _locationGenerationService.GeneratePreviewAsync(request);
                return Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating location preview");
                return StatusCode(500, "An error occurred while generating the location preview");
            }
        }

        [HttpPost("generate")]
        public async Task<ActionResult> Generate(LocationGenerationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _locationGenerationService.GenerateLocationsAsync(request);
                
                if (result)
                {
                    return Ok(new { success = true, message = "Locations generated successfully" });
                }
                else
                {
                    return StatusCode(500, new { success = false, message = "Failed to generate locations" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating locations");
                return StatusCode(500, "An error occurred while generating locations");
            }
        }
    }
} 