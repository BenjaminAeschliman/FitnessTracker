using System.Security.Claims;
using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;
using FitnessTracker.BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        private bool TryGetUserId(out int userId)
        {
            userId = 0;

            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                         ?? User.FindFirstValue("sub");

            return int.TryParse(idValue, out userId);
        }

        [AllowAnonymous]
        [HttpGet("status")]
        public IActionResult Status() => Ok(_fitnessService.GetStatus());

        [HttpGet("activities")]
        public async Task<IActionResult> GetActivities(
            [FromQuery] string? type,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken ct)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            var activities = await _fitnessService.GetActivitiesAsync(userId, type, from, to, ct);
            return Ok(activities.Select(ToResponse));
        }

        [HttpGet("activities/{id:int}")]
        public async Task<IActionResult> GetActivityById(int id, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            var activity = await _fitnessService.GetActivityByIdAsync(userId, id, ct);
            return activity == null
                ? NotFound(new { error = $"Activity with ID {id} not found." })
                : Ok(ToResponse(activity));
        }

        [HttpPost("activities")]
        public async Task<IActionResult> AddActivity([FromBody] CreateActivityRequest request, CancellationToken ct)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest(new { error = "Activity type is required." });

            if (request.DurationMinutes <= 0)
                return BadRequest(new { error = "DurationMinutes must be greater than 0." });

            var activity = await _fitnessService.AddActivityAsync(userId, request, ct);

            return CreatedAtAction(
                nameof(GetActivityById),
                new { id = activity.Id },
                ToResponse(activity)
            );
        }

        [HttpPut("activities/{id:int}")]
        public async Task<IActionResult> UpdateActivity(int id, [FromBody] CreateActivityRequest request, CancellationToken ct)
        {
            if (request == null)
                return BadRequest(new { error = "Request body is required." });

            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest(new { error = "Activity type is required." });

            if (request.DurationMinutes <= 0)
                return BadRequest(new { error = "DurationMinutes must be greater than 0." });

            var updated = await _fitnessService.UpdateActivityAsync(userId, id, request, ct);
            return updated
                ? NoContent()
                : NotFound(new { error = $"Activity with ID {id} not found." });
        }

        [HttpDelete("activities/{id:int}")]
        public async Task<IActionResult> DeleteActivity(int id, CancellationToken ct)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            var deleted = await _fitnessService.DeleteActivityAsync(userId, id, ct);
            return deleted
                ? NoContent()
                : NotFound(new { error = $"Activity with ID {id} not found." });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery(Name = "from")] DateTime? from,
            [FromQuery(Name = "to")] DateTime? to,
            CancellationToken ct)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            var effectiveStart = startDate ?? from;
            var effectiveEnd = endDate ?? to;

            if (effectiveStart.HasValue && effectiveEnd.HasValue && effectiveEnd.Value.Date < effectiveStart.Value.Date)
                return BadRequest(new { error = "end date must be on/after start date." });

            var stats = await _fitnessService.GetStatsAsync(userId, effectiveStart, effectiveEnd, ct);
            return Ok(stats);
        }

        [HttpGet("activity-types")]
        public async Task<IActionResult> GetActivityTypes(CancellationToken ct)
        {
            if (!TryGetUserId(out var userId))
                return Unauthorized(new { error = "Missing or invalid user identity." });

            var types = await _fitnessService.GetActivityTypesAsync(userId, ct);
            return Ok(types);
        }

        private static ActivityResponse ToResponse(Activity a) => new()
        {
            Id = a.Id,
            Type = a.Type,
            DurationMinutes = a.DurationMinutes,
            Date = a.Date
        };
    }
}
