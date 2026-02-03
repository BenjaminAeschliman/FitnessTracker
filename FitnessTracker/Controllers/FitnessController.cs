using Microsoft.AspNetCore.Mvc;
using FitnessTracker.Services;

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
    [HttpGet]
    public IActionResult Get()
    {
      return Ok(_fitnessService.GetStatus());
    }
  }
}
