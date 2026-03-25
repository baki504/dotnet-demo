namespace HrAttendance.Domain.Entities;

public class AttendanceRecord
{
    private readonly List<TimeStampEntry> _timeStampEntries = [];

    public Guid Id { get; private set; }
    public Guid EmployeeId { get; private set; }
    public DateOnly Date { get; private set; }
    public Guid WorkTypeId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<TimeStampEntry> TimeStampEntries => _timeStampEntries;

    private AttendanceRecord() { }

    public static AttendanceRecord Create(Guid employeeId, DateOnly date, Guid workTypeId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(employeeId));

        if (workTypeId == Guid.Empty)
            throw new ArgumentException("WorkTypeId cannot be empty.", nameof(workTypeId));

        var now = DateTime.Now;
        return new AttendanceRecord
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            Date = date,
            WorkTypeId = workTypeId,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void AddTimeStamp(TimeStampType type, DateTime timestamp)
    {
        if (type == TimeStampType.ClockIn)
        {
            if (_timeStampEntries.Any(e => e.Type == TimeStampType.ClockIn))
                throw new InvalidOperationException("ClockIn has already been recorded.");
        }
        else if (type == TimeStampType.ClockOut)
        {
            var clockIn = _timeStampEntries.FirstOrDefault(e => e.Type == TimeStampType.ClockIn);

            if (clockIn is null)
                throw new InvalidOperationException("Cannot record ClockOut without ClockIn.");

            if (_timeStampEntries.Any(e => e.Type == TimeStampType.ClockOut))
                throw new InvalidOperationException("ClockOut has already been recorded.");

            if (timestamp <= clockIn.Timestamp)
                throw new InvalidOperationException("ClockOut must be after ClockIn.");
        }

        _timeStampEntries.Add(TimeStampEntry.Create(Id, type, timestamp));
        UpdatedAt = DateTime.Now;
    }

    public void ChangeWorkType(Guid workTypeId)
    {
        if (workTypeId == Guid.Empty)
            throw new ArgumentException("WorkTypeId cannot be empty.", nameof(workTypeId));

        WorkTypeId = workTypeId;
        UpdatedAt = DateTime.Now;
    }
}
