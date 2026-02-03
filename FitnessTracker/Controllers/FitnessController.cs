using Microsoft.AspNetCore.Mvc;
using FitnessTracker.Services;
using FitnessTracker.DTOs;

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

    [HttpGet("activities")]
    public IActionResult GetActivities()
    {
      return Ok(_fitnessService.GetActivities());
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

      return CreatedAtAction(nameof(GetActivities), activity);
    }
  }
}
