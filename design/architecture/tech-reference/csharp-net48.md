---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# C# (`LangVersion latest`) @ net48

## Canonical source
- Official docs: https://learn.microsoft.com/dotnet/csharp/
- Last verified: 2026-06-06

## API surface used in project
- `<LangVersion>latest</LangVersion>`: every project compiles with the newest C# language version the installed Roslyn/SDK supports, decoupled from the runtime target. Set in both `Source/EquipmentManager/EquipmentManager.csproj` and `Source/EquipmentManager.Tests/EquipmentManager.Tests.csproj`.
- `<TargetFramework>net48</TargetFramework>`: .NET Framework 4.8 — RimWorld's Mono runtime target. Identical in production and test projects.
- JetBrains nullability annotations (`[CanBeNull]`, `[NotNull]`, `[UsedImplicitly]`, `[Pure]`): production code carries nullability intent via attributes (the production project has not yet enabled the C# nullable context).
- C# nullable reference types (`<Nullable>enable</Nullable>`): enabled in the test project only; `?` reference-type syntax and flow analysis are available there.
- `InternalsVisibleTo`: production exposes internals to `EquipmentManager.Tests` (declared in the csproj `ItemGroup`).

## Version-specific notes
- `net48` is the runtime ceiling: RimWorld runs on Unity's Mono, which targets .NET Framework 4.8. The project cannot move to a modern .NET (Core/5+) runtime — the assembly is loaded into the game process.
- `latest` LangVersion gives modern C# *syntax* (records, pattern matching, target-typed `new`, etc.) while the *runtime* and *BCL* stay at net48. Compiler-only features work; features that depend on newer BCL types or runtime support do not.
- Features needing newer BCL / runtime support are unavailable on net48 or require polyfills / shim types defined in-project:
  - `init`-only setters and `record` types need `System.Runtime.CompilerServices.IsExternalInit` — absent from the net48 BCL; must be polyfilled if used.
  - C# 11 `required` members need `RequiredMemberAttribute` / `CompilerFeatureRequiredAttribute` — not in net48 BCL.
  - `Index`/`Range` (`^1`, `1..3`) need `System.Index`/`System.Range` — not in net48 BCL.
  - Default-interface-method dispatch, static abstract interface members, `Span<T>`-backed runtime intrinsics, and ref-struct interfaces depend on runtime features net48 lacks.
- Nullable reference types are a compile-time feature only; they do not require runtime support, so they are usable on net48 (the test project proves this). They are gated by the `<Nullable>` MSBuild property / `#nullable` directives, not by the runtime.

## Deprecations and breaking changes from prior version
- No prior in-project version to migrate from; this is the established baseline.
- Planned change (tracked as a dedicated future sprint, not done ad hoc): flip the production project to `<Nullable>enable</Nullable>` to match the standard already met by the test project. Doing so surfaces ~148 nullable warnings that fail the build under `TreatWarningsAsErrors`, which is why it is scoped to its own sprint.

## Project conventions
- Target standard: `<Nullable>enable</Nullable>` for all projects. Test project already complies; production project does not yet.
- Until the production migration lands, production code uses JetBrains `[CanBeNull]`/`[NotNull]` annotations for nullability intent. New/rewritten production code MUST stay annotation-consistent and MUST NOT introduce `T?` reference-type syntax — it errors without the nullable context enabled.
- After migration, prefer C# nullable reference types over JetBrains annotations; the two MUST NOT contradict. Never disable the nullable context anywhere.
- Zero-warning policy: both projects build with `TreatWarningsAsErrors=true` and `WarningLevel 9999` (Debug and Release). Any warning — including analyzer findings — fails the build, so code MUST compile warning-clean.
- Mod identity: reference `EquipmentManagerMod.ModId` rather than a bare `"LordKuper.EquipmentManager"` literal.

## Known issues and workarounds
- Using a newer-C# feature that depends on a missing BCL type fails to compile on net48 with a missing-type error. Workaround: add the minimal polyfill attribute/type in-project, or avoid the feature. Prefer avoidance unless the polyfill is clearly justified.
- `T?` reference-type syntax in production code errors today because the nullable context is off there — use JetBrains annotations instead until the migration sprint.
