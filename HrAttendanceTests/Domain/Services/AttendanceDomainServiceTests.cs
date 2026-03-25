using HrAttendance.Domain;
using HrAttendance.Domain.Entities;
using HrAttendance.Domain.Services;

namespace HrAttendanceTests.Domain.Services;

public class AttendanceDomainServiceTests
{
    private readonly AttendanceDomainService _service = new();
    private readonly Guid _employeeId = Guid.NewGuid();
    private readonly Guid _workTypeId = Guid.NewGuid();

    private AttendanceRecord CreateCompletedRecord(DateTime clockIn, DateTime clockOut)
    {
        var record = AttendanceRecord.Create(_employeeId, DateOnly.FromDateTime(clockIn), _workTypeId);
        record.AddTimeStamp(TimeStampType.ClockIn, clockIn);
        record.AddTimeStamp(TimeStampType.ClockOut, clockOut);
        return record;
    }

    private AttendanceRecord CreateClockInOnlyRecord(DateTime clockIn)
    {
        var record = AttendanceRecord.Create(_employeeId, DateOnly.FromDateTime(clockIn), _workTypeId);
        record.AddTimeStamp(TimeStampType.ClockIn, clockIn);
        return record;
    }

    // --- ValidateClockIn ---

    [Fact]
    public void ValidateClockIn_NoExistingRecords_Succeeds()
    {
        var existingRecords = new List<AttendanceRecord>();

        _service.ValidateClockIn(existingRecords, new DateTime(2026, 3, 24, 9, 0, 0));
    }

    [Fact]
    public void ValidateClockIn_PreviousSetCompleted_Succeeds()
    {
        var existingRecords = new List<AttendanceRecord>
        {
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 9, 0, 0),
                new DateTime(2026, 3, 24, 12, 0, 0))
        };

        _service.ValidateClockIn(existingRecords, new DateTime(2026, 3, 24, 13, 0, 0));
    }

    [Fact]
    public void ValidateClockIn_PreviousSetNotCompleted_ThrowsInvalidOperationException()
    {
        var existingRecords = new List<AttendanceRecord>
        {
            CreateClockInOnlyRecord(new DateTime(2026, 3, 24, 9, 0, 0))
        };

        Assert.Throws<InvalidOperationException>(() =>
            _service.ValidateClockIn(existingRecords, new DateTime(2026, 3, 24, 13, 0, 0)));
    }

    [Fact]
    public void ValidateClockIn_TimeOverlapWithPreviousSet_ThrowsInvalidOperationException()
    {
        var existingRecords = new List<AttendanceRecord>
        {
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 9, 0, 0),
                new DateTime(2026, 3, 24, 14, 0, 0))
        };

        Assert.Throws<InvalidOperationException>(() =>
            _service.ValidateClockIn(existingRecords, new DateTime(2026, 3, 24, 13, 0, 0)));
    }

    [Fact]
    public void ValidateClockIn_ExactlyAfterPreviousClockOut_Succeeds()
    {
        var existingRecords = new List<AttendanceRecord>
        {
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 9, 0, 0),
                new DateTime(2026, 3, 24, 12, 0, 0))
        };

        // ClockIn exactly at previous ClockOut time should be allowed
        _service.ValidateClockIn(existingRecords, new DateTime(2026, 3, 24, 12, 0, 0));
    }

    [Fact]
    public void ValidateClockIn_MultipleCompletedSets_ValidatesAgainstLast()
    {
        var existingRecords = new List<AttendanceRecord>
        {
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 9, 0, 0),
                new DateTime(2026, 3, 24, 12, 0, 0)),
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 13, 0, 0),
                new DateTime(2026, 3, 24, 17, 0, 0))
        };

        _service.ValidateClockIn(existingRecords, new DateTime(2026, 3, 24, 18, 0, 0));
    }

    [Fact]
    public void ValidateClockIn_MultipleCompletedSets_OverlapWithLast_ThrowsInvalidOperationException()
    {
        var existingRecords = new List<AttendanceRecord>
        {
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 9, 0, 0),
                new DateTime(2026, 3, 24, 12, 0, 0)),
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 13, 0, 0),
                new DateTime(2026, 3, 24, 17, 0, 0))
        };

        Assert.Throws<InvalidOperationException>(() =>
            _service.ValidateClockIn(existingRecords, new DateTime(2026, 3, 24, 16, 0, 0)));
    }

    // --- ValidateTimeStampUpdate ---

    [Fact]
    public void ValidateTimeStampUpdate_NoOverlap_Succeeds()
    {
        var otherRecords = new List<AttendanceRecord>
        {
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 9, 0, 0),
                new DateTime(2026, 3, 24, 12, 0, 0))
        };

        _service.ValidateTimeStampUpdate(
            otherRecords,
            new DateTime(2026, 3, 24, 13, 0, 0),
            new DateTime(2026, 3, 24, 17, 0, 0));
    }

    [Fact]
    public void ValidateTimeStampUpdate_OverlapWithOtherRecord_ThrowsInvalidOperationException()
    {
        var otherRecords = new List<AttendanceRecord>
        {
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 9, 0, 0),
                new DateTime(2026, 3, 24, 14, 0, 0))
        };

        Assert.Throws<InvalidOperationException>(() =>
            _service.ValidateTimeStampUpdate(
                otherRecords,
                new DateTime(2026, 3, 24, 13, 0, 0),
                new DateTime(2026, 3, 24, 17, 0, 0)));
    }

    [Fact]
    public void ValidateTimeStampUpdate_AdjacentToOtherRecord_Succeeds()
    {
        var otherRecords = new List<AttendanceRecord>
        {
            CreateCompletedRecord(
                new DateTime(2026, 3, 24, 9, 0, 0),
                new DateTime(2026, 3, 24, 12, 0, 0))
        };

        _service.ValidateTimeStampUpdate(
            otherRecords,
            new DateTime(2026, 3, 24, 12, 0, 0),
            new DateTime(2026, 3, 24, 17, 0, 0));
    }
}
