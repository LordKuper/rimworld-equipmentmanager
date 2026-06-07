---
responsibility:
  owns: sprint scope, goal, top-level acceptance criteria
  excludes: task breakdown, design decisions, code, audit findings
  delegates_to: plan.md (tasks), design/ docs (decisions), audit.md (audit)
---

# Sprint 001-full-audit-alignment

## Goal

Perform a full audit of the EquipmentManager codebase and design corpus, then align the project end-to-end: bring code, docs, and tests into a consistent, defect-free, simplified, well-performing, and fully nullable-aware state. The headline deliverable is the production nullable migration; the audit drives remediation across the following nine workstreams:

1. **Production nullable migration (headline)** — enable `Nullable enable` on `Source/EquipmentManager/EquipmentManager.csproj`, resolve all surfaced NRT errors under `TreatWarningsAsErrors` (the ~148 nullable errors known from init), and replace ReSharper `[CanBeNull]`/`[NotNull]` nullability attributes with real nullable reference types. This is the dedicated nullable sprint referenced by the init decisions-log.
2. **Correctness / defects** — find and fix bugs, logic errors, stale caches, and incorrect behavior surfaced by the audit.
3. **Simplification** — remove dead code, redundant abstractions, and accidental complexity; collapse needless indirection.
4. **Performance** — fix hot-path inefficiencies, redundant computation, and allocation/caching problems.
5. **Design-doc alignment** — reconcile persistent `design/` docs (concept, stack, ADRs, C4) with the actual code; close drift identified during init (e.g. candidate ADR for the dependency-integration pattern).
6. **Coding-standard conformance** — bring code into line with `custom-coding-rules.md` (style, NetAnalyzers, annotation conventions), including the production nullable migration covered by workstream 1.
7. **Test coverage / quality** — extend and harden tests for audited subsystems using the existing isolation infrastructure (`StateIsolationTestBase`, `RimWorldAssemblyResolverFixture`).
8. **Upstream-contract consumption** — verify correct consumption of the `LordKuper.Common` public surface and the SimpleSidearms reference; no forking or reimplementation of upstream behavior.
9. **Localization / keyed translations** — if the audit finds hardcoded user-facing strings or localization issues, remediate them with keyed translations (see decision below).

This sprint mirrors the sibling project's `001-full-audit-alignment` sprint (rimworld-workmanager, PR #20) as the structural analog.

### Scope decisions baked in

- **Remediation ceiling: free expansion.** The audit may freely expand implementation scope. Everything found is fixed this sprint — defects, simplifications, and optimizations alike. There is no fixed cap. The running scope is documented and tracked through the audit and plan phases.
- **Localization: in-scope-if-found.** Keyed-translation / localization fixes enter remediation only if the audit surfaces hardcoded strings or localization issues. They are not pursued speculatively.

## Acceptance

- The production nullable migration is complete: `Nullable enable` is set on `Source/EquipmentManager/EquipmentManager.csproj`, the build is green with zero NRT errors under `TreatWarningsAsErrors`, and all ReSharper `[CanBeNull]`/`[NotNull]` nullability attributes are removed in favor of real nullable reference types.
- A merged `audit.md` enumerates findings across all nine workstreams, with each finding classified (nullable-migration / defect / simplification / performance / doc-drift / standard-conformance / test-gap / upstream-contract / localization).
- Every audit-confirmed finding is either remediated this sprint or explicitly recorded as deferred with rationale (free-expansion default favors remediation).
- Persistent `design/` docs are reconciled with the post-remediation code; identified doc-drift is closed.
- The build is green and the test suite passes; coverage is extended for audited subsystems.
- Code conforms to `custom-coding-rules.md`, including the now-completed production nullable migration.
- Consumption of `LordKuper.Common` and SimpleSidearms remains contract-correct (no forking / reimplementation).
- Any hardcoded user-facing strings found are converted to keyed translations.

## Out of scope

- Per-project layout restructure and ASD workflow setup — already completed and committed (1d931b5).
- Any workflow-infrastructure changes (`.asd/rules/`, `.asd/templates/`, `.claude/`).
