namespace FitnessTracker.DTOs
{
  public class CreateActivityRequest
  {
    public string Type { get; set; } = "";
    public int DurationMinutes { get; set; }
    public DateTime Date { get; set; }
  }
}
