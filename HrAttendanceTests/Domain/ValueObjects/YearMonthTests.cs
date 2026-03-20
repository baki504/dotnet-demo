using HrAttendance.Domain.ValueObjects;

namespace HrAttendanceTests.Domain.ValueObjects;

public class YearMonthTests
{
    [Fact]
    public void Of_WithValidValues_CreatesInstance()
    {
        var ym = YearMonth.Of(2026, 3);

        Assert.Equal(2026, ym.Year);
        Assert.Equal(3, ym.Month);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Of_WithInvalidMonth_ThrowsArgumentException(int month)
    {
        Assert.Throws<ArgumentException>(() => YearMonth.Of(2026, month));
    }

    [Fact]
    public void GetFirstDay_ReturnsFirstDayOfMonth()
    {
        var ym = YearMonth.Of(2026, 3);

        Assert.Equal(new DateOnly(2026, 3, 1), ym.GetFirstDay());
    }

    [Fact]
    public void GetLastDay_ReturnsLastDayOfMonth()
    {
        var ym = YearMonth.Of(2026, 3);

        Assert.Equal(new DateOnly(2026, 3, 31), ym.GetLastDay());
    }

    [Fact]
    public void GetLastDay_February_ReturnsCorrectDay()
    {
        var ym = YearMonth.Of(2026, 2);

        Assert.Equal(new DateOnly(2026, 2, 28), ym.GetLastDay());
    }

    [Fact]
    public void GetLastDay_LeapYear_ReturnsCorrectDay()
    {
        var ym = YearMonth.Of(2024, 2);

        Assert.Equal(new DateOnly(2024, 2, 29), ym.GetLastDay());
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var ym = YearMonth.Of(2026, 3);

        Assert.Equal("2026-03", ym.ToString());
    }

    [Fact]
    public void ToString_SingleDigitMonth_PadsWithZero()
    {
        var ym = YearMonth.Of(2026, 1);

        Assert.Equal("2026-01", ym.ToString());
    }

    [Fact]
    public void Equals_SameYearAndMonth_ReturnsTrue()
    {
        var a = YearMonth.Of(2026, 3);
        var b = YearMonth.Of(2026, 3);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentMonth_ReturnsFalse()
    {
        var a = YearMonth.Of(2026, 3);
        var b = YearMonth.Of(2026, 4);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Equals_DifferentYear_ReturnsFalse()
    {
        var a = YearMonth.Of(2026, 3);
        var b = YearMonth.Of(2025, 3);

        Assert.NotEqual(a, b);
    }
}
