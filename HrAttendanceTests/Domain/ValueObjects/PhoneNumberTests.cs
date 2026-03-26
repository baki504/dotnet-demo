namespace HrAttendanceTests.Domain.ValueObjects;

using HrAttendance.Domain.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void TestCreate()
    {
        var phone = PhoneNumber.Create("090-1234-5678");
        Assert.Equal("090-1234-5678", phone.value);
    }

    [Fact]
    public void TestNull()
    {
        Assert.Throws<ArgumentException>(() => PhoneNumber.Create(null!));
    }

    [Fact]
    public void TestEquals()
    {
        var a = PhoneNumber.Create("090-1234-5678");
        var b = PhoneNumber.Create("090-1234-5678");
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void TestNotEquals()
    {
        var a = PhoneNumber.Create("090-1234-5678");
        var b = PhoneNumber.Create("080-9876-5432");
        Assert.False(a.Equals(b));
    }
}
