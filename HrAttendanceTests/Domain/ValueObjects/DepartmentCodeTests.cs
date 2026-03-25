using HrAttendance.Domain.ValueObjects;

namespace HrAttendanceTests.Domain.ValueObjects;

public class DepartmentCodeTests
{
    [Theory]
    [InlineData("SALES")]
    [InlineData("HR01")]
    [InlineData("A")]
    [InlineData("12345678901234567890")] // 20文字ちょうど
    public void Of_WithValidValue_CreatesInstance(string code)
    {
        var departmentCode = DepartmentCode.Of(code);

        Assert.Equal(code, departmentCode.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Of_WithNullOrEmpty_ThrowsArgumentException(string? code)
    {
        Assert.Throws<ArgumentException>(() => DepartmentCode.Of(code!));
    }

    [Fact]
    public void Of_WithOver20Characters_ThrowsArgumentException()
    {
        var longCode = new string('A', 21);

        Assert.Throws<ArgumentException>(() => DepartmentCode.Of(longCode));
    }

    [Theory]
    [InlineData("SALES-01")]
    [InlineData("HR_DEPT")]
    [InlineData("部門A")]
    [InlineData("DEPT@1")]
    public void Of_WithNonAlphanumeric_ThrowsArgumentException(string code)
    {
        Assert.Throws<ArgumentException>(() => DepartmentCode.Of(code));
    }

    [Fact]
    public void Equals_SameValue_ReturnsTrue()
    {
        var a = DepartmentCode.Of("SALES");
        var b = DepartmentCode.Of("SALES");

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equals_DifferentValue_ReturnsFalse()
    {
        var a = DepartmentCode.Of("SALES");
        var b = DepartmentCode.Of("HR");

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        var code = DepartmentCode.Of("SALES");

        Assert.Equal("SALES", code.ToString());
    }
}
