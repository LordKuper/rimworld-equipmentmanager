[REVIEW-impl-external]: CONCERNS

# External Review (Codex CLI) — impl-review iter-02

- **Reviewer**: Codex CLI (codex-cli 0.130.0, model gpt-5.5), invoked via `codex.cmd` stdin pipe.
- **Availability**: AVAILABLE.
- **Scope**: incremental — fix-mode commits since iter-01 (`04356fb..93b8272`): test changes (`ItemRuleAndLoadoutTests.cs`) + coding-rule doc update. Plus verification that the three iter-01 findings are genuinely resolved.
- **Severity floor**: MEDIUM (low dropped).
- **New project rule applied**: tests must use FluentAssertions wherever possible; no NUnit `Assert.*` left.

## Prior iter-01 finding verification (stalemate check)

| # | iter-01 finding | Status this iter | Evidence |
|---|---|---|---|
| 1 | (high) StateIsolationTestBase null-tolerant guard silently skipped isolation | RESOLVED (genuine) | `StateIsolationTestBase.SnapshotState`/`RestoreState` now `throw InvalidOperationException` when the reflected `_equipmentManager` field is missing — fail-loud, not papered over. |
| 2 | (medium) empty assertion-free `[Test]` methods | RESOLVED (genuine) | Copy tests carry real `.Should()` assertions; game-context tests (`Loadout_PrimaryRuleType_*`, `Loadout_Initialize_*`) are `[Ignore("reason")]` with rationale — the allowed mechanism for un-runnable tests. |
| 3 | (medium) tests referenced ASD artifacts | PARTIALLY RESOLVED — recurs in a sibling file | Removed from `ItemRuleAndLoadoutTests.cs`, but `StatRangesTests.cs` still references audit finding-ID `C-4` (lines 8, 37, 52). Same category, different file. |

**Stalemate status: NOT a stalemate.** The qualifying finding this iteration concerns a *different file* (`StatRangesTests.cs`) than the iter-01 finding (`ItemRuleAndLoadoutTests.cs`). The finding *set* is not identical across two consecutive iterations, so the stalemate trigger does not fire. No escalation.

## Kept findings (≥ MEDIUM floor)

| ID | Severity (Codex → ASD) | File | Issue |
|---|---|---|---|
| T-ARTIFACT-REFS | major → **high** | `Source/EquipmentManager.Tests/StatRangesTests.cs` (lines 8, 37, 52) | Test references ASD audit finding-ID `C-4` ("C-4 defect fix" / "C-4 correctness fix") in comments. The coding rule requires test references to ASD audit artifacts / finding IDs to be removed. This is a recurrence of iter-01 finding #3 in a file not touched by the fix commits — the iter-01 fix was scoped to `ItemRuleAndLoadoutTests.cs` only. Source: Codex finding `T-ARTIFACT-REFS`. |

## Dropped — below floor

None. (No low-severity findings raised; all changed-code observations were either resolved-confirmations or above floor.)

## Dropped — nitpick categories

| Item | Reason |
|---|---|
| Two stray blank lines added in `ItemRuleAndLoadoutTests.cs` (diff @@36, @@61) | Cosmetic whitespace; nitpick. |
| Object-initializer refactor of Copy tests | Equivalent behavior, style-only improvement; no defect. |

## Out-of-scope confirmations

- No NUnit `Assert.*` / `Assert.That` / `ClassicAssert` / `Assert.Pass`/`Fail`/`Inconclusive` anywhere in the test project — new FluentAssertions-only rule satisfied.
- Deferred findings C-10 / C-14 / C-15 not flagged. UI review waived.
- Nullable / Scribe field conventions: no violations in the reviewed diff (diff is test-only + doc; no production field changes in range).

## Verdict

**CONCERNS** — one qualifying high-severity finding (`T-ARTIFACT-REFS`). It is a test-hygiene / artifact-leakage issue, not a build or correctness blocker, so it does not warrant FAIL; it does warrant a fix iteration. The three iter-01 findings are otherwise genuinely resolved (#1, #2) or resolved-in-scope with one sibling-file recurrence (#3).

> Note on Codex run: Codex's own in-sandbox re-grep attempts failed (`CreateProcessWithLogonW 1326`, read-only sandbox); it produced its verdict from the supplied diff + grep facts. Codex emitted FAIL on the single finding; mapped down to CONCERNS here per ASD severity semantics (single high finding, no build/correctness breakage).

## Next action

Remove the `C-4` audit finding-ID references from `Source/EquipmentManager.Tests/StatRangesTests.cs` comments (lines 8, 37, 52), keeping the behavioral description of the test without the ASD identifier. Re-run impl-review iter-03 to confirm.
