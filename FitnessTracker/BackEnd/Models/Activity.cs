namespace FitnessTracker.BackEnd.Models
{
  public class Activity
  {
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public int DurationMinutes { get; set; }
    public DateTime Date { get; set; }

    public int? UserId { get; set; }
    public AppUser? User { get; set; }

    }
}
