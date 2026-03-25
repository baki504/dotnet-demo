# サービス定義

## サービス一覧

### UseCase 実装（Application/UseCases/）

Input Port を実装し、Domain のエンティティ・サービスと Output Port を組み合わせてユースケースを実行する。

| サービス | 実装するPort | 責務 |
|---------|-------------|------|
| **CreateEmployeeService** | ICreateEmployeeUseCase | 社員登録（番号・メールのユニークチェック、部署所属設定） |
| **UpdateEmployeeService** | IUpdateEmployeeUseCase | 社員情報更新 |
| **DeleteEmployeeService** | IDeleteEmployeeUseCase | 社員論理削除 |
| **GetEmployeeService** | IGetEmployeeUseCase | 社員取得・一覧 |
| **ManageDepartmentService** | IManageDepartmentUseCase | 部署CRUD（所属社員チェック付き削除） |
| **AssignDepartmentService** | IAssignDepartmentUseCase | 部署所属の追加・削除（兼務対応） |
| **StampAttendanceService** | IStampAttendanceUseCase | 打刻登録（AttendanceDomainService で検証） |
| **GetAttendanceService** | IGetAttendanceUseCase | 勤怠一覧・検索 |
| **UpdateAttendanceService** | IUpdateAttendanceUseCase | 勤怠編集（Admin権限） |
| **ManageWorkTypeService** | IManageWorkTypeUseCase | 勤務区分CRUD（使用中チェック付き削除） |
| **GetMonthlySummaryService** | IGetMonthlySummaryUseCase | 月次サマリー取得（MonthlyAttendanceDomainService でモデル構築） |

### ドメインサービス（Domain/Services/）

エンティティ単体では表現できないビジネスルールを実装する。

| サービス | 責務 | オーケストレーション |
|---------|------|-------------------|
| **AttendanceDomainService** | 打刻ルールの整合性検証 | 打刻種別の順序検証、二重打刻防止 |
| **MonthlyAttendanceDomainService** | 月次勤怠サマリーの構築 | 複数テーブルの生データからリッチドメインモデル(MonthlyAttendanceSummary)を組み立て |

---

## サービス間相互作用

### 打刻フロー
```
[Adapters/In/Web/AttendanceController.Stamp(POST)]
    -> IStampAttendanceUseCase.ExecuteAsync()                   # Input Port 経由
        -> StampAttendanceService                               # Application/UseCases
            -> IAttendanceRepository.GetByEmployeeAndDate()     # Output Port 経由
            -> AttendanceDomainService.ValidateTimeStamp()      # Domain/Services
            -> AttendanceRecord.AddTimeStamp()                  # Domain/Entities
            -> IAttendanceRepository.AddAsync/UpdateAsync()     # Output Port 経由
```

### 社員登録フロー
```
[Adapters/In/Web/EmployeesController.Create(POST)]
    -> ICreateEmployeeUseCase.ExecuteAsync()                    # Input Port 経由
        -> CreateEmployeeService                                # Application/UseCases
            -> IEmployeeRepository.ExistsByEmployeeNumberAsync()# Output Port 経由
            -> IEmployeeRepository.ExistsByEmailAsync()         # Output Port 経由
            -> IDepartmentRepository.GetByIdAsync()             # Output Port 経由
            -> Employee.Create()                                # Domain/Entities
            -> Employee.AddDepartment()                         # Domain/Entities
            -> IEmployeeRepository.AddAsync()                   # Output Port 経由
```

### 月次勤怠サマリー取得フロー（リッチドメインモデル）
```
[Adapters/In/Web/MonthlyReportController.Index(GET)]
    -> IGetMonthlySummaryUseCase.GetSummariesAsync()            # Input Port 経由
        -> GetMonthlySummaryService                             # Application/UseCases
            -> IMonthlyAttendanceQueryService.GetMonthlyDataAsync() # Output Port 経由
               # 6テーブルJOIN（Adapters/Out で実装）
            -> MonthlyAttendanceDomainService.BuildSummaries()  # Domain/Services
               # 生データ → MonthlyAttendanceSummary（ビジネスロジック）
            -> DTO変換                                           # Application/DTOs
```

### 勤怠検索フロー
```
[Adapters/In/Web/AttendanceController.Index(GET)]
    -> IGetAttendanceUseCase.GetByDateRangeAsync()              # Input Port 経由
        -> GetAttendanceService                                 # Application/UseCases
            -> IAttendanceRepository.SearchAsync(criteria)      # Output Port 経由
            -> DTO変換                                           # Application/DTOs
```

---

## DI（依存性注入）設定

```
# Input Ports -> UseCase 実装 (Application/UseCases)
services.AddScoped<ICreateEmployeeUseCase, CreateEmployeeService>();
services.AddScoped<IUpdateEmployeeUseCase, UpdateEmployeeService>();
services.AddScoped<IDeleteEmployeeUseCase, DeleteEmployeeService>();
services.AddScoped<IGetEmployeeUseCase, GetEmployeeService>();
services.AddScoped<IManageDepartmentUseCase, ManageDepartmentService>();
services.AddScoped<IAssignDepartmentUseCase, AssignDepartmentService>();
services.AddScoped<IStampAttendanceUseCase, StampAttendanceService>();
services.AddScoped<IGetAttendanceUseCase, GetAttendanceService>();
services.AddScoped<IUpdateAttendanceUseCase, UpdateAttendanceService>();
services.AddScoped<IManageWorkTypeUseCase, ManageWorkTypeService>();
services.AddScoped<IGetMonthlySummaryUseCase, GetMonthlySummaryService>();

# Domain Services
services.AddScoped<AttendanceDomainService>();
services.AddScoped<MonthlyAttendanceDomainService>();

# Output Ports -> Adapter 実装 (Adapters/Out/Persistence)
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IDepartmentRepository, DepartmentRepository>();
services.AddScoped<IAttendanceRepository, AttendanceRepository>();
services.AddScoped<IWorkTypeRepository, WorkTypeRepository>();
services.AddScoped<IMonthlyAttendanceQueryService, MonthlyAttendanceQueryService>();
services.AddDbContext<AppDbContext>(options => ...);
```
