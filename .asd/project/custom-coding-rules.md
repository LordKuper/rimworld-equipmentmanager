---
responsibility:
  owns: project-owner custom rules read during impl and impl-review phases
  excludes: universal rules, design-only rules
  delegates_to: custom-common-rules.md (all phases), custom-design-rules.md (design/design-review)
---

# Custom Coding Rules

Inherited from the `LordKuper.Common` parent library and adapted to EquipmentManager's actual setup.

## Nullability

- **Target standard: `<Nullable>enable</Nullable>` for all projects.** The test project (`EquipmentManager.Tests.csproj`) already has it. The production project (`EquipmentManager.csproj`) does NOT yet — flipping it surfaces ~148 nullable warnings that fail the build under `TreatWarningsAsErrors`, so the production migration is tracked as a dedicated sprint, not done ad hoc.
- Until then, production code uses JetBrains `[CanBeNull]`/`[NotNull]` annotations for nullability intent. New/rewritten production code MUST stay annotation-consistent and MUST NOT introduce `T?` reference-type syntax (it errors without the nullable context).
- Once the production migration sprint lands, prefer C# nullable reference types over JetBrains annotations; the two MUST NOT contradict each other. Do NOT disable the nullable context anywhere.

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

- Test framework is **NUnit 4.x** (`[Test]`, `[TestCase]`, `[TestFixture]`, `[SetUp]`/`[TearDown]`, `[SetUpFixture]`/`[OneTimeSetUp]`); runner is `NUnit3TestAdapter`; host is `Microsoft.NET.Test.Sdk`. Assertions are **FluentAssertions 7.x** (`.Should()`) — do NOT use NUnit `Assert.*` / `Assert.That`. FluentAssertions is pinned to 7.x (Apache-2.0); never float to 8.x (commercial license).
- Use global `<Using Include="NUnit.Framework" />` and `<Using Include="FluentAssertions" />` rather than per-file `using` directives.
- **Static state isolation** — tests mutating global/cached/static state MUST save/restore via per-test `[SetUp]` (snapshot) / `[TearDown]` (restore) on a shared base class (`StateIsolationTestBase`). Use per-test `[SetUp]`/`[TearDown]` (not per-class `[OneTimeSetUp]`) so each test gets true isolation. NUnit runs non-parallel by default; mark static-touching classes `[NonParallelizable]` and never add `[assembly: Parallelizable]`.
- RimWorld-typed test types require the RimWorld `AppDomain.AssemblyResolve` handler registered before any such type loads — it lives in the namespace-less (global) `[SetUpFixture]` `RimWorldAssemblyResolverFixture`. Do not duplicate or bypass it.
- Do not depend on test execution order.
- RimWorld APIs requiring live game context must be abstracted or guarded; don't call them directly in unit tests without isolation.
