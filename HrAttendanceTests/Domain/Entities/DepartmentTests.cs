using HrAttendance.Domain.Entities;
using HrAttendance.Domain.ValueObjects;

namespace HrAttendanceTests.Domain.Entities;

public class DepartmentTests
{
    // --- Create ---

    [Fact]
    public void Create_WithValidInput_ReturnsDepartment()
    {
        var code = DepartmentCode.Of("DEV01");

        var department = Department.Create(code, "開発部");

        Assert.Equal(code, department.Code);
        Assert.Equal("開発部", department.Name);
        Assert.NotEqual(Guid.Empty, department.Id);
    }

    [Fact]
    public void Create_SetsCreatedAtAndUpdatedAt()
    {
        var before = DateTime.Now;
        var department = Department.Create(DepartmentCode.Of("DEV01"), "開発部");
        var after = DateTime.Now;

        Assert.InRange(department.CreatedAt, before, after);
        Assert.InRange(department.UpdatedAt, before, after);
    }

    [Fact]
    public void Create_WithNullCode_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Department.Create(null!, "開発部"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            Department.Create(DepartmentCode.Of("DEV01"), name!));
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ThrowsArgumentException()
    {
        var longName = new string('あ', 101);
        Assert.Throws<ArgumentException>(() =>
            Department.Create(DepartmentCode.Of("DEV01"), longName));
    }

    // --- UpdateName ---

    [Fact]
    public void UpdateName_ChangesName()
    {
        var department = Department.Create(DepartmentCode.Of("DEV01"), "開発部");

        department.UpdateName("開発第一部");

        Assert.Equal("開発第一部", department.Name);
    }

    [Fact]
    public void UpdateName_UpdatesUpdatedAt()
    {
        var department = Department.Create(DepartmentCode.Of("DEV01"), "開発部");
        var originalUpdatedAt = department.UpdatedAt;

        department.UpdateName("開発第一部");

        Assert.True(department.UpdatedAt >= originalUpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var department = Department.Create(DepartmentCode.Of("DEV01"), "開発部");

        Assert.Throws<ArgumentException>(() => department.UpdateName(name!));
    }

    [Fact]
    public void UpdateName_WithNameExceeding100Characters_ThrowsArgumentException()
    {
        var department = Department.Create(DepartmentCode.Of("DEV01"), "開発部");
        var longName = new string('あ', 101);

        Assert.Throws<ArgumentException>(() => department.UpdateName(longName));
    }
}
