using FitnessTracker.BackEnd.Data;
using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.BackEnd.Services
{
    public class FitnessService : IFitnessService
    {
        private readonly FitnessDbContext _db;

        public FitnessService(FitnessDbContext db)
        {
            _db = db;
        }

        public string GetStatus() => "Fitness service is working";

        public async Task<List<Activity>> GetActivitiesAsync(int userId, string? type, DateTime? from, DateTime? to, CancellationToken ct)
        {
            IQueryable<Activity> query = _db.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(a => a.Type == type);

            if (from.HasValue)
                query = query.Where(a => a.Date >= from.Value);

            if (to.HasValue)
            {
                var inclusiveEnd = to.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.Date <= inclusiveEnd);
            }

            return await query
                .OrderByDescending(a => a.Date)
                .ToListAsync(ct);
        }

        public async Task<Activity?> GetActivityByIdAsync(int userId, int id, CancellationToken ct)
        {
            return await _db.Activities
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);
        }

        public async Task<Activity> AddActivityAsync(int userId, CreateActivityRequest request, CancellationToken ct)
        {
            var activity = new Activity
            {
                UserId = userId,
                Type = request.Type,
                DurationMinutes = request.DurationMinutes,
                Date = request.Date
            };

            await _db.Activities.AddAsync(activity, ct);
            await _db.SaveChangesAsync(ct);

            return activity;
        }

        public async Task<bool> UpdateActivityAsync(int userId, int id, CreateActivityRequest request, CancellationToken ct)
        {
            var activity = await _db.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

            if (activity == null)
                return false;

            activity.Type = request.Type;
            activity.DurationMinutes = request.DurationMinutes;
            activity.Date = request.Date;

            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<bool> DeleteActivityAsync(int userId, int id, CancellationToken ct)
        {
            var activity = await _db.Activities
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);

            if (activity == null)
                return false;

            _db.Activities.Remove(activity);
            await _db.SaveChangesAsync(ct);
            return true;
        }

        public async Task<FitnessStatsDto> GetStatsAsync(int userId, DateTime? startDate, DateTime? endDate, CancellationToken ct)
        {
            IQueryable<Activity> query = _db.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);

            if (endDate.HasValue)
            {
                var inclusiveEnd = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.Date <= inclusiveEnd);
            }

            var items = await query
                .Select(a => new { a.Type, a.DurationMinutes })
                .ToListAsync(ct);

            int totalMinutes = items.Sum(x => x.DurationMinutes);
            int count = items.Count;
            double avg = count == 0 ? 0 : Math.Round((double)totalMinutes / count, 2);

            var minutesByType = items
                .GroupBy(x => string.IsNullOrWhiteSpace(x.Type) ? "Unknown" : x.Type!)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.DurationMinutes));

            return new FitnessStatsDto(
                totalMinutes,
                count,
                avg,
                minutesByType,
                startDate,
                endDate
            );
        }

        public async Task<List<string>> GetActivityTypesAsync(int userId, CancellationToken ct)
        {
            return await _db.Activities
                .AsNoTracking()
                .Where(a => a.UserId == userId && !string.IsNullOrWhiteSpace(a.Type))
                .Select(a => a.Type!)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync(ct);
        }
    }
}
