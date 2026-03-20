# 要件確認 - 追加質問

回答の分析で以下の点を確認させてください。

---

## 曖昧さ 1: SQL Server on macOS

Q5で「SQL Server」を選択されましたが、「macOSで使えたっけ？」とコメントがありました。
SQL ServerはmacOS上ではネイティブ動作しませんが、**Docker（Azure SQL Edge / SQL Server Linuxコンテナ）** で利用可能です。

### 確認質問 1

データベースはどうしますか？

A) SQL Server を Docker コンテナで利用する（Azure SQL Edge）
B) 開発環境では SQLite を使い、将来的に SQL Server へ移行する構成にする
C) PostgreSQL に変更する（macOSネイティブ対応、.NETとの相性も良好）
D) Other (please describe after [Answer]: tag below)

[Answer]: A

---

## 曖昧さ 2: PoC + セキュリティルール全適用

Q9で「PoC」を選択しつつ、Q10でセキュリティルール「全適用」を選択されています。
PoC段階ではセキュリティルール全適用により開発速度が低下する可能性があります。

### 確認質問 2

セキュリティルールの適用方針はどうしますか？

A) PoC段階でもセキュリティルールを全適用する（意図通り）
B) PoC段階ではセキュリティルールをスキップし、MVP以降で適用する
C) Other (please describe after [Answer]: tag below)

[Answer]: B
