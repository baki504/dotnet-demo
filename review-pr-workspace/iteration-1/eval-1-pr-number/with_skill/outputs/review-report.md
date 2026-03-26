# PR #1 レビュー: feat: add PhoneNumber value object

## スコア

- **総合スコア**: 3 / 10
- **指摘件数**: 🔴高 5件 / 🟡中 3件 / 🟢低 1件

## 概要

PhoneNumber 値オブジェクトの追加PRだが、既存の値オブジェクト（EmployeeId, DepartmentCode）で確立されたプロジェクト規約に複数違反している。値オブジェクトの不変性が守られておらず、ファクトリメソッド名の規約違反、`IEquatable<T>` 未実装、バリデーション不足など、設計・品質に重大な懸念がある。テストコードも網羅性・命名規約の面で改善が必要。

## 指摘事項

### 1. 値オブジェクトが不変（immutable）でない

- **優先度**: 🔴高
- **理由**: `value` プロパティに `set` アクセサがあり、生成後に値を変更できてしまう。値オブジェクトは不変であることが原則であり、既存の EmployeeId / DepartmentCode はすべて `get` のみで定義されている。
- **対処法**: `HrAttendance/Domain/ValueObjects/PhoneNumber.cs:8` を以下のように修正する。
  ```csharp
  public string Value { get; }
  ```
- **効果**: 値オブジェクトの不変性が保証され、予期しない状態変更によるバグを防止できる。

### 2. ファクトリメソッド名が規約に違反している

- **優先度**: 🔴高
- **理由**: プロジェクト規約（`.review-prompts/backend.md`）では、値オブジェクトのファクトリメソッド名は `Of` または `From~` で統一すると定めている。既存の EmployeeId は `Of`、DepartmentCode も `Of` を使っている。しかし PhoneNumber は `Create` を使用している。
- **対処法**: `HrAttendance/Domain/ValueObjects/PhoneNumber.cs:17` のメソッド名を `Create` から `Of` に変更する。
  ```csharp
  public static PhoneNumber Of(string value)
  ```
- **効果**: プロジェクト全体で一貫したAPI設計が維持される。

### 3. コンストラクタが public になっている

- **優先度**: 🔴高
- **理由**: 既存の値オブジェクト（EmployeeId, DepartmentCode）ではコンストラクタを `private` にし、ファクトリメソッド経由でのみ生成する設計になっている。PhoneNumber はコンストラクタが `public` であり、ファクトリメソッドを迂回してインスタンスを生成できてしまう。
- **対処法**: `HrAttendance/Domain/ValueObjects/PhoneNumber.cs:10` のコンストラクタを `private` に変更する。
  ```csharp
  private PhoneNumber(string value)
  ```
- **効果**: バリデーションを迂回した不正なインスタンス生成を防止できる。

### 4. IEquatable\<PhoneNumber\> を実装していない

- **優先度**: 🔴高
- **理由**: 既存の値オブジェクトはすべて `IEquatable<T>` を実装しており、`Equals(T?)` と `Equals(object?)` の両方をオーバーライドしている。PhoneNumber は `Equals(PhoneNumber?)` メソッドを定義しているが、インターフェースを実装しておらず、`Equals(object?)` のオーバーライドも欠落している。このため `object.Equals` 経由の比較（コレクション内の検索等）が正しく動作しない。
- **対処法**: クラス宣言を以下のように変更し、`Equals(object?)` オーバーライドを追加する。
  ```csharp
  public sealed class PhoneNumber : IEquatable<PhoneNumber>
  ```
  ```csharp
  public override bool Equals(object? obj) => Equals(obj as PhoneNumber);
  ```
- **効果**: コレクション操作や辞書のキーとして使用した際の等価比較が正しく動作する。

### 5. 電話番号のフォーマットバリデーションがない

- **優先度**: 🔴高
- **理由**: 既存の DepartmentCode では文字種・長さのバリデーションを行っているが、PhoneNumber は null チェックのみで、空文字列や不正なフォーマットの値を受け入れてしまう。`System.Text.RegularExpressions` を using しているが実際には使用していない。
- **対処法**: 空文字列チェックと電話番号フォーマットのバリデーションを追加する。少なくとも `string.IsNullOrWhiteSpace` チェックを行い、電話番号として妥当な正規表現パターンで検証する。
  ```csharp
  if (string.IsNullOrWhiteSpace(value))
      throw new ArgumentException("Phone number cannot be null or empty.", nameof(value));
  ```
- **効果**: 不正な値を持つ値オブジェクトの生成を防止し、ドメインの不変条件を保証する。

### 6. プロパティ名が PascalCase になっていない

- **優先度**: 🟡中
- **理由**: `value` プロパティが camelCase で定義されているが、C# の public プロパティは PascalCase（`Value`）が規約である。既存の値オブジェクトもすべて `Value` を使用している。
- **対処法**: `HrAttendance/Domain/ValueObjects/PhoneNumber.cs:8` のプロパティ名を `Value` に変更する。
- **効果**: プロジェクト全体の命名規則の一貫性が保たれる。

### 7. クラスが sealed でない

- **優先度**: 🟡中
- **理由**: 既存の値オブジェクト（EmployeeId, DepartmentCode）はすべて `sealed` クラスとして定義されている。PhoneNumber は `sealed` が付いておらず、継承が可能な状態になっている。値オブジェクトは継承される想定がないため `sealed` にすべき。
- **対処法**: クラス宣言に `sealed` を追加する。
  ```csharp
  public sealed class PhoneNumber : IEquatable<PhoneNumber>
  ```
- **効果**: 意図しない継承を防止し、値オブジェクトの設計意図を明確にする。

### 8. テストメソッド名が命名規約に従っていない

- **優先度**: 🟡中
- **理由**: テスト命名規約（`.review-prompts/test.md`）では「何を」「どの条件で」「どうなるか」を表す名前（例: `CreateEmployee_WithDuplicateEmail_ThrowsException`）を推奨している。`TestCreate`、`TestNull`、`TestEquals`、`TestNotEquals` はいずれも意図が不明瞭。
- **対処法**: 以下のように改名する。
  - `TestCreate` -> `Of_WithValidPhoneNumber_ReturnsPhoneNumberWithCorrectValue`
  - `TestNull` -> `Of_WithNull_ThrowsArgumentException`
  - `TestEquals` -> `Equals_WithSameValue_ReturnsTrue`
  - `TestNotEquals` -> `Equals_WithDifferentValue_ReturnsFalse`
- **効果**: テストの意図が名前から読み取れるようになり、失敗時の原因特定が容易になる。

### 9. テストケースの網羅性不足

- **優先度**: 🟢低
- **理由**: 空文字列、空白のみ、フォーマット不正な入力に対するテストケースがない。バリデーション実装後にこれらの異常系・境界値テストが必要になる。また、`GetHashCode` の一貫性テスト（等しいオブジェクトが同じハッシュコードを返すこと）もない。
- **対処法**: バリデーション実装に合わせて、以下のテストケースを追加する。
  - 空文字列 -> ArgumentException
  - 空白のみ -> ArgumentException
  - 不正フォーマット -> ArgumentException
  - `GetHashCode` の等価性テスト
- **効果**: バリデーションロジックの正しさが保証され、回帰バグを防止できる。

## 補足

- PR には `.claude/skills/skill-creator/` 配下の18ファイル（5,655行追加）が含まれているが、これはスキル定義ファイル群であり、PhoneNumber 値オブジェクトの追加とは無関係のため本レビューの対象外とした。PRのスコープを絞って関連する変更のみを含めることを推奨する。
- `ToString()` メソッドのオーバーライドが欠落している（既存の値オブジェクトにはすべて存在する）。デバッグ時の可読性向上のため追加を推奨する。
- 未使用の `using System.Text.RegularExpressions;` が残っている。バリデーション追加時に使用するなら残してもよいが、現状では不要。
