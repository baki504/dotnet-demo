using HrAttendance.Domain;
using HrAttendance.Domain.Entities;

namespace HrAttendanceTests.Domain.Entities;

public class AttendanceRecordTests
{
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _workTypeId = Guid.NewGuid();

    private AttendanceRecord CreateDefaultRecord()
    {
        return AttendanceRecord.Create(
            _employeeId,
            new DateOnly(2026, 3, 24),
            _workTypeId);
    }

    // --- Create ---

    [Fact]
    public void Create_WithValidInput_ReturnsAttendanceRecord()
    {
        var date = new DateOnly(2026, 3, 24);

        var record = AttendanceRecord.Create(_employeeId, date, _workTypeId);

        Assert.NotEqual(Guid.Empty, record.Id);
        Assert.Equal(_employeeId, record.EmployeeId);
        Assert.Equal(date, record.Date);
        Assert.Equal(_workTypeId, record.WorkTypeId);
        Assert.Empty(record.TimeStampEntries);
    }

    [Fact]
    public void Create_SetsCreatedAtAndUpdatedAt()
    {
        var before = DateTime.Now;
        var record = CreateDefaultRecord();
        var after = DateTime.Now;

        Assert.InRange(record.CreatedAt, before, after);
        Assert.InRange(record.UpdatedAt, before, after);
    }

    [Fact]
    public void Create_WithEmptyEmployeeId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            AttendanceRecord.Create(Guid.Empty, new DateOnly(2026, 3, 24), _workTypeId));
    }

    [Fact]
    public void Create_WithEmptyWorkTypeId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            AttendanceRecord.Create(_employeeId, new DateOnly(2026, 3, 24), Guid.Empty));
    }

    // --- AddTimeStamp ---

    [Fact]
    public void AddTimeStamp_ClockIn_AddsEntry()
    {
        var record = CreateDefaultRecord();
        var timestamp = new DateTime(2026, 3, 24, 9, 0, 0);

        record.AddTimeStamp(TimeStampType.ClockIn, timestamp);

        Assert.Single(record.TimeStampEntries);
        var entry = record.TimeStampEntries[0];
        Assert.Equal(TimeStampType.ClockIn, entry.Type);
        Assert.Equal(timestamp, entry.Timestamp);
    }

    [Fact]
    public void AddTimeStamp_ClockOut_AfterClockIn_AddsEntry()
    {
        var record = CreateDefaultRecord();
        record.AddTimeStamp(TimeStampType.ClockIn, new DateTime(2026, 3, 24, 9, 0, 0));

        record.AddTimeStamp(TimeStampType.ClockOut, new DateTime(2026, 3, 24, 18, 0, 0));

        Assert.Equal(2, record.TimeStampEntries.Count);
        var clockOut = record.TimeStampEntries.First(e => e.Type == TimeStampType.ClockOut);
        Assert.Equal(new DateTime(2026, 3, 24, 18, 0, 0), clockOut.Timestamp);
    }

    [Fact]
    public void AddTimeStamp_DuplicateClockIn_ThrowsInvalidOperationException()
    {
        var record = CreateDefaultRecord();
        record.AddTimeStamp(TimeStampType.ClockIn, new DateTime(2026, 3, 24, 9, 0, 0));

        Assert.Throws<InvalidOperationException>(() =>
            record.AddTimeStamp(TimeStampType.ClockIn, new DateTime(2026, 3, 24, 10, 0, 0)));
    }

    [Fact]
    public void AddTimeStamp_DuplicateClockOut_ThrowsInvalidOperationException()
    {
        var record = CreateDefaultRecord();
        record.AddTimeStamp(TimeStampType.ClockIn, new DateTime(2026, 3, 24, 9, 0, 0));
        record.AddTimeStamp(TimeStampType.ClockOut, new DateTime(2026, 3, 24, 18, 0, 0));

        Assert.Throws<InvalidOperationException>(() =>
            record.AddTimeStamp(TimeStampType.ClockOut, new DateTime(2026, 3, 24, 19, 0, 0)));
    }

    [Fact]
    public void AddTimeStamp_ClockOut_WithoutClockIn_ThrowsInvalidOperationException()
    {
        var record = CreateDefaultRecord();

        Assert.Throws<InvalidOperationException>(() =>
            record.AddTimeStamp(TimeStampType.ClockOut, new DateTime(2026, 3, 24, 18, 0, 0)));
    }

    [Fact]
    public void AddTimeStamp_ClockOut_BeforeClockIn_ThrowsInvalidOperationException()
    {
        var record = CreateDefaultRecord();
        record.AddTimeStamp(TimeStampType.ClockIn, new DateTime(2026, 3, 24, 18, 0, 0));

        Assert.Throws<InvalidOperationException>(() =>
            record.AddTimeStamp(TimeStampType.ClockOut, new DateTime(2026, 3, 24, 9, 0, 0)));
    }

    [Fact]
    public void AddTimeStamp_ClockOut_EqualToClockIn_ThrowsInvalidOperationException()
    {
        var record = CreateDefaultRecord();
        var sameTime = new DateTime(2026, 3, 24, 9, 0, 0);
        record.AddTimeStamp(TimeStampType.ClockIn, sameTime);

        Assert.Throws<InvalidOperationException>(() =>
            record.AddTimeStamp(TimeStampType.ClockOut, sameTime));
    }

    [Fact]
    public void AddTimeStamp_UpdatesUpdatedAt()
    {
        var record = CreateDefaultRecord();
        var originalUpdatedAt = record.UpdatedAt;

        record.AddTimeStamp(TimeStampType.ClockIn, new DateTime(2026, 3, 24, 9, 0, 0));

        Assert.True(record.UpdatedAt >= originalUpdatedAt);
    }

    // --- ChangeWorkType ---

    [Fact]
    public void ChangeWorkType_ChangesWorkTypeId()
    {
        var record = CreateDefaultRecord();
        var newWorkTypeId = Guid.NewGuid();

        record.ChangeWorkType(newWorkTypeId);

        Assert.Equal(newWorkTypeId, record.WorkTypeId);
    }

    [Fact]
    public void ChangeWorkType_UpdatesUpdatedAt()
    {
        var record = CreateDefaultRecord();
        var originalUpdatedAt = record.UpdatedAt;

        record.ChangeWorkType(Guid.NewGuid());

        Assert.True(record.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void ChangeWorkType_WithEmptyId_ThrowsArgumentException()
    {
        var record = CreateDefaultRecord();

        Assert.Throws<ArgumentException>(() => record.ChangeWorkType(Guid.Empty));
    }
}
