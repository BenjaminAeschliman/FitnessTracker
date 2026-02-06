using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;

namespace FitnessTracker.BackEnd.Services
{
    public interface IFitnessService
    {
        string GetStatus();

        List<Activity> GetActivities(int userId, string? type, DateTime? from, DateTime? to);
        Activity? GetActivityById(int userId, int id);
        Activity AddActivity(int userId, CreateActivityRequest request);
        bool UpdateActivity(int userId, int id, CreateActivityRequest request);
        bool DeleteActivity(int userId, int id);
        Task<FitnessStatsDto> GetStatsAsync(string userId, DateTime? startDate, DateTime? endDate);
    }
}
