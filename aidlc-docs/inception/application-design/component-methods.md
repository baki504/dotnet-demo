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
- Employee.Delete() -> void  # IsDeleted = true
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

### MonthlyAttendanceSummary（リッチドメインモデル）
```
- MonthlyAttendanceSummary.Build(employee, departmentName, records, workTypes) -> MonthlyAttendanceSummary
- GetTotalWorkDays() -> int
- GetTotalWorkDuration() -> WorkDuration
- GetOvertimeDuration(standardHoursPerDay: WorkDuration) -> WorkDuration
- GetPaidLeaveDays() -> int
- GetAbsentDays() -> int
- GetWorkDaysByType() -> Dictionary<string, int>
```

### WorkDuration（値オブジェクト）
```
- WorkDuration.FromMinutes(minutes) -> WorkDuration
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

### WorkSession（値オブジェクト）
```
- WorkSession.Create(clockIn, clockOut) -> WorkSession
- Duration -> WorkDuration
```

### AttendanceDomainService
```
- ValidateTimeStamp(record, newStampType, timestamp) -> bool
  # 打刻順序の検証（出勤→退勤）
  # 二重打刻の防止
```

### MonthlyAttendanceDomainService
```
- BuildSummary(employee, departmentName, records, workTypes) -> MonthlyAttendanceSummary
- BuildSummaries(queryResults) -> List<MonthlyAttendanceSummary>
```

---

## Application/Ports/In（Input Ports）

### ICreateEmployeeUseCase
```
- ExecuteAsync(dto: CreateEmployeeDto) -> EmployeeDto
```

### IUpdateEmployeeUseCase
```
- ExecuteAsync(id: Guid, dto: UpdateEmployeeDto) -> EmployeeDto
```

### IDeleteEmployeeUseCase
```
- ExecuteAsync(id: Guid) -> void
```

### IGetEmployeeUseCase
```
- GetAllAsync(searchTerm?) -> List<EmployeeDto>
- GetByIdAsync(id: Guid) -> EmployeeDto?
```

### IManageDepartmentUseCase
```
- GetAllAsync(searchTerm?) -> List<DepartmentDto>
- GetByIdAsync(id: Guid) -> DepartmentDto?
- CreateAsync(dto: CreateDepartmentDto) -> DepartmentDto
- UpdateAsync(id: Guid, dto: UpdateDepartmentDto) -> DepartmentDto
- DeleteAsync(id: Guid) -> void
```

### IAssignDepartmentUseCase
```
- AssignAsync(employeeId: Guid, departmentId: Guid, isPrimary: bool) -> void
- RemoveAsync(employeeId: Guid, departmentId: Guid) -> void
```

### IStampAttendanceUseCase
```
- ExecuteAsync(employeeId: Guid, stampType: TimeStampType, timestamp?: DateTime) -> TimeStampEntryDto
```

### IGetAttendanceUseCase
```
- GetByDateRangeAsync(criteria: AttendanceSearchCriteria) -> List<AttendanceRecordDto>
- GetByEmployeeAndDateAsync(employeeId: Guid, date: DateOnly) -> AttendanceRecordDto?
```

### IUpdateAttendanceUseCase
```
- ExecuteAsync(recordId: Guid, dto: UpdateAttendanceDto) -> AttendanceRecordDto
```

### IManageWorkTypeUseCase
```
- GetAllAsync() -> List<WorkTypeDto>
- GetByIdAsync(id: Guid) -> WorkTypeDto?
- CreateAsync(dto: CreateWorkTypeDto) -> WorkTypeDto
- UpdateAsync(id: Guid, dto: CreateWorkTypeDto) -> WorkTypeDto
- DeleteAsync(id: Guid) -> void
```

### IGetMonthlySummaryUseCase
```
- GetSummariesAsync(yearMonth: YearMonth, departmentId?: Guid) -> List<MonthlyAttendanceSummaryDto>
- GetSummaryByEmployeeAsync(employeeId: Guid, yearMonth: YearMonth) -> MonthlyAttendanceSummaryDto?
```

---

## Application/Ports/Out（Output Ports）

### IEmployeeRepository
```
- GetAllAsync(searchTerm?, includeDeleted?: bool) -> List<Employee>
- GetByIdAsync(id: Guid) -> Employee?
- AddAsync(employee: Employee) -> void
- UpdateAsync(employee: Employee) -> void
- ExistsByEmployeeNumberAsync(employeeNumber: string) -> bool
- ExistsByEmailAsync(email: string) -> bool
```

### IDepartmentRepository
```
- GetAllAsync(searchTerm?) -> List<Department>
- GetByIdAsync(id: Guid) -> Department?
- AddAsync(department: Department) -> void
- UpdateAsync(department: Department) -> void
- DeleteAsync(department: Department) -> void
- HasEmployeesAsync(id: Guid) -> bool
```

### IAttendanceRepository
```
- GetByIdAsync(id: Guid) -> AttendanceRecord?
- GetByEmployeeAndDateAsync(employeeId: Guid, date: DateOnly) -> List<AttendanceRecord>
- SearchAsync(criteria: AttendanceSearchCriteria) -> List<AttendanceRecord>
- AddAsync(record: AttendanceRecord) -> void
- UpdateAsync(record: AttendanceRecord) -> void
```

### IWorkTypeRepository
```
- GetAllAsync() -> List<WorkType>
- GetByIdAsync(id: Guid) -> WorkType?
- GetByCodeAsync(code: string) -> WorkType?
- AddAsync(workType: WorkType) -> void
- UpdateAsync(workType: WorkType) -> void
- DeleteAsync(workType: WorkType) -> void
- HasAttendanceRecordsAsync(id: Guid) -> bool
```

### IMonthlyAttendanceQueryService
```
- GetMonthlyDataAsync(yearMonth: YearMonth, departmentId?: Guid) -> List<MonthlyAttendanceRawData>
```

---

## Adapters/In/Web（Controller）

### EmployeesController
```
- Index(searchTerm?) -> IActionResult
- Details(id) -> IActionResult
- Create() -> IActionResult [GET]
- Create(EmployeeFormModel) -> IActionResult [POST]
- Edit(id) -> IActionResult [GET]
- Edit(id, EmployeeFormModel) -> IActionResult [POST]
- Delete(id) -> IActionResult [GET]
- DeleteConfirmed(id) -> IActionResult [POST]
```

### DepartmentsController
```
# EmployeesController と同パターン。IManageDepartmentUseCase を使用。
```

### WorkTypesController
```
# EmployeesController と同パターン（Delete なし）。IManageWorkTypeUseCase を使用。
```

### AttendanceController
```
- Index(AttendanceSearchFormModel) -> IActionResult
- Stamp() -> IActionResult [GET]
- Stamp(stampType) -> IActionResult [POST]
- Edit(recordId) -> IActionResult [GET]
- Edit(recordId, AttendanceEditFormModel) -> IActionResult [POST]
```

### MonthlyReportController
```
- Index(year?, month?, departmentId?) -> IActionResult
```

### HomeController
```
- Index() -> IActionResult
```

---

## Adapters/Out/Persistence

### リポジトリ実装（共通パターン）
各リポジトリは対応する Output Port インターフェースを実装し、EF Core を使用してデータアクセスを行う。

### MonthlyAttendanceQueryService
```
- GetMonthlyDataAsync(yearMonth, departmentId?) -> List<MonthlyAttendanceRawData>
  # Employees JOIN EmployeeDepartments JOIN Departments JOIN AttendanceRecords
  #   JOIN TimeStampEntries JOIN WorkTypes
```

### AppDbContext
```
- DbSet<Employee> Employees
- DbSet<Department> Departments
- DbSet<EmployeeDepartment> EmployeeDepartments
- DbSet<AttendanceRecord> AttendanceRecords
- DbSet<TimeStampEntry> TimeStampEntries
- DbSet<WorkType> WorkTypes
- OnModelCreating(modelBuilder)
```
