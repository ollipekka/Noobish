# Allocation-Friendly UI Plan

## Goals
- Avoid per-frame allocations from `SetChildren` and other builder helpers.
- Allow UI to be described and re-evaluated every frame (immediate-mode style) without churn.
- Keep component identity stable enough for focus, input, and layout caching.
- Avoid shared pools; prefer local frame arenas or persistent buffers.

## Start With UIComponentId
Today `UIComponentId` is a measured `int` with `index` and `id` packed into 32 bits, and `empty` is `0xFFFFFFFF`. For immediate-mode rendering you need IDs that are both stable across frames and cheap to generate without allocations.

### Proposed ID direction
- Move from `int<UIComponentId>` to a small struct record with explicit fields.
- Keep the fast index lookup but add a namespace + generation to prevent accidental reuse.

Example layout (64-bit, no allocation):
- `uint16 namespace`
- `uint16 generation`
- `uint16 index`
- `uint16 localId` (optional, for user-provided ids like "button-1")

This preserves O(1) index lookups while letting you separate UI areas (namespace) and distinguish component instances across frames (generation). If you want to stay 32-bit, shrink to:
- `uint8 namespace`, `uint8 generation`, `uint16 index`

### Shape choice (F#)
Struct record (chosen approach):
```fsharp
[<Struct>]
type UIComponentId = {
    Namespace: uint16
    Generation: uint16
    Index: uint16
    LocalId: uint16
}
```
Notes:
- This keeps identity readable and easy to evolve without changing call sites.
- `empty` can be represented as all `0xFFFF`.

### Migration steps
1. Introduce a `struct` `UIComponentId` with explicit fields and helper constructors.
2. Keep `UIComponentId.empty` as `namespace = 0xFF`/`index = 0xFFFF` sentinel.
3. Update `UIComponentId.index` to read the field directly.
4. Update `NoobishComponents.Id` and `ParentId` arrays to the new struct type.

## Frames, Pages, and Internal Namespaces
Use a user-facing frame label (string) to describe the current UI page/context, and keep namespaces as internal vocabulary for partitioning.

### Proposed API
- `Noobish.BeginFrame(page: string)` -> returns a scoped builder for a labeled UI frame.
- `Noobish.EndFrame()` -> closes the frame scope.

Rules:
- The `page` label is stored for diagnostics, focus scoping, and frame-to-frame mapping.
- The namespace id remains part of `UIComponentId` but is managed internally by the UI system.
- Each namespace maintains its own `RunningId` and `Count` for the frame.
- Input focus is stored with a full `UIComponentId` (including namespace), but page label gates cross-page reuse.

## Frame-Driven UI Build
A rebuild-each-frame approach can be allocation-friendly if the backing storage is reusable.

### Frame model
- Keep a fixed-capacity `NoobishComponents` per frame, as you do now.
- At the start of a frame, reset counts and clear per-frame arrays without reallocating.
- When a component is (re)created, it reuses the same index if possible.

### Stable identity without caching arrays
- Use a `namespace + localId` mapping table to resolve existing component index.
- The mapping table can be a persistent `Dictionary<uint32, uint16>` keyed by `(namespace, localId)`.
- If no localId is provided, use a running index and accept frame-local identity.

## Replace SetChildren/AddChildren to Avoid Arrays
`SetChildren` currently allocates by taking an array. The goal is to feed children without array creation.

### Pipeline-style API sketch (Noobish 2.0)
This keeps everything in a frame context, avoids lambdas, and ensures child scopes are closed.
```fsharp
ui.BeginFrame("Settings/Audio")
    |> Panel
    |> SetGridLayout(1, 9)
    |> SetFill
    |> AddChildren
        |> DivVertical()
        |> SetRowspan 2
        |> SetFill
        |> BeginChildren
            |> Header "Audio"
            |> HorizontalRule()
            |> EndChildren
        |> Label "Music" |> ui.FillHorizontal
        |> Slider (0f, 100f) 1.0f 50f (fun _ -> ())
            |> ui.FillHorizontal
    |> EndChildren
|> ui.EndFrame
```

Notes on the sketch:
- `BeginFrame` returns a frame-scoped context value (likely a struct) used by `Panel`, `Label`, etc.
- `AddChildren` switches the pipeline into a children-builder context; `EndChildren` returns to component context.
- `BeginChildren`/`EndChildren` must be balanced; the builder can track its parent index to prevent misuse.

### Draft context signatures
```fsharp
type ContainerContext = struct end

// Frame entry/exit
val BeginFrame : string -> ContainerContext
val EndFrame : ContainerContext -> unit

// Component builders (convenience surface)
val Panel : ContainerContext -> struct(ContainerContext * UIComponentId)
val PanelVertical : ContainerContext -> struct(ContainerContext * UIComponentId)
val PanelHorizontal : ContainerContext -> struct(ContainerContext * UIComponentId)
val DivVertical : ContainerContext -> struct(ContainerContext * UIComponentId)
val DivHorizontal : ContainerContext -> struct(ContainerContext * UIComponentId)
val Header : string -> ContainerContext -> struct(ContainerContext * UIComponentId)
val Label : string -> ContainerContext -> struct(ContainerContext * UIComponentId)
val Paragraph : string -> ContainerContext -> struct(ContainerContext * UIComponentId)
val HorizontalRule : ContainerContext -> struct(ContainerContext * UIComponentId)
val Button : string -> (UIComponentId -> GameTime -> unit) -> ContainerContext -> struct(ContainerContext * UIComponentId)
val Checkbox : bool -> (bool -> unit) -> ContainerContext -> struct(ContainerContext * UIComponentId)
val Slider : (float32 * float32) -> float32 -> float32 -> (float32 -> unit) -> ContainerContext -> struct(ContainerContext * UIComponentId)
val ProgressBar : float32 -> ContainerContext -> struct(ContainerContext * UIComponentId)
val Textbox : string -> (string -> unit) -> ContainerContext -> struct(ContainerContext * UIComponentId)
val Image : ContainerContext -> struct(ContainerContext * UIComponentId)
val Space : ContainerContext -> struct(ContainerContext * UIComponentId)

// Component modifiers
val SetGridLayout : (int * int) -> struct(ContainerContext * UIComponentId) -> struct(ContainerContext * UIComponentId)
val SetRowspan : int -> struct(ContainerContext * UIComponentId) -> struct(ContainerContext * UIComponentId)
val SetFill : struct(ContainerContext * UIComponentId) -> struct(ContainerContext * UIComponentId)
val FillHorizontal : struct(ContainerContext * UIComponentId) -> struct(ContainerContext * UIComponentId)

// Children wiring
val AddChildren : struct(ContainerContext * UIComponentId) -> ContainerContext
val BeginChildren : struct(ContainerContext * UIComponentId) -> ContainerContext
val EndChildren : ContainerContext -> struct(ContainerContext * UIComponentId)

// Container additions
val Add : struct(ContainerContext * UIComponentId) -> ContainerContext -> ContainerContext
```

Notes:
- `ContainerContext` must carry a reference to the ECS backing storage so modifiers know where to write.
- Component builders return `struct(ContainerContext * UIComponentId)` so pipes keep access to the backing context.
- `AddChildren` and `BeginChildren` can be aliases; keep one public surface if possible.

### Context shape (suggested)
```fsharp
type ContainerContext = {
    FrameId: int
    Page: string
    Components: NoobishComponents
    ParentId: UIComponentId
}
```

Notes:
- `ContainerContext` can be a reference type (class/record) and pooled per frame if you want to avoid allocations.

### Proposed options
1. **Builder-style scope**
   - `BeginChildren(parentId)` returns a builder that exposes `Add(childId)` and must be closed with `EndChildren(builder)`.
   - Builder writes directly into `Components.Children[parentIndex]`.
   - No arrays required; storage is the existing `ResizeArray`.

2. **Span-based overload**
   - `SetChildrenSpan(parentId, children: ReadOnlySpan<UIComponentId>)`.
   - Allows `stackalloc` in call sites for tiny, fixed-size child sets.

3. **Immediate mode traversal**
   - Replace `SetChildren` with `Build` functions that return a "layout cursor" and push children as they are created.
   - Example: `let panel = ui.Panel(); ui.AddChild(panel, ui.Label("A")); ui.AddChild(panel, ui.Label("B"))`.

### Recommendation
Start with builder-style `BeginChildren` + `AddChild` as the minimal change that removes array creation without introducing shared pools.

### Example usage (no lambdas)
```fsharp
ui.BeginFrame("Settings/Audio")
    |> PanelVertical
    |> SetFill
    |> AddChildren
        |> Label "Title"
        |> Space |> SetRowspan 1 |> SetColspan 16
        |> Button "Apply" (fun _ _ -> ())
    |> EndChildren
|> EndFrame
```

## Memory Strategy (No Shared Pool)
- Keep `ResizeArray` buffers per component but clear them per frame (do not reallocate).
- Add a `ClearChildrenForFrame` pass to reset `Count` and clear child lists only for active components.
- Avoid allocations by pre-allocating `ResizeArray` capacity when a component is first created.

## Incremental Steps (V2-first migration)
1. Add `docs/allocation-friendly-ui-plan.md` (this document).
2. Introduce V2 types alongside existing ones (`UIComponentIdV2`, `NoobishComponentsV2`, `ContainerContextV2`, `BuilderV2`, etc.).
3. Implement the new `UIComponentIdV2` struct and wire it through `NoobishComponentsV2`.
4. Add the V2 frame lifecycle (`BeginFrame`, `EndFrame`) and `ContainerContextV2` plumbing.
5. Build `BuilderV2` that mirrors the current APIs (e.g., `Panel`, `Label`, `SetFill`) but returns `struct(ContainerContextV2 * UIComponentIdV2)` and operates on the V2 ECS.
6. Introduce `BeginChildren`/`EndChildren` in `BuilderV2` and migrate `SetChildren` call sites to V2.
7. Update the project to use V2 types end-to-end (demo + tests).
8. Remove old types once they are no longer referenced.
9. Drop the `V2` suffixes after the project runs fully on the new path.

## Open Questions
- Do you want `localId` to be user-defined or derived from call-site order?
- Should `generation` be per-frame or per namespace?
- How many namespaces should exist by default (e.g. 1 for user UI, 1 for debug UI)?
