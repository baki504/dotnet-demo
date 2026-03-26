# PR #1 Review Report: feat: add PhoneNumber value object

**PR URL**: https://github.com/baki504/dotnet-demo/pull/1
**Author**: baki504
**State**: OPEN
**Additions**: 5723 | **Deletions**: 0

---

## Summary

This PR adds two categories of changes:

1. **PhoneNumber value object** (`HrAttendance/Domain/ValueObjects/PhoneNumber.cs`) and its unit tests (`HrAttendanceTests/Domain/ValueObjects/PhoneNumberTests.cs`) -- the core feature of the PR.
2. **Skill-creator tooling** (18 files under `.claude/skills/skill-creator/`) -- a Claude skill for creating, evaluating, and improving other skills. This includes agents, scripts, templates, and an eval viewer.

The review below focuses on the PhoneNumber domain code and tests, as the skill-creator files are tooling/configuration and not application code.

---

## PhoneNumber Value Object Review

### Critical Issues

#### 1. Not immutable -- property has public setter
The `value` property is declared as `public string value { get; set; }`. Value objects must be immutable. The setter should be removed or made private/init-only. All existing value objects in the codebase (e.g., `EmployeeId`, `DepartmentCode`) use `{ get; }` only.

**File**: `HrAttendance/Domain/ValueObjects/PhoneNumber.cs`, line with `public string value { get; set; }`

#### 2. Property naming violates project conventions
The property is named `value` (camelCase). Per project conventions (`.review-prompts/common.md`), public properties must use PascalCase. All other value objects use `Value` (PascalCase). This will also break consistency for consumers of the API.

#### 3. Class is not sealed
Existing value objects (`EmployeeId`, `DepartmentCode`) are declared `sealed`. `PhoneNumber` should also be `sealed` to prevent inheritance, which is consistent with value object semantics.

#### 4. Does not implement `IEquatable<PhoneNumber>`
Existing value objects implement `IEquatable<T>` (e.g., `EmployeeId : IEquatable<EmployeeId>`). `PhoneNumber` only has an `Equals(PhoneNumber?)` method but does not declare the interface, which means the type system cannot guarantee equality contract compliance.

#### 5. Missing `override Equals(object?)` method
The class overrides `GetHashCode()` but does not override `Equals(object?)`. This violates the .NET contract that `Equals` and `GetHashCode` must be overridden together, and means `object.Equals()` comparisons will use reference equality while `GetHashCode` uses value equality -- a source of subtle bugs. Both `EmployeeId` and `DepartmentCode` include `public override bool Equals(object? obj) => Equals(obj as PhoneNumber);`.

#### 6. No phone number format validation
The constructor only checks for `null`. There is no validation of phone number format (length, character set, pattern). By contrast, `DepartmentCode` validates length, format, and character set. A phone number value object should validate that the input matches an expected phone number pattern (e.g., Japanese phone numbers like `0XX-XXXX-XXXX`). The `using System.Text.RegularExpressions;` is imported but never used, suggesting format validation was intended but not implemented.

#### 7. Factory method named `Create` instead of `Of`
Per project conventions (`.review-prompts/backend.md`): "Factory method names should be unified as `Of` or `From~` (`Create` is not used)." All existing value objects use `Of`. The method should be renamed from `Create` to `Of`.

#### 8. Constructor is public
The constructor is `public`, allowing direct instantiation without going through the factory method. Existing value objects use `private` constructors to enforce creation via the factory method. The constructor should be `private`.

### Minor Issues

#### 9. Missing `ToString()` override
Existing value objects provide a `ToString()` override. `PhoneNumber` does not.

#### 10. Null check uses `== null` instead of `is null`
In `Equals(PhoneNumber?)`, the null check uses `== null`. The project convention (visible in `EmployeeId` and `DepartmentCode`) uses the pattern-matching `is null` form, which cannot be overloaded and is safer.

#### 11. Comment is generic
The comment `// phone number value object` adds no value. Per `.review-prompts/common.md`: "Are there unnecessary comments? (Is the code self-documenting?)" This comment should be removed.

---

## Test Code Review

### Issues

#### 1. Test method names do not follow conventions
Test names like `TestCreate`, `TestNull`, `TestEquals`, `TestNotEquals` do not describe "what", "under what condition", and "what happens" as required by `.review-prompts/test.md`. Expected pattern: `Create_WithValidPhoneNumber_ReturnsPhoneNumberWithCorrectValue`, `Create_WithNull_ThrowsArgumentException`, etc.

#### 2. Missing test coverage
The tests do not cover:
- **Empty string input** -- should be rejected
- **Whitespace-only input** -- should be rejected
- **Format validation** -- invalid phone number formats (once validation is added)
- **Boundary values** -- minimum/maximum length phone numbers
- **`GetHashCode` consistency** -- equal objects should produce equal hash codes
- **`Equals(object?)` override** -- once added, should be tested
- **Immutability** -- verify the value cannot be changed after creation (once the setter is removed)

#### 3. Tests reference `Create` factory method
Tests use `PhoneNumber.Create(...)` which should be renamed to `PhoneNumber.Of(...)` to match project conventions.

#### 4. Tests reference `phone.value` (camelCase)
Tests reference the camelCase property `phone.value` which should be `phone.Value` after the property is renamed.

#### 5. No use of `[Theory]` with `[InlineData]`
Per `.review-prompts/test.md`: "Are multiple input pattern tests for the same method parameterized using [Theory] + [InlineData]?" The equality tests (`TestEquals`, `TestNotEquals`) could be combined into a `[Theory]` with `[InlineData]`.

---

## Skill-Creator Files

The PR includes 18 files under `.claude/skills/skill-creator/`. These are tooling files (Python scripts, HTML templates, Markdown documentation) for a Claude skill-creation framework. They are not application code and appear to be a standard skill-creator package with Apache 2.0 license. No issues with these files from a code review perspective, though bundling them in the same PR as the PhoneNumber value object makes the PR scope broader than ideal. Consider splitting into separate PRs.

---

## Overall Assessment

**Recommendation: Request Changes**

The `PhoneNumber` value object has significant deviations from the established patterns in this codebase. The issues are not minor style preferences -- they include broken equality semantics (missing `Equals(object?)` override), mutability (public setter), missing validation, and naming convention violations. The test coverage is also insufficient.

### Required Changes Before Merge

1. Make the class `sealed` and implement `IEquatable<PhoneNumber>`
2. Change `value` property to `Value` (PascalCase) with getter only (`{ get; }`)
3. Make the constructor `private`
4. Rename `Create` to `Of`
5. Add `override Equals(object?)` method
6. Add phone number format validation (the regex import is already there)
7. Add `ToString()` override
8. Rename test methods to follow naming conventions
9. Add missing test cases (empty/whitespace, format validation, hash code, boundary values)
10. Remove the generic comment
