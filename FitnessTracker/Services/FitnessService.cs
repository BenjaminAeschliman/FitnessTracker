using FitnessTracker.Data;
using FitnessTracker.DTOs;
using FitnessTracker.Models;
using System.Linq;

namespace FitnessTracker.Services
{
    public class FitnessService : IFitnessService
    {
        private readonly FitnessDbContext _db;

        public FitnessService(FitnessDbContext db)
        {
            _db = db;
        }

        public string GetStatus()
        {
            return "Fitness service is working (EF Core)";
        }

        public List<Activity> GetActivities(string? type = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _db.Activities.AsQueryable();

            if (!string.IsNullOrWhiteSpace(type))
            {
                var trimmed = type.Trim();
                query = query.Where(a => a.Type == trimmed);
            }

            if (from.HasValue)
                query = query.Where(a => a.Date >= from.Value);

            if (to.HasValue)
                query = query.Where(a => a.Date <= to.Value);

            return query
                .OrderByDescending(a => a.Date)
                .ToList();
        }

        public Activity? GetActivityById(int id)
        {
            return _db.Activities.Find(id);
        }

        public Activity AddActivity(CreateActivityRequest request)
        {
            var activity = new Activity
            {
                Type = request.Type,
                DurationMinutes = request.DurationMinutes,
                Date = request.Date
            };

            _db.Activities.Add(activity);
            _db.SaveChanges();

            return activity;
        }

        public bool UpdateActivity(int id, CreateActivityRequest request)
        {
            var activity = _db.Activities.Find(id);
            if (activity == null)
                return false;

            activity.Type = request.Type;
            activity.DurationMinutes = request.DurationMinutes;
            activity.Date = request.Date;

            _db.SaveChanges();
            return true;
        }

        public bool DeleteActivity(int id)
        {
            var activity = _db.Activities.Find(id);
            if (activity == null)
                return false;

            _db.Activities.Remove(activity);
            _db.SaveChanges();
            return true;
        }
    }
}
