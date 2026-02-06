namespace FitnessTracker.BackEnd.DTOs
{
    public record FitnessStatsDto(
        int TotalMinutes,
        int ActivityCount,
        double AverageDurationMinutes,
        Dictionary<string, int> MinutesByType,
        DateTime? StartDate,
        DateTime? EndDate
    );
}
