# コンポーネント定義

## プロジェクト構成（ヘキサゴナルアーキテクチャ）

```
HrAttendance.slnx
+-- HrAttendance/                              # プロダクションコード
|   +-- Domain/                                # コア（最内層）
|   |   +-- Entities/
|   |   +-- ValueObjects/
|   |   +-- Services/                          # DomainService のみ
|   +-- Application/                           # ユースケース層
|   |   +-- Ports/
|   |   |   +-- In/                            # Input Ports (ユースケースIF)
|   |   |   +-- Out/                           # Output Ports (リポジトリIF等)
|   |   +-- UseCases/                          # UseCase 実装
|   |   +-- DTOs/
|   +-- Adapters/                              # ヘキサゴンの外側
|   |   +-- In/Web/                            # Driving Adapter (MVC)
|   |   +-- Out/Persistence/                   # Driven Adapter (EF Core)
|   +-- Program.cs
+-- HrAttendanceTests/                         # テストコード
|   +-- Domain/
|   |   +-- ValueObjects/
|   |   +-- Entities/
|   |   +-- Services/
|   +-- Application/
|       +-- UseCases/
+-- docker-compose.yml
```

---

## Domain - コア（最内層）

**責務**: ビジネスルールとドメインモデルの定義。外部依存なし。

### エンティティ (Entities)

| エンティティ | 責務 |
|-------------|------|
| **Employee** | 社員情報（社員番号、氏名、メールアドレス、ロール） |
| **Department** | 部署情報（部署コード、部署名） |
| **EmployeeDepartment** | 社員-部署の所属関係（兼務対応、主所属フラグ） |
| **AttendanceRecord** | 勤怠日次レコード（社員・日付ごとの集約） |
| **TimeStampEntry** | 打刻エントリ（出勤・退勤） |
| **WorkType** | 勤務区分マスタ（通常勤務、有給休暇、半休、欠勤等） |

### リッチドメインモデル (Rich Domain Models)

| ドメインモデル | 責務 | 構成元テーブル |
|--------------|------|--------------|
| **MonthlyAttendanceSummary** | 社員の月次勤怠サマリー。勤務日数・総労働時間・残業時間・有給取得日数等を算出 | Employees, Departments, EmployeeDepartments, AttendanceRecords, TimeStampEntries, WorkTypes |

### 値オブジェクト (Value Objects)

| 値オブジェクト | 責務 |
|--------------|------|
| **EmployeeId** | 社員IDの型安全なラッパー |
| **DepartmentCode** | 部署コードの型安全なラッパー |
| **TimeStampType** | 打刻種別（ClockIn / ClockOut） |
| **Role** | ユーザーロール（Admin / User） |
| **WorkDuration** | 労働時間（分）の値オブジェクト。加算・比較・残業判定のロジックを持つ |
| **YearMonth** | 年月を表す値オブジェクト（月次サマリーの期間指定用） |
| **WorkSession** | 1セットの出退勤（ClockIn/ClockOut）を表す値オブジェクト |

### ドメインサービス (Domain/Services)

| サービス | 責務 |
|---------|------|
| **AttendanceDomainService** | 打刻の整合性チェック（二重打刻防止、打刻順序検証） |
| **MonthlyAttendanceDomainService** | 月次勤怠サマリーの構築。複数テーブルの生データからMonthlyAttendanceSummaryを組み立て |

---

## Application - ユースケース層

**責務**: ユースケースの定義と実装。Domain を使ってビジネスフローをオーケストレーション。

### Input Ports (Ports/In) - ユースケースインターフェース

| ポート | 責務 |
|-------|------|
| **ICreateEmployeeUseCase** | 社員登録のユースケース |
| **IUpdateEmployeeUseCase** | 社員更新のユースケース |
| **IDeleteEmployeeUseCase** | 社員削除のユースケース |
| **IGetEmployeeUseCase** | 社員取得・一覧のユースケース |
| **IManageDepartmentUseCase** | 部署CRUDのユースケース |
| **IAssignDepartmentUseCase** | 部署所属管理のユースケース |
| **IStampAttendanceUseCase** | 打刻登録のユースケース |
| **IGetAttendanceUseCase** | 勤怠一覧・検索のユースケース |
| **IUpdateAttendanceUseCase** | 勤怠編集のユースケース |
| **IManageWorkTypeUseCase** | 勤務区分CRUDのユースケース |
| **IGetMonthlySummaryUseCase** | 月次サマリー取得のユースケース |

### Output Ports (Ports/Out) - リポジトリ・クエリインターフェース

| ポート | 責務 |
|-------|------|
| **IEmployeeRepository** | 社員の永続化操作 |
| **IDepartmentRepository** | 部署の永続化操作 |
| **IAttendanceRepository** | 勤怠データの永続化操作 |
| **IWorkTypeRepository** | 勤務区分の永続化操作 |
| **IMonthlyAttendanceQueryService** | 月次勤怠サマリー構築用のデータ取得（複数テーブルJOIN） |

### UseCase 実装 (UseCases)

| サービス | 実装するPort | 責務 |
|---------|-------------|------|
| **CreateEmployeeService** | ICreateEmployeeUseCase | 社員登録 |
| **UpdateEmployeeService** | IUpdateEmployeeUseCase | 社員情報更新 |
| **DeleteEmployeeService** | IDeleteEmployeeUseCase | 社員論理削除 |
| **GetEmployeeService** | IGetEmployeeUseCase | 社員取得・一覧 |
| **ManageDepartmentService** | IManageDepartmentUseCase | 部署CRUD |
| **AssignDepartmentService** | IAssignDepartmentUseCase | 部署所属管理 |
| **StampAttendanceService** | IStampAttendanceUseCase | 打刻登録 |
| **GetAttendanceService** | IGetAttendanceUseCase | 勤怠一覧・検索 |
| **UpdateAttendanceService** | IUpdateAttendanceUseCase | 勤怠編集 |
| **ManageWorkTypeService** | IManageWorkTypeUseCase | 勤務区分CRUD |
| **GetMonthlySummaryService** | IGetMonthlySummaryUseCase | 月次サマリー取得 |

### DTO (Data Transfer Objects)

| DTO | 用途 |
|-----|------|
| **EmployeeDto / CreateEmployeeDto / UpdateEmployeeDto** | 社員データの転送 |
| **DepartmentDto / CreateDepartmentDto / UpdateDepartmentDto** | 部署データの転送 |
| **AttendanceRecordDto / TimeStampEntryDto** | 勤怠データの転送 |
| **WorkTypeDto / CreateWorkTypeDto** | 勤務区分データの転送 |
| **AttendanceSearchCriteria** | 勤怠検索条件 |
| **MonthlyAttendanceSummaryDto** | 月次勤怠サマリーの転送 |
| **MonthlyAttendanceRawData** | 月次勤怠クエリの生データ |

---

## Adapters/In/Web - Driving Adapter (MVC)

**責務**: ASP.NET Core MVC によるUI。Controller が Input Port 経由で UseCase を呼び出す。

### レイヤー内の責務分離

```
Razor View (.cshtml)
  | ViewModel / FormModel（モデルバインド）
  v
Controller (.cs)
  | Input Port IF 経由（IXxxUseCase）
  v
UseCase 実装 (Application/UseCases)
```

- **Controller** (xxxController.cs): HTTPリクエスト処理、入力バリデーション、FormModel <-> DTO 変換、UseCase呼び出し
- **ViewModel**: Controller -> View へ表示データを渡すモデル（読み取り専用）
- **FormModel**: View -> Controller へフォーム送信データを受け取るモデル（バリデーション属性付き）

### Controller 構成

| Controller | Action | 責務 |
|------------|--------|------|
| **HomeController** | Index | ダッシュボード |
| **EmployeesController** | Index, Create, Edit, Details, Delete | 社員CRUD・検索 |
| **DepartmentsController** | Index, Create, Edit, Delete | 部署CRUD |
| **WorkTypesController** | Index, Create, Edit | 勤務区分CRUD |
| **AttendanceController** | Index, Stamp, Edit | 勤怠一覧・検索、打刻、編集 |
| **MonthlyReportController** | Index | 月次勤怠サマリー表示 |

### View 構成

```
Views/
+-- Home/
|   +-- Index.cshtml
+-- Employees/
|   +-- Index.cshtml, Create.cshtml, Edit.cshtml, Details.cshtml, Delete.cshtml
+-- Departments/
|   +-- Index.cshtml, Create.cshtml, Edit.cshtml, Delete.cshtml
+-- WorkTypes/
|   +-- Index.cshtml, Create.cshtml, Edit.cshtml
+-- Attendance/
|   +-- Index.cshtml, Stamp.cshtml, Edit.cshtml
+-- MonthlyReport/
|   +-- Index.cshtml
+-- Shared/
    +-- _Layout.cshtml, _ValidationScriptsPartial.cshtml
```

### ViewModel 一覧

| ViewModel | 用途 | 使用View |
|-----------|------|----------|
| **EmployeeListViewModel** | 社員一覧表示（検索条件含む） | Employees/Index |
| **EmployeeDetailViewModel** | 社員詳細表示（所属部署一覧含む） | Employees/Details |
| **DepartmentListViewModel** | 部署一覧表示 | Departments/Index |
| **WorkTypeListViewModel** | 勤務区分一覧表示 | WorkTypes/Index |
| **AttendanceListViewModel** | 勤怠一覧表示（検索条件含む） | Attendance/Index |
| **StampViewModel** | 打刻画面表示（現在の打刻状態含む） | Attendance/Stamp |
| **MonthlyReportViewModel** | 月次勤怠サマリー一覧 | MonthlyReport/Index |

### FormModel 一覧

| FormModel | 用途 | 使用View | 主なバリデーション |
|-----------|------|----------|------------------|
| **EmployeeFormModel** | 社員登録・編集フォーム | Employees/Create, Edit | 必須項目、メール形式、社員番号形式 |
| **DepartmentFormModel** | 部署登録・編集フォーム | Departments/Create, Edit | 必須項目、部署コード形式 |
| **WorkTypeFormModel** | 勤務区分登録・編集フォーム | WorkTypes/Create, Edit | 必須項目、コード形式 |
| **AttendanceEditFormModel** | 勤怠編集フォーム | Attendance/Edit | 日付・時刻の妥当性 |
| **AttendanceSearchFormModel** | 勤怠検索フォーム | Attendance/Index | 日付範囲の妥当性 |

---

## Adapters/Out/Persistence - Driven Adapter (EF Core)

**責務**: Output Port の実装。EF Core を使用してデータアクセス。

### コンポーネント

| コンポーネント | 責務 |
|--------------|------|
| **AppDbContext** | EF Core DbContext（SQL Server接続） |
| **EmployeeRepository** | IEmployeeRepository の実装 |
| **DepartmentRepository** | IDepartmentRepository の実装 |
| **AttendanceRepository** | IAttendanceRepository の実装 |
| **WorkTypeRepository** | IWorkTypeRepository の実装 |
| **MonthlyAttendanceQueryService** | IMonthlyAttendanceQueryService の実装（複数テーブルJOINクエリ） |
| **EF Core Configurations** | エンティティのテーブルマッピング設定 |
