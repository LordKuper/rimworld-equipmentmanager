---
responsibility:
  owns: append-only chronology of approved decisions across project lifetime
  excludes: sprint state, code review notes, custom rules
  delegates_to: .asd/sprints/ (sprint state), reviews/ (review notes), custom-common-rules.md / custom-design-rules.md / custom-coding-rules.md (rules)
---

# Decisions Log

Append-only. Never edited or removed. New entries appended below.

## Entry format

```markdown
## YYYY-MM-DD — <one-line summary>

- **Decision**: <what was decided>
- **Rationale**: <why>
- **Affected docs**: <links> (optional)
```

## Entries

<!-- entries appended below this line -->

## 2026-06-06 — ASD initialized for project

- **Decision**: Initialized ASD workflow. chat=ru, docs=en; decomposition=disabled, diagram_tool=n/a; backward_compat=none; external_review=enabled (Codex 0.130.0); OS=windows.
- **Rationale**: Brownfield RimWorld C# mod. Configured analogous to the sibling project rimworld-workmanager per project owner. Imported jb (ReSharper CLI) build/lint commands and adapted custom rules to EquipmentManager's actual setup (net48, SimpleSidearms/CombatExtended/VEF integrations, NetAnalyzers).
- **Affected docs**: `.asd/project/config.yaml`, `commands.yaml`, `custom-*-rules.md`.

## 2026-06-06 — Logger, jb, and project-wide nullable

- **Decision**: Ported `Logger.cs` (wraps `LordKuper.Common.Logger` with `EquipmentManagerMod.ModId`, `[CanBeNull]` annotation style) analogous to WorkManager; added `EquipmentManagerMod.ModId` constant; installed JetBrains ReSharper global tools (`jb` 2026.1.2). Project-wide `Nullable enable` set as the target standard in coding rules: tests already have it, but the production csproj flip surfaces ~148 nullable errors under `TreatWarningsAsErrors`, so production nullable migration is deferred to a dedicated sprint rather than done during init.
- **Rationale**: Project owner directive to mirror WorkManager conventions; production nullable migration is real annotation work that must go through a sprint and stay green.
- **Affected docs**: `Source/EquipmentManager/Logger.cs`, `Source/EquipmentManager/EquipmentManagerMod.cs`, `.asd/project/custom-coding-rules.md`.

## 2026-06-06 — Concept reverse-engineered from brownfield

- **Decision**: Created `design/product/concept.html` via /asd-concept variant D (brownfield extraction). Sections locked: Vision, Target users, Value proposition (required) + Pillars, Anti-Pillars, Constraints (optional). Omitted: Unique Hook, Core Identity, Success metrics (no extractable basis). User directive applied: Combat Extended removed from all sections (project owner choice), so the mod-ecosystem framing names only Simple Sidearms and Vanilla Factions Expanded. provenance=reverse-engineered.
- **Rationale**: Published RimWorld mod with existing About.xml + source; concept extracted, not invented.
- **Affected docs**: `design/product/concept.html`.

## 2026-06-06 — Tech stack reverse-engineered

- **Decision**: Created `design/architecture/stack.html` via /asd-stack variant D (brownfield extraction); all versions verified latest-stable on 2026-06-06. Sections: Languages, Frameworks/libraries, Runtime/infrastructure (required) + Tooling, Constraints, Knowledge-gap risk (optional). Omitted Layers diagram (single-assembly mod; belongs to C4) and Architecture Principles (duplicate of concept/ADR). Created 13 tech-reference docs under `design/architecture/tech-reference/`: rimworld-1.6, unityengine-modules, lib-harmony-2.4.2, lordkuper-common-1.6, simplesidearms-1.6, csharp-net48, dotnet-sdk-10.0.300, netanalyzers-9.0.0, jetbrains-resharper-cli-2026.1.2, microsoft-net-test-sdk-18.6.0, nunit-4.6.1, nunit3testadapter-6.2.0, fluentassertions-7.2.2. Decisions captured: FluentAssertions pinned to 7.x (Apache-2.0; 8.x is commercial) — hard constraint; production Nullable migration scheduled as first sprint; NetAnalyzers 9.0.0 pin is deliberate with 10.0.300 as future upgrade candidate.
- **Rationale**: Published RimWorld mod with existing manifests; stack extracted and version-verified, not invented. Overall knowledge-gap risk HIGH, driven by the private upstream LordKuper.Common.
- **Affected docs**: `design/architecture/stack.html`, `design/architecture/tech-reference/*.md`.

## 2026-06-06 — Candidate ADR — dependency integration pattern

- **Decision**: Flag for architecture phase — EquipmentManager uses two integration patterns: direct typed references for hard deps (SimpleSidearms, LordKuper.Common) vs reflective Harmony AccessTools/MethodDelegate binding for optional deps (CombatExtended, VanillaFactionsExpanded.Core, see `CombatExtendedHelper.cs`). Candidate for an ADR if not already captured.
- **Rationale**: Surfaced by architect during stack extraction; real architectural decision, out of scope for stack.html.
- **Affected docs**: (none yet — future ADR).
