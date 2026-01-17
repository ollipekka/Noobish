# Custom/Composite Components (V2)

This doc describes how to add custom or composite components to Noobish V2.

## Composite components (built from existing primitives)
- Write helpers that take and return `ComponentContextV2`.
- Use `begin*/end*` calls inside the helper and return the parent context.
- Pass stable `localId` values for interactive children so input polling stays deterministic.
- Avoid per-frame allocations or closures in the builder chain.

Example:
```fsharp
let fancyCard (title: string) (buttonId: uint16) (ctx: ComponentContextV2) =
    NoobishV2.beginPanel ctx
        |> NoobishV2.setPadding { NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f }
        |> NoobishV2.beginHeader title
            |> NoobishV2.endHeader
        |> NoobishV2.beginButton "Go" buttonId
            |> NoobishV2.setMinHeight 28f
            |> NoobishV2.endButton
        |> NoobishV2.endPanel
```

## New primitive components (new storage/behavior)
Add a `begin*/end*` pair that calls `createComponent` and sets component fields.
Then extend the V2 pipeline modules where the new data is used.

Required touch points:
- Storage/flags: `src/Noobish/NoobishComponentsV2.fs`
- Builder API: `src/Noobish/NoobishV2.fs`
- Measure/layout/render: `src/Noobish/NoobishMeasureV2.fs`, `src/Noobish/NoobishLayoutV2.fs`, `src/Noobish/NoobishRenderV2.fs`
- Input flags/processing (if interactive): `src/Noobish/NoobishInputV2.fs`

Guidelines:
- Prefer explicit `Begin*/End*` for any component that can contain children.
- Limit shorthands to leaf components (no nested children).
- Keep local ids stable to preserve input/query behavior.
- Keep APIs allocation-aware; avoid per-frame allocations.
