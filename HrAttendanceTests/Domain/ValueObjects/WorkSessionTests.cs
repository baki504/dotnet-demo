using HrAttendance.Domain.ValueObjects;

namespace HrAttendanceTests.Domain.ValueObjects;

public class WorkSessionTests
{
    [Fact]
    public void Of_WithClockInAndClockOut_CreatesInstance()
    {
        var clockIn = new DateTime(2026, 3, 20, 9, 0, 0);
        var clockOut = new DateTime(2026, 3, 20, 18, 0, 0);

        var session = WorkSession.Of(clockIn, clockOut);

        Assert.Equal(clockIn, session.ClockIn);
        Assert.Equal(clockOut, session.ClockOut);
    }

    [Fact]
    public void Of_WithClockInOnly_CreatesInstance()
    {
        var clockIn = new DateTime(2026, 3, 20, 9, 0, 0);

        var session = WorkSession.Of(clockIn, null);

        Assert.Equal(clockIn, session.ClockIn);
        Assert.Null(session.ClockOut);
    }

    [Fact]
    public void Of_WithClockOutBeforeClockIn_ThrowsArgumentException()
    {
        var clockIn = new DateTime(2026, 3, 20, 18, 0, 0);
        var clockOut = new DateTime(2026, 3, 20, 9, 0, 0);

        Assert.Throws<ArgumentException>(() => WorkSession.Of(clockIn, clockOut));
    }

    [Fact]
    public void Duration_WithClockOut_ReturnsCorrectDuration()
    {
        var clockIn = new DateTime(2026, 3, 20, 9, 0, 0);
        var clockOut = new DateTime(2026, 3, 20, 18, 0, 0);
        var session = WorkSession.Of(clockIn, clockOut);

        var duration = session.Duration;

        Assert.Equal(WorkDuration.FromMinutes(540), duration);
    }

    [Fact]
    public void Duration_WithoutClockOut_ReturnsZero()
    {
        var clockIn = new DateTime(2026, 3, 20, 9, 0, 0);
        var session = WorkSession.Of(clockIn, null);

        var duration = session.Duration;

        Assert.Equal(WorkDuration.FromMinutes(0), duration);
    }

    [Fact]
    public void Duration_WithPartialHour_ReturnsCorrectMinutes()
    {
        var clockIn = new DateTime(2026, 3, 20, 9, 0, 0);
        var clockOut = new DateTime(2026, 3, 20, 9, 30, 0);
        var session = WorkSession.Of(clockIn, clockOut);

        var duration = session.Duration;

        Assert.Equal(WorkDuration.FromMinutes(30), duration);
    }

    [Fact]
    public void Equals_SameClockInAndClockOut_ReturnsTrue()
    {
        var clockIn = new DateTime(2026, 3, 20, 9, 0, 0);
        var clockOut = new DateTime(2026, 3, 20, 18, 0, 0);
        var a = WorkSession.Of(clockIn, clockOut);
        var b = WorkSession.Of(clockIn, clockOut);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentClockOut_ReturnsFalse()
    {
        var clockIn = new DateTime(2026, 3, 20, 9, 0, 0);
        var a = WorkSession.Of(clockIn, new DateTime(2026, 3, 20, 17, 0, 0));
        var b = WorkSession.Of(clockIn, new DateTime(2026, 3, 20, 18, 0, 0));

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equals_BothWithoutClockOut_ReturnsTrue()
    {
        var clockIn = new DateTime(2026, 3, 20, 9, 0, 0);
        var a = WorkSession.Of(clockIn, null);
        var b = WorkSession.Of(clockIn, null);

        Assert.Equal(a, b);
    }
}
