using Microsoft.AspNetCore.Mvc;
using FitnessTracker.Services;
using FitnessTracker.DTOs;
using FitnessTracker.Models;

namespace FitnessTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FitnessController : ControllerBase
    {
        private readonly IFitnessService _fitnessService;

        public FitnessController(IFitnessService fitnessService)
        {
            _fitnessService = fitnessService;
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(_fitnessService.GetStatus());
        }

        // GET /api/fitness/activities?type=Run&from=2026-02-01&to=2026-02-28
        [HttpGet("activities")]
        public IActionResult GetActivities([FromQuery] string? type, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var activities = _fitnessService.GetActivities(type, from, to);

            var response = activities.Select(ToResponse).ToList();
            return Ok(response);
        }

        [HttpGet("activities/{id}")]
        public IActionResult GetActivityById(int id)
        {
            var activity = _fitnessService.GetActivityById(id);
            if (activity == null)
                return NotFound($"Activity with ID {id} not found.");

            return Ok(ToResponse(activity));
        }

        [HttpPost("activities")]
        public IActionResult AddActivity([FromBody] CreateActivityRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest("Activity type is required.");
            if (request.DurationMinutes <= 0)
                return BadRequest("DurationMinutes must be greater than 0");

            var activity = _fitnessService.AddActivity(request);

            return CreatedAtAction(nameof(GetActivityById), new { id = activity.Id }, ToResponse(activity));
        }

        [HttpPut("activities/{id}")]
        public IActionResult UpdateActivity(int id, [FromBody] CreateActivityRequest request)
        {
            if (request == null)
                return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest("Activity type is required.");
            if (request.DurationMinutes <= 0)
                return BadRequest("DurationMinutes must be greater than 0");

            var updated = _fitnessService.UpdateActivity(id, request);
            if (!updated)
                return NotFound($"Activity with ID {id} not found.");

            return NoContent();
        }

        [HttpDelete("activities/{id}")]
        public IActionResult DeleteActivity(int id)
        {
            var deleted = _fitnessService.DeleteActivity(id);
            if (!deleted)
                return NotFound($"Activity with ID {id} not found.");

            return NoContent();
        }

        private static ActivityResponse ToResponse(Activity a)
        {
            return new ActivityResponse
            {
                Id = a.Id,
                Type = a.Type,
                DurationMinutes = a.DurationMinutes,
                Date = a.Date
            };
        }
    }
}
