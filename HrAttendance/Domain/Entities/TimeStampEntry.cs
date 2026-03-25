namespace HrAttendance.Domain.Entities;

public class TimeStampEntry
{
    public Guid Id { get; private set; }
    public Guid AttendanceRecordId { get; private set; }
    public TimeStampType Type { get; private set; }
    public DateTime Timestamp { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TimeStampEntry() { }

    internal static TimeStampEntry Create(Guid attendanceRecordId, TimeStampType type, DateTime timestamp)
    {
        return new TimeStampEntry
        {
            Id = Guid.NewGuid(),
            AttendanceRecordId = attendanceRecordId,
            Type = type,
            Timestamp = timestamp,
            CreatedAt = DateTime.Now,
        };
    }
}
