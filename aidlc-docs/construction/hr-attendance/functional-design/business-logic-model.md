# ビジネスロジックモデル

## 開発手法: TDD（テスト駆動開発）

全てのビジネスロジックはTDDサイクルで実装する：
1. **Red**: テストを先に書く（コンパイルエラー or テスト失敗を確認）
2. **Green**: テストを通す最小限の実装を書く
3. **Refactor**: コードを整理（テストは引き続きパス）

**TDD対象レイヤー**:
- Domain層: 値オブジェクト、エンティティ、ドメインサービス、リッチドメインモデル
- Application層: アプリケーションサービス（リポジトリはモック）

---

## サービスフロー

### 1. 打刻フロー（AttendanceAppService.StampAsync）

```
入力: employeeId, stampType(ClockIn/ClockOut), timestamp(省略時=現在時刻)

1. Employee を取得（IsDeleted=false を確認）
2. stampType が ClockIn の場合:
   a. 新規 AttendanceRecord を作成（WorkType=通常勤務）
   b. 同一日の既存レコードを確認
   c. 既存レコードがある場合、最後のレコードの ClockOut が記録済みか確認
      → 未記録の場合はエラー（前のセットを完了してください）
   d. 既存レコードがあり ClockOut 済みの場合、時間重複チェック
      → 前レコードの ClockOut < 新 ClockIn であることを確認
   e. TimeStampEntry(ClockIn) を追加
3. stampType が ClockOut の場合:
   a. 同一日の最新 AttendanceRecord を取得
      → 存在しない場合はエラー（出勤打刻がありません）
   b. ClockIn が存在し、ClockOut が未記録であることを確認
      → ClockOut 済みの場合はエラー（既に退勤済みです）
   c. ClockIn < ClockOut であることを確認
   d. TimeStampEntry(ClockOut) を追加
4. 永続化
5. TimeStampEntryDto を返却
```

### 2. 社員登録フロー（EmployeeAppService.CreateAsync）

```
入力: CreateEmployeeDto (employeeNumber, name, email, role, departmentId)

1. EmployeeNumber の形式チェック（EMP-XXX）
2. EmployeeNumber のユニークチェック
3. Email のユニークチェック
4. Department の存在確認
5. Employee エンティティ生成
6. EmployeeDepartment 生成（IsPrimary=true）
7. 永続化
8. EmployeeDto を返却
```

### 3. 社員削除フロー（EmployeeAppService.DeleteAsync）

```
入力: employeeId

1. Employee を取得
2. IsDeleted = true に設定
3. UpdatedAt を更新
4. 永続化
※ EmployeeDepartment、AttendanceRecord は保持（論理削除のため）
```

### 4. 勤怠一覧取得フロー（AttendanceAppService.GetByDateRangeAsync）

```
入力: AttendanceSearchCriteria (dateFrom, dateTo, employeeId?, departmentId?)

1. 権限チェック:
   - User ロール: 自分の employeeId に限定（パラメータ上書き）
   - Admin ロール: 全社員対象、フィルタリング任意
2. IAttendanceRepository.SearchAsync(criteria)
3. IsDeleted=true の社員を除外
4. Entity → DTO 変換（TimeStampEntries 含む）
5. List<AttendanceRecordDto> を返却
```

### 5. 月次勤怠サマリー取得フロー（MonthlyAttendanceAppService.GetMonthlySummariesAsync）

```
入力: yearMonth, departmentId?(フィルタ)

1. 権限チェック:
   - User ロール: 自分のサマリーのみ
   - Admin ロール: 全社員（部署フィルタ可能）
2. IMonthlyAttendanceQueryService.GetMonthlyDataAsync(yearMonth, departmentId?)
   → 6テーブルJOINで生データ取得
3. MonthlyAttendanceDomainService.BuildSummaries(rawData)
   → 社員ごとにグルーピング
   → 各社員の MonthlyAttendanceSummary を構築
     - DailyAttendanceDetail 生成（日別）
     - WorkSession 生成（出退勤セットごと）
     - WorkDuration 計算
4. MonthlyAttendanceSummary → MonthlyAttendanceSummaryDto 変換
5. List<MonthlyAttendanceSummaryDto> を返却
```

### 6. 勤怠編集フロー（AttendanceAppService.UpdateRecordAsync）

```
入力: recordId, UpdateAttendanceDto

1. 権限チェック: Admin ロールであることを確認
2. AttendanceRecord を取得
3. WorkType 変更がある場合、WorkType の存在確認
4. TimeStampEntry の修正がある場合:
   a. ClockIn < ClockOut の整合性チェック
   b. 同一日の他セットとの時間重複チェック
5. エンティティ更新
6. 永続化
7. AttendanceRecordDto を返却
```

---

## 状態遷移

### AttendanceRecord の状態遷移（1日の1セット）

```
[未作成]
    | ClockIn 打刻
    v
[出勤済み]
    | ClockOut 打刻
    v
[出退勤完了]
```

### 1日の出退勤フロー（複数セット対応）

```
[日の開始]
    | ClockIn（セット1）
    v
[セット1: 出勤済み]
    | ClockOut（セット1）
    v
[セット1: 完了]
    | ClockIn（セット2）← 新しい AttendanceRecord を作成
    v
[セット2: 出勤済み]
    | ClockOut（セット2）
    v
[セット2: 完了]
    | ...繰り返し可能
```

### Employee の状態遷移

```
[未登録]
    | CreateAsync
    v
[有効 (IsDeleted=false)]
    | DeleteAsync（論理削除）
    v
[削除済み (IsDeleted=true)]
```
