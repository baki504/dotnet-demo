# Application Design 計画 - 人事・勤怠管理システム (PoC)

## 設計計画

### INCEPTION - Application Design

- [x] コンポーネント定義（components.md）
  - [x] エンティティ（データモデル）の特定
  - [x] リポジトリ層の定義
  - [x] サービス層の定義
  - [x] Razor Pages（UI層）の定義
- [x] コンポーネントメソッド定義（component-methods.md）
  - [x] 各コンポーネントのメソッドシグネチャ
  - [x] 入出力の型定義
- [x] サービス定義（services.md）
  - [x] サービスの責務とオーケストレーション
- [x] コンポーネント依存関係（component-dependency.md）
  - [x] 依存関係マトリクス
  - [x] データフロー
- [x] 設計の整合性検証

---

## 設計に関する質問

以下の質問に回答してください。`[Answer]:` タグの後に選択肢を記入してください。

### Question 1

プロジェクト構成はどうしますか？

A) 単一プロジェクト（Web + データアクセス + モデルすべて1プロジェクト）- PoC向け、シンプル
B) レイヤー分離（Web / Application / Domain / Infrastructure の複数プロジェクト）- 拡張性高い
C) Other (please describe after [Answer]: tag below)

[Answer]: DDDに合わせた構成。設計思想もDDDをベースに

### Question 2

勤怠データの粒度はどのレベルですか？

A) 日単位（1日1レコード：出勤時刻・退勤時刻）
B) 打刻単位（出勤・退勤・休憩開始・休憩終了をそれぞれ記録）
C) Other (please describe after [Answer]: tag below)

[Answer]: B

### Question 3

社員と部署の関係はどうしますか？

A) 1社員 = 1部署（シンプルな所属）
B) 1社員 = 複数部署（兼務対応）
C) Other (please describe after [Answer]: tag below)

[Answer]: B

### Question 4

管理者と一般社員の権限分離はどうしますか？

A) ロールベース（Admin / User の2ロール）- 管理者は全社員の勤怠閲覧・編集可能
B) 部署ベース（部署管理者は自部署のみ閲覧・編集可能）
C) PoC段階では権限分離なし（全員が全機能を利用可能）
D) Other (please describe after [Answer]: tag below)

[Answer]: A
