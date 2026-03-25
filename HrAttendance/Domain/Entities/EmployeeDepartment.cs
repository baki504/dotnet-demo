namespace HrAttendance.Domain.Entities;

public class EmployeeDepartment
{
    public Guid EmployeeId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public bool IsPrimary { get; internal set; }

    private EmployeeDepartment() { }

    internal static EmployeeDepartment Create(Guid employeeId, Guid departmentId, bool isPrimary)
    {
        return new EmployeeDepartment
        {
            EmployeeId = employeeId,
            DepartmentId = departmentId,
            IsPrimary = isPrimary,
        };
    }
}
