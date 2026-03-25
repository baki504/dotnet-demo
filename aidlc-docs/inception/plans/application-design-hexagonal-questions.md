# Application Design - ヘキサゴナルアーキテクチャ設計質問

現在のDDD 4層アーキテクチャからヘキサゴナルアーキテクチャ（Ports & Adapters）に変更するにあたり、以下の設計判断が必要です。

## Question 1

Input Port（ユースケースの入口）の設計方針はどちらにしますか？

A) Use Case Interface を定義する（ICreateEmployeeUseCase, IStampAttendanceUseCase 等のインターフェースを定義し、Application層の具象クラスが実装）
B) Application Service をそのまま使う（現在のEmployeeAppService等をそのままInput Portとして扱い、インターフェースは定義しない）
C) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 2

プロジェクト構成はどうしますか？（現在は HrAttendance/ 1プロジェクトに Domain/Application/Infrastructure/Web フォルダ）

A) 単一プロジェクト・フォルダ分離（現在と同じ。Domain/Application/Infrastructure/Web フォルダで分離。PoC向き）
B) マルチプロジェクト分離（HrAttendance.Domain, HrAttendance.Application, HrAttendance.Infrastructure, HrAttendance.Web を別プロジェクトに。参照制約でレイヤー違反をコンパイル時に検出）
C) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 3

Output Port（driven port）のインターフェース配置場所はどこにしますか？

A) Domain層に配置（現在の設計と同じ。IEmployeeRepository 等は Domain/Ports/ に配置）
B) Application層に配置（ユースケースが必要とするポートはApplication層で定義。Domain層はエンティティ・VOに専念）
C) Other (please describe after [Answer]: tag below)

[Answer]:C 簡単なディレクトリ構成例を見て判断したい

## Question 4

フォルダ命名はヘキサゴナル用語に合わせますか？

A) ヘキサゴナル用語（Domain/Ports/, Infrastructure/Adapters/ など Ports & Adapters の用語を使用）
B) 機能ベース（Domain/Repositories/, Infrastructure/Persistence/ など具体的な機能名を使用。ヘキサゴナルの構造は保ちつつ、名前は直感的に）
C) Other (please describe after [Answer]: tag below)

[Answer]: Q3と同じ
