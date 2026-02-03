using FitnessTracker.DTOs;
using FitnessTracker.Models;

namespace FitnessTracker.Services
{
  public interface IFitnessService
  {
    string GetStatus();
    List<Activity> GetActivities();
    Activity AddActivity(CreateActivityRequest request);
  }
}
