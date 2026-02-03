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
      var created = _fitnessService.AddActivity(request);
      return Ok(created);
    }
  }
}
