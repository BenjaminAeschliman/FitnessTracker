using FitnessTracker.DTOs;
using FitnessTracker.Models;


// Next step: replace with database + EF Core. Update service only


namespace FitnessTracker.Services
{
    public class FitnessService : IFitnessService
    {
        private static readonly List<Activity> _activities = new();
        private static int _nextId = 1;

        public string GetStatus()
        {
            return "Fitness service is working";
        }

        public List<Activity> GetActivities()
        {
            return _activities;
        }

        public Activity AddActivity(CreateActivityRequest request)
        {
            var activity = new Activity
            {
                Id = _nextId++,
                Type = request.Type,
                DurationMinutes = request.DurationMinutes,
                Date = request.Date
            };

            _activities.Add(activity);
            return activity;
        }
    }
}
