namespace HrAttendance.Domain.ValueObjects;

public sealed class YearMonth : IEquatable<YearMonth>
{
    public int Year { get; }
    public int Month { get; }

    private YearMonth(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public static YearMonth Of(int year, int month)
    {
        if (month < 1 || month > 12)
            throw new ArgumentException("Month must be between 1 and 12.", nameof(month));

        return new YearMonth(year, month);
    }

    public DateOnly GetFirstDay()
    {
        return new DateOnly(Year, Month, 1);
    }

    public DateOnly GetLastDay()
    {
        return new DateOnly(Year, Month, DateTime.DaysInMonth(Year, Month));
    }

    public override string ToString()
    {
        return $"{Year:D4}-{Month:D2}";
    }

    public bool Equals(YearMonth? other)
    {
        if (other is null) return false;
        return Year == other.Year && Month == other.Month;
    }

    public override bool Equals(object? obj) => Equals(obj as YearMonth);

    public override int GetHashCode() => HashCode.Combine(Year, Month);
}
