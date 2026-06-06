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

## 2026-06-06 — Sprint 001-full-audit-alignment scope approved

- **Decision**: User approved the scope for sprint `001-full-audit-alignment`: a full audit + end-to-end alignment of code, docs, and tests across eight workstreams (correctness/defects, simplification, performance, design-doc alignment, coding-standard conformance, test coverage/quality, upstream-contract consumption, localization). Two gate decisions baked in: (1) remediation ceiling = **free expansion** — audit may freely expand scope, everything found is fixed this sprint (no fixed cap; running scope tracked through audit/plan phases); (2) localization = **in-scope-if-found** — keyed-translation fixes enter remediation only if the audit surfaces hardcoded strings or localization issues. Out of scope: per-project layout restructure + ASD setup (already committed 1d931b5) and the deferred production-csproj nullable migration. Structural analog: rimworld-workmanager sprint `001-full-audit-alignment` (PR #20).
- **Rationale**: Brownfield mod recently onboarded to ASD; owner wants a clean, defect-free, simplified, doc-aligned baseline before further feature work, mirroring the sibling WorkManager project's first sprint.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/sprint.md`, `.asd/sprints/001-full-audit-alignment/state.json`.

## 2026-06-06 — Sprint 001 scope corrected: production nullable migration IN SCOPE

- **Decision**: Corrected the scope of sprint `001-full-audit-alignment` to make the production nullable migration a first-class, headline IN-SCOPE workstream. Specifically: enable `Nullable enable` on `Source/EquipmentManager/EquipmentManager.csproj`, resolve all surfaced NRT errors under `TreatWarningsAsErrors` (~148 known from init), and replace ReSharper `[CanBeNull]`/`[NotNull]` nullability attributes with real nullable reference types. Workstream count moved from eight to nine; the prior "excluding the deferred production nullable migration" wording in workstream 5/coding-standard conformance was removed, the Out-of-scope deferral bullet was deleted, and Acceptance now requires the migration to be complete (Nullable enabled, build green with zero NRT errors, ReSharper nullability attributes removed). This **supersedes** the earlier init-time deferral (2026-06-06 "Logger, jb, and project-wide nullable" and 2026-06-06 "Tech stack reverse-engineered"), which scheduled the migration to a dedicated future sprint — THIS sprint is that dedicated nullable sprint.
- **Rationale**: Explicit user approval. The user's literal request was "подгоняем под новые правила, full-nullable", approving a refined goal whose workstream 2 was the production nullable migration. Explicit in-sprint user approval supersedes the prior init deferral.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/sprint.md`.

## 2026-06-07 — Sprint 001 audit approved (reuse decisions resolved)

- **Decision**: User APPROVED the audit for sprint `001-full-audit-alignment`. Reuse-finding resolutions baked in: (1) **RU-1** (`WorkTypeRule` → `Common.WorkTypeThingRule`/`WorkTypeStatMap`) — APPROVED, migrate **without** a Scribe save-migration shim (`backward_compat=none`; old saves lose persisted WorkType rules); auto-resolves C-9 and removes the largest nullable hotspot (36 sites). (2) **RU-2** (`EquipmentManagerGameComponent_StatRanges` → `Common.StatRanges`) — UNBLOCKED and APPROVED for migration: the upstream Common fix is done and verified (rimworld-common commit `17199b6` — seeding bug fixed, `StatRanges`/`NormalizeStatValue` made public, `Clear()` added, 11 tests green; EM builds clean against the rebuilt Common dll dated 2026-06-07). EM deletes its duplicate `_statRanges` dict + `ExposeData_StatRanges` and the `LegacyCustomStatDefs.NormalizeStatRanges` save-migration, consuming `Common.StatRanges`; stat ranges become process-global ephemeral (not save-persisted), rebuilt via `InitializeStatRanges`; auto-resolves the EM copy of C-4. (3) Sequencing: non-breaking reuse deletes (RU-3/RU-4/RU-6) and RU-1 land **before** the nullable flip. (4) Remediation ceiling = **free expansion** (everything found is fixed this sprint). (5) Localization = **clean** — no hardcoded-string / keyed-translation work surfaced.
- **Rationale**: User approval at the audit HARD gate. RU-2 was previously blocked on `Common.StatRanges` being `internal`; the upstream change (commit `17199b6`) removed that blocker and corrected C-4 at the source, so EM consumes the library rather than maintaining a duplicate (per `custom-common-rules.md` anti-reimplementation rule).
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/audit.md`, `.asd/sprints/001-full-audit-alignment/state.json`.

## 2026-06-07 — Sprint 001 design phase complete (lean design)

- **Decision**: Design phase complete for sprint `001-full-audit-alignment`. Lean design path taken: two user-APPROVED artifacts — `design/prd.html` (41 acceptance criteria) and `design/adr.html` (ADR-0001 production nullable migration, ADR-0002 consolidation/reuse, ADR-0003 dependency-integration pattern, ADR-0004 defect/perf remediation). The UX-spec and design-system gate were intentionally SKIPPED: the sprint makes zero UI changes, so no UX-spec.html or DESIGN.md/design-system work is warranted. Three open questions deferred downstream: **OQ-1** (WorkType default-weight parity after `WorkTypeRule` → `Common.WorkTypeThingRule`/`WorkTypeStatMap` migration), **OQ-2** (RU-7 widget timing), **OQ-3** (C-10 extraction — pending a Complication Approval) — all to be resolved in plan/impl.
- **Rationale**: Sprint is an audit-driven alignment/remediation effort with no user-facing UI surface change; lean design (PRD + ADRs only) is the proportionate artifact set. User approved both PRD and ADR at the design gate.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/design/prd.html`, `.asd/sprints/001-full-audit-alignment/design/adr.html`, `.asd/sprints/001-full-audit-alignment/state.json`.

## 2026-06-07 — UI reviewer waived for sprint 001-full-audit-alignment

- **Decision**: UI reviewer waived for sprint 001-full-audit-alignment (lean, zero-UI-change remediation sprint). design/ux/accessibility.html intentionally absent per the lean-design decision; UI review is N/A and EXCLUDED from the design-review AND impl-review Definition of Done for this sprint only. Future sprints with UI work must create design/ux/accessibility.html (via asd-design-system) to restore the UI gate.
- **Rationale**: Sprint makes zero user-facing UI changes (per the 2026-06-07 lean-design decision), so the UI reviewer has no artifact to evaluate; user waived it.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/state.json`.

## 2026-06-07 — Sprint 001 design-review iter-02 APPROVE; DoD met

- **Decision**: design-review iter-02: APPROVE (documentation, simplification, external all APPROVE; UI waived). 3 low findings from iter-01 autofixed (prd AC-22 trace, prd AC-16 xref, adr original-provenance chips). DoD met; advancing to design-promote.
- **Rationale**: All required design-review reviewers returned APPROVE at iteration 02 after the iter-01 low findings were autofixed; UI reviewer waived for this lean zero-UI sprint. Design-review Definition of Done satisfied.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/state.json`, `.asd/sprints/001-full-audit-alignment/reviews/`.

## 2026-06-07 — Sprint 001 design-promote finalized; first persistent ADR corpus established

- **Decision**: User CONFIRMED the design-promote persistent writes for sprint `001-full-audit-alignment`. Established the project's **first persistent ADR corpus** at `design/architecture/adr/` with four ADRs, all `status=accepted`: **adr-0001** production-nullable-migration, **adr-0002** consume-lordkuper-common, **adr-0003** dependency-integration-pattern (provenance=reverse-engineered — fulfills the 2026-06-06 "Candidate ADR — dependency integration pattern" flag), **adr-0004** defect-performance-policy. The sprint-draft `design/adr.html` (ADR-0001..0004) is thereby promoted into the persistent per-ADR corpus.
- **Rationale**: User confirmation at the design-promote final HARD gate. The sprint produced durable architectural decisions (nullable migration, upstream-Common consumption, integration pattern, defect/perf policy) that outlive the sprint and belong in the persistent `design/architecture/adr/` corpus, not just the sprint-local draft.
- **Affected docs**: `design/architecture/adr/adr-0001-production-nullable-migration.html`, `design/architecture/adr/adr-0002-consume-lordkuper-common.html`, `design/architecture/adr/adr-0003-dependency-integration-pattern.html`, `design/architecture/adr/adr-0004-defect-performance-policy.html`, `.asd/sprints/001-full-audit-alignment/design/adr.html`.

## 2026-06-07 — Sprint 001 design-promote: stack.html DM-1 reconciled (nullable)

- **Decision**: During design-promote, reconciled `design/architecture/stack.html` doc-mismatch **DM-1**: the production-csproj nullable posture was reframed from "deferred / not enabled" to **decided + in-progress**, now linking **ADR-0001** (production-nullable-migration) as the authoritative decision record. As-built confirmation (Nullable actually enabled, build green with zero NRT errors, ReSharper nullability attributes removed) is explicitly **deferred to the impl/pr phases** — stack.html records the decision and in-progress status, not the completed state.
- **Rationale**: The earlier stack extraction (2026-06-06 "Tech stack reverse-engineered") recorded nullable migration as a deferred future sprint; sprint 001 IS that sprint (per 2026-06-06 "Sprint 001 scope corrected"), so the persistent stack doc had to be reconciled to the decided+in-progress reality and cross-linked to ADR-0001. Confirming as-built before impl lands would misrepresent state, hence the deferral to impl/pr.
- **Affected docs**: `design/architecture/stack.html`, `design/architecture/adr/adr-0001-production-nullable-migration.html`.

## 2026-06-07 — Sprint 001 design-promote: PRD kept sprint-local (no persistent requirements corpus)

- **Decision**: During design-promote, the sprint `design/prd.html` was deliberately **NOT** promoted to a persistent requirements corpus; it remains sprint-local under `.asd/sprints/001-full-audit-alignment/design/`. The PRD's 41 acceptance criteria are transient remediation ACs scoped to this sprint, not durable product requirements.
- **Rationale**: Three-fold: (1) `artifact-layout.md` carries no mandate to maintain a persistent requirements corpus; (2) the PRD's own non-goal disclaims standing up such a corpus; (3) promoting transient remediation ACs would pollute persistent design with sprint-scoped, soon-stale content. The durable architectural decisions from this sprint live in the ADR corpus (this date's design-promote-finalized entry) instead.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/design/prd.html` (kept sprint-local; not promoted).

## 2026-06-07 — Sprint 001 plan approved

- **Decision**: User APPROVED the plan for sprint `001-full-audit-alignment` (20 tasks, all 41 AC covered). Decisions baked into `plan.md`: **RU-7** (Task 16, AC-7) widget **MIGRATE to Common** (do NOT defer) — workmanager has no analog of this widget (different domain), so "follow workmanager" means mirror its consolidation philosophy (consume Common's public surface, not retain a local reimplementation); consume Common's `WorkTypeThingRuleWidget` per Common's actual API (referenced at `D:\Storage\Projects\RimWorld\rimworld-common`) and VERIFY UI pixel-parity (manual in-game check if needed). **C-10** (Task 15, AC-18) helper extraction **DEFERRED** — recorded-deferred only, not implemented this sprint (Simplicity Default; would add abstraction; no Complication Approval sought). **DM-3/DM-4** (Task 16, AC-38/AC-39) README expansion + About.xml `<description>` completion **IN SCOPE now** — moved out of optional/defer-candidate into mandated subtasks. Also recorded: stack.html as-built nullable confirmation (DM-1/AC-36, Task 19) is finalized during impl as a doc-to-code reconciliation, validated by the impl-review documentation reviewer (impl may edit persistent docs to match as-built code).
- **Rationale**: User approval at the plan HARD gate. RU-7 migration upholds the `custom-common-rules.md` anti-reimplementation rule; C-10 defer upholds the Simplicity Default; DM-3/DM-4 in-scope completes doc reconciliation this sprint.
- **Affected docs**: `.asd/sprints/001-full-audit-alignment/plan.md`, `.asd/sprints/001-full-audit-alignment/state.json`.
