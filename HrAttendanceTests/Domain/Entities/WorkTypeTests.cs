using HrAttendance.Domain.Entities;

namespace HrAttendanceTests.Domain.Entities;

public class WorkTypeTests
{
    // --- Create ---

    [Fact]
    public void Create_WithValidInput_ReturnsWorkType()
    {
        var workType = WorkType.Create("NORMAL", "通常勤務", isWorkDay: true);

        Assert.NotEqual(Guid.Empty, workType.Id);
        Assert.Equal("NORMAL", workType.Code);
        Assert.Equal("通常勤務", workType.Name);
        Assert.True(workType.IsWorkDay);
    }

    [Fact]
    public void Create_SetsCreatedAtAndUpdatedAt()
    {
        var before = DateTime.Now;
        var workType = WorkType.Create("NORMAL", "通常勤務", isWorkDay: true);
        var after = DateTime.Now;

        Assert.InRange(workType.CreatedAt, before, after);
        Assert.InRange(workType.UpdatedAt, before, after);
    }

    [Fact]
    public void Create_WithIsWorkDayFalse_SetsCorrectly()
    {
        var workType = WorkType.Create("ABSENT", "欠勤", isWorkDay: false);

        Assert.False(workType.IsWorkDay);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidCode_ThrowsArgumentException(string? code)
    {
        Assert.Throws<ArgumentException>(() =>
            WorkType.Create(code!, "通常勤務", isWorkDay: true));
    }

    [Fact]
    public void Create_WithCodeExceeding20Characters_ThrowsArgumentException()
    {
        var longCode = new string('A', 21);
        Assert.Throws<ArgumentException>(() =>
            WorkType.Create(longCode, "通常勤務", isWorkDay: true));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidName_ThrowsArgumentException(string? name)
    {
        Assert.Throws<ArgumentException>(() =>
            WorkType.Create("NORMAL", name!, isWorkDay: true));
    }

    [Fact]
    public void Create_WithNameExceeding50Characters_ThrowsArgumentException()
    {
        var longName = new string('あ', 51);
        Assert.Throws<ArgumentException>(() =>
            WorkType.Create("NORMAL", longName, isWorkDay: true));
    }

    // --- UpdateName ---

    [Fact]
    public void UpdateName_ChangesName()
    {
        var workType = WorkType.Create("NORMAL", "通常勤務", isWorkDay: true);

        workType.UpdateName("通常出勤");

        Assert.Equal("通常出勤", workType.Name);
    }

    [Fact]
    public void UpdateName_UpdatesUpdatedAt()
    {
        var workType = WorkType.Create("NORMAL", "通常勤務", isWorkDay: true);
        var originalUpdatedAt = workType.UpdatedAt;

        workType.UpdateName("通常出勤");

        Assert.True(workType.UpdatedAt >= originalUpdatedAt);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateName_WithInvalidName_ThrowsArgumentException(string? name)
    {
        var workType = WorkType.Create("NORMAL", "通常勤務", isWorkDay: true);

        Assert.Throws<ArgumentException>(() => workType.UpdateName(name!));
    }

    [Fact]
    public void UpdateName_WithNameExceeding50Characters_ThrowsArgumentException()
    {
        var workType = WorkType.Create("NORMAL", "通常勤務", isWorkDay: true);
        var longName = new string('あ', 51);

        Assert.Throws<ArgumentException>(() => workType.UpdateName(longName));
    }
}
