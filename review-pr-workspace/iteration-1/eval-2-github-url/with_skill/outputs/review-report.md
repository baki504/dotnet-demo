# PR Review: feat: add PhoneNumber value object

**PR**: https://github.com/baki504/dotnet-demo/pull/1
**Author**: baki504
**Base**: master <- test/review-pr-skill
**変更ファイル**: 20ファイル (+5723/-0)

> **レビュー対象の絞り込み**: 本PRは skill-creator ツール一式（18ファイル）と PhoneNumber 値オブジェクト（2ファイル）を追加している。skill-creator はサードパーティツールの取り込みであるため、レビューは PhoneNumber 値オブジェクトとそのテストに集中する。

---

## スコア

- **総合スコア**: 3 / 10
- **指摘件数**: :red_circle:高 5件 / :yellow_circle:中 3件 / :green_circle:低 2件

---

## 概要

PhoneNumber 値オブジェクトの実装は、同プロジェクト内の既存値オブジェクト（`EmployeeId`, `DepartmentCode`）が確立している設計パターン（`sealed class`, `IEquatable<T>`, private コンストラクタ, `Of` ファクトリメソッド, `PascalCase` プロパティ）にほぼ全て違反している。値オブジェクトとしての基本要件である不変性も満たされておらず、電話番号のフォーマットバリデーションも欠落しているため、設計・品質の両面で重大な改善が必要である。

---

## 指摘事項

### 1. 値オブジェクトが不変（immutable）でない

- **優先度**: :red_circle:高
- **理由**: `value` プロパティが `{ get; set; }` で宣言されており、生成後に値を変更できてしまう。値オブジェクトの最も基本的な要件である不変性が破られている。既存の `EmployeeId` や `DepartmentCode` は全て `{ get; }` のみ。
- **対処法**: プロパティを `public string Value { get; }` に変更し、コンストラクタ内でのみ代入する。
- **効果**: 値オブジェクトとしての不変性が保証され、スレッドセーフかつ予測可能な動作になる。

### 2. ファクトリメソッド名が `Create` で、プロジェクト規約の `Of` に違反

- **優先度**: :red_circle:高
- **理由**: `.review-prompts/backend.md` の値オブジェクト観点に「ファクトリメソッド名が `Of` または `From~` で統一されているか（`Create` は使わない）」と明記されている。既存の `EmployeeId.Of()`, `DepartmentCode.Of()` も同パターンに従っている。
- **対処法**: `Create` を `Of` にリネームする。
- **効果**: プロジェクト全体の命名一貫性が保たれる。

### 3. コンストラクタが `public` で公開されている

- **優先度**: :red_circle:高
- **理由**: `.review-prompts/backend.md` に「エンティティの生成はファクトリメソッド経由か（コンストラクタの直接公開を避ける）」とあり、値オブジェクトも同様。既存の `EmployeeId`, `DepartmentCode` は全て `private` コンストラクタ。`public` コンストラクタが存在するとファクトリメソッドのバリデーションをバイパスして生成できてしまう。
- **対処法**: コンストラクタを `private PhoneNumber(string value)` に変更する。
- **効果**: 不正な値の PhoneNumber インスタンスが生成される経路を排除できる。

### 4. `IEquatable<PhoneNumber>` を実装していない

- **優先度**: :red_circle:高
- **理由**: 既存の値オブジェクトは全て `IEquatable<T>` を実装しており、`Equals(object?)` のオーバーライドも持つ。現在の実装は `Equals(PhoneNumber?)` メソッドはあるが、インターフェースを宣言していないため、`object.Equals()` 経由での呼び出しやコレクション内での比較が正しく動作しない。
- **対処法**: クラス宣言を `public sealed class PhoneNumber : IEquatable<PhoneNumber>` にし、`public override bool Equals(object? obj) => Equals(obj as PhoneNumber);` を追加する。
- **効果**: コレクション操作や LINQ クエリで等価比較が正しく動作する。

### 5. `sealed` 修飾子が付いていない

- **優先度**: :red_circle:高
- **理由**: 既存の値オブジェクトは全て `sealed` で宣言されている。値オブジェクトは継承されるべきでなく、`sealed` にすることで `Equals` / `GetHashCode` の正しさも担保しやすくなる。
- **対処法**: `public sealed class PhoneNumber : IEquatable<PhoneNumber>` に変更する。
- **効果**: 意図しない継承を防ぎ、等価比較の一貫性を保つ。

### 6. 電話番号のフォーマットバリデーションがない

- **優先度**: :yellow_circle:中
- **理由**: null チェックのみで、空文字列や不正な形式（例: "abc", "12345"）も受け入れてしまう。`DepartmentCode` が正規表現で形式チェックを行っているように、電話番号にも形式の妥当性検証が必要。`using System.Text.RegularExpressions;` が import されているのにバリデーションに使われていない。
- **対処法**: 日本の電話番号形式（`0X0-XXXX-XXXX` 等）を正規表現でバリデーションする。空文字列チェックも追加する（`string.IsNullOrWhiteSpace`）。
- **効果**: 不正な電話番号がドメインモデルに混入するのを防ぐ。

### 7. プロパティ名が `value`（camelCase）で命名規則に違反

- **優先度**: :yellow_circle:中
- **理由**: C# の public プロパティは PascalCase が規約であり、プロジェクト内の既存値オブジェクトも全て `Value`（PascalCase）を使用している。`.review-prompts/common.md` にも「PascalCase: public メンバー、クラス、メソッド、プロパティ」と明記。
- **対処法**: `public string value` を `public string Value` に変更する。
- **効果**: C# の標準規約およびプロジェクト規約に準拠する。

### 8. `ToString()` オーバーライドが未実装

- **優先度**: :yellow_circle:中
- **理由**: 既存の `EmployeeId`, `DepartmentCode` は全て `ToString()` をオーバーライドして値を返している。デバッグやログ出力で有用。
- **対処法**: `public override string ToString() => Value;` を追加する。
- **効果**: デバッグ・ログ出力時の可読性が向上し、プロジェクトの一貫性が保たれる。

### 9. テストメソッド名がプロジェクトの命名規約に準拠していない

- **優先度**: :green_circle:低
- **理由**: `.review-prompts/test.md` に「テストメソッド名が『何を』『どの条件で』『どうなるか』を表しているか（例: CreateEmployee_WithDuplicateEmail_ThrowsException）」と記載されている。現在の `TestCreate`, `TestNull`, `TestEquals`, `TestNotEquals` は何をテストしているか不明瞭。
- **対処法**: 以下のようにリネームする:
  - `TestCreate` -> `Of_WithValidPhoneNumber_ReturnsPhoneNumberWithCorrectValue`
  - `TestNull` -> `Of_WithNull_ThrowsArgumentException`
  - `TestEquals` -> `Equals_WithSameValue_ReturnsTrue`
  - `TestNotEquals` -> `Equals_WithDifferentValue_ReturnsFalse`
- **効果**: テスト失敗時にどのケースが落ちたか即座に判断できる。

### 10. テストケースの網羅性が不足

- **優先度**: :green_circle:低
- **理由**: 空文字列、空白のみ、不正形式の電話番号など、境界値・異常系のテストが不足している。バリデーション実装後はこれらのテストも必要になる。また、`TestNull` と `TestCreate` は `[Theory]` + `[InlineData]` でパラメータ化すべきケース。
- **対処法**: バリデーション実装に合わせて、空文字列・空白のみ・不正形式・正常な各種形式のテストを `[Theory]` で追加する。
- **効果**: バリデーションロジックの正しさを保証し、リグレッションを防止する。

---

## 補足

- skill-creator ツール一式（18ファイル、5,656行）がこの PR に含まれているが、PhoneNumber 値オブジェクトの追加とは無関係に見える。関心の分離の観点から、別の PR に分割することを推奨する。
- `PhoneNumber.cs` に `using System.Text.RegularExpressions;` がインポートされているが使用されていない。バリデーション実装時に使用するか、不要なら削除すべき。
- `PhoneNumber.cs` のコメント `// phone number value object` は、クラス名から自明であり不要なコメントに該当する（`.review-prompts/common.md`: 「不要なコメントがないか（コードが自己文書化されているか）」）。
