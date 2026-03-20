# Requirements Clarification Questions

以下の質問に回答して、業務システムの要件を明確にしてください。
各質問の `[Answer]:` タグの後に選択肢の文字を記入してください。

---

## Question 1

どのような業務ドメインのシステムですか？

A) 販売管理（受注・売上・請求）
B) 在庫管理（入出庫・棚卸）
C) 顧客管理（CRM）
D) 人事・勤怠管理
E) 会計・経理
F) Other (please describe after [Answer]: tag below)

[Answer]: D。

## Question 2

システムの主な利用者は誰ですか？

A) 社内の一般社員（数十名規模）
B) 社内の一般社員（数百名以上）
C) 社外の取引先・顧客も利用する
D) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 3

技術スタック（バックエンド）の希望はありますか？

A) .NET (C#)
B) Java (Spring Boot)
C) Python (Django/FastAPI)
D) Node.js (TypeScript)
E) 特にこだわりなし（おすすめを提案してほしい）
F) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 4

フロントエンドの形態はどうしますか？

A) Webアプリケーション（SPA - React/Vue/Angular等）
B) Webアプリケーション（サーバーサイドレンダリング - Blazor/Razor等）
C) デスクトップアプリケーション
D) モバイルアプリケーション
E) API のみ（フロントエンドは別途）
F) Other (please describe after [Answer]: tag below)

[Answer]: B → Razor Pages に変更（将来Blazor併用も検討）

## Question 5

データベースの希望はありますか？

A) PostgreSQL
B) MySQL / MariaDB
C) SQL Server
D) SQLite（小規模・開発用）
E) 特にこだわりなし
F) Other (please describe after [Answer]: tag below)

[Answer]: C(macOSでつかえたっけ？)

## Question 6

認証・認可の要件はありますか？

A) ユーザー名/パスワードによるログイン
B) SSO（シングルサインオン - Azure AD/SAML等）
C) OAuth2/OpenID Connect
D) 認証不要（社内ネットワーク限定）
E) Other (please describe after [Answer]: tag below)

[Answer]: C

## Question 7

主要な機能として必要なものを選んでください（複数回答可、カンマ区切り）

A) マスタデータ管理（CRUD画面）
B) 一覧・検索・フィルタリング
C) 帳票・レポート出力（PDF/Excel）
D) ワークフロー・承認フロー
E) ダッシュボード・集計表示
F) 外部システム連携（API）
G) Other (please describe after [Answer]: tag below)

[Answer]: A,B

## Question 8

デプロイ先の環境はどこを想定していますか？

A) クラウド（AWS）
B) クラウド（Azure）
C) クラウド（GCP）
D) オンプレミス
E) ローカル開発環境のみ（まずは動くものを作りたい）
F) Other (please describe after [Answer]: tag below)

[Answer]: E

## Question 9

プロジェクトの規模感・スコープはどの程度ですか？

A) プロトタイプ・PoC（まず動くものを確認したい）
B) MVP（最小限の機能で本番利用開始したい）
C) 本格的な業務システム（全機能を網羅）
D) Other (please describe after [Answer]: tag below)

[Answer]: A

## Question 10: Security Extensions

セキュリティ拡張ルールをこのプロジェクトに適用しますか？

A) Yes - すべてのSECURITYルールをブロッキング制約として適用する（本番グレードのアプリケーション向け推奨）
B) No - SECURITYルールをスキップする（PoC、プロトタイプ、実験的プロジェクト向け）
C) Other (please describe after [Answer]: tag below)

[Answer]: A
