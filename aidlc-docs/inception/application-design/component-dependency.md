# コンポーネント依存関係

## 依存関係マトリクス

```
                        Domain    Application    Infrastructure    Web
Domain                    -           -               -            -
Application             依存          -               -            -
Infrastructure          依存          -               -            -
Web                       -         依存              -            -
```

**ルール**: 依存方向は常に外側→内側。Domain は何にも依存しない。

---

## レイヤー間依存関係

```
+--------------------------------------------------+
|           Web (MVC Controller + Razor View)       |
|  依存先: Application (AppService, DTO)           |
+--------------------------------------------------+
                        |
                        v
+--------------------------------------------------+
|              Application (AppServices)            |
|  依存先: Domain (Entity, Repository IF, Service) |
+--------------------------------------------------+
                        |
                        v
+--------------------------------------------------+
|              Domain (Entities, VOs, IF)           |
|  依存先: なし                                     |
+--------------------------------------------------+
                        ^
                        |
+--------------------------------------------------+
|          Infrastructure (EF Core, Repos)          |
|  依存先: Domain (Repository IF, Entity)          |
+--------------------------------------------------+
```

---

## データフロー

### 打刻操作
```
[ブラウザ]
    |  HTTP POST (stampType)
    v
[AttendanceController.Stamp()]
    |  入力バリデーション
    |  AttendanceAppService.StampAsync(employeeId, stampType)
    v
[AttendanceAppService]
    |  IAttendanceRepository.GetByEmployeeAndDate()
    |  AttendanceDomainService.ValidateTimeStamp()
    |  AttendanceRecord.AddTimeStamp()
    |  IAttendanceRepository.SaveAsync()
    v
[AttendanceRepository -> AppDbContext -> SQL Server]
```

### 一覧・検索操作
```
[ブラウザ]
    |  HTTP GET (searchTerm, dateFrom, dateTo, departmentId)
    v
[AttendanceController.Index()]
    |  AttendanceSearchFormModel → AttendanceSearchCriteria 変換
    |  AttendanceAppService.GetByDateRangeAsync(criteria)
    v
[AttendanceAppService]
    |  IAttendanceRepository.SearchAsync(criteria)
    v
[AttendanceRepository -> AppDbContext -> SQL Server]
    |  List<AttendanceRecord>
    v
[AttendanceAppService]
    |  Entity -> DTO 変換
    v
[AttendanceController]
    |  DTO -> AttendanceListViewModel 変換
    v
[Razor View (.cshtml) -> ブラウザ]
```

---

## プロジェクト参照

| プロジェクト | 参照先 |
|-------------|--------|
| HrAttendance/Domain | (なし) |
| HrAttendance/Application | HrAttendance/Domain |
| HrAttendance/Infrastructure | HrAttendance/Domain |
| HrAttendance/Web | HrAttendance/Application, HrAttendance/Infrastructure (DI登録のみ) |
| HrAttendanceTests/Domain | HrAttendance/Domain |
| HrAttendanceTests/Application | HrAttendance/Application, HrAttendance/Domain |
