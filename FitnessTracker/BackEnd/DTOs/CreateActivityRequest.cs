using System.ComponentModel.DataAnnotations;

namespace FitnessTracker.BackEnd.DTOs
{
    public class CreateActivityRequest
    {
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = "";

        [Range(1, 1440, ErrorMessage = "DurationMinutes must be between 1 and 1440.")]
        public int DurationMinutes { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
