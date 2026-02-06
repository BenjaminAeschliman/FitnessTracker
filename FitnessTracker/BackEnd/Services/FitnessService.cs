using FitnessTracker.BackEnd.Data;
using FitnessTracker.BackEnd.DTOs;
using FitnessTracker.BackEnd.Models;

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

        public List<Activity> GetActivities(int userId, string? type, DateTime? from, DateTime? to)
        {
            IQueryable<Activity> query = _db.Activities.Where(a => a.UserId == userId);

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(a => a.Type == type);

            if (from.HasValue)
                query = query.Where(a => a.Date >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.Date <= to.Value);

            return query
                .OrderByDescending(a => a.Date)
                .ToList();
        }

        public Activity? GetActivityById(int userId, int id)
        {
            return _db.Activities.FirstOrDefault(a => a.Id == id && a.UserId == userId);
        }

        public Activity AddActivity(int userId, CreateActivityRequest request)
        {
            var activity = new Activity
            {
                Type = request.Type,
                DurationMinutes = request.DurationMinutes,
                Date = request.Date,
                UserId = userId
            };

            _db.Activities.Add(activity);
            _db.SaveChanges();

            return activity;
        }

        public bool UpdateActivity(int userId, int id, CreateActivityRequest request)
        {
            var activity = _db.Activities.FirstOrDefault(a => a.Id == id && a.UserId == userId);
            if (activity == null) return false;

            activity.Type = request.Type;
            activity.DurationMinutes = request.DurationMinutes;
            activity.Date = request.Date;

            _db.SaveChanges();
            return true;
        }

        public bool DeleteActivity(int userId, int id)
        {
            var activity = _db.Activities.FirstOrDefault(a => a.Id == id && a.UserId == userId);
            if (activity == null) return false;

            _db.Activities.Remove(activity);
            _db.SaveChanges();
            return true;
        }
    }
}
