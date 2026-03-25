# ドメインエンティティ詳細設計

## 共通前提

- 日時は全てJST（日本標準時、UTC+9）を前提とする。タイムゾーン変換は行わない。

## エンティティ一覧（テーブルと1:1）

### Employee

| 属性 | 型 | 制約 | 備考 |
|------|------|------|------|
| Id | Guid | PK, 自動生成 | |
| EmployeeNumber | string | 必須, ユニーク, 形式: EMP-XXX | プレフィックス付き連番 |
| Name | string | 必須, 最大100文字 | |
| Email | string | 必須, ユニーク, メール形式 | |
| Role | Role (enum) | 必須 | Admin / User |
| IsDeleted | bool | デフォルト: false | 論理削除フラグ |
| CreatedAt | DateTime | 自動設定 | |
| UpdatedAt | DateTime | 自動設定 | |

**リレーション**:
- Employee 1 --- * EmployeeDepartment（兼務対応）
- Employee 1 --- * AttendanceRecord

---

### Department

| 属性 | 型 | 制約 | 備考 |
|------|------|------|------|
| Id | Guid | PK, 自動生成 | |
| Code | DepartmentCode (VO) | 必須, ユニーク | |
| Name | string | 必須, 最大100文字 | |
| CreatedAt | DateTime | 自動設定 | |
| UpdatedAt | DateTime | 自動設定 | |

**リレーション**:
- Department 1 --- * EmployeeDepartment
- 階層構造なし（フラット）

---

### EmployeeDepartment

| 属性 | 型 | 制約 | 備考 |
|------|------|------|------|
| EmployeeId | Guid | PK (複合), FK → Employee | |
| DepartmentId | Guid | PK (複合), FK → Department | |
| IsPrimary | bool | 必須 | 主所属フラグ（社員ごとに1つのみtrue） |

---

### AttendanceRecord

| 属性 | 型 | 制約 | 備考 |
|------|------|------|------|
| Id | Guid | PK, 自動生成 | |
| EmployeeId | Guid | FK → Employee, 必須 | |
| Date | DateOnly | 必須 | 勤怠対象日 |
| WorkTypeId | Guid | FK → WorkType, 必須 | |
| CreatedAt | DateTime | 自動設定 | |
| UpdatedAt | DateTime | 自動設定 | |

**リレーション**:
- AttendanceRecord 1 --- * TimeStampEntry
- 同一社員・同一日に複数レコード可（複数セット出退勤対応）
- ユニーク制約なし（EmployeeId + Date の組み合わせは重複許可）

---

### TimeStampEntry

| 属性 | 型 | 制約 | 備考 |
|------|------|------|------|
| Id | Guid | PK, 自動生成 | |
| AttendanceRecordId | Guid | FK → AttendanceRecord, 必須 | |
| Type | TimeStampType (enum) | 必須 | ClockIn / ClockOut |
| Timestamp | DateTime | 必須 | 打刻日時 |
| CreatedAt | DateTime | 自動設定 | |

**制約**:
- 1つの AttendanceRecord に対して ClockIn は最大1回、ClockOut は最大1回
- ClockIn の Timestamp < ClockOut の Timestamp

---

### WorkType

| 属性 | 型 | 制約 | 備考 |
|------|------|------|------|
| Id | Guid | PK, 自動生成 | |
| Code | string | 必須, ユニーク, 最大20文字 | |
| Name | string | 必須, 最大50文字 | |
| IsWorkDay | bool | 必須 | 出勤日として扱うか |
| CreatedAt | DateTime | 自動設定 | |
| UpdatedAt | DateTime | 自動設定 | |

**初期データ（シード）**:

| Code | Name | IsWorkDay |
|------|------|-----------|
| NORMAL | 通常勤務 | true |
| PAID_LEAVE | 有給休暇 | false |
| HALF_AM | 半休（午前） | true |
| HALF_PM | 半休（午後） | true |
| ABSENT | 欠勤 | false |

---

## リッチドメインモデル（テーブルと1:1ではない）

### MonthlyAttendanceSummary

独自テーブルを持たない。6テーブルの生データからDomain層で構築する。

| 属性 | 型 | 構成元 |
|------|------|--------|
| EmployeeId | Guid | Employees.Id |
| EmployeeNumber | string | Employees.EmployeeNumber |
| EmployeeName | string | Employees.Name |
| DepartmentName | string | Departments.Name（主所属部署） |
| YearMonth | YearMonth (VO) | 計算パラメータ |
| DailyRecords | List&lt;DailyAttendanceDetail&gt; | 下記参照 |

**DailyAttendanceDetail（MonthlyAttendanceSummary内部の値オブジェクト）**:

| 属性 | 型 | 構成元 |
|------|------|--------|
| Date | DateOnly | AttendanceRecords.Date |
| WorkTypeName | string | WorkTypes.Name |
| IsWorkDay | bool | WorkTypes.IsWorkDay |
| Sessions | List&lt;WorkSession&gt; | AttendanceRecords + TimeStampEntries |

**WorkSession（1セットの出退勤を表す値オブジェクト）**:

| 属性 | 型 | 構成元 |
|------|------|--------|
| ClockIn | DateTime? | TimeStampEntries (Type=ClockIn) |
| ClockOut | DateTime? | TimeStampEntries (Type=ClockOut) |
| Duration | WorkDuration | ClockOut - ClockIn から算出 |

**ビジネスロジック（メソッド）**:
- `GetTotalWorkDays()` → IsWorkDay=true のレコード数
- `GetTotalWorkDuration()` → 全 WorkSession の Duration 合算
- `GetOvertimeDuration(standardHoursPerDay)` → 日ごとの所定時間超過分の合算
- `GetPaidLeaveDays()` → WorkType=有給休暇のレコード数
- `GetAbsentDays()` → WorkType=欠勤のレコード数
- `GetWorkDaysByType()` → 勤務区分別の日数内訳

---

## 値オブジェクト (Value Objects)

### EmployeeId
- Guid のラッパー
- 空Guid禁止

### DepartmentCode
- string のラッパー
- 必須、最大20文字、英数字のみ

### TimeStampType (Enum)
- `ClockIn` - 出勤
- `ClockOut` - 退勤

※ 休憩開始・終了はスコープ外

### Role (Enum)
- `Admin` - 管理者
- `User` - 一般社員

### WorkDuration
- 内部表現: int (分)
- `FromMinutes(minutes)` - ファクトリ
- `Add(other)` / `Subtract(other)` - 演算
- `IsGreaterThan(other)` - 比較
- `ToHoursAndMinutes()` → (hours, minutes)
- `TotalMinutes` → int

### YearMonth
- 内部表現: int year, int month
- `Of(year, month)` - ファクトリ（1-12月の範囲チェック）
- `GetFirstDay()` → DateOnly
- `GetLastDay()` → DateOnly
- `ToString()` → "2026-03" 形式

### WorkSession
- ClockIn / ClockOut のペア
- `Duration` → ClockOut が null の場合は WorkDuration.FromMinutes(0)
