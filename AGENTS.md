# Coding Standards

## Testing and Coverage
- Use coverlet for test coverage.
- Add tests for all new code.
- Aim for high branch coverage, not just line coverage.
- Prefer deterministic tests and avoid time-based flakiness.

## General
- Keep APIs allocation-aware; avoid unnecessary per-frame allocations.
- Prefer explicit scoping (`Begin*/End*`) for clarity in UI construction.
- Keep changes small and reviewable; update documentation alongside behavior changes.
- When a plan step is completed, update the Implementation Status in the relevant docs to reflect progress.
