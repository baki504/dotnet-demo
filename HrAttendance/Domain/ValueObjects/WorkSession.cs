namespace HrAttendance.Domain.ValueObjects;

public sealed class WorkSession : IEquatable<WorkSession>
{
    public DateTime ClockIn { get; }
    public DateTime? ClockOut { get; }

    private WorkSession(DateTime clockIn, DateTime? clockOut)
    {
        ClockIn = clockIn;
        ClockOut = clockOut;
    }

    public static WorkSession Of(DateTime clockIn, DateTime? clockOut)
    {
        if (clockOut.HasValue && clockOut.Value <= clockIn)
            throw new ArgumentException("ClockOut must be after ClockIn.", nameof(clockOut));

        return new WorkSession(clockIn, clockOut);
    }

    public WorkDuration Duration
    {
        get
        {
            if (!ClockOut.HasValue)
                return WorkDuration.FromMinutes(0);

            var minutes = (int)(ClockOut.Value - ClockIn).TotalMinutes;
            return WorkDuration.FromMinutes(minutes);
        }
    }

    public bool Equals(WorkSession? other)
    {
        if (other is null) return false;
        return ClockIn == other.ClockIn && ClockOut == other.ClockOut;
    }

    public override bool Equals(object? obj) => Equals(obj as WorkSession);

    public override int GetHashCode() => HashCode.Combine(ClockIn, ClockOut);
}
