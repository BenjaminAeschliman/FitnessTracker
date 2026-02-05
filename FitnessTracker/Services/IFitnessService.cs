using FitnessTracker.DTOs;
using FitnessTracker.Models;

namespace FitnessTracker.Services
{
  public interface IFitnessService
  {
    string GetStatus(); //requires GetStatus method to be in class implementing this interface
    List<Activity> GetActivities(); //provides all activities
    Activity AddActivity(CreateActivityRequest request); //adds a new activity based on the request
    }
}
