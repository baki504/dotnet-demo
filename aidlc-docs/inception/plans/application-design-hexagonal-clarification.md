# Application Design - ヘキサゴナル構成の追加確認

Q3・Q4について、具体的なディレクトリ構成例を示します。比較して選択してください。

---

## パターンA: Domain層にPort配置 + ヘキサゴナル用語

```
HrAttendance/
+-- Domain/                              # コア（ビジネスロジック）
|   +-- Entities/
|   |   +-- Employee.cs
|   |   +-- Department.cs
|   |   +-- AttendanceRecord.cs
|   |   +-- ...
|   +-- ValueObjects/
|   |   +-- EmployeeId.cs              # 既存（変更なし）
|   |   +-- WorkDuration.cs            # 既存（変更なし）
|   |   +-- ...
|   +-- Services/
|   |   +-- AttendanceDomainService.cs
|   |   +-- MonthlyAttendanceDomainService.cs
|   +-- Ports/                           # ★ Output Ports（driven ports）
|       +-- IEmployeeRepository.cs
|       +-- IDepartmentRepository.cs
|       +-- IAttendanceRepository.cs
|       +-- IWorkTypeRepository.cs
|       +-- IMonthlyAttendanceQueryService.cs
+-- Application/                         # ユースケース層
|   +-- Ports/                           # ★ Input Ports（driving ports）
|   |   +-- ICreateEmployeeUseCase.cs
|   |   +-- IStampAttendanceUseCase.cs
|   |   +-- IGetMonthlySummaryUseCase.cs
|   |   +-- ...
|   +-- UseCases/                        # Input Port の実装
|   |   +-- CreateEmployeeUseCase.cs
|   |   +-- StampAttendanceUseCase.cs
|   |   +-- GetMonthlySummaryUseCase.cs
|   |   +-- ...
|   +-- DTOs/
|       +-- EmployeeDto.cs
|       +-- ...
+-- Infrastructure/                      # Driven Adapters
|   +-- Adapters/
|   |   +-- Persistence/
|   |   |   +-- AppDbContext.cs
|   |   |   +-- EmployeeRepository.cs   # Domain/Ports/IEmployeeRepository の実装
|   |   |   +-- ...
|   |   +-- Configurations/
|   |       +-- EmployeeConfiguration.cs
|   |       +-- ...
+-- Web/                                 # Driving Adapters
    +-- Adapters/
    |   +-- Controllers/
    |   |   +-- EmployeesController.cs   # Application/Ports/ICreateEmployeeUseCase を呼ぶ
    |   |   +-- AttendanceController.cs
    |   |   +-- ...
    +-- ViewModels/
    +-- FormModels/
    +-- Views/
```

**特徴**: Ports & Adapters の用語がそのまま。ヘキサゴナルの教科書に近い。

---

## パターンB: Application層にPort配置 + 機能ベース命名

```
HrAttendance/
+-- Domain/                              # コア（ビジネスロジック）
|   +-- Entities/
|   |   +-- Employee.cs
|   |   +-- ...
|   +-- ValueObjects/
|   |   +-- EmployeeId.cs              # 既存（変更なし）
|   |   +-- ...
|   +-- Services/
|       +-- AttendanceDomainService.cs
|       +-- MonthlyAttendanceDomainService.cs
+-- Application/                         # ユースケース層
|   +-- Interfaces/                      # ★ Input Ports + Output Ports の両方
|   |   +-- UseCases/                    # Input Ports
|   |   |   +-- ICreateEmployeeUseCase.cs
|   |   |   +-- IStampAttendanceUseCase.cs
|   |   |   +-- ...
|   |   +-- Repositories/               # Output Ports
|   |       +-- IEmployeeRepository.cs
|   |       +-- IDepartmentRepository.cs
|   |       +-- IAttendanceRepository.cs
|   |       +-- IWorkTypeRepository.cs
|   |       +-- IMonthlyAttendanceQueryService.cs
|   +-- UseCases/                        # Input Port の実装
|   |   +-- CreateEmployeeUseCase.cs
|   |   +-- StampAttendanceUseCase.cs
|   |   +-- ...
|   +-- DTOs/
|       +-- EmployeeDto.cs
|       +-- ...
+-- Infrastructure/                      # Driven Adapters
|   +-- Persistence/
|   |   +-- AppDbContext.cs
|   |   +-- EmployeeRepository.cs       # Application/Interfaces/Repositories/IEmployeeRepository の実装
|   |   +-- ...
|   +-- Configurations/
|       +-- EmployeeConfiguration.cs
|       +-- ...
+-- Web/                                 # Driving Adapters
    +-- Controllers/
    |   +-- EmployeesController.cs       # Application/Interfaces/UseCases/ICreateEmployeeUseCase を呼ぶ
    |   +-- ...
    +-- ViewModels/
    +-- FormModels/
    +-- Views/
```

**特徴**: Domain層はピュアなビジネスロジックのみ。ポート定義はApplication層に集約。フォルダ名は具体的で直感的。

---

## パターンC: Domain層にPort配置 + 機能ベース命名（ハイブリッド）

```
HrAttendance/
+-- Domain/                              # コア（ビジネスロジック）
|   +-- Entities/
|   |   +-- Employee.cs
|   |   +-- ...
|   +-- ValueObjects/
|   |   +-- EmployeeId.cs              # 既存（変更なし）
|   |   +-- ...
|   +-- Services/
|   |   +-- AttendanceDomainService.cs
|   |   +-- MonthlyAttendanceDomainService.cs
|   +-- Repositories/                    # ★ Output Ports（リポジトリIF）
|       +-- IEmployeeRepository.cs
|       +-- IDepartmentRepository.cs
|       +-- IAttendanceRepository.cs
|       +-- IWorkTypeRepository.cs
|       +-- IMonthlyAttendanceQueryService.cs
+-- Application/                         # ユースケース層
|   +-- UseCases/                        # Input Ports（インターフェース + 実装を同居）
|   |   +-- ICreateEmployeeUseCase.cs
|   |   +-- CreateEmployeeUseCase.cs
|   |   +-- IStampAttendanceUseCase.cs
|   |   +-- StampAttendanceUseCase.cs
|   |   +-- ...
|   +-- DTOs/
|       +-- EmployeeDto.cs
|       +-- ...
+-- Infrastructure/                      # Driven Adapters
|   +-- Persistence/
|   |   +-- AppDbContext.cs
|   |   +-- EmployeeRepository.cs       # Domain/Repositories/IEmployeeRepository の実装
|   |   +-- ...
|   +-- Configurations/
|       +-- EmployeeConfiguration.cs
|       +-- ...
+-- Web/                                 # Driving Adapters
    +-- Controllers/
    |   +-- EmployeesController.cs
    |   +-- ...
    +-- ViewModels/
    +-- FormModels/
    +-- Views/
```

**特徴**: Domain層にリポジトリIFを配置（DDD慣習に近い）。フォルダ名は機能ベースで直感的。ヘキサゴナルの構造は保ちつつ、用語は抑えめ。

---

## Clarification Question 1
上記3パターンのうち、どれを採用しますか？

A) パターンA: Domain層にPort + ヘキサゴナル用語（教科書的）
B) パターンB: Application層にPort + 機能ベース命名（ポート集約型）
C) パターンC: Domain層にPort + 機能ベース命名（DDD寄りハイブリッド）
D) Other (please describe after [Answer]: tag below)

[Answer]:
