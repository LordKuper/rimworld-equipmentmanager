---
responsibility:
  owns: project-owner custom rules read during impl and impl-review phases
  excludes: universal rules, design-only rules
  delegates_to: custom-common-rules.md (all phases), custom-design-rules.md (design/design-review)
---

# Custom Coding Rules

Inherited from the `LordKuper.Common` parent library and adapted to EquipmentManager's actual setup.

## Nullability

- **Standard: `<Nullable>enable</Nullable>` for all projects.** Both `EquipmentManager.csproj` and `EquipmentManager.Tests.csproj` have it enabled (production migration completed in sprint 001-full-audit-alignment).
- Use C# nullable reference types (`T?` + guards) for nullability intent. JetBrains nullability attributes (`[CanBeNull]`/`[NotNull]`/`[ItemNotNull]`) MUST NOT be used — they were removed in the migration. Non-nullability JetBrains attributes (`[UsedImplicitly]`, `[Pure]`) are still allowed.
- Scribe-serialized collection fields are declared nullable (`List<…>? = []` / `Dictionary<…>? = new()`) and restored with `??=` in `Initialize()`, because `Scribe_Collections.Look` writes `null` back on empty load. Scalar reference fields set by `Scribe`/reflection use `= null!`. CE reflection-delegate fields stay nullable `T?` with guards.
- Do NOT disable the nullable context anywhere. Do NOT mask nullable warnings with `#pragma warning disable`.

## Zero warnings

- Source builds with `TreatWarningsAsErrors=true` and `WarningLevel 9999` in both Debug and Release. Code MUST compile warning-clean. A warning fails the build. `Microsoft.CodeAnalysis.NetAnalyzers` findings count as warnings.

## Build / lint flow

- Before `build`: run `jb-cleanup` (applies the solution code-cleanup profile).
- After `lint`: run `jb-inspect`, then verify `TestResults/jb-inspect.sarif` has no `error` or `warning` severity entries.
- Commands defined in `.asd/project/commands.yaml` (`jb-cleanup`, `jb-inspect`).

## Analyzer / linter suppressions

- Suppress findings only as a last resort — fix the real issue first.
- Prefer attribute-based suppression (`[SuppressMessage]`, `[UsedImplicitly]`, `[Pure]`) over comment pragmas (`#pragma warning disable`, `// ReSharper disable`). Use comments only when no attribute applies.
- Every suppression MUST carry a real reason saying *why*. "false positive" / "by design" alone is not enough.

## Self-contained code — no design-doc references

- The codebase (Source AND Tests) MUST be self-sufficient without the ASD design docs. Code and comments MUST NOT reference or quote ASD artifacts: ADR, PRD, acceptance criteria (`AC-N`), improvement items (`IMP-N`), `Task N`, sprint ids, or rule-doc filenames (`custom-*-rules.md`).
- Explain the *why* directly in the comment instead of citing a doc.
- The only exception: forward-looking `TODO`/`FIXME` comments MAY reference a sprint/issue for future work.

## Logging

- Use the project `Logger` (`Source/EquipmentManager/Logger.cs`), which wraps `LordKuper.Common.Logger` with the mod id. Actionable, gated, no spam. Prefer it over raw `Verse.Log.*`.
- The in-memory player-facing log buffer (`EquipmentManagerGameComponent.LogMessage`, surfaced by `LogDialog`) is a separate, user-visible feature — do not conflate it with diagnostic logging.

## Testing (NUnit + FluentAssertions)

- Test framework is **NUnit 4.x** (`[Test]`, `[TestCase]`, `[TestFixture]`, `[SetUp]`/`[TearDown]`, `[SetUpFixture]`/`[OneTimeSetUp]`); runner is `NUnit3TestAdapter`; host is `Microsoft.NET.Test.Sdk`. FluentAssertions is pinned to 7.x (Apache-2.0); never float to 8.x (commercial license).
- **All assertions MUST use FluentAssertions (`.Should()`) wherever possible.** Do NOT use NUnit `Assert.*` / `Assert.That` / `ClassicAssert.*` for value, state, or reference checks. Map every common shape to its FluentAssertions form:
  - equality/identity → `actual.Should().Be(expected)` / `.BeSameAs(...)` / `.NotBeNull()`
  - booleans → `flag.Should().BeTrue()/BeFalse()`
  - collections → `coll.Should().BeEmpty()/HaveCount(n)/Contain(x)/BeEquivalentTo(...)/NotContain(...)`
  - exceptions → `act.Should().Throw<T>()` / `.NotThrow()` (wrap the call in an `Action`/`Func`), NOT `Assert.Throws`
  - numeric tolerance → `value.Should().BeApproximately(expected, precision)`
  - reference null-state → `obj.Should().BeNull()/NotBeNull()`
  - The only non-FluentAssertions test constructs allowed are NUnit structural attributes (`[Test]`, `[TestCase]`, `[SetUp]`, `[Ignore("reason")]`, etc.) and genuine non-assertion control flow. `Assert.Fail`/`Assert.Pass`/`Assert.Inconclusive` are NOT assertions of behavior — prefer a real `.Should()` assertion; use `[Ignore]` for legitimately un-runnable tests rather than an empty/`Assert.Pass` body.
  - "Wherever possible" carve-out: only when a check genuinely has no FluentAssertions equivalent (extremely rare) may a non-FA construct be used, with an inline comment stating why.
- Use global `<Using Include="NUnit.Framework" />` and `<Using Include="FluentAssertions" />` rather than per-file `using` directives.
- **Static state isolation** — tests mutating global/cached/static state MUST save/restore via per-test `[SetUp]` (snapshot) / `[TearDown]` (restore) on a shared base class (`StateIsolationTestBase`). Use per-test `[SetUp]`/`[TearDown]` (not per-class `[OneTimeSetUp]`) so each test gets true isolation. NUnit runs non-parallel by default; mark static-touching classes `[NonParallelizable]` and never add `[assembly: Parallelizable]`.
- RimWorld-typed test types require the RimWorld `AppDomain.AssemblyResolve` handler registered before any such type loads — it lives in the namespace-less (global) `[SetUpFixture]` `RimWorldAssemblyResolverFixture`. Do not duplicate or bypass it.
- Do not depend on test execution order.
- RimWorld APIs requiring live game context must be abstracted or guarded; don't call them directly in unit tests without isolation.
