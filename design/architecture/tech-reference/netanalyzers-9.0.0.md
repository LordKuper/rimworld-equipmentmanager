---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# Microsoft.CodeAnalysis.NetAnalyzers @ 9.0.0

## Canonical source
- Official docs: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview
- Last verified: 2026-06-06

## API surface used in project
- `PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="9.0.0"` in `Source/EquipmentManager/EquipmentManager.csproj`.
  - `PrivateAssets=all`: the analyzer is a build-only asset and does not flow to downstream consumers of the assembly.
  - `IncludeAssets=runtime; build; native; contentfiles; analyzers; buildtransitive`: pulls in the analyzer assets needed at build time.
- Roslyn "CAxxxx" code-quality rules (security, performance, design, reliability, usage, interoperability) run during `dotnet build` and report as warnings or errors.
- The package is referenced explicitly rather than relying on the SDK's built-in analyzers; for `net48` (a non-`.NET 5+` target) the explicit reference is how these analyzers are enabled. Installing the NuGet package turns off the SDK's built-in analyzer copy.

## Version-specific notes
- `9.0.0` corresponds to the .NET 9 analyzer band — the CA rule set and default severities that shipped with the .NET 9 SDK.
- NuGet latest for this package is `10.0.300` (the .NET 10 band, which adds rules such as CA2023 and CA2266 and may change default severities). The project deliberately pins `9.0.0`: pinning decouples analyzer rule updates from the installed .NET 10 SDK, so the rule set stays stable across machines and SDK servicing updates.
- `10.0.300` is a future upgrade candidate. Treat the bump as an ADR future-consideration, not an automatic change — moving to the .NET 10 band can surface new findings that, under `TreatWarningsAsErrors`, break the build until addressed.
- When the explicit NuGet package version is older than the SDK's bundled analyzer assembly, the build can emit an upgrade warning; `_SkipUpgradeNetAnalyzersNuGetWarning=true` suppresses it if it appears. Do not add `EnableNETAnalyzers` while the NuGet package is referenced — combining them produces a build warning.

## Deprecations and breaking changes from prior version
- No prior in-project analyzer version to migrate from; `9.0.0` is the established baseline.
- Future move to `10.0.300`: expect newly-enabled CA rules to appear as build-breaking errors under the zero-warning policy. Migration path = bump version, build, then fix or justifiably suppress each new finding within a dedicated sprint.

## Project conventions
- Enforced as errors: `TreatWarningsAsErrors=true` plus `WarningLevel 9999` in both Debug and Release (`EquipmentManager.csproj`). Analyzer (CA) findings count as warnings, so any CA finding fails the build. Code MUST compile analyzer-clean.
- Suppressions are a last resort — fix the real issue first. Prefer attribute-based suppression (`[SuppressMessage]`, `[UsedImplicitly]`, `[Pure]`) over pragmas (`#pragma warning disable`, `// ReSharper disable`); use comment pragmas only when no attribute applies.
- Every suppression MUST state a concrete reason *why*. "false positive" / "by design" alone is insufficient.

## Known issues and workarounds
- A pinned analyzer older than the SDK band may trigger a version-mismatch upgrade warning — suppress with `_SkipUpgradeNetAnalyzersNuGetWarning=true` only if it actually fires (not currently set).
- Because CA findings are build-breaking, an SDK or dependency change that alters analyzer inputs can break the build without any source change; diagnose by reading the exact CA id in the build error and consulting the rule's docs page.
