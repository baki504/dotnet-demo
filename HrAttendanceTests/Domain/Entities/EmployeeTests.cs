using HrAttendance.Domain;
using HrAttendance.Domain.Entities;
using HrAttendance.Domain.ValueObjects;

namespace HrAttendanceTests.Domain.Entities;

public class EmployeeTests
{
    private readonly Guid _departmentId = Guid.NewGuid();

    private Employee CreateDefaultEmployee()
    {
        return Employee.Create(
            employeeNumber: "EMP-001",
            name: "田中太郎",
            email: "tanaka@example.com",
            role: Role.User,
            departmentId: _departmentId);
    }

    // --- Create ---

    [Fact]
    public void Create_WithValidInput_ReturnsEmployee()
    {
        var employee = CreateDefaultEmployee();

        Assert.Equal("EMP-001", employee.EmployeeNumber);
        Assert.Equal("田中太郎", employee.Name);
        Assert.Equal("tanaka@example.com", employee.Email);
        Assert.Equal(Role.User, employee.Role);
        Assert.False(employee.IsDeleted);
        Assert.NotEqual(Guid.Empty, employee.Id);
    }

    [Fact]
    public void Create_SetsCreatedAtAndUpdatedAt()
    {
        var before = DateTime.Now;
        var employee = CreateDefaultEmployee();
        var after = DateTime.Now;

        Assert.InRange(employee.CreatedAt, before, after);
        Assert.InRange(employee.UpdatedAt, before, after);
    }

    [Fact]
    public void Create_AssignsPrimaryDepartment()
    {
        var employee = CreateDefaultEmployee();

        var departments = employee.EmployeeDepartments;
        Assert.Single(departments);
        Assert.Equal(_departmentId, departments[0].DepartmentId);
        Assert.True(departments[0].IsPrimary);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidEmployeeNumber_ThrowsArgumentException(string? employeeNumber)
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(employeeNumber!, "田中太郎", "tanaka@example.com", Role.User, _departmentId));
    }

    [Theory]
    [InlineData("001")]
    [InlineData("EMP001")]
    [InlineData("emp-001")]
    [InlineData("EMP-")]
    public void Create_WithInvalidEmployeeNumberFormat_ThrowsArgumentException(string employeeNumber)
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create(employeeNumber, "田中太郎", "tanaka@example.com", Role.User, _departmentId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create("EMP-001", name!, "tanaka@example.com", Role.User, _departmentId));
    }

    [Fact]
    public void Create_WithNameExceeding100Characters_ThrowsArgumentException()
    {
        var longName = new string('あ', 101);
        Assert.Throws<ArgumentException>(() =>
            Employee.Create("EMP-001", longName, "tanaka@example.com", Role.User, _departmentId));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    public void Create_WithInvalidEmail_ThrowsArgumentException(string? email)
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create("EMP-001", "田中太郎", email!, Role.User, _departmentId));
    }

    [Fact]
    public void Create_WithEmptyDepartmentId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Employee.Create("EMP-001", "田中太郎", "tanaka@example.com", Role.User, Guid.Empty));
    }

    // --- UpdateInfo ---

    [Fact]
    public void UpdateInfo_ChangesNameAndEmail()
    {
        var employee = CreateDefaultEmployee();
        var originalUpdatedAt = employee.UpdatedAt;

        employee.UpdateInfo("佐藤花子", "sato@example.com");

        Assert.Equal("佐藤花子", employee.Name);
        Assert.Equal("sato@example.com", employee.Email);
        Assert.True(employee.UpdatedAt >= originalUpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateInfo_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var employee = CreateDefaultEmployee();
        Assert.Throws<ArgumentException>(() => employee.UpdateInfo(name!, "sato@example.com"));
    }

    [Fact]
    public void UpdateInfo_WithNameExceeding100Characters_ThrowsArgumentException()
    {
        var employee = CreateDefaultEmployee();
        var longName = new string('あ', 101);
        Assert.Throws<ArgumentException>(() => employee.UpdateInfo(longName, "sato@example.com"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void UpdateInfo_WithInvalidEmail_ThrowsArgumentException(string? email)
    {
        var employee = CreateDefaultEmployee();
        Assert.Throws<ArgumentException>(() => employee.UpdateInfo("佐藤花子", email!));
    }

    // --- ChangeRole ---

    [Fact]
    public void ChangeRole_UpdatesRole()
    {
        var employee = CreateDefaultEmployee();

        employee.ChangeRole(Role.Admin);

        Assert.Equal(Role.Admin, employee.Role);
    }

    [Fact]
    public void ChangeRole_UpdatesUpdatedAt()
    {
        var employee = CreateDefaultEmployee();
        var originalUpdatedAt = employee.UpdatedAt;

        employee.ChangeRole(Role.Admin);

        Assert.True(employee.UpdatedAt >= originalUpdatedAt);
    }

    // --- Delete (論理削除) ---

    [Fact]
    public void Delete_SetsIsDeletedToTrue()
    {
        var employee = CreateDefaultEmployee();

        employee.Delete();

        Assert.True(employee.IsDeleted);
    }

    [Fact]
    public void Delete_UpdatesUpdatedAt()
    {
        var employee = CreateDefaultEmployee();
        var originalUpdatedAt = employee.UpdatedAt;

        employee.Delete();

        Assert.True(employee.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void Delete_WhenAlreadyDeleted_ThrowsInvalidOperationException()
    {
        var employee = CreateDefaultEmployee();
        employee.Delete();

        Assert.Throws<InvalidOperationException>(() => employee.Delete());
    }

    // --- AddDepartment (兼務) ---

    [Fact]
    public void AddDepartment_AddsNonPrimaryDepartment()
    {
        var employee = CreateDefaultEmployee();
        var secondDeptId = Guid.NewGuid();

        employee.AddDepartment(secondDeptId);

        Assert.Equal(2, employee.EmployeeDepartments.Count);
        var added = employee.EmployeeDepartments.First(d => d.DepartmentId == secondDeptId);
        Assert.False(added.IsPrimary);
    }

    [Fact]
    public void AddDepartment_WithAlreadyAssignedDepartment_ThrowsInvalidOperationException()
    {
        var employee = CreateDefaultEmployee();

        Assert.Throws<InvalidOperationException>(() => employee.AddDepartment(_departmentId));
    }

    [Fact]
    public void AddDepartment_WithEmptyId_ThrowsArgumentException()
    {
        var employee = CreateDefaultEmployee();

        Assert.Throws<ArgumentException>(() => employee.AddDepartment(Guid.Empty));
    }

    // --- RemoveDepartment ---

    [Fact]
    public void RemoveDepartment_RemovesNonPrimaryDepartment()
    {
        var employee = CreateDefaultEmployee();
        var secondDeptId = Guid.NewGuid();
        employee.AddDepartment(secondDeptId);

        employee.RemoveDepartment(secondDeptId);

        Assert.Single(employee.EmployeeDepartments);
        Assert.Equal(_departmentId, employee.EmployeeDepartments[0].DepartmentId);
    }

    [Fact]
    public void RemoveDepartment_LastDepartment_ThrowsInvalidOperationException()
    {
        var employee = CreateDefaultEmployee();

        Assert.Throws<InvalidOperationException>(() => employee.RemoveDepartment(_departmentId));
    }

    [Fact]
    public void RemoveDepartment_WhenPrimaryAndOtherExists_PromotesAnother()
    {
        var employee = CreateDefaultEmployee();
        var secondDeptId = Guid.NewGuid();
        employee.AddDepartment(secondDeptId);

        employee.RemoveDepartment(_departmentId);

        Assert.Single(employee.EmployeeDepartments);
        Assert.True(employee.EmployeeDepartments[0].IsPrimary);
        Assert.Equal(secondDeptId, employee.EmployeeDepartments[0].DepartmentId);
    }

    [Fact]
    public void RemoveDepartment_NotAssigned_ThrowsInvalidOperationException()
    {
        var employee = CreateDefaultEmployee();
        var unknownDeptId = Guid.NewGuid();

        Assert.Throws<InvalidOperationException>(() => employee.RemoveDepartment(unknownDeptId));
    }

    // --- SetPrimaryDepartment ---

    [Fact]
    public void SetPrimaryDepartment_ChangePrimary()
    {
        var employee = CreateDefaultEmployee();
        var secondDeptId = Guid.NewGuid();
        employee.AddDepartment(secondDeptId);

        employee.SetPrimaryDepartment(secondDeptId);

        var primary = employee.EmployeeDepartments.Single(d => d.IsPrimary);
        Assert.Equal(secondDeptId, primary.DepartmentId);
        var original = employee.EmployeeDepartments.Single(d => d.DepartmentId == _departmentId);
        Assert.False(original.IsPrimary);
    }

    [Fact]
    public void SetPrimaryDepartment_NotAssigned_ThrowsInvalidOperationException()
    {
        var employee = CreateDefaultEmployee();

        Assert.Throws<InvalidOperationException>(() => employee.SetPrimaryDepartment(Guid.NewGuid()));
    }
}
