# Application Design - 人事・勤怠管理システム (PoC)

## 設計方針
- **アーキテクチャ**: DDD（Domain-Driven Design）ベース
- **レイヤー構成**: Domain / Application / Infrastructure / Web の4層
- **フロントエンド**: ASP.NET Core MVC（Razor View + Controller）
- **ORM**: Entity Framework Core
- **DB**: SQL Server（Docker - Azure SQL Edge）
- **認証**: OAuth2 / OpenID Connect

---

## ソリューション構成

```
HrAttendance.sln
+-- HrAttendance/                     # プロダクションコード
|   +-- Domain/                       # エンティティ, VO, リポジトリIF, ドメインサービス
|   +-- Application/                  # アプリケーションサービス, DTO
|   +-- Infrastructure/               # EF Core DbContext, リポジトリ実装
|   +-- Web/                          # MVC (Controller + Razor View), DI設定, 認証設定
+-- HrAttendanceTests/                # テストコード
|   +-- Domain/                       # Domain層のユニットテスト
|   +-- Application/                  # Application層のユニットテスト
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

| レイヤー | サービス | 責務 |
|---------|---------|------|
| Domain | AttendanceDomainService | 打刻順序検証、二重打刻防止 |
| Domain | MonthlyAttendanceDomainService | 月次勤怠サマリーの構築（リッチドメインモデル組み立て） |
| Application | EmployeeAppService | 社員CRUD、部署所属管理 |
| Application | DepartmentAppService | 部署CRUD |
| Application | AttendanceAppService | 打刻登録、勤怠一覧・検索 |
| Application | WorkTypeAppService | 勤務区分CRUD |
| Application | MonthlyAttendanceAppService | 月次勤怠サマリーの取得 |

---

## 依存関係

```
Web -> Application -> Domain <- Infrastructure
```

Domain は外部依存なし。Infrastructure は Domain のリポジトリインターフェースを実装。

---

## Web層（MVC）の構成

### レイヤー内の責務分離
- **Controller** (xxxController.cs): HTTPリクエスト処理、入力バリデーション、FormModel ↔ DTO 変換、AppService呼び出し
- **ViewModel**: Controller → View への表示データモデル
- **FormModel**: View → Controller へのフォーム送信データモデル（バリデーション属性付き）

### データフロー
```
Razor View (.cshtml)
  ↕ ViewModel / FormModel（モデルバインド）
Controller (.cs)
  ↕ DTO（Application層との境界）
Application Service
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
