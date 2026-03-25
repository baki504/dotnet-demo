using System.Text.RegularExpressions;

namespace HrAttendance.Domain.Entities;

public partial class Employee
{
    private const int NameMaxLength = 100;
    private readonly List<EmployeeDepartment> _employeeDepartments = [];

    public Guid Id { get; private set; }
    public string EmployeeNumber { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public Role Role { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyList<EmployeeDepartment> EmployeeDepartments => _employeeDepartments;

    private Employee() { }

    public static Employee Create(
        string employeeNumber,
        string name,
        string email,
        Role role,
        Guid departmentId)
    {
        ValidateEmployeeNumber(employeeNumber);
        ValidateName(name);
        ValidateEmail(email);

        if (departmentId == Guid.Empty)
            throw new ArgumentException("DepartmentId cannot be empty.", nameof(departmentId));

        var now = DateTime.Now;
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = employeeNumber,
            Name = name,
            Email = email,
            Role = role,
            IsDeleted = false,
            CreatedAt = now,
            UpdatedAt = now,
        };

        employee._employeeDepartments.Add(
            EmployeeDepartment.Create(employee.Id, departmentId, isPrimary: true));

        return employee;
    }

    public void UpdateInfo(string name, string email)
    {
        ValidateName(name);
        ValidateEmail(email);

        Name = name;
        Email = email;
        UpdatedAt = DateTime.Now;
    }

    public void ChangeRole(Role role)
    {
        Role = role;
        UpdatedAt = DateTime.Now;
    }

    public void Delete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Employee is already deleted.");

        IsDeleted = true;
        UpdatedAt = DateTime.Now;
    }

    public void AddDepartment(Guid departmentId)
    {
        if (departmentId == Guid.Empty)
            throw new ArgumentException("DepartmentId cannot be empty.", nameof(departmentId));

        if (_employeeDepartments.Any(d => d.DepartmentId == departmentId))
            throw new InvalidOperationException("Employee is already assigned to this department.");

        _employeeDepartments.Add(
            EmployeeDepartment.Create(Id, departmentId, isPrimary: false));

        UpdatedAt = DateTime.Now;
    }

    public void RemoveDepartment(Guid departmentId)
    {
        var target = _employeeDepartments.FirstOrDefault(d => d.DepartmentId == departmentId)
            ?? throw new InvalidOperationException("Employee is not assigned to this department.");

        if (_employeeDepartments.Count == 1)
            throw new InvalidOperationException("Employee must belong to at least one department.");

        _employeeDepartments.Remove(target);

        if (target.IsPrimary)
        {
            _employeeDepartments[0].IsPrimary = true;
        }

        UpdatedAt = DateTime.Now;
    }

    public void SetPrimaryDepartment(Guid departmentId)
    {
        var target = _employeeDepartments.FirstOrDefault(d => d.DepartmentId == departmentId)
            ?? throw new InvalidOperationException("Employee is not assigned to this department.");

        foreach (var dept in _employeeDepartments)
        {
            dept.IsPrimary = false;
        }

        target.IsPrimary = true;
        UpdatedAt = DateTime.Now;
    }

    private static void ValidateEmployeeNumber(string employeeNumber)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new ArgumentException("EmployeeNumber cannot be null or empty.", nameof(employeeNumber));

        if (!EmployeeNumberPattern().IsMatch(employeeNumber))
            throw new ArgumentException("EmployeeNumber must be in the format 'EMP-XXX' (e.g., EMP-001).", nameof(employeeNumber));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Name cannot exceed {NameMaxLength} characters.", nameof(name));
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty.", nameof(email));

        if (!EmailPattern().IsMatch(email))
            throw new ArgumentException("Email must be a valid email address.", nameof(email));
    }

    [GeneratedRegex(@"^EMP-\d+$")]
    private static partial Regex EmployeeNumberPattern();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
