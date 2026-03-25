namespace HrAttendance.Domain.Entities;

public class WorkType
{
    private const int CodeMaxLength = 20;
    private const int NameMaxLength = 50;

    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public bool IsWorkDay { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private WorkType() { }

    public static WorkType Create(string code, string name, bool isWorkDay)
    {
        ValidateCode(code);
        ValidateName(name);

        var now = DateTime.Now;
        return new WorkType
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            IsWorkDay = isWorkDay,
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

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));

        if (code.Length > CodeMaxLength)
            throw new ArgumentException($"Code cannot exceed {CodeMaxLength} characters.", nameof(code));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        if (name.Length > NameMaxLength)
            throw new ArgumentException($"Name cannot exceed {NameMaxLength} characters.", nameof(name));
    }
}
