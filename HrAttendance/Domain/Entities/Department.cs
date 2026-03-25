using HrAttendance.Domain.ValueObjects;

namespace HrAttendance.Domain.Entities;

public class Department
{
    private const int NameMaxLength = 100;

    public Guid Id { get; private set; }
    public DepartmentCode Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Department() { }

    public static Department Create(DepartmentCode code, string name)
    {
        ArgumentNullException.ThrowIfNull(code);
        ValidateName(name);

        var now = DateTime.Now;
        return new Department
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
        UpdatedAt = DateTime.Now;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Name cannot exceed {NameMaxLength} characters.", nameof(name));
    }
}
