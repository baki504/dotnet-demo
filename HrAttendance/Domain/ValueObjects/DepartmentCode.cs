using System.Text.RegularExpressions;

namespace HrAttendance.Domain.ValueObjects;

public sealed partial class DepartmentCode : IEquatable<DepartmentCode>
{
    private const int MaxLength = 20;

    public string Value { get; }

    private DepartmentCode(string value)
    {
        Value = value;
    }

    public static DepartmentCode Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("DepartmentCode cannot be null or empty.", nameof(value));

        if (value.Length > MaxLength)
            throw new ArgumentException($"DepartmentCode cannot exceed {MaxLength} characters.", nameof(value));

        if (!AlphanumericPattern().IsMatch(value))
            throw new ArgumentException("DepartmentCode must contain only alphanumeric characters.", nameof(value));

        return new DepartmentCode(value);
    }

    public override string ToString() => Value;

    public bool Equals(DepartmentCode? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as DepartmentCode);

    public override int GetHashCode() => Value.GetHashCode();

    [GeneratedRegex("^[a-zA-Z0-9]+$")]
    private static partial Regex AlphanumericPattern();
}
