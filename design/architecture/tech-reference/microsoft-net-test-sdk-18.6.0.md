---
responsibility:
  owns: project-vetted reference for one technology version (apis used, version specifics, project conventions)
  excludes: adr rationale, code, full stack overview, build commands
  delegates_to: stack.html (overview), adr/ (decisions), commands.yaml (commands)
---

# Microsoft.NET.Test.Sdk @ 18.6.0

## Canonical source
- Official docs: https://learn.microsoft.com/dotnet/core/testing/
- Last verified: 2026-06-06

## API surface used in project
- `<PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.6.0" />` in `Source/EquipmentManager.Tests/EquipmentManager.Tests.csproj`: marks the project as a runnable test project and supplies the test platform (the engine that runs tests and communicates with IDEs and the CLI). It has no compile-time API used directly in test code; its surface is MSBuild targets and the VSTest test host.
- Test host / platform: provides the host process that the NUnit3TestAdapter plugs into for `dotnet test`, `vstest.console`, and Visual Studio / Rider Test Explorer execution.

## Version-specific notes
- 18.6.0 is the confirmed latest release as of 2026-05-26.
- Supplies the VSTest-based test platform. Microsoft docs distinguish two platforms: VSTest and Microsoft.Testing.Platform (MTP). This project uses the VSTest path via NUnit3TestAdapter; MTP is not adopted here.
- Targets `net48` in this project (the test project's `TargetFramework`), which the SDK supports as a VSTest host.

## Deprecations and breaking changes from prior version
- No project-affecting breaking changes in scope: the SDK is referenced only as a build/runtime dependency and exposes no source API. Treat major version bumps as a platform/host change and re-verify `dotnet test` against `net48` before adopting.

## Project conventions
- Referenced once in the test project alongside the framework (`NUnit`), adapter (`NUnit3TestAdapter`), and assertion library (`FluentAssertions`). The SDK + adapter + framework triad is the standard `dotnet test` arrangement; do not remove the SDK reference or the test host disappears.
- Tests run on `net48` and depend on a RimWorld AssemblyResolve handler (see `nunit-4.6.1` reference, `RimWorldAssemblyResolverFixture`); the SDK host is the process in which that resolver is registered at runtime.

## Known issues and workarounds
- None observed in this project. The SDK is a passive platform dependency; failures surface as "no tests discovered/run", which usually trace to a missing or mismatched adapter (`NUnit3TestAdapter`) rather than the SDK itself.
