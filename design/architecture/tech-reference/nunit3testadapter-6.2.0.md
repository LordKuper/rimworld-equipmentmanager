---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# NUnit3TestAdapter @ 6.2.0

## Canonical source
- Official docs: https://docs.nunit.org/articles/vs-test-adapter/Index.html
- Last verified: 2026-06-06

## API surface used in project
- `<PackageReference Include="NUnit3TestAdapter" Version="6.2.0" />` in `Source/EquipmentManager.Tests/EquipmentManager.Tests.csproj`: the VSTest adapter that bridges NUnit tests to the VSTest platform. No source-level API; it is a discovery/execution plugin installed as a NuGet package.
- Acts as the communication layer between the NUnit framework (`NUnit` 4.6.1) and the test platform (`Microsoft.NET.Test.Sdk` 18.6.0), enabling `dotnet test`, `vstest.console`, and Visual Studio / Rider Test Explorer to discover and run the tests.

## Version-specific notes
- 6.2.0 is the confirmed latest release as of 2026-03-21.
- Despite the "3" in the name, the adapter runs both NUnit 3 and NUnit 4 tests. This project uses it to run NUnit 4.6.1 tests.
- Cannot run NUnit 2.x tests (not relevant here).
- Per the docs the adapter supports .NET Framework 3.5+ and modern .NET; this project's `net48` target is covered.

## Deprecations and breaking changes from prior version
- None affecting this project: the adapter is a passive discovery/execution plugin with no consumed source API. Treat a major version bump as a discovery/runner change and re-verify `dotnet test` discovers and runs the suite before adopting.

## Project conventions
- Paired with `Microsoft.NET.Test.Sdk` and `NUnit` in the single test project. The SDK (host) + adapter (bridge) + framework (attributes/API) triad is required together; removing the adapter causes "no tests discovered".
- No `.runsettings` or adapter-specific configuration is present; default discovery applies. Static-state tests rely on NUnit's `[NonParallelizable]` (see `nunit-4.6.1` reference) rather than adapter-level parallelism settings.

## Known issues and workarounds
- Symptom "0 tests discovered/run" almost always means the adapter or SDK reference is missing or version-mismatched, not a framework issue. Verify the adapter + SDK + framework triad is intact in the test csproj.
