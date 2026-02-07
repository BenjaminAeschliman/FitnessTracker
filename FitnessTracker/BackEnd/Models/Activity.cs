using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.BackEnd.Models
{
    public class Activity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "";

        [Range(1, 1440)]
        public int DurationMinutes { get; set; }

        public DateTime Date { get; set; }

        public int? UserId { get; set; }
        public AppUser? User { get; set; }
    }
}
