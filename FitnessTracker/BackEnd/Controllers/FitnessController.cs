using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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

        private int GetUserId()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(idValue!);
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
            var userId = GetUserId();
            var activities = _fitnessService.GetActivities(userId, type, from, to);

            return Ok(activities.Select(ToResponse));
        }

        [HttpGet("activities/{id}")]
        public IActionResult GetActivityById(int id)
        {
            var userId = GetUserId();
            var activity = _fitnessService.GetActivityById(userId, id);

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

            var userId = GetUserId();
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
                return BadRequest("Request body is required.");
            if (string.IsNullOrWhiteSpace(request.Type))
                return BadRequest("Activity type is required.");
            if (request.DurationMinutes <= 0)
                return BadRequest("DurationMinutes must be greater than 0");

            var userId = GetUserId();
            var updated = _fitnessService.UpdateActivity(userId, id, request);

            if (!updated)
                return NotFound($"Activity with ID {id} not found.");

            return NoContent();
        }

        [HttpDelete("activities/{id}")]
        public IActionResult DeleteActivity(int id)
        {
            var userId = GetUserId();
            var deleted = _fitnessService.DeleteActivity(userId, id);

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
