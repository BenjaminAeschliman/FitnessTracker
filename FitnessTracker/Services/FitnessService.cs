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

        public List<Activity> GetActivities()
        {
            return _db.Activities
                      .OrderByDescending(a => a.Date)
                      .ToList();
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
    }
}
