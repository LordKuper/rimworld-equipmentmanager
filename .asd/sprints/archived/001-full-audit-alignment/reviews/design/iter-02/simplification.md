---
responsibility:
  owns: single reviewer verdict for one iteration
  excludes: other reviewers, other iterations, fixes
  delegates_to: creator agent (fixes), sibling review files (other reviewers)
---

[REVIEW-design-simplification]: APPROVE

# Review — simplification

- **Phase**: design-review
- **Iteration**: 02
- **Severity floor**: MEDIUM (over-engineering checklist items remain critical/undroppable regardless of floor)

## Scope

Over-engineering / complexity-vs-value assessment of `design/adr.html` (ADR-0001…ADR-0004 + open questions) and `design/prd.html` (41 AC, non-goals, traceability) for sprint `001-full-audit-alignment`. This is a LEAN consolidation/deletion sprint — the design is itself an anti-over-engineering effort (net code removal). Verified: no new abstraction/interface/generic/factory/flag/layer is introduced; ADR-0004 carries real cross-cutting decisions; C-10 extraction stays gated behind Complication Approval; ADR-0002 consolidation does not over-reach.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| — | — | — | No findings at or above MEDIUM floor; no over-engineering checklist item tripped. | — |

## Checklist verification (over-engineering, critical/undroppable)

| Checklist item | Verdict | Evidence |
|---|---|---|
| Interface with one implementer | keep-as-is (none introduced) | No new interface anywhere. ADR-0003 documents as-built integration; "prescribes no code change beyond keeping it intact." |
| Generic with one concrete type param | keep-as-is (none introduced) | No new generic. Composite cache key (ADR-0004) is a key tuple on existing `ToolCache`, not a new generic type. |
| Factory for <3 classes | escalate-gated, not introduced | C-10's optional `Predicate<Thing>` area-restriction factory is the only factory candidate; it is PROPOSED, gated behind Complication Approval (AC-18, OQ-3), not mandated. Correctly handled — see Cross-reviewer guard. |
| Plugin system with no plugin | keep-as-is (none) | Not present. |
| Abstraction with no second use case | keep-as-is (none introduced) | Net deletion; C-10 abstraction explicitly deferred unless approved. |
| Premature config flag | keep-as-is (none introduced) | No new config/feature flag. `<Nullable>enable</Nullable>` (ADR-0001) is a compiler setting required by the sprint goal, not a behavioral toggle. |
| Defensive code for impossible-by-contract case | keep-as-is | ADR-0001 deliberately removes blanket `= null!` suppression in favor of real guards only where a path can be null (CE soft-dep fields, AC-14). Guards are contract-justified, not defensive padding. |
| Helper that wraps one stdlib call w/o value | keep-as-is | `Logger` wrapper kept (ADR-0002 kept-local table): it injects `ModId` into `Common.Logger`, which is the intended upstream consumption pattern (Common's `Logger` takes a `modId` param). Adds value, not a no-op wrapper. RU-6 deletes the genuine no-value duplicate (`GetWindowSize`). |
| Inheritance depth ≥3 w/o polymorphic dispatch | keep-as-is (none) | No new inheritance. |
| Framework wrapping a framework | keep-as-is (none) | No wrapper layer over Harmony/Common/SimpleSidearms; ADR-0003 consumes hard deps via direct typed refs. |
| Mock of a mock in tests | keep-as-is | Tests use existing `StateIsolationTestBase` / real `DefProvider` seam (RU-8); no nested mocking proposed. |
| Comment that restates code | n/a (design docs) | Not applicable to design drafts. |
| Dead code left "in case we need it" | keep-as-is (actively removed) | The sprint deletes dead/duplicate code (WorkTypeRule, ItemCache, StatRanges copy, EquipmentManagerStatDefs, duplicate guards C-9, DefaultWorkTypeStats literal). Opposite of the smell. |

## Complexity-vs-value spot checks

- **ADR-0002 kept-local table earns its weight.** The "kept local" rationale is sound and resists over-eager consolidation (which would itself be a different failure): `SkillWeight`/`PawnCapacityWeight`/`PassionLimit` are weights/enum-gate, a genuinely different concept from Common's min/max *limits* — not duplicates. Weapon-scoring caches keep EM-specific CE/ammo/work-type logic while only re-basing storage on `ThingCache`. `LegacyCustomStatDefs` remainder is adaptation *of* Common, not reimplementation. AC-33 protects these from deletion. This is correct restraint, not under-delivery.
- **ADR-0004 keeps the ADR corpus lean.** Only the two items with real decision content (C-3 cache-key contract; R-C2 characterize-before-behavior-change) are recorded; mechanical fixes (C-12, C-13, C-5…C-8) are explicitly pushed to the plan with the Simplicity Default cited. The rejected alternative "Manufacture separate ADRs for C-12/C-13" is correctly rejected as ADRs-with-no-decision. Good anti-over-engineering hygiene.
- **ADR-0004 cache-key fix is minimal.** "Key by `(statDef, workTypeDefs)` OR do not cache the WorkType-dependent stat at all" — offers the no-cache escape hatch, keeps the fix EM-local on top of `ThingCache` rather than pushing keying complexity upstream. Correctness-driven, not speculative.
- **PRD honors Simplicity Default throughout.** Lower-severity items folded into other ACs rather than inventing standalone requirements; non-goals explicitly forbid inventing scope and reaffirm C-10 is gated. No requirement is over-specified into new machinery.
- **ADR-0001 reduces ceremony.** Removes 224 attributes + 36 redundant usings; no `#pragma`/blanket-`!` shortcut that would mask real nulls. Net simplification.

## Cross-reviewer guard

The one place this sprint could acquire complexity is **C-10 (assignment-helper extraction, AC-18 / OQ-3)** — a new shared helper / `Predicate<Thing>` factory. The design correctly: (a) marks it PROPOSED not mandated, (b) gates it behind a Complication Approval (What/Why/Justification/Alternatives), (c) allows the minimal form (extract only the duplicated area-restriction predicate) or full deferral. No other reviewer's "fix" should be allowed to mandate this extraction, introduce a save-migration shim for RU-1 (explicitly rejected under `backward_compat=none`), or re-add a local `StatRanges`/`ItemCache` copy "for safety" — any such proposal must itself go through Complication Approval. Flagging proactively so a downstream concern does not smuggle in an abstraction the gate is meant to stop.

## Verdict
APPROVE

## Next action
APPROVE — reviewer done. No autofix or escalation required from asd-architect/asd-ba. Note for plan/impl phases: if C-10 (AC-18) or any reintroduced local copy is later proposed, it MUST carry a Complication Approval per `core.md`; the Simplification reviewer will re-evaluate at impl-review.
