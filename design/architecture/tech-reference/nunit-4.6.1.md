---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# NUnit @ 4.6.1

## Canonical source
- Official docs: https://docs.nunit.org/
- Breaking changes (4.0): https://docs.nunit.org/articles/nunit/release-notes/breaking-changes.html
- Last verified: 2026-06-06

## API surface used in project
NUnit is imported globally via `<Using Include="NUnit.Framework" />` in `Source/EquipmentManager.Tests/EquipmentManager.Tests.csproj` (no per-file `using NUnit.Framework;` directive). Attributes observed in the existing tests under `Source/EquipmentManager.Tests/`:
- `[TestFixture]`: marks a test class; on `SkillWeightTests`, `PassionLimitTests`.
- `[Test]`: marks a parameterless test method; example: `[Test] public void WeightCap_IsTwo()`.
- `[TestCase]`: parameterized test cases. Permitted by project rules; not yet used in the current tests but reserved for data-driven cases.
- `[SetUp]` / `[TearDown]`: per-test snapshot/restore. Example: `StateIsolationTestBase` uses `[SetUp] SnapshotState()` and `[TearDown] RestoreState()` to snapshot and restore mutable static state around every test.
- `[SetUpFixture]` + `[OneTimeSetUp]`: assembly/namespace-scoped one-time setup. Example: `RimWorldAssemblyResolverFixture` is a `[SetUpFixture]` in the root `EquipmentManager.Tests` namespace whose `[OneTimeSetUp] RegisterAssemblyResolver()` runs once before any test in the namespace or sub-namespaces.
- `[NonParallelizable]`: forces serial execution. Applied to `StateIsolationTestBase` so static-state mutation cannot race across parallel tests.

## Version-specific notes
- 4.6.1 is the confirmed latest release as of 2026-05-19.
- NUnit 4.x minimum supported targets are .NET Framework 4.6.2 and .NET 6.0. This project targets `net48`, which is supported.
- Test discovery/execution requires the VSTest adapter `NUnit3TestAdapter` (despite the "3" in its name it runs NUnit 4 tests) plus `Microsoft.NET.Test.Sdk`. See the `nunit3testadapter-6.2.0` and `microsoft-net-test-sdk-18.6.0` references.

## Deprecations and breaking changes from prior version (3.x -> 4.x)
- Classic asserts moved to a legacy library/namespace: `Assert.AreEqual`, `Assert.IsTrue`, etc. are now `NUnit.Framework.Legacy.ClassicAssert.*`. Standalone helpers `CollectionAssert`, `StringAssert`, `DirectoryAssert`, `FileAssert` also moved to the `NUnit.Framework.Legacy` namespace. Not relevant in practice here because the project asserts with FluentAssertions, not NUnit asserts (see Project conventions).
- `Assert.That` overloads taking a format string + `params object[]` were removed in favor of a `FormattableString`-based overload.
- The constraint model (`Assert.That(actual, Is...)`) is the 4.x-preferred assertion model; classic syntax is legacy.
- Minimum framework versions raised (see Version-specific notes).

## Project conventions
- Assertions use FluentAssertions `.Should()`, NOT NUnit `Assert.*`. The NUnit 3->4 assert migration is therefore moot for new code; do not introduce `Assert.*` or `ClassicAssert.*`. Example from `SkillWeightTests`: `skillWeight.Weight.Should().Be(0f);`. See the `fluentassertions-7.2.2` reference.
- Static-state isolation is mandatory for any test that touches mutable static state. Derive from `StateIsolationTestBase`, which snapshots the private static `_equipmentManager` field of the caching/rule types in `[SetUp]` and restores it in `[TearDown]`. Tests that touch static state must run serially (`[NonParallelizable]`, inherited from the base).
- RimWorld and Unity assemblies are not NuGet packages; they live in the local RimWorld `Managed` directory. A global `[SetUpFixture]` (`RimWorldAssemblyResolverFixture`) registers an `AppDomain.CurrentDomain.AssemblyResolve` handler in `[OneTimeSetUp]` before any RimWorld-typed test loads. The Managed dir is passed in via an `AssemblyMetadata` attribute (`RimWorldManagedDir`) set from the csproj. Keep all test classes under the `EquipmentManager.Tests` namespace (or sub-namespaces) so the fixture's setup runs first.
- Test naming convention in existing tests: `Method_Scenario_ExpectedOutcome` (e.g. `Constructor_NameOnly_StoresNameAndZeroWeight`).

## Known issues and workarounds
- If `StateIsolationTestBase` reflects a static field that no longer exists in production code, `SetStaticFieldValue` throws `InvalidOperationException` by design ("test infrastructure may be out of sync with production code"). Keep the `CachingTypes` array and `_equipmentManager` field name in sync with production when caching/rule types change.
