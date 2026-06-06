[REVIEW-design-simplification]: APPROVE

# Review — simplification

- **Phase**: design-review
- **Iteration**: 1

## Scope

Over-engineering assessment of the sprint-001 DESIGN drafts (`adr.html`, `prd.html`) against the over-engineering checklist (`review-policy.md`) and the design principles (`design-principles.md`). This sprint is a consolidation/deletion/remediation effort (delete EM reimplementations in favor of `LordKuper.Common`, enable nullable, fix defects) — inherently anti-over-engineering. The review verifies the decisions actually *reduce* complexity and introduce no unapproved abstraction, layer, interface, generic, factory, or config flag.

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|----------|----------|-------------|---------------|
| — | — | — | no findings at or above floor (LOW) | — |

## Checklist trace (over-engineering items, all verified clean)

| Checklist item | Result | Evidence |
|---|---|---|
| Interface with one implementer | none | No new interface proposed anywhere in the ADRs/PRD. |
| Generic with one concrete type param | none | No new generic introduced. ToolCache composite key (C-3) is a key-tuple correctness fix, not a generic abstraction. |
| Factory for < 3 classes | gated, not mandated | The only factory-shaped item is C-10's optional area-restriction `Predicate<Thing>` factory — explicitly PROPOSED, priority `may`, gated behind Complication Approval (AC-18, OQ-3, R-C5). Not added without approval. |
| Plugin system with no plugin | none | n/a. |
| Abstraction with no second use case | none | ADR-0002 deletes abstractions; ADR-0003 documents an as-built pattern (no new code). The kept-local `Logger` wrapper has a real second consumer (it injects `ModId` into the shared multi-mod `Common.Logger` — the intended consumption pattern). |
| Premature config flag | none | `<Nullable>enable</Nullable>` is a compiler standard-conformance setting, not a runtime/caller-facing flag. No new config flag introduced. |
| Defensive code for impossible case | none | CE reflective fields stay `T?` with existing guards because CE is genuinely absent at runtime (ADR-0003, AC-14) — guards are for a real, contractual nullable case, not an impossible one. |
| Helper wrapping one stdlib call | none | RU-4/RU-6 *delete* thin pass-through wrappers (`EquipmentManagerStatDefs`, `UiHelpers.GetWindowSize`) and inline the Common call — the correct anti-wrapper direction. |
| Inheritance depth ≥ 3 | none | No inheritance changes. |
| Framework wrapping a framework | none | ADR-0003 keeps Harmony/Common/SimpleSidearms as direct compile-only typed refs; CE bound by reflection. No wrapper layer added. |
| Mock of a mock in tests | none | New tests reuse existing isolation infra (`StateIsolationTestBase`); no mock-of-mock proposed. |
| Comment that restates code | n/a | Design-phase docs, no code comments under review. |
| Dead code left "in case we need it" | none | The opposite: C-9 dead duplicate guards are removed (AC-17, auto-resolved by RU-1's deletion). |

## Targeted concern checks (per dispatch payload)

- **ADR-0004 not a manufactured/empty ADR.** It carries real decision content: (1) the cache-key correctness contract for work-type-dependent tool stats (C-3) — a contract change, not a one-line patch, and explicitly noted as an EM-local concern that survives the RU-3 `ThingCache` migration; (2) the characterize-before-behavioral-change discipline (R-C2). The ADR itself *applies* the Simplicity Default: its Alternatives reject manufacturing separate ADRs for the mechanical C-12/C-13 items ("Simplicity Default forbids ADRs with no decision content"). This is correct anti-over-engineering posture, not a smell.
- **C-10 helper extraction correctly gated.** AC-18 marks it `may`, PROPOSED-not-mandated, implementable only on a granted Complication Approval (What/Why/Justification/Alternatives), else recorded deferred. OQ-3 and the Non-goals section reinforce "gated, not mandated," and note the minimal form (extract only the duplicated area-restriction predicate) may need no new layer. No silent abstraction.
- **ADR-0002 consolidation is not over-reaching.** The "Kept local — explicitly NOT migrated" table gives sound per-type rationale: `SkillWeight`/`PawnCapacityWeight`/`PassionLimit` are a different concept (weights/enum gate) from Common's min/max limits; weapon-scoring caches and `CombatExtendedHelper` have no Common analog; the `Logger` wrapper is the intended downstream-`modId` pattern; `LegacyCustomStatDefs` remainder is adaptation-of-Common, not reimplementation. AC-33 guards against over-eager deletion. Conservative and correct — consolidation removes only verified duplicates.
- **No new interface/generic/factory/flag/layer proposed by the design** — confirmed across all four ADRs and all 41 AC.

## Verdict
APPROVE

## Next action
Reviewer done. No autofix and no escalation required from the Simplification axis. The C-10 Complication Approval gate (AC-18) is correctly deferred to plan/impl and is not an open item for this design-review iteration.

## Escalations (optional)
- none.
