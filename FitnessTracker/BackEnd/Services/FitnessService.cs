using FitnessTracker.BackEnd.Data;
using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.BackEnd.Services
{
    public class FitnessService : IFitnessService
    {
        private readonly FitnessDbContext _db; // <-- change if your DbContext is named differently

        public FitnessService(FitnessDbContext db) // <-- change if your DbContext is named differently
        {
            _db = db;
        }

        public string GetStatus()
        {
            return "Fitness service is working";
        }

        public List<Activity> GetActivities(int userId, string? type, DateTime? from, DateTime? to)
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

            return query
                .OrderByDescending(a => a.Date)
                .ToList();
        }

        public Activity? GetActivityById(int userId, int id)
        {
            return _db.Activities
                .AsNoTracking()
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);
        }

        public Activity AddActivity(int userId, CreateActivityRequest request)
        {
            var activity = new Activity
            {
                UserId = userId,
                Type = request.Type,
                DurationMinutes = request.DurationMinutes,
                Date = request.Date
            };

            _db.Activities.Add(activity);
            _db.SaveChanges();

            return activity;
        }

        public bool UpdateActivity(int userId, int id, CreateActivityRequest request)
        {
            var activity = _db.Activities
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (activity == null)
                return false;

            activity.Type = request.Type;
            activity.DurationMinutes = request.DurationMinutes;
            activity.Date = request.Date;

            _db.SaveChanges();
            return true;
        }

        public bool DeleteActivity(int userId, int id)
        {
            var activity = _db.Activities
                .FirstOrDefault(a => a.Id == id && a.UserId == userId);

            if (activity == null)
                return false;

            _db.Activities.Remove(activity);
            _db.SaveChanges();
            return true;
        }

        public async Task<FitnessStatsDto> GetStatsAsync(string userId, DateTime? startDate, DateTime? endDate)
        {
            if (!int.TryParse(userId, out int parsedUserId))
                throw new ArgumentException("Invalid user id.");

            IQueryable<Activity> query = _db.Activities
                .AsNoTracking()
                .Where(a => a.UserId == parsedUserId);

            if (startDate.HasValue)
                query = query.Where(a => a.Date >= startDate.Value);

            if (endDate.HasValue)
            {
                var inclusiveEnd = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.Date <= inclusiveEnd);
            }

            var items = await query
                .Select(a => new { a.Type, a.DurationMinutes })
                .ToListAsync();

            int totalMinutes = items.Sum(x => x.DurationMinutes);
            int count = items.Count;
            double avg = count == 0 ? 0 : Math.Round((double)totalMinutes / count, 2);

            var minutesByType = items
                .GroupBy(x => x.Type ?? "Unknown")
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.DurationMinutes)
                );

            return new FitnessStatsDto(
                totalMinutes,
                count,
                avg,
                minutesByType,
                startDate,
                endDate
            );
        }
    }
}
