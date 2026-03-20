# セルフレビュー観点: フロントエンド（Web / MVC層）

## Controller
- [ ] Controller が薄いか（ビジネスロジック・データアクセスを含んでいないか）
- [ ] Controller の責務: リクエスト受付 → バリデーション → FormModel/DTO変換 → AppService呼び出し → ViewModel変換 → View返却
- [ ] 全 Action が async Task<IActionResult> を返しているか
- [ ] ModelState.IsValid を POST/PUT アクションで確認しているか
- [ ] 適切な HTTP ステータスコード / リダイレクトを返しているか
- [ ] [Authorize] / [AllowAnonymous] が適切に設定されているか
- [ ] [ValidateAntiForgeryToken] が POST アクションに設定されているか

## ViewModel / FormModel
- [ ] ViewModel が View に必要なデータのみを持つか（過剰なプロパティがないか）
- [ ] FormModel に適切な DataAnnotations バリデーション属性があるか（[Required], [MaxLength], [RegularExpression] 等）
- [ ] ViewModel にビジネスロジックが含まれていないか
- [ ] FormModel と DTO の変換ロジックが Controller 内に閉じているか

## Razor View
- [ ] View に複雑な C# ロジックが含まれていないか（ViewModel またはヘルパーに移す）
- [ ] Tag Helper（asp-for, asp-validation-for, asp-action）を使用しているか
- [ ] @Html.AntiForgeryToken() または asp-antiforgery が全 POST フォームにあるか
- [ ] 出力が Razor のデフォルトエンコードで XSS 対策されているか（@Html.Raw の使用を避ける）
- [ ] Partial View / ViewComponent で再利用可能な部分を共通化しているか
- [ ] _ViewImports.cshtml で共通の using / Tag Helper が定義されているか

## HTML / アクセシビリティ
- [ ] セマンティックHTML を使用しているか（nav, main, section, article, header, footer）
- [ ] 見出しの階層が正しいか（h1 → h2 → h3、スキップしない）
- [ ] フォーム入力に label 要素が関連付けられているか（asp-for で自動生成含む）
- [ ] 画像に alt 属性があるか
- [ ] ARIA 属性が必要な箇所で使われているか（動的コンテンツ、モーダル等）
- [ ] キーボード操作で全機能にアクセスできるか

## テスト容易性
- [ ] インタラクティブ要素（ボタン、入力、リンク、フォーム）に data-testid 属性があるか
- [ ] data-testid の命名が一貫しているか（{component}-{element-role} 形式）
- [ ] data-testid が動的生成でなく安定した値か

## セキュリティ
- [ ] Cookie に HttpOnly / Secure / SameSite 属性が設定されているか
- [ ] エラー画面でスタックトレースやシステム情報が表示されないか
- [ ] ユーザー入力がそのまま HTML に出力されていないか
