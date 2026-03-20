using HrAttendance.Domain.ValueObjects;

namespace HrAttendanceTests.Domain.ValueObjects;

public class WorkDurationTests
{
    [Fact]
    public void FromMinutes_WithPositiveValue_CreatesInstance()
    {
        var duration = WorkDuration.FromMinutes(120);

        Assert.Equal(120, duration.TotalMinutes);
    }

    [Fact]
    public void FromMinutes_WithZero_CreatesZeroDuration()
    {
        var duration = WorkDuration.FromMinutes(0);

        Assert.Equal(0, duration.TotalMinutes);
    }

    [Fact]
    public void FromMinutes_WithNegativeValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => WorkDuration.FromMinutes(-1));
    }

    [Fact]
    public void Add_TwoDurations_ReturnsSummedDuration()
    {
        var a = WorkDuration.FromMinutes(60);
        var b = WorkDuration.FromMinutes(90);

        var result = a.Add(b);

        Assert.Equal(150, result.TotalMinutes);
    }

    [Fact]
    public void Subtract_SmallerFromLarger_ReturnsDifference()
    {
        var a = WorkDuration.FromMinutes(120);
        var b = WorkDuration.FromMinutes(30);

        var result = a.Subtract(b);

        Assert.Equal(90, result.TotalMinutes);
    }

    [Fact]
    public void Subtract_LargerFromSmaller_ThrowsInvalidOperationException()
    {
        var a = WorkDuration.FromMinutes(30);
        var b = WorkDuration.FromMinutes(120);

        Assert.Throws<InvalidOperationException>(() => a.Subtract(b));
    }

    [Fact]
    public void IsGreaterThan_WhenGreater_ReturnsTrue()
    {
        var a = WorkDuration.FromMinutes(120);
        var b = WorkDuration.FromMinutes(60);

        Assert.True(a.IsGreaterThan(b));
    }

    [Fact]
    public void IsGreaterThan_WhenEqual_ReturnsFalse()
    {
        var a = WorkDuration.FromMinutes(60);
        var b = WorkDuration.FromMinutes(60);

        Assert.False(a.IsGreaterThan(b));
    }

    [Fact]
    public void IsGreaterThan_WhenSmaller_ReturnsFalse()
    {
        var a = WorkDuration.FromMinutes(30);
        var b = WorkDuration.FromMinutes(60);

        Assert.False(a.IsGreaterThan(b));
    }

    [Fact]
    public void ToHoursAndMinutes_ReturnsCorrectTuple()
    {
        var duration = WorkDuration.FromMinutes(150);

        var (hours, minutes) = duration.ToHoursAndMinutes();

        Assert.Equal(2, hours);
        Assert.Equal(30, minutes);
    }

    [Fact]
    public void ToHoursAndMinutes_ExactHours_ReturnsZeroMinutes()
    {
        var duration = WorkDuration.FromMinutes(480);

        var (hours, minutes) = duration.ToHoursAndMinutes();

        Assert.Equal(8, hours);
        Assert.Equal(0, minutes);
    }

    [Fact]
    public void Equals_SameMinutes_ReturnsTrue()
    {
        var a = WorkDuration.FromMinutes(60);
        var b = WorkDuration.FromMinutes(60);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentMinutes_ReturnsFalse()
    {
        var a = WorkDuration.FromMinutes(60);
        var b = WorkDuration.FromMinutes(90);

        Assert.NotEqual(a, b);
    }
}
