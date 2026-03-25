# Application Design - 人事・勤怠管理システム (PoC)

## 設計方針
- **アーキテクチャ**: ヘキサゴナルアーキテクチャ（Ports & Adapters）
- **フロントエンド**: ASP.NET Core MVC（Razor View + Controller）
- **ORM**: Entity Framework Core
- **DB**: SQL Server（Docker - Azure SQL Edge）
- **認証**: OAuth2 / OpenID Connect

---

## アーキテクチャ概要

```
          Adapters/In                                            Adapters/Out
       (Driving Adapters)                                     (Driven Adapters)
      +------------------+                                   +------------------+
      |  Web/Controllers |     +-------------------------+   |  Persistence/    |
      |  Web/ViewModels  |---->| Application/Ports/In    |   |    AppDbContext   |
      |  Web/FormModels  |     |   (UseCase IF)          |   |    Repositories  |
      |  Web/Views       |     |         |               |   |    Configurations|
      +------------------+     |         v               |   +------------------+
                               | Application/UseCases    |           ^
                               |   (UseCase 実装)        |           |
                               |         |               |           |
                               |         v               |           |
                               | Domain/Entities         |           |
                               | Domain/ValueObjects     |           |
                               | Domain/Services         |           |
                               |         |               |           |
                               | Application/Ports/Out   |-----------+
                               |   (Repository IF)       |
                               +-------------------------+
```

**依存方向**: Adapters → Application/Domain（内向き）。内側は Adapters を知らない。

---

## ソリューション構成

```
HrAttendance.slnx
+-- HrAttendance/                              # 単一プロジェクト
|   +-- Domain/                                # コア（最内層）
|   |   +-- Entities/                          # エンティティ
|   |   +-- ValueObjects/                      # 値オブジェクト
|   |   +-- Services/                          # ドメインサービスのみ
|   +-- Application/                           # ユースケース層
|   |   +-- Ports/                             # ポート定義
|   |   |   +-- In/                            # Input Ports（ユースケースIF）
|   |   |   +-- Out/                           # Output Ports（リポジトリIF等）
|   |   +-- UseCases/                          # UseCase 実装
|   |   +-- DTOs/                              # データ転送オブジェクト
|   +-- Adapters/                              # ヘキサゴンの外側
|   |   +-- In/                                # Driving Adapters（外→内）
|   |   |   +-- Web/                           # ASP.NET Core MVC
|   |   |       +-- Controllers/
|   |   |       +-- ViewModels/
|   |   |       +-- FormModels/
|   |   |       +-- Views/
|   |   +-- Out/                               # Driven Adapters（内→外）
|   |       +-- Persistence/                   # EF Core
|   |           +-- AppDbContext.cs
|   |           +-- Repositories/
|   |           +-- Configurations/
|   +-- Program.cs                             # DI設定・起動
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

## エンティティ一覧（テーブルと1:1）

| エンティティ | 責務 | 主要属性 |
|-------------|------|---------|
| **Employee** | 社員情報 | Id, EmployeeNumber, Name, Email, Role |
| **Department** | 部署情報 | Id, Code, Name |
| **EmployeeDepartment** | 社員-部署所属（兼務対応） | EmployeeId, DepartmentId, IsPrimary |
| **AttendanceRecord** | 勤怠日次レコード | Id, EmployeeId, Date, WorkTypeId |
| **TimeStampEntry** | 打刻エントリ | Id, AttendanceRecordId, Type, Timestamp |
| **WorkType** | 勤務区分マスタ | Id, Code, Name, IsWorkDay |

## リッチドメインモデル（テーブルと1:1ではない）

| ドメインモデル | 責務 | 構成元テーブル |
|--------------|------|--------------|
| **MonthlyAttendanceSummary** | 社員の月次勤怠サマリー | Employees, Departments, EmployeeDepartments, AttendanceRecords, TimeStampEntries, WorkTypes |

MonthlyAttendanceSummaryは独自テーブルを持たず、6テーブルのデータを組み合わせてDomain層で構築する。勤務日数・総労働時間・残業時間・有給取得日数等の算出ロジックを持つ。

---

## サービス一覧

| 配置 | サービス | 責務 | 実装する Port |
|------|---------|------|--------------|
| Application/UseCases | CreateEmployeeService | 社員登録 | ICreateEmployeeUseCase |
| Application/UseCases | UpdateEmployeeService | 社員更新 | IUpdateEmployeeUseCase |
| Application/UseCases | DeleteEmployeeService | 社員削除（論理） | IDeleteEmployeeUseCase |
| Application/UseCases | GetEmployeeService | 社員取得・一覧 | IGetEmployeeUseCase |
| Application/UseCases | ManageDepartmentService | 部署CRUD | IManageDepartmentUseCase |
| Application/UseCases | AssignDepartmentService | 部署所属管理 | IAssignDepartmentUseCase |
| Application/UseCases | StampAttendanceService | 打刻登録 | IStampAttendanceUseCase |
| Application/UseCases | GetAttendanceService | 勤怠一覧・検索 | IGetAttendanceUseCase |
| Application/UseCases | UpdateAttendanceService | 勤怠編集 | IUpdateAttendanceUseCase |
| Application/UseCases | ManageWorkTypeService | 勤務区分CRUD | IManageWorkTypeUseCase |
| Application/UseCases | GetMonthlySummaryService | 月次サマリー取得 | IGetMonthlySummaryUseCase |
| Domain/Services | AttendanceDomainService | 打刻順序検証、二重打刻防止 | - |
| Domain/Services | MonthlyAttendanceDomainService | 月次サマリー構築 | - |

---

## 依存関係

```
Adapters/In/Web ----> Application/Ports/In (Input Port IF)
                            |
                            v
                      Application/UseCases (UseCase 実装)
                            |
                      +-----+-----+
                      v           v
               Domain/         Application/Ports/Out (Output Port IF)
          (Entities, VOs,           ^
           Services)                |
                            Adapters/Out/Persistence (Output Port 実装)
```

内側（Domain + Application）は Adapters を知らない。Ports のインターフェースのみに依存。

---

## Web層（MVC）の構成

### データフロー
```
Razor View (.cshtml)
  | ViewModel / FormModel（モデルバインド）
  v
Controller (.cs)
  | Input Port IF 経由で UseCase を呼び出し
  v
UseCase 実装 (Application/UseCases)
  | Domain のエンティティ・サービスを使用
  | Output Port IF 経由でデータアクセス
  v
Repository 実装 (Adapters/Out)
```

### Controller / Action 一覧

| Controller | Action | 概要 |
|------------|--------|------|
| **HomeController** | Index | ダッシュボード |
| **EmployeesController** | Index, Create, Edit, Details, Delete | 社員管理 CRUD |
| **DepartmentsController** | Index, Create, Edit, Delete | 部署管理 CRUD |
| **WorkTypesController** | Index, Create, Edit | 勤務区分管理 |
| **AttendanceController** | Index, Stamp, Edit | 勤怠一覧・検索、打刻、編集 |
| **MonthlyReportController** | Index | 月次勤怠サマリー表示 |

---

## 権限モデル
- **Admin**: 全社員の勤怠閲覧・編集、マスタ管理の全操作
- **User**: 自身の勤怠入力・閲覧のみ

---

詳細は以下の個別ドキュメントを参照：
- [components.md](components.md) - コンポーネント定義
- [component-methods.md](component-methods.md) - メソッド定義
- [services.md](services.md) - サービス定義
- [component-dependency.md](component-dependency.md) - 依存関係
