# PR Review: feat: add PhoneNumber value object

**PR**: test/review-pr-skill -> master
**Author**: baki504
**変更規模**: +5,723 / -0 (ただしアプリケーションコードは PhoneNumber.cs: +32行, PhoneNumberTests.cs: +35行。残りは `.claude/skills/skill-creator/` 配下のツールファイル)

> 本レビューはPRの本質的な変更である `PhoneNumber` 値オブジェクトとそのテストに焦点を当てます。`.claude/skills/skill-creator/` 配下のファイルはツール関連の追加であり、ドメインコードのレビュー対象外とします。

---

## スコア

- **総合スコア**: 3 / 10
- **指摘件数**: :red_circle:高 4件 / :yellow_circle:中 3件 / :green_circle:低 1件

---

## 概要

PhoneNumber 値オブジェクトの実装は、プロジェクト内の既存値オブジェクト（`EmployeeId`, `DepartmentCode`）の設計規約から大きく逸脱しています。不変性の欠如、命名規約違反、バリデーション不足、`IEquatable<T>` 未実装など、値オブジェクトとして基本的な品質要件を満たしていません。テストコードも命名規約に従っておらず、網羅性が不足しています。

---

## 指摘事項

### 1. 値オブジェクトが不変（immutable）でない

- **優先度**: :red_circle:高
- **理由**: `value` プロパティに `set` アクセサがあるため、生成後に値を変更できてしまう。値オブジェクトの最も重要な性質である不変性が破られている。既存の `EmployeeId` や `DepartmentCode` はすべて `{ get; }` のみで不変になっている。
- **対処法**: プロパティを `public string Value { get; }` に変更し、コンストラクタを `private` にする。
- **効果**: 値オブジェクトの不変条件が保証され、安全に共有・比較できるようになる。

```csharp
// Before
public string value { get; set; }

// After
public string Value { get; }
```

### 2. プロパティ名が命名規約に違反している

- **優先度**: :red_circle:高
- **理由**: public プロパティが `value`（camelCase）になっている。プロジェクトの命名規約および既存コードでは public メンバーは PascalCase（`Value`）を使用している。
- **対処法**: `value` を `Value` にリネームする。
- **効果**: プロジェクト全体の一貫性が保たれ、可読性が向上する。

### 3. ファクトリメソッド名が規約に従っていない

- **優先度**: :red_circle:高
- **理由**: バックエンドレビュー観点で「ファクトリメソッド名が `Of` または `From~` で統一されているか（`Create` は使わない）」と明記されている。既存の `EmployeeId.Of()`, `DepartmentCode.Of()` もこの規約に準拠している。
- **対処法**: `Create` を `Of` にリネームする。コンストラクタを `private` にする（既存パターンに合わせる）。
- **効果**: プロジェクト全体のAPI設計の一貫性が保たれる。

```csharp
// Before
public static PhoneNumber Create(string value)

// After
public static PhoneNumber Of(string value)
```

### 4. 電話番号のバリデーションが不足している

- **優先度**: :red_circle:高
- **理由**: null チェックのみで、空文字、ホワイトスペース、不正な形式（アルファベットや記号の混入）を許容してしまう。`using System.Text.RegularExpressions;` をインポートしているにもかかわらず正規表現によるバリデーションが実装されていない。`DepartmentCode` では形式・長さの両方をバリデーションしている。
- **対処法**: `string.IsNullOrWhiteSpace` チェックと、電話番号として有効な形式の正規表現バリデーションを追加する。
- **効果**: ドメインの不変条件が保証され、不正な値がシステムに入り込むのを防げる。

### 5. `IEquatable<PhoneNumber>` インターフェースが未実装

- **優先度**: :yellow_circle:中
- **理由**: `Equals(PhoneNumber?)` メソッドは定義されているが、`IEquatable<PhoneNumber>` インターフェースを明示的に実装していない。また `override Equals(object?)` も未実装。既存の `EmployeeId`, `DepartmentCode` はすべて `IEquatable<T>` を実装し、`override Equals(object?)` も定義している。
- **対処法**: クラス宣言に `: IEquatable<PhoneNumber>` を追加し、`override Equals(object?)` を実装する。
- **効果**: コレクション内での比較やDictionary のキーとして正しく動作する。

```csharp
// Before
public class PhoneNumber

// After
public sealed class PhoneNumber : IEquatable<PhoneNumber>
```

### 6. `sealed` 修飾子が欠如している

- **優先度**: :yellow_circle:中
- **理由**: 既存の値オブジェクトはすべて `sealed` クラスとして定義されている。値オブジェクトは継承されるべきではない。
- **対処法**: `public sealed class PhoneNumber` にする。
- **効果**: 値オブジェクトの等価性の契約が継承により破壊されるリスクを防止できる。

### 7. テストメソッド名が規約に従っていない

- **優先度**: :yellow_circle:中
- **理由**: テストの命名規約は「何を」「どの条件で」「どうなるか」を表すこと（例: `CreateEmployee_WithDuplicateEmail_ThrowsException`）とされている。`TestCreate`, `TestNull`, `TestEquals` は何をテストしているのか不明確。
- **対処法**: 以下のようにリネームする。
  - `TestCreate` -> `Of_WithValidPhoneNumber_ReturnsPhoneNumberWithValue`
  - `TestNull` -> `Of_WithNull_ThrowsArgumentException`
  - `TestEquals` -> `Equals_WithSameValue_ReturnsTrue`
  - `TestNotEquals` -> `Equals_WithDifferentValue_ReturnsFalse`
- **効果**: テストの意図が名前から即座に理解でき、失敗時のデバッグが容易になる。

### 8. `ToString()` メソッドが未実装

- **優先度**: :green_circle:低
- **理由**: 既存の値オブジェクト（`EmployeeId`, `DepartmentCode`）はすべて `ToString()` をオーバーライドしている。デバッグやログ出力時に有用。
- **対処法**: `public override string ToString() => Value;` を追加する。
- **効果**: デバッグ・ログ出力時に分かりやすい表示が得られる。

---

## 補足

- **未使用 using**: `System.Text.RegularExpressions` がインポートされているが使用されていない。バリデーション追加時に使用するか、不要なら削除すべき。
- **テストの網羅性不足**: 空文字、ホワイトスペースのみ、不正形式（`"abc"`, `"123"`）などの異常系・境界値テストが欠けている。バリデーション追加後にこれらのテストケースも追加すべき。`[Theory]` + `[InlineData]` によるパラメータ化テストの活用も検討すべき。
- **`.claude/skills/skill-creator/` 配下のファイル群**: PRの説明文（"Add PhoneNumber value object and unit tests."）と一致しないファイルが大量に含まれている。PRのスコープを明確にするため、別PRに分離することを推奨する。
