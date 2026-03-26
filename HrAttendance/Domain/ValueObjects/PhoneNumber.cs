namespace HrAttendance.Domain.ValueObjects;

using System.Text.RegularExpressions;

// phone number value object
public class PhoneNumber
{
    public string value { get; set; }

    public PhoneNumber(string value)
    {
        if (value == null)
            throw new ArgumentException("Phone number cannot be null.");
        this.value = value;
    }

    public static PhoneNumber Create(string value)
    {
        return new PhoneNumber(value);
    }

    public bool Equals(PhoneNumber? other)
    {
        if (other == null) return false;
        return value == other.value;
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
}
