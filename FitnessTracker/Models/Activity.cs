namespace FitnessTracker.Models
{
  public class Activity
  {
    public int Id { get; set; }
    public string Type { get; set; } = "";
    public int DurationMinutes { get; set; }
    public DateTime Date { get; set; }
  }
}
