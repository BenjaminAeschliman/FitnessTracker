using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;
using FitnessTracker.BackEnd.Services;

namespace FitnessTracker.BackEnd.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FitnessController : ControllerBase
    {
        private readonly IFitnessService _fitnessService;

        public FitnessController(IFitnessService fitnessService)
        {
            _fitnessService = fitnessService;
        }

        // Returns false instead of throwing (prevents random 500s)
        private bool TryGetUserId(out int userId)
        {
            userId = 0;
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub"); // sometimes used in JWT

            return int.TryParse(idValue, out userId);
        }

        private string? GetUserIdString()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok(_fitnessService.GetStatus());
        }

        [HttpGet("activities")]
        public IActionResult GetActivities(
            [FromQuery] string? type,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            var activities = _fitnessService.GetActivities(userId, type, from, to);
            return Ok(activities.Select(ToResponse));
        }

        [HttpGet("activities/{id}")]
        public IActionResult GetActivityById(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            var activity = _fitnessService.GetActivityById(userId, id);

            if (activity == null)
                return NotFound(new { error = $"Activity with ID {id} not found." });

            return Ok(ToResponse(activity));
        }

        [HttpPost("activities")]
        public IActionResult AddActivity([FromBody] CreateActivityRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest(new { error = "Activity type is required." });

            if (request.DurationMinutes <= 0)
                return BadRequest(new { error = "DurationMinutes must be greater than 0." });

            var activity = _fitnessService.AddActivity(userId, request);

            return CreatedAtAction(
                nameof(GetActivityById),
                new { id = activity.Id },
                ToResponse(activity)
            );
        }

        [HttpPut("activities/{id}")]
        public IActionResult UpdateActivity(int id, [FromBody] CreateActivityRequest request)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest(new { error = "Activity type is required." });

            if (request.DurationMinutes <= 0)
                return BadRequest(new { error = "DurationMinutes must be greater than 0." });

            var updated = _fitnessService.UpdateActivity(userId, id, request);

            if (!updated)
                return NotFound(new { error = $"Activity with ID {id} not found." });

            return NoContent();
        }

        [HttpDelete("activities/{id}")]
        public IActionResult DeleteActivity(int id)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            var deleted = _fitnessService.DeleteActivity(userId, id);

            if (!deleted)
                return NotFound(new { error = $"Activity with ID {id} not found." });

            return NoContent();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            var userId = GetUserIdString();

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { error = "Missing user identity." });

            if (startDate.HasValue && endDate.HasValue && endDate.Value.Date < startDate.Value.Date)
                return BadRequest(new { error = "endDate must be on/after startDate." });

            var stats = await _fitnessService.GetStatsAsync(userId, startDate, endDate);
            return Ok(stats);
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
