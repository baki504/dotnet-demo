# コンポーネント依存関係

## ヘキサゴナルアーキテクチャ依存ルール

**原則**: 依存方向は常に外側 → 内側。Domain + Application（内側）は Adapters（外側）を知らない。

```
+---------------------------------------------------------------+
|                    Adapters/In (Driving)                       |
|  Web/Controllers -> Input Port IF を通じて UseCase を呼ぶ     |
+---------------------------------------------------------------+
                             |
                             v
+---------------------------------------------------------------+
|                    Application (ユースケース層)                 |
|  Ports/In:   Input Port インターフェース定義                    |
|  UseCases:   UseCase 実装（Ports/In を実装）                   |
|  Ports/Out:  Output Port インターフェース定義                   |
|  DTOs:       データ転送オブジェクト                              |
+---------------------------------------------------------------+
                             |
                             v
+---------------------------------------------------------------+
|                    Domain (コア・最内層)                        |
|  Entities:       エンティティ                                   |
|  ValueObjects:   値オブジェクト                                  |
|  Services:       ドメインサービス                                |
+---------------------------------------------------------------+
                             ^
                             |
+---------------------------------------------------------------+
|                    Adapters/Out (Driven)                       |
|  Persistence/Repositories -> Output Port IF を実装             |
+---------------------------------------------------------------+
```

---

## 依存関係マトリクス

```
                  Domain   App/Ports/In  App/UseCases  App/Ports/Out  Adapters/In  Adapters/Out
Domain              -          -             -             -             -             -
App/Ports/In       ※1          -             -             -             -             -
App/UseCases       依存        依存           -            依存           -             -
App/Ports/Out      ※2          -             -             -             -             -
Adapters/In         -         依存            -             -             -             -
Adapters/Out       ※3          -             -            依存           -             -
```

※1: Ports/In の UseCase IF は Domain の DTO・VO を参照（メソッド引数・戻り値）
※2: Ports/Out の Repository IF は Domain のエンティティ・VO を参照（メソッド引数・戻り値）
※3: Adapters/Out はエンティティを EF Core でマッピングするため Domain に依存

**重要な依存ルール**:
- Application/UseCases は Domain と Ports/Out の両方に依存する（UseCase がドメインロジックを使い、Port 経由で外部アクセス）
- Adapters/In は Ports/In のみに依存（UseCase 実装を直接参照しない）
- Adapters/Out は Ports/Out と Domain に依存（Port IF を実装し、エンティティをマッピング）

---

## データフロー

### 打刻操作
```
[ブラウザ]
    |  HTTP POST (stampType)
    v
[Adapters/In/Web/AttendanceController.Stamp()]
    |  FormModel -> DTO 変換
    |  IStampAttendanceUseCase.ExecuteAsync()   # Input Port 経由
    v
[Application/UseCases/StampAttendanceService]
    |  IAttendanceRepository.GetByEmployeeAndDate()  # Output Port 経由
    |  AttendanceDomainService.ValidateTimeStamp()    # Domain/Services
    |  AttendanceRecord.AddTimeStamp()                # Domain/Entities
    |  IAttendanceRepository.AddAsync/UpdateAsync()   # Output Port 経由
    v
[Adapters/Out/Persistence/AttendanceRepository -> AppDbContext -> SQL Server]
```

### 月次勤怠サマリー取得
```
[ブラウザ]
    |  HTTP GET (year, month, departmentId?)
    v
[Adapters/In/Web/MonthlyReportController.Index()]
    |  IGetMonthlySummaryUseCase.GetSummariesAsync()  # Input Port 経由
    v
[Application/UseCases/GetMonthlySummaryService]
    |  IMonthlyAttendanceQueryService.GetMonthlyDataAsync()  # Output Port 経由
    v
[Adapters/Out/Persistence/MonthlyAttendanceQueryService]
    |  6テーブル JOIN -> MonthlyAttendanceRawData
    v
[Application/UseCases/GetMonthlySummaryService]
    |  MonthlyAttendanceDomainService.BuildSummaries()  # Domain/Services
    |  MonthlyAttendanceSummary -> MonthlyAttendanceSummaryDto 変換
    v
[Adapters/In/Web/MonthlyReportController]
    |  DTO -> MonthlyReportViewModel 変換
    v
[Razor View -> ブラウザ]
```

---

## 名前空間と参照ルール

単一プロジェクトのため名前空間で論理的に分離:

| 名前空間 | 参照可能な名前空間 |
|---------|-------------------|
| HrAttendance.Domain.Entities | （なし） |
| HrAttendance.Domain.ValueObjects | （なし） |
| HrAttendance.Domain.Services | Domain.Entities, Domain.ValueObjects |
| HrAttendance.Application.Ports.In | Application.DTOs, Domain.ValueObjects |
| HrAttendance.Application.Ports.Out | Domain.Entities, Domain.ValueObjects |
| HrAttendance.Application.UseCases | Domain.*, Application.Ports.Out, Application.DTOs |
| HrAttendance.Application.DTOs | Domain.ValueObjects |
| HrAttendance.Adapters.In.Web | Application.Ports.In, Application.DTOs, Domain.ValueObjects |
| HrAttendance.Adapters.Out.Persistence | Application.Ports.Out, Domain.Entities, Domain.ValueObjects |
