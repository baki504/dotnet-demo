# Code Generation Plan - hr-attendance

## ユニットコンテキスト

- **プロジェクトタイプ**: Greenfield
- **ワークスペースルート**: /Users/takadayu/dev/gitrepo/amv/dotnet-demo
- **アーキテクチャ**: DDD 4層（Domain / Application / Infrastructure / Web）
- **技術スタック**: .NET 10, ASP.NET Core MVC, EF Core, SQL Server (Docker)
- **認証**: OAuth2 / OpenID Connect
- **開発手法**: TDD（テスト駆動開発）- テストを先に書き、実装はテストを通すために書く

## TDD サイクル（テストクラス単位で繰り返す）

1. **Red**: テストクラスを1つ書く → コンパイルエラー or テスト失敗 → **人間レビュー依頼**（承認後に次へ）
2. **Green → Refactor**: テストを通す実装 → セルフレビュー → 改善 → テスト再実行（納得いくまで繰り返す）
3. **Lint & Format**: `dotnet format` 実行
4. **人間レビュー依頼**（承認後に次のテストクラスへ）

## ソリューション構成

```
/Users/takadayu/dev/gitrepo/dotnet-demo/
+-- HrAttendance.slnx
+-- HrAttendance/               (HrAttendance.csproj - Web SDK)
|   +-- Domain/                 (名前空間: HrAttendance.Domain)
|   |   +-- ValueObjects/
|   |   +-- Entities/
|   |   +-- Services/
|   +-- Application/            (名前空間: HrAttendance.Application)
|   +-- Infrastructure/         (名前空間: HrAttendance.Infrastructure)
|   +-- Web/                    (名前空間: HrAttendance.Web)
+-- HrAttendanceTests/          (HrAttendanceTests.csproj - xUnit)
|   +-- Domain/                 (名前空間: HrAttendanceTests.Domain)
|   +-- Application/            (名前空間: HrAttendanceTests.Application)
+-- docker-compose.yml
```

## 機能トレーサビリティ

| 機能要件 | 実装ステップ |
|---------|------------|
| FR-01: マスタCRUD | Step 2-5, 7-8, 10-12 |
| FR-02: 勤怠データ管理 | Step 2-5, 7-8, 10-12 |
| FR-03: 一覧・検索 | Step 4-5, 7-8, 10-12 |
| FR-04: 月次勤怠サマリー | Step 2-5, 7-8, 10-12 |

---

## 実行ステップ

### Step 1: プロジェクト構造セットアップ
- [x] ソリューション (HrAttendance.slnx) 作成 ※.NET 10では.slnx形式
- [x] HrAttendance プロジェクト作成 (Web SDK, Domain/Application/Infrastructure/Webはフォルダで分離)
- [x] HrAttendanceTests プロジェクト作成 (xunit, Domain/Applicationはフォルダで分離)
- [x] プロジェクト参照の設定 (HrAttendanceTests → HrAttendance)
- [x] docker-compose.yml 作成 (SQL Server - Azure SQL Edge)
- [x] .gitignore 更新 (bin/, obj/ 除外)

### Step 2: Domain層 TDD - 値オブジェクト・Enum（Red → Green → Refactor）
- [ ] **RED**: WorkDuration テスト作成（演算、比較、ゼロ値、FromMinutes）
- [ ] **GREEN**: WorkDuration 実装
- [ ] **RED**: YearMonth テスト作成（範囲チェック、日付変換、ToString）
- [ ] **GREEN**: YearMonth 実装
- [ ] **RED**: WorkSession テスト作成（Duration計算、ClockOut null時）
- [ ] **GREEN**: WorkSession 実装
- [ ] Role enum / TimeStampType enum 作成（テスト不要のシンプルenum）
- [ ] **RED**: EmployeeId テスト作成（空Guid禁止）
- [ ] **GREEN**: EmployeeId 実装
- [ ] **RED**: DepartmentCode テスト作成（空文字禁止、形式チェック）
- [ ] **GREEN**: DepartmentCode 実装
- [ ] **REFACTOR**: 値オブジェクト全体のコード整理
- [ ] テスト実行・全パス確認

### Step 3: Domain層 TDD - エンティティ（Red → Green → Refactor）
- [ ] **RED**: Employee テスト作成（Create、UpdateInfo、ChangeRole、論理削除、兼務ルール：AddDepartment/RemoveDepartment/SetPrimaryDepartment、最低1部署制約）
- [ ] **GREEN**: Employee エンティティ実装
- [ ] **RED**: Department テスト作成（Create、UpdateName）
- [ ] **GREEN**: Department エンティティ実装
- [ ] EmployeeDepartment エンティティ作成（テストはEmployee経由で検証済み）
- [ ] **RED**: AttendanceRecord テスト作成（Create、AddTimeStamp打刻順序、二重打刻防止、ChangeWorkType）
- [ ] **GREEN**: AttendanceRecord エンティティ実装
- [ ] TimeStampEntry エンティティ作成
- [ ] **RED**: WorkType テスト作成（Create、UpdateName）
- [ ] **GREEN**: WorkType エンティティ実装
- [ ] **REFACTOR**: エンティティ全体のコード整理
- [ ] テスト実行・全パス確認

### Step 4: Domain層 TDD - ドメインサービス・リッチドメインモデル（Red → Green → Refactor）
- [ ] リポジトリインターフェース作成（IEmployeeRepository, IDepartmentRepository, IAttendanceRepository, IWorkTypeRepository, IMonthlyAttendanceQueryService）
- [ ] **RED**: AttendanceDomainService テスト作成（打刻順序検証、二重打刻防止、複数セット時間重複チェック）
- [ ] **GREEN**: AttendanceDomainService 実装
- [ ] **RED**: MonthlyAttendanceSummary テスト作成（GetTotalWorkDays、GetTotalWorkDuration、GetOvertimeDuration、GetPaidLeaveDays、GetAbsentDays、GetWorkDaysByType）
- [ ] **GREEN**: MonthlyAttendanceSummary 実装（Build ファクトリメソッド含む）
- [ ] **RED**: MonthlyAttendanceDomainService テスト作成（BuildSummary、BuildSummaries）
- [ ] **GREEN**: MonthlyAttendanceDomainService 実装
- [ ] **REFACTOR**: ドメインサービス全体のコード整理
- [ ] テスト実行・全パス確認

### Step 5: Application層 TDD - DTO・サービス（Red → Green → Refactor）
- [ ] DTO作成（EmployeeDto, CreateEmployeeDto, UpdateEmployeeDto, DepartmentDto, CreateDepartmentDto, UpdateDepartmentDto, AttendanceRecordDto, TimeStampEntryDto, WorkTypeDto, CreateWorkTypeDto, AttendanceSearchCriteria, MonthlyAttendanceSummaryDto, MonthlyAttendanceRawData）
- [ ] **RED**: EmployeeAppService テスト作成（CRUD、論理削除、社員番号バリデーション、ユニークチェック、部署所属管理）※ リポジトリはモック
- [ ] **GREEN**: EmployeeAppService 実装
- [ ] **RED**: DepartmentAppService テスト作成（CRUD、削除時の所属社員チェック）
- [ ] **GREEN**: DepartmentAppService 実装
- [ ] **RED**: AttendanceAppService テスト作成（打刻フロー、権限チェック、勤怠一覧取得、編集）
- [ ] **GREEN**: AttendanceAppService 実装
- [ ] **RED**: WorkTypeAppService テスト作成（CRUD、使用中削除禁止）
- [ ] **GREEN**: WorkTypeAppService 実装
- [ ] **RED**: MonthlyAttendanceAppService テスト作成（サマリー取得、権限チェック）
- [ ] **GREEN**: MonthlyAttendanceAppService 実装
- [ ] **REFACTOR**: Application層全体のコード整理
- [ ] テスト実行・全パス確認

### Step 6: Infrastructure層 - DbContext・マッピング
- [ ] AppDbContext（DbSet、OnModelCreating）
- [ ] EF Core Entity Configurations（各エンティティのFluent APIマッピング）
- [ ] シードデータ（WorkType初期データ5種）

### Step 7: Infrastructure層 - リポジトリ・クエリサービス
- [ ] EmployeeRepository（論理削除フィルタ含む）
- [ ] DepartmentRepository
- [ ] AttendanceRepository
- [ ] WorkTypeRepository
- [ ] MonthlyAttendanceQueryService（複数テーブルJOINクエリ）

### Step 8: Web層 - ViewModel・FormModel
- [ ] DashboardViewModel / TodayAttendanceInfo / MonthlySummaryInfo
- [ ] EmployeeListViewModel / EmployeeDetailViewModel / EmployeeFormModel
- [ ] DepartmentListViewModel / DepartmentFormModel
- [ ] WorkTypeListViewModel / WorkTypeFormModel
- [ ] AttendanceListViewModel / StampViewModel / AttendanceEditFormModel / AttendanceSearchFormModel
- [ ] MonthlyReportViewModel / MonthlyReportSearchFormModel

### Step 9: Web層 - Controller
- [ ] HomeController（ダッシュボード）
- [ ] EmployeesController（CRUD）
- [ ] DepartmentsController（CRUD）
- [ ] WorkTypesController（CRUD）
- [ ] AttendanceController（一覧・打刻・編集）
- [ ] MonthlyReportController（月次サマリー）

### Step 10: Web層 - Razor View・レイアウト
- [ ] _Layout.cshtml（ナビゲーション、権限別メニュー）
- [ ] Home/Index.cshtml（ダッシュボード）
- [ ] Employees/（Index, Create, Edit, Details, Delete）
- [ ] Departments/（Index, Create, Edit, Delete）
- [ ] WorkTypes/（Index, Create, Edit）
- [ ] Attendance/（Index, Stamp, Edit）
- [ ] MonthlyReport/Index.cshtml
- [ ] _ValidationScriptsPartial.cshtml

### Step 11: Web層 - DI設定・認証・スタートアップ
- [ ] Program.cs（DI登録、認証設定、DbContext設定）
- [ ] appsettings.json（接続文字列、認証設定）
- [ ] appsettings.Development.json

### Step 12: ドキュメント生成
- [ ] aidlc-docs/construction/hr-attendance/code/code-summary.md

---

## 合計: 12ステップ
## TDD対象: Step 2, 3, 4（Domain層）, Step 5（Application層）
## 各TDDステップ終了時にテスト実行・全パス確認を行う
