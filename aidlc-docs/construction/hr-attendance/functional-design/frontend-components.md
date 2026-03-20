# フロントエンドコンポーネント設計（MVC）

## Controller - View - Model 対応表

### HomeController

#### Index（ダッシュボード）
- **ViewModel**: `DashboardViewModel`
  - TodayAttendance: TodayAttendanceInfo? （本日の勤怠状況）
  - MonthlySummary: MonthlySummaryInfo （今月のサマリー）
- **TodayAttendanceInfo**:
  - Status: string （"未出勤" / "出勤中" / "退勤済み"）
  - Sessions: List&lt;SessionInfo&gt; （本日のセット一覧）
  - CanClockIn: bool
  - CanClockOut: bool
- **MonthlySummaryInfo**:
  - WorkDays: int
  - TotalWorkHours: string（"XXh XXm" 形式）
  - OvertimeHours: string
  - PaidLeaveDays: int
- **View内容**:
  - 打刻ボタン（出勤 / 退勤、状態に応じて活性・非活性）
  - 本日の打刻履歴テーブル
  - 今月のサマリーカード

---

### EmployeesController

#### Index（社員一覧）
- **ViewModel**: `EmployeeListViewModel`
  - Employees: List&lt;EmployeeListItem&gt;
  - SearchTerm: string?
- **EmployeeListItem**: Id, EmployeeNumber, Name, Email, DepartmentName(主所属), Role
- **View内容**: 検索フォーム + テーブル（社員番号、氏名、メール、主所属部署、ロール、操作リンク）

#### Create / Edit（社員登録・編集）
- **FormModel**: `EmployeeFormModel`
  - EmployeeNumber: string [Required, RegularExpression("^EMP-\\d{3,}$")]
  - Name: string [Required, MaxLength(100)]
  - Email: string [Required, EmailAddress]
  - Role: Role [Required]
  - PrimaryDepartmentId: Guid [Required]
  - AdditionalDepartmentIds: List&lt;Guid&gt; （兼務部署、任意）
- **追加データ**: Departments（セレクトリスト用）
- **View内容**: フォーム + バリデーションメッセージ

#### Details（社員詳細）
- **ViewModel**: `EmployeeDetailViewModel`
  - Employee 基本情報
  - Departments: List&lt;DepartmentInfo&gt;（所属部署一覧、主所属フラグ付き）
- **View内容**: 詳細カード + 所属部署テーブル

#### Delete（削除確認）
- **ViewModel**: EmployeeDetailViewModel（再利用）
- **View内容**: 社員情報表示 + 「論理削除します。よろしいですか？」確認

---

### DepartmentsController

#### Index（部署一覧）
- **ViewModel**: `DepartmentListViewModel`
  - Departments: List&lt;DepartmentListItem&gt;
- **DepartmentListItem**: Id, Code, Name, EmployeeCount
- **View内容**: テーブル（部署コード、部署名、所属人数、操作リンク）

#### Create / Edit（部署登録・編集）
- **FormModel**: `DepartmentFormModel`
  - Code: string [Required, MaxLength(20), RegularExpression("^[a-zA-Z0-9]+$")]
  - Name: string [Required, MaxLength(100)]
- **View内容**: フォーム + バリデーションメッセージ

#### Delete（削除確認）
- **View内容**: 部署情報表示 + 所属社員がいる場合はエラーメッセージ

---

### WorkTypesController

#### Index（勤務区分一覧）
- **ViewModel**: `WorkTypeListViewModel`
  - WorkTypes: List&lt;WorkTypeListItem&gt;
- **WorkTypeListItem**: Id, Code, Name, IsWorkDay
- **View内容**: テーブル（コード、名称、出勤日フラグ、操作リンク）

#### Create / Edit（勤務区分登録・編集）
- **FormModel**: `WorkTypeFormModel`
  - Code: string [Required, MaxLength(20)]
  - Name: string [Required, MaxLength(50)]
  - IsWorkDay: bool
- **View内容**: フォーム + バリデーションメッセージ

---

### AttendanceController

#### Index（勤怠一覧・検索）
- **FormModel（検索用）**: `AttendanceSearchFormModel`
  - DateFrom: DateOnly?
  - DateTo: DateOnly?
  - EmployeeId: Guid? （Admin のみ表示）
  - DepartmentId: Guid? （Admin のみ表示）
- **ViewModel**: `AttendanceListViewModel`
  - Records: List&lt;AttendanceListItem&gt;
  - SearchForm: AttendanceSearchFormModel
  - Employees: SelectList? （Admin のみ）
  - Departments: SelectList? （Admin のみ）
- **AttendanceListItem**: RecordId, EmployeeName, Date, WorkTypeName, ClockIn, ClockOut, Duration
- **View内容**: 検索フォーム + テーブル（日付、社員名、勤務区分、出勤、退勤、労働時間）
- **権限表示**:
  - User: 自分のデータのみ（社員名列は非表示）
  - Admin: 全社員（フィルタ付き、編集リンク表示）

#### Stamp（打刻画面）
- **ViewModel**: `StampViewModel`
  - CurrentStatus: string （"未出勤" / "出勤中" / "退勤済み"）
  - TodaySessions: List&lt;SessionInfo&gt; （本日のセット一覧）
  - CanClockIn: bool
  - CanClockOut: bool
  - CurrentTime: DateTime
- **SessionInfo**: SetNumber, ClockIn, ClockOut, Duration
- **View内容**: 現在時刻表示 + 打刻ボタン + 本日の打刻履歴

#### Edit（勤怠編集、Admin のみ）
- **FormModel**: `AttendanceEditFormModel`
  - WorkTypeId: Guid [Required]
  - ClockIn: DateTime?
  - ClockOut: DateTime?
- **追加データ**: WorkTypes（セレクトリスト用）、対象社員情報、対象日付
- **View内容**: フォーム（勤務区分セレクト、出勤時刻、退勤時刻）

---

### MonthlyReportController

#### Index（月次勤怠サマリー）
- **FormModel（検索用）**: `MonthlyReportSearchFormModel`
  - Year: int
  - Month: int
  - DepartmentId: Guid? （Admin のみ）
- **ViewModel**: `MonthlyReportViewModel`
  - YearMonth: string （"2026年3月"）
  - Summaries: List&lt;MonthlyReportItem&gt;
  - SearchForm: MonthlyReportSearchFormModel
  - Departments: SelectList? （Admin のみ）
- **MonthlyReportItem**:
  - EmployeeName, DepartmentName
  - WorkDays, TotalWorkHours, OvertimeHours
  - PaidLeaveDays, AbsentDays
- **View内容**: 年月セレクタ + 部署フィルタ（Admin） + サマリーテーブル
- **権限表示**:
  - User: 自分のサマリーのみ
  - Admin: 全社員テーブル + 部署フィルタ

---

## 共通レイアウト（_Layout.cshtml）

### ナビゲーション
```
[ダッシュボード] [社員管理] [部署管理] [勤務区分管理] [勤怠管理] [月次レポート]
```

- **User**: 社員管理・部署管理・勤務区分管理は閲覧のみ（Create/Edit/Delete リンク非表示）
- **Admin**: 全メニュー・全操作リンク表示

### フッター
- ログインユーザー名、ロール表示
