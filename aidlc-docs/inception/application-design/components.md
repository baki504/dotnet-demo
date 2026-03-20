# コンポーネント定義

## プロジェクト構成（DDD ベース）

```
HrAttendance.sln
+-- HrAttendance/                     # プロダクションコード
|   +-- Domain/                       # ドメイン層
|   +-- Application/                  # アプリケーション層
|   +-- Infrastructure/               # インフラストラクチャ層
|   +-- Web/                          # プレゼンテーション層 (MVC Controller + Razor View)
+-- HrAttendanceTests/                # テストコード
|   +-- Domain/                       # Domain層のユニットテスト
|   +-- Application/                  # Application層のユニットテスト
+-- docker-compose.yml
```

---

## Domain 層 - HrAttendance.Domain

**責務**: ビジネスルールとドメインモデルの定義。外部依存なし。

### エンティティ (Entities)

| エンティティ | 責務 |
|-------------|------|
| **Employee** | 社員情報（社員番号、氏名、メールアドレス、ロール） |
| **Department** | 部署情報（部署コード、部署名） |
| **EmployeeDepartment** | 社員-部署の所属関係（兼務対応、主所属フラグ） |
| **AttendanceRecord** | 勤怠日次レコード（社員・日付ごとの集約） |
| **TimeStampEntry** | 打刻エントリ（出勤・退勤・休憩開始・休憩終了） |
| **WorkType** | 勤務区分マスタ（通常勤務、有給休暇、半休、欠勤等） |

### リッチドメインモデル (Rich Domain Models)

| ドメインモデル | 責務 | 構成元テーブル |
|--------------|------|--------------|
| **MonthlyAttendanceSummary** | 社員の月次勤怠サマリー。勤務日数・総労働時間・残業時間・有給取得日数等を算出 | Employees, Departments, EmployeeDepartments, AttendanceRecords, TimeStampEntries, WorkTypes |

※ MonthlyAttendanceSummary は独自テーブルを持たない。複数テーブルのデータを組み合わせてドメイン層で構築し、ビジネスロジック（労働時間計算、残業判定等）を持つ。

### 値オブジェクト (Value Objects)

| 値オブジェクト | 責務 |
|--------------|------|
| **EmployeeId** | 社員IDの型安全なラッパー |
| **DepartmentCode** | 部署コードの型安全なラッパー |
| **TimeStampType** | 打刻種別（ClockIn / ClockOut / BreakStart / BreakEnd） |
| **Role** | ユーザーロール（Admin / User） |
| **WorkDuration** | 労働時間（時間・分）の値オブジェクト。加算・比較・残業判定のロジックを持つ |
| **YearMonth** | 年月を表す値オブジェクト（月次サマリーの期間指定用） |

### リポジトリインターフェース (Repository Interfaces)

| インターフェース | 責務 |
|----------------|------|
| **IEmployeeRepository** | 社員の永続化操作 |
| **IDepartmentRepository** | 部署の永続化操作 |
| **IAttendanceRepository** | 勤怠データの永続化操作 |
| **IWorkTypeRepository** | 勤務区分の永続化操作 |
| **IMonthlyAttendanceQueryService** | 月次勤怠サマリー構築用のデータ取得（複数テーブルJOIN） |

### ドメインサービス (Domain Services)

| サービス | 責務 |
|---------|------|
| **AttendanceDomainService** | 打刻の整合性チェック（二重打刻防止、打刻順序検証） |
| **MonthlyAttendanceDomainService** | 月次勤怠サマリーの構築。複数テーブルの生データからMonthlyAttendanceSummaryを組み立て、労働時間・残業・有給日数を算出 |

---

## Application 層 - HrAttendance.Application

**責務**: ユースケースの実装、ドメイン操作のオーケストレーション。

### アプリケーションサービス

| サービス | 責務 |
|---------|------|
| **EmployeeAppService** | 社員CRUD操作、部署所属管理 |
| **DepartmentAppService** | 部署CRUD操作 |
| **AttendanceAppService** | 打刻登録、勤怠一覧取得、検索・フィルタリング |
| **WorkTypeAppService** | 勤務区分CRUD操作 |
| **MonthlyAttendanceAppService** | 月次勤怠サマリーの取得・表示用オーケストレーション |

### DTO (Data Transfer Objects)

| DTO | 用途 |
|-----|------|
| **EmployeeDto / CreateEmployeeDto / UpdateEmployeeDto** | 社員データの転送 |
| **DepartmentDto / CreateDepartmentDto / UpdateDepartmentDto** | 部署データの転送 |
| **AttendanceRecordDto / TimeStampEntryDto** | 勤怠データの転送 |
| **WorkTypeDto / CreateWorkTypeDto** | 勤務区分データの転送 |
| **AttendanceSearchCriteria** | 勤怠検索条件 |
| **MonthlyAttendanceSummaryDto** | 月次勤怠サマリーの転送（社員情報、部署名、勤務日数、労働時間、残業時間、有給日数等） |

---

## Infrastructure 層 - HrAttendance.Infrastructure

**責務**: 外部リソースへのアクセス実装。

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

---

## Presentation 層 - HrAttendance.Web

**責務**: ASP.NET Core MVC によるUI。Controller が Application Service を呼び出す。

### レイヤー内の責務分離

```
Razor View (.cshtml)
  ↕ ViewModel / FormModel（モデルバインド）
Controller (.cs)
  ↕ DTO（Application層との境界）
Application Service
```

- **Controller** (xxxController.cs): HTTPリクエスト処理、入力バリデーション、FormModel ↔ DTO 変換、AppService呼び出し
- **ViewModel**: Controller → View へ表示データを渡すモデル（読み取り専用）
- **FormModel**: View → Controller へフォーム送信データを受け取るモデル（バリデーション属性付き）

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
| **MonthlyReportViewModel** | 月次勤怠サマリー一覧（社員名、部署、勤務日数、労働時間、残業、有給） | MonthlyReport/Index |

### FormModel 一覧

| FormModel | 用途 | 使用View | 主なバリデーション |
|-----------|------|----------|------------------|
| **EmployeeFormModel** | 社員登録・編集フォーム | Employees/Create, Edit | 必須項目、メール形式、社員番号形式 |
| **DepartmentFormModel** | 部署登録・編集フォーム | Departments/Create, Edit | 必須項目、部署コード形式 |
| **WorkTypeFormModel** | 勤務区分登録・編集フォーム | WorkTypes/Create, Edit | 必須項目、コード形式 |
| **AttendanceEditFormModel** | 勤怠編集フォーム | Attendance/Edit | 日付・時刻の妥当性 |
| **AttendanceSearchFormModel** | 勤怠検索フォーム | Attendance/Index | 日付範囲の妥当性 |
