# Code Generation Plan - hr-attendance (Hexagonal Architecture)

## ユニットコンテキスト

- **プロジェクトタイプ**: Greenfield
- **ワークスペースルート**: /Users/takadayu/dev/gitrepo/dotnet-demo
- **アーキテクチャ**: ヘキサゴナルアーキテクチャ（Ports & Adapters）
- **技術スタック**: .NET 10, ASP.NET Core MVC, EF Core, SQL Server (Docker)
- **認証**: OAuth2 / OpenID Connect
- **開発手法**: TDD（テスト駆動開発）- テストを先に書き、実装はテストを通すために書く

## TDD サイクル（テストクラス単位で繰り返す）

1. **Red**: テストクラスを1つ書く → コンパイルエラー or テスト失敗 → **人間レビュー依頼**（承認後に次へ）
2. **Green → Refactor**: テストを通す実装 → セルフレビュー → 改善 → テスト再実行（納得いくまで繰り返す）
3. **Lint & Format**: `dotnet format` 実行
4. **人間レビュー依頼**（承認後に次のテストクラスへ）

## ソリューション構成（ヘキサゴナル）

```
/Users/takadayu/dev/gitrepo/dotnet-demo/
+-- HrAttendance.slnx
+-- HrAttendance/                          (HrAttendance.csproj - Web SDK)
|   +-- Domain/                            (名前空間: HrAttendance.Domain)
|   |   +-- Entities/
|   |   +-- ValueObjects/                  ← 既存5ファイル（変更不要）
|   |   +-- Services/                      DomainService のみ
|   +-- Application/                       (名前空間: HrAttendance.Application)
|   |   +-- Ports/
|   |   |   +-- In/                        Input Ports（UseCase IF）
|   |   |   +-- Out/                       Output Ports（Repository IF）
|   |   +-- UseCases/                      UseCase 実装
|   |   +-- DTOs/
|   +-- Adapters/                          (名前空間: HrAttendance.Adapters)
|   |   +-- In/
|   |   |   +-- Web/
|   |   |       +-- Controllers/
|   |   |       +-- ViewModels/
|   |   |       +-- FormModels/
|   |   |       +-- Views/
|   |   +-- Out/
|   |       +-- Persistence/
|   |           +-- Repositories/
|   |           +-- Configurations/
|   +-- Program.cs
+-- HrAttendanceTests/                     (HrAttendanceTests.csproj - xUnit)
|   +-- Domain/                            ← 既存テスト5ファイル（変更不要）
|   |   +-- ValueObjects/
|   |   +-- Entities/
|   |   +-- Services/
|   +-- Application/
|       +-- UseCases/
+-- docker-compose.yml
```

## 機能トレーサビリティ

| 機能要件 | 実装ステップ |
|---------|------------|
| FR-01: マスタCRUD | Step 2-6, 8-9, 11-13 |
| FR-02: 勤怠データ管理 | Step 2-6, 8-9, 11-13 |
| FR-03: 一覧・検索 | Step 5-6, 8-9, 11-13 |
| FR-04: 月次勤怠サマリー | Step 2-6, 8-9, 11-13 |

---

## 実行ステップ

### Step 1: プロジェクト構造セットアップ
- [x] ソリューション (HrAttendance.slnx) 作成
- [x] HrAttendance プロジェクト作成 (Web SDK)
- [x] HrAttendanceTests プロジェクト作成 (xUnit)
- [x] プロジェクト参照の設定 (HrAttendanceTests → HrAttendance)
- [x] docker-compose.yml 作成 (SQL Server - Azure SQL Edge)
- [x] .gitignore 更新
- [ ] ヘキサゴナル用ディレクトリ構造作成（Application/Ports/In, Application/Ports/Out, Application/UseCases, Application/DTOs, Adapters/In/Web/Controllers, Adapters/In/Web/ViewModels, Adapters/In/Web/FormModels, Adapters/In/Web/Views, Adapters/Out/Persistence/Repositories, Adapters/Out/Persistence/Configurations）

### Step 2: Domain層 TDD - 値オブジェクト・Enum（Red → Green → Refactor）
- [x] **RED→GREEN**: EmployeeId テスト・実装（空Guid禁止） ※完了済み
- [x] **RED→GREEN**: DepartmentCode テスト・実装（空文字禁止、形式チェック） ※完了済み
- [x] **RED→GREEN**: WorkDuration テスト・実装（演算、比較、ゼロ値、FromMinutes） ※完了済み
- [x] **RED→GREEN**: YearMonth テスト・実装（範囲チェック、日付変換、ToString） ※完了済み
- [x] **RED→GREEN**: WorkSession テスト・実装（Duration計算、ClockOut null時） ※完了済み
- [x] Role enum / TimeStampType enum 作成（テスト不要のシンプルenum）
- [x] **REFACTOR**: 値オブジェクト全体のコード整理（セルフレビュー実施、改善不要と判断）
- [x] テスト実行・全パス確認（55テスト全パス、警告0）

### Step 3: Domain層 TDD - エンティティ（Red → Green → Refactor）
- [x] **RED**: Employee テスト作成（Create、UpdateInfo、ChangeRole、論理削除、兼務ルール：AddDepartment/RemoveDepartment/SetPrimaryDepartment、最低1部署制約）
- [x] **GREEN**: Employee エンティティ実装（28テスト全パス）
- [x] EmployeeDepartment エンティティ作成（Employee経由で検証済み）
- [x] **RED**: Department テスト作成（Create、UpdateName）
- [x] **GREEN**: Department エンティティ実装（9テスト全パス）
- [x] **RED**: AttendanceRecord テスト作成（Create、AddTimeStamp打刻順序、二重打刻防止、ChangeWorkType）
- [x] **GREEN**: AttendanceRecord エンティティ実装（15テスト全パス）
- [x] TimeStampEntry エンティティ作成（AttendanceRecord経由で検証済み）
- [x] **RED**: WorkType テスト作成（Create、UpdateName）
- [x] **GREEN**: WorkType エンティティ実装（11テスト全パス）
- [x] **REFACTOR**: エンティティ全体のコード整理（セルフレビュー実施、改善不要と判断）
- [x] テスト実行・全パス確認（142テスト全パス、警告0）

### Step 4: Domain層 TDD - ドメインサービス・リッチドメインモデル（Red → Green → Refactor）
- [ ] **RED**: AttendanceDomainService テスト作成（打刻順序検証、二重打刻防止、複数セット時間重複チェック）
- [ ] **GREEN**: AttendanceDomainService 実装
- [ ] **RED**: MonthlyAttendanceSummary テスト作成（GetTotalWorkDays、GetTotalWorkDuration、GetOvertimeDuration、GetPaidLeaveDays、GetAbsentDays、GetWorkDaysByType）
- [ ] **GREEN**: MonthlyAttendanceSummary 実装（Build ファクトリメソッド含む）
- [ ] **RED**: MonthlyAttendanceDomainService テスト作成（BuildSummary、BuildSummaries）
- [ ] **GREEN**: MonthlyAttendanceDomainService 実装
- [ ] **REFACTOR**: ドメインサービス全体のコード整理
- [ ] テスト実行・全パス確認

### Step 5: Application層 - Port定義・DTO
- [ ] Output Ports 作成（Application/Ports/Out/）
  - IEmployeeRepository, IDepartmentRepository, IAttendanceRepository, IWorkTypeRepository, IMonthlyAttendanceQueryService
- [ ] Input Ports 作成（Application/Ports/In/）
  - ICreateEmployeeUseCase, IUpdateEmployeeUseCase, IDeleteEmployeeUseCase, IGetEmployeeUseCase
  - IManageDepartmentUseCase, IAssignDepartmentUseCase
  - IStampAttendanceUseCase, IGetAttendanceUseCase, IUpdateAttendanceUseCase
  - IManageWorkTypeUseCase, IGetMonthlySummaryUseCase
- [ ] DTO 作成（Application/DTOs/）
  - EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto
  - DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto
  - AttendanceRecordDto, TimeStampEntryDto, UpdateAttendanceDto
  - WorkTypeDto, CreateWorkTypeDto
  - AttendanceSearchCriteria
  - MonthlyAttendanceSummaryDto, MonthlyAttendanceRawData

### Step 6: Application層 TDD - UseCase実装（Red → Green → Refactor）
- [ ] **RED**: CreateEmployeeService テスト作成（社員番号バリデーション、ユニークチェック、部署所属設定）※ Output Port はモック
- [ ] **GREEN**: CreateEmployeeService 実装
- [ ] **RED**: GetEmployeeService テスト作成（一覧取得、ID検索）
- [ ] **GREEN**: GetEmployeeService 実装
- [ ] **RED**: UpdateEmployeeService テスト作成
- [ ] **GREEN**: UpdateEmployeeService 実装
- [ ] **RED**: DeleteEmployeeService テスト作成（論理削除）
- [ ] **GREEN**: DeleteEmployeeService 実装
- [ ] **RED**: AssignDepartmentService テスト作成（兼務対応）
- [ ] **GREEN**: AssignDepartmentService 実装
- [ ] **RED**: ManageDepartmentService テスト作成（CRUD、削除時の所属社員チェック）
- [ ] **GREEN**: ManageDepartmentService 実装
- [ ] **RED**: StampAttendanceService テスト作成（打刻フロー）
- [ ] **GREEN**: StampAttendanceService 実装
- [ ] **RED**: GetAttendanceService テスト作成（権限チェック、検索）
- [ ] **GREEN**: GetAttendanceService 実装
- [ ] **RED**: UpdateAttendanceService テスト作成（Admin権限、整合性チェック）
- [ ] **GREEN**: UpdateAttendanceService 実装
- [ ] **RED**: ManageWorkTypeService テスト作成（CRUD、使用中削除禁止）
- [ ] **GREEN**: ManageWorkTypeService 実装
- [ ] **RED**: GetMonthlySummaryService テスト作成（サマリー取得、権限チェック）
- [ ] **GREEN**: GetMonthlySummaryService 実装
- [ ] **REFACTOR**: UseCase全体のコード整理
- [ ] テスト実行・全パス確認

### Step 7: Adapters/Out - DbContext・マッピング
- [ ] AppDbContext（DbSet、OnModelCreating）
- [ ] EF Core Entity Configurations（各エンティティのFluent APIマッピング）
- [ ] シードデータ（WorkType初期データ5種）

### Step 8: Adapters/Out - リポジトリ・クエリサービス（Output Port 実装）
- [ ] EmployeeRepository : IEmployeeRepository（論理削除フィルタ含む）
- [ ] DepartmentRepository : IDepartmentRepository
- [ ] AttendanceRepository : IAttendanceRepository
- [ ] WorkTypeRepository : IWorkTypeRepository
- [ ] MonthlyAttendanceQueryService : IMonthlyAttendanceQueryService（複数テーブルJOINクエリ）

### Step 9: Adapters/In/Web - ViewModel・FormModel
- [ ] DashboardViewModel / TodayAttendanceInfo / MonthlySummaryInfo
- [ ] EmployeeListViewModel / EmployeeDetailViewModel / EmployeeFormModel
- [ ] DepartmentListViewModel / DepartmentFormModel
- [ ] WorkTypeListViewModel / WorkTypeFormModel
- [ ] AttendanceListViewModel / StampViewModel / AttendanceEditFormModel / AttendanceSearchFormModel
- [ ] MonthlyReportViewModel / MonthlyReportSearchFormModel

### Step 10: Adapters/In/Web - Controller（Input Port 経由で UseCase 呼び出し）
- [ ] HomeController（ダッシュボード）
- [ ] EmployeesController（ICreateEmployeeUseCase, IGetEmployeeUseCase, IUpdateEmployeeUseCase, IDeleteEmployeeUseCase を注入）
- [ ] DepartmentsController（IManageDepartmentUseCase を注入）
- [ ] WorkTypesController（IManageWorkTypeUseCase を注入）
- [ ] AttendanceController（IStampAttendanceUseCase, IGetAttendanceUseCase, IUpdateAttendanceUseCase を注入）
- [ ] MonthlyReportController（IGetMonthlySummaryUseCase を注入）

### Step 11: Adapters/In/Web - Razor View・レイアウト
- [ ] _Layout.cshtml（ナビゲーション、権限別メニュー）
- [ ] Home/Index.cshtml（ダッシュボード）
- [ ] Employees/（Index, Create, Edit, Details, Delete）
- [ ] Departments/（Index, Create, Edit, Delete）
- [ ] WorkTypes/（Index, Create, Edit）
- [ ] Attendance/（Index, Stamp, Edit）
- [ ] MonthlyReport/Index.cshtml
- [ ] _ValidationScriptsPartial.cshtml

### Step 12: DI設定・認証・スタートアップ
- [ ] Program.cs（Input Port → UseCase実装、Output Port → Repository実装 のDI登録、認証設定、DbContext設定）
- [ ] appsettings.json（接続文字列、認証設定）
- [ ] appsettings.Development.json

### Step 13: ドキュメント生成
- [ ] aidlc-docs/construction/hr-attendance/code/code-summary.md

---

## 合計: 13ステップ
## TDD対象: Step 2, 3, 4（Domain層）, Step 6（Application/UseCases層）
## 各TDDステップ終了時にテスト実行・全パス確認を行う
## 既存コード（Value Objects 5つ + テスト5つ）はそのまま流用（ディレクトリ・名前空間変更なし）
