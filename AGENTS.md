# Coding Standards

## Testing and Coverage
- Use coverlet for test coverage.
- Add tests for all new code.
- Target 90% branch coverage (minimum goal for new code).
- Prefer deterministic tests and avoid time-based flakiness.

## General
- Keep APIs allocation-aware; avoid unnecessary per-frame allocations.
- Prefer explicit scoping (`Begin*/End*`) for clarity in UI construction.
- Keep changes small and reviewable; update documentation alongside behavior changes.
- When a plan step is completed, update the Implementation Status in the relevant docs to reflect progress.
- New code should live in the `Noobish` namespace (avoid `Noobish.V2` in new files).
