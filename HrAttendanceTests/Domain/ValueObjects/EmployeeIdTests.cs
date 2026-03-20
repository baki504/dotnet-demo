using HrAttendance.Domain.ValueObjects;

namespace HrAttendanceTests.Domain.ValueObjects;

public class EmployeeIdTests
{
    [Fact]
    public void Of_WithValidGuid_CreatesInstance()
    {
        var guid = Guid.NewGuid();

        var id = EmployeeId.Of(guid);

        Assert.Equal(guid, id.Value);
    }

    [Fact]
    public void Of_WithEmptyGuid_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => EmployeeId.Of(Guid.Empty));
    }

    [Fact]
    public void Equals_SameGuid_ReturnsTrue()
    {
        var guid = Guid.NewGuid();
        var a = EmployeeId.Of(guid);
        var b = EmployeeId.Of(guid);

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentGuid_ReturnsFalse()
    {
        var a = EmployeeId.Of(Guid.NewGuid());
        var b = EmployeeId.Of(Guid.NewGuid());

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ReturnsGuidString()
    {
        var guid = Guid.NewGuid();
        var id = EmployeeId.Of(guid);

        Assert.Equal(guid.ToString(), id.ToString());
    }
}
