# サービス定義

## サービス一覧

### ドメインサービス

| サービス | 責務 | オーケストレーション |
|---------|------|-------------------|
| **AttendanceDomainService** | 打刻ルールの整合性検証 | 打刻種別の順序検証、二重打刻防止 |
| **MonthlyAttendanceDomainService** | 月次勤怠サマリーの構築 | 複数テーブルの生データからリッチドメインモデル(MonthlyAttendanceSummary)を組み立て |

### アプリケーションサービス

| サービス | 責務 | オーケストレーション |
|---------|------|-------------------|
| **EmployeeAppService** | 社員ライフサイクル管理 | Employee + EmployeeDepartment の作成・更新・削除を協調 |
| **DepartmentAppService** | 部署ライフサイクル管理 | Department のCRUD操作 |
| **AttendanceAppService** | 勤怠操作管理 | AttendanceDomainService で検証 → AttendanceRecord/TimeStampEntry の永続化 |
| **WorkTypeAppService** | 勤務区分管理 | WorkType のCRUD操作 |
| **MonthlyAttendanceAppService** | 月次勤怠サマリー | IMonthlyAttendanceQueryService → MonthlyAttendanceDomainService → DTO変換 |

---

## サービス間相互作用

### 打刻フロー
```
[AttendanceController.Stamp(POST)]
    -> AttendanceAppService.StampAsync()
        -> IAttendanceRepository.GetByEmployeeAndDate() # 当日レコード取得
        -> AttendanceDomainService.ValidateTimeStamp()  # 打刻検証
        -> AttendanceRecord.AddTimeStamp()              # エンティティに追加
        -> IAttendanceRepository.SaveAsync()            # 永続化
```

### 社員登録フロー
```
[EmployeesController.Create(POST)]
    -> EmployeeAppService.CreateAsync()
        -> Employee.Create()                            # エンティティ生成
        -> Employee.AddDepartment()                     # 部署所属設定
        -> IEmployeeRepository.AddAsync()               # 永続化
```

### 勤怠検索フロー
```
[AttendanceController.Index(GET)]
    -> AttendanceAppService.GetByDateRangeAsync()
        -> IAttendanceRepository.SearchAsync(criteria)  # 条件検索
        -> DTO変換                                       # エンティティ→DTO
```

### 月次勤怠サマリー取得フロー（リッチドメインモデル）
```
[MonthlyReportController.Index(GET)]
    -> MonthlyAttendanceAppService.GetMonthlySummariesAsync(yearMonth, departmentId?)
        -> IMonthlyAttendanceQueryService.GetMonthlyDataAsync()  # 複数テーブルJOIN（Infrastructure層）
           # Employees + Departments + AttendanceRecords + TimeStampEntries + WorkTypes
        -> MonthlyAttendanceDomainService.BuildSummaries()       # リッチドメインモデル構築（Domain層）
           # 生データ → MonthlyAttendanceSummary（労働時間計算、残業判定等のビジネスロジック）
        -> DTO変換                                                # MonthlyAttendanceSummary → Dto
    -> MonthlyReportViewModel へ変換 → View返却
```

**ポイント**: テーブルとドメインモデルが1:1ではない例。
- Infrastructure層: 複数テーブルをJOINして生データ（MonthlyAttendanceRawData）を返すだけ
- Domain層: 生データからリッチドメインモデルを構築し、ビジネスロジック（時間計算等）を実行
- Application層: 上記をオーケストレーションし、DTOに変換

---

## DI（依存性注入）設定

```
# Domain
services.AddScoped<AttendanceDomainService>();
services.AddScoped<MonthlyAttendanceDomainService>();

# Application
services.AddScoped<EmployeeAppService>();
services.AddScoped<DepartmentAppService>();
services.AddScoped<AttendanceAppService>();
services.AddScoped<WorkTypeAppService>();
services.AddScoped<MonthlyAttendanceAppService>();

# Infrastructure
services.AddScoped<IEmployeeRepository, EmployeeRepository>();
services.AddScoped<IDepartmentRepository, DepartmentRepository>();
services.AddScoped<IAttendanceRepository, AttendanceRepository>();
services.AddScoped<IWorkTypeRepository, WorkTypeRepository>();
services.AddScoped<IMonthlyAttendanceQueryService, MonthlyAttendanceQueryService>();
services.AddDbContext<AppDbContext>(options => ...);
```
