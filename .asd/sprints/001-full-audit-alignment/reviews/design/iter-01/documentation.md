[REVIEW-design-documentation]: CONCERNS

# Review — documentation

- **Phase**: design-review
- **Iteration**: 01

## Summary of checks performed

- **SSoT integrity** — each fact has a single home: audit.md owns findings (C-*/RU-*/DM-*/R-C*), prd.html owns acceptance criteria, adr.html owns decisions. Downstream docs reference audit IDs rather than re-deriving facts. The measured nullable baseline (131 distinct sites / 300 occurrences / 32 files; 224 JetBrains attrs = 205 `[NotNull]` + 19 `[CanBeNull]`; CS8618 dominant; `WorkTypeRule.cs` 36-site hotspot) is stated identically in audit.md, prd.html (AC-11, AC-12), and adr.html (ADR-0001 Context). The RU-2 unblock (rimworld-common `17199b6`, `StatRanges` made public + C-4 fixed) is consistent across audit decision, AC-3/AC-16, and ADR-0002. No contradicting duplications found.
- **Traceability completeness** — all 41 ACs trace to an audit finding via the prd traceability table; design-decision ACs map to ADRs and every ADR lists its driving ACs. AC-1↔ADR-0002 sequencing, AC-9…AC-14↔ADR-0001, AC-15/AC-16/AC-25/AC-29↔ADR-0004, AC-30…AC-33↔ADR-0003 all reconcile. AC-40 (Architect-owned dependency-integration ADR) ↔ ADR-0003 "promoted in design-promote (satisfies AC-40)" is consistent. Lower-severity findings (C-5…C-8, C-11, C-14, C-15, R-C1/2/4/6) are explicitly accounted for as folded/deferred per the Simplicity Default rather than silently dropped.
- **Audit-ID accuracy** — every C-*/RU-*/DM-* id cited in prd/adr exists in audit.md. DM-1…DM-6 verified against the documentation migration plan table. AC count = 41 (AC-1…AC-41), header "41 AC / 3 goals / 9 stories / 6 non-goals" all verified.
- **Provenance frontmatter** — prd.html `provenance: original`, source empty: correct; header badge correctly omits the provenance badge. adr.html doc-level `provenance: original`; ADR-0003 carries `provenance: reverse-engineered` with a source line, matching DM-5's reverse-engineered classification.
- **HTML-shell wrapping** — both artifacts are full shell documents (DOCTYPE, responsibility frontmatter, all required meta placeholders filled, TOC, header badges, footer, scrollspy). No bare fragments; chrome is not duplicated within content.
- **Custom-rules consistency** — `WeaponAssingment` upstream typo preserved (AC-31), `LordKuper.Common`/SimpleSidearms not forked (AC-30/AC-32), "EquipmentManager wording wins" honored in OQ-1. The intentionally absent ux-spec/design-system is an approved scope decision (not flagged).

## Findings

| # | Severity | Location | Description | Suggested fix |
|---|---|---|---|---|
| 1 | low | prd.html § Traceability, AC-22 row | AC-22 cites "C-16 (Logger wrapper; RU-8 / kept-local Logger)". RU-8 is the `DefProvider`/`DefDatabase<T>` consistency seam (audit.md line 182); the kept-local `Logger` wrapper is an explicit non-finding in the audit's "NOT duplicates" section (line 192), not RU-8. The audit-id citation is inaccurate. | Cite the kept-local Logger reference as the "NOT duplicates" non-finding (or just `C-16`), and remove the `RU-8` token from this row, OR move `RU-8` to a row where the DefProvider seam is actually the driver (it is referenced descriptively under AC-25's test note). |
| 2 | low | adr.html, per-ADR `.meta` blocks (ADR-0001 line 138, ADR-0002 line 202, ADR-0004 line 352) | Each `original`-provenance ADR renders an explicit `<span class="chip">provenance: original</span>` chip. The HTML-shell convention (t_html-shell.html line 216) is that the provenance badge is omitted when provenance == original. Showing "provenance: original" contradicts that convention. | Drop the `provenance: original` chip from the three original ADRs' meta blocks; retain the `provenance: reverse-engineered` chip on ADR-0003 (correctly shown). |

## Verdict
CONCERNS: 2

## Next action
asd-ba autofixes finding #1 (prd.html traceability AC-22 audit-id citation); asd-architect autofixes finding #2 (adr.html original-provenance chips). Both are in-loop creator autofixes — no escalation required. Re-dispatch documentation review next iteration. SSoT integrity, traceability completeness, provenance frontmatter, and shell-wrapping are otherwise sound.

## Escalations (optional)
- none
