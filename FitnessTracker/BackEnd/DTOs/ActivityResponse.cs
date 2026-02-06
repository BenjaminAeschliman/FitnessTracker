namespace FitnessTracker.BackEnd.DTOs
{
    public class ActivityResponse
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime Date { get; set; }
    }
}
