namespace HrAttendance.Domain.ValueObjects;

public sealed class WorkDuration : IEquatable<WorkDuration>
{
    public int TotalMinutes { get; }

    private WorkDuration(int totalMinutes)
    {
        TotalMinutes = totalMinutes;
    }

    public static WorkDuration FromMinutes(int minutes)
    {
        if (minutes < 0)
            throw new ArgumentException("Minutes cannot be negative.", nameof(minutes));

        return new WorkDuration(minutes);
    }

    public WorkDuration Add(WorkDuration other)
    {
        return new WorkDuration(TotalMinutes + other.TotalMinutes);
    }

    public WorkDuration Subtract(WorkDuration other)
    {
        if (other.TotalMinutes > TotalMinutes)
            throw new InvalidOperationException("Cannot subtract a larger duration from a smaller one.");

        return new WorkDuration(TotalMinutes - other.TotalMinutes);
    }

    public bool IsGreaterThan(WorkDuration other)
    {
        return TotalMinutes > other.TotalMinutes;
    }

    public (int Hours, int Minutes) ToHoursAndMinutes()
    {
        return (TotalMinutes / 60, TotalMinutes % 60);
    }

    public bool Equals(WorkDuration? other)
    {
        if (other is null) return false;
        return TotalMinutes == other.TotalMinutes;
    }

    public override bool Equals(object? obj) => Equals(obj as WorkDuration);

    public override int GetHashCode() => TotalMinutes.GetHashCode();
}
