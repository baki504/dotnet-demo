# コンポーネントメソッド定義

## Domain 層

### Employee
```
- Employee.Create(employeeNumber, name, email, role) -> Employee
- Employee.UpdateInfo(name, email) -> void
- Employee.ChangeRole(role) -> void
- Employee.AddDepartment(department, isPrimary) -> EmployeeDepartment
- Employee.RemoveDepartment(departmentId) -> void
- Employee.SetPrimaryDepartment(departmentId) -> void
```

### Department
```
- Department.Create(code, name) -> Department
- Department.UpdateName(name) -> void
```

### AttendanceRecord
```
- AttendanceRecord.Create(employeeId, date, workTypeId) -> AttendanceRecord
- AttendanceRecord.AddTimeStamp(type, timestamp) -> TimeStampEntry
- AttendanceRecord.ChangeWorkType(workTypeId) -> void
```

### TimeStampEntry
```
- TimeStampEntry.Create(type, timestamp) -> TimeStampEntry
- TimeStampEntry.Correct(newTimestamp) -> void
```

### WorkType
```
- WorkType.Create(code, name, isWorkDay) -> WorkType
- WorkType.UpdateName(name) -> void
```

### AttendanceDomainService
```
- ValidateTimeStamp(record, newStampType, timestamp) -> bool
  # 打刻順序の検証（出勤→休憩開始→休憩終了→退勤）
  # 二重打刻の防止
```

### MonthlyAttendanceSummary（リッチドメインモデル）
```
- MonthlyAttendanceSummary.Build(employee, departmentName, records, workTypes) -> MonthlyAttendanceSummary
  # 複数テーブルの生データからサマリーを構築（ファクトリメソッド）
- GetTotalWorkDays() -> int
  # 勤務区分が出勤日(IsWorkDay=true)のレコード数を集計
- GetTotalWorkDuration() -> WorkDuration
  # 全勤務日の出勤～退勤から休憩を差し引いた実労働時間を合算
- GetOvertimeDuration(standardHoursPerDay: WorkDuration) -> WorkDuration
  # 日ごとに所定労働時間を超えた分を残業として合算
- GetPaidLeaveDays() -> int
  # 勤務区分が有給休暇のレコード数を集計
- GetAbsentDays() -> int
  # 勤務区分が欠勤のレコード数を集計
- GetWorkDaysByType() -> Dictionary<string, int>
  # 勤務区分別の日数内訳
```

### WorkDuration（値オブジェクト）
```
- WorkDuration.FromMinutes(minutes) -> WorkDuration
- WorkDuration.Zero -> WorkDuration
- Add(other: WorkDuration) -> WorkDuration
- Subtract(other: WorkDuration) -> WorkDuration
- IsGreaterThan(other: WorkDuration) -> bool
- ToHoursAndMinutes() -> (int hours, int minutes)
- TotalMinutes -> int
```

### YearMonth（値オブジェクト）
```
- YearMonth.Of(year, month) -> YearMonth
- GetFirstDay() -> DateOnly
- GetLastDay() -> DateOnly
- ToString() -> string  # "2026-03" 形式
```

### MonthlyAttendanceDomainService
```
- BuildSummary(employee, departmentName, records, workTypes) -> MonthlyAttendanceSummary
  # IMonthlyAttendanceQueryServiceから取得した生データをMonthlyAttendanceSummaryに組み立て
- BuildSummaries(queryResults) -> List<MonthlyAttendanceSummary>
  # 複数社員分のサマリーを一括構築
```

---

## Application 層

### EmployeeAppService
```
- GetAllAsync(searchTerm?) -> List<EmployeeDto>
- GetByIdAsync(id) -> EmployeeDto?
- CreateAsync(dto: CreateEmployeeDto) -> EmployeeDto
- UpdateAsync(id, dto: UpdateEmployeeDto) -> EmployeeDto
- DeleteAsync(id) -> void
- AssignDepartmentAsync(employeeId, departmentId, isPrimary) -> void
- RemoveDepartmentAsync(employeeId, departmentId) -> void
```

### DepartmentAppService
```
- GetAllAsync(searchTerm?) -> List<DepartmentDto>
- GetByIdAsync(id) -> DepartmentDto?
- CreateAsync(dto: CreateDepartmentDto) -> DepartmentDto
- UpdateAsync(id, dto: UpdateDepartmentDto) -> DepartmentDto
- DeleteAsync(id) -> void
```

### AttendanceAppService
```
- GetByDateRangeAsync(criteria: AttendanceSearchCriteria) -> List<AttendanceRecordDto>
- GetByEmployeeAndDateAsync(employeeId, date) -> AttendanceRecordDto?
- StampAsync(employeeId, stampType, timestamp?) -> TimeStampEntryDto
- UpdateRecordAsync(recordId, dto) -> AttendanceRecordDto
```

### WorkTypeAppService
```
- GetAllAsync() -> List<WorkTypeDto>
- GetByIdAsync(id) -> WorkTypeDto?
- CreateAsync(dto: CreateWorkTypeDto) -> WorkTypeDto
- UpdateAsync(id, dto: CreateWorkTypeDto) -> WorkTypeDto
- DeleteAsync(id) -> void
```

### MonthlyAttendanceAppService
```
- GetMonthlySummariesAsync(yearMonth: YearMonth, departmentId?) -> List<MonthlyAttendanceSummaryDto>
  # IMonthlyAttendanceQueryService でデータ取得
  # → MonthlyAttendanceDomainService でリッチドメインモデル構築
  # → DTO変換して返却
- GetMonthlySummaryByEmployeeAsync(employeeId, yearMonth: YearMonth) -> MonthlyAttendanceSummaryDto?
  # 特定社員の月次サマリーを取得
```

---

## Presentation 層 (Controller)

### EmployeesController
```
- Index(searchTerm?) -> IActionResult
  # EmployeeAppService.GetAllAsync() → EmployeeListViewModel へ変換 → View返却
- Details(id) -> IActionResult
  # EmployeeAppService.GetByIdAsync() → EmployeeDetailViewModel へ変換 → View返却
- Create() -> IActionResult [GET]
  # 空の EmployeeFormModel + 部署リスト → View返却
- Create(EmployeeFormModel) -> IActionResult [POST]
  # バリデーション → CreateEmployeeDto へ変換 → EmployeeAppService.CreateAsync() → Redirect
- Edit(id) -> IActionResult [GET]
  # EmployeeAppService.GetByIdAsync() → EmployeeFormModel へ変換 → View返却
- Edit(id, EmployeeFormModel) -> IActionResult [POST]
  # バリデーション → UpdateEmployeeDto へ変換 → EmployeeAppService.UpdateAsync() → Redirect
- Delete(id) -> IActionResult [GET]
  # 削除確認画面表示
- DeleteConfirmed(id) -> IActionResult [POST]
  # EmployeeAppService.DeleteAsync() → Redirect
```

### DepartmentsController
```
# EmployeesController と同パターン。DepartmentAppService を使用。
# FormModel: DepartmentFormModel
# ViewModel: DepartmentListViewModel
```

### WorkTypesController
```
# EmployeesController と同パターン（Delete なし）。WorkTypeAppService を使用。
# FormModel: WorkTypeFormModel
# ViewModel: WorkTypeListViewModel
```

### AttendanceController
```
- Index(AttendanceSearchFormModel) -> IActionResult
  # AttendanceSearchFormModel → AttendanceSearchCriteria 変換
  # AttendanceAppService.GetByDateRangeAsync() → AttendanceListViewModel へ変換 → View返却
- Stamp() -> IActionResult [GET]
  # 現在の打刻状態を取得 → StampViewModel へ変換 → View返却
- Stamp(stampType) -> IActionResult [POST]
  # AttendanceAppService.StampAsync() → Redirect
- Edit(recordId) -> IActionResult [GET]
  # AttendanceAppService.GetByEmployeeAndDateAsync() → AttendanceEditFormModel へ変換 → View返却
- Edit(recordId, AttendanceEditFormModel) -> IActionResult [POST]
  # バリデーション → DTO変換 → AttendanceAppService.UpdateRecordAsync() → Redirect
```

### MonthlyReportController
```
- Index(year?, month?, departmentId?) -> IActionResult
  # YearMonth構築 → MonthlyAttendanceAppService.GetMonthlySummariesAsync()
  # → MonthlyReportViewModel へ変換 → View返却
```

### HomeController
```
- Index() -> IActionResult
  # ダッシュボード表示
```

---

## Infrastructure 層

### リポジトリ実装（共通パターン）
各リポジトリは対応するインターフェースを実装し、EF Core を使用してデータアクセスを行う。

### MonthlyAttendanceQueryService
```
- GetMonthlyDataAsync(yearMonth, departmentId?) -> List<MonthlyAttendanceRawData>
  # Employees JOIN EmployeeDepartments JOIN Departments JOIN AttendanceRecords
  #   JOIN TimeStampEntries JOIN WorkTypes
  # 複数テーブルをJOINして生データを返す（ドメインロジックは持たない）
```

### AppDbContext
```
- DbSet<Employee> Employees
- DbSet<Department> Departments
- DbSet<EmployeeDepartment> EmployeeDepartments
- DbSet<AttendanceRecord> AttendanceRecords
- DbSet<TimeStampEntry> TimeStampEntries
- DbSet<WorkType> WorkTypes
- OnModelCreating(modelBuilder) # Fluent API設定
```
