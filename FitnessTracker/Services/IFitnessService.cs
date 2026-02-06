using FitnessTracker.DTOs;
using FitnessTracker.Models;

namespace FitnessTracker.Services
{
    public interface IFitnessService
    {
        string GetStatus();

        List<Activity> GetActivities(string? type = null, DateTime? from = null, DateTime? to = null);

        Activity? GetActivityById(int id);

        Activity AddActivity(CreateActivityRequest request);

        bool UpdateActivity(int id, CreateActivityRequest request);

        bool DeleteActivity(int id);
    }
}
