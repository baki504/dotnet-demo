namespace HrAttendance.Domain.ValueObjects;

public sealed class EmployeeId : IEquatable<EmployeeId>
{
    public Guid Value { get; }

    private EmployeeId(Guid value)
    {
        Value = value;
    }

    public static EmployeeId Of(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("EmployeeId cannot be empty.", nameof(value));

        return new EmployeeId(value);
    }

    public override string ToString() => Value.ToString();

    public bool Equals(EmployeeId? other)
    {
        if (other is null) return false;
        return Value == other.Value;
    }

    public override bool Equals(object? obj) => Equals(obj as EmployeeId);

    public override int GetHashCode() => Value.GetHashCode();
}
