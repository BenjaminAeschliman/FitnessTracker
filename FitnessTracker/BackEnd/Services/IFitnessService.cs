using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;

namespace FitnessTracker.BackEnd.Services
{
    public interface IFitnessService
    {
        string GetStatus();

        Task<List<Activity>> GetActivitiesAsync(int userId, string? type, DateTime? from, DateTime? to, CancellationToken ct);
        Task<Activity?> GetActivityByIdAsync(int userId, int id, CancellationToken ct);

        Task<Activity> AddActivityAsync(int userId, CreateActivityRequest request, CancellationToken ct);
        Task<bool> UpdateActivityAsync(int userId, int id, CreateActivityRequest request, CancellationToken ct);
        Task<bool> DeleteActivityAsync(int userId, int id, CancellationToken ct);

        Task<FitnessStatsDto> GetStatsAsync(int userId, DateTime? startDate, DateTime? endDate, CancellationToken ct);
        Task<List<string>> GetActivityTypesAsync(int userId, CancellationToken ct);
    }
}
