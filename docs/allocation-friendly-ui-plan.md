# Allocation-Friendly UI Plan (Immediate-Mode: Noobish IM)

## Goals
- Avoid per-frame allocations from `SetChildren` and other builder helpers.
- Allow UI to be described and re-evaluated every frame (immediate-mode style) without churn.
- Keep component identity stable enough for focus, input, and layout caching.
- Avoid shared pools; prefer local frame arenas or persistent buffers.

## Example Usage (No Lambdas)
```fsharp
ui.BeginFrame("Settings/Audio")
    |> PanelVertical
    |> SetFill
    |> BeginChildren
        |> Label "Title"
        |> Space |> SetRowspan 1 |> SetColspan 16
        |> Button "Apply" (fun _ _ -> ())
    |> EndChildren
|> EndFrame
```

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
- Derive the internal namespace from a deterministic hash of `page` (FNV-1a) with collision handling.
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
    |> BeginChildren
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
- `BeginFrame` returns a container context used by `Panel`, `Label`, etc.
- `AddChildren` switches the pipeline into a children-builder context; `EndChildren` returns to component context.
- `BeginChildren`/`EndChildren` must be balanced; the builder can track its parent index to prevent misuse.

### Draft context signatures
```fsharp
type ComponentContext = struct end

// Frame entry/exit
val BeginFrame : string -> ComponentContext
val EndFrame : ComponentContext -> unit

// Component builders (convenience surface)
val Panel : ComponentContext -> struct(ComponentContext * UIComponentId)
val PanelVertical : ComponentContext -> struct(ComponentContext * UIComponentId)
val PanelHorizontal : ComponentContext -> struct(ComponentContext * UIComponentId)
val DivVertical : ComponentContext -> struct(ComponentContext * UIComponentId)
val DivHorizontal : ComponentContext -> struct(ComponentContext * UIComponentId)
val Header : string -> ComponentContext -> struct(ComponentContext * UIComponentId)
val Label : string -> ComponentContext -> struct(ComponentContext * UIComponentId)
val Paragraph : string -> ComponentContext -> struct(ComponentContext * UIComponentId)
val HorizontalRule : ComponentContext -> struct(ComponentContext * UIComponentId)
val Button : string -> (UIComponentId -> GameTime -> unit) -> ComponentContext -> struct(ComponentContext * UIComponentId)
val Checkbox : bool -> (bool -> unit) -> ComponentContext -> struct(ComponentContext * UIComponentId)
val Slider : (float32 * float32) -> float32 -> float32 -> (float32 -> unit) -> ComponentContext -> struct(ComponentContext * UIComponentId)
val ProgressBar : float32 -> ComponentContext -> struct(ComponentContext * UIComponentId)
val Textbox : string -> (string -> unit) -> ComponentContext -> struct(ComponentContext * UIComponentId)
val Image : ComponentContext -> struct(ComponentContext * UIComponentId)
val Space : ComponentContext -> struct(ComponentContext * UIComponentId)

// Component modifiers
val SetGridLayout : (int * int) -> struct(ComponentContext * UIComponentId) -> struct(ComponentContext * UIComponentId)
val SetRowspan : int -> struct(ComponentContext * UIComponentId) -> struct(ComponentContext * UIComponentId)
val SetFill : struct(ComponentContext * UIComponentId) -> struct(ComponentContext * UIComponentId)
val FillHorizontal : struct(ComponentContext * UIComponentId) -> struct(ComponentContext * UIComponentId)

// Children wiring
val BeginChildren : struct(ComponentContext * UIComponentId) -> ComponentContext
val EndChildren : ComponentContext -> struct(ComponentContext * UIComponentId)

// Container additions
val Add : struct(ComponentContext * UIComponentId) -> ComponentContext -> ComponentContext
```

Notes:
- `ComponentContext` must carry a reference to the ECS backing storage so modifiers know where to write.
- Component builders return `struct(ComponentContext * UIComponentId)` so pipes keep access to the backing context.
- Keep only `BeginChildren`/`EndChildren` as the public surface.
- Use a single `ParentId` in `ComponentContext` (no stack); treat the backing arrays as the structural source of truth.

### Context shape (suggested)
```fsharp
type ComponentContext = class
    FrameId: int
    Page: string
    Components: NoobishComponents
    ParentId: UIComponentId
end
```

Notes:
- `ComponentContext` should be a reference type and pooled per frame to avoid allocations.
- V2 APIs live in separate modules until the rename step.

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

### Begin/End component style with shorthands
For explicit scoping, introduce `Begin<Component>`/`End<Component>` variants. Simple components can be exposed as shorthands that internally call `Begin` + `End`.

Example:
```fsharp
ui.BeginFrame("Settings/Audio")
    |> BeginPanel
        |> SetGridLayout(1, 9)
        |> SetFill
        |> BeginChildren
            |> BeginDivVertical
                |> SetRowspan 2
                |> BeginHeader "Audio" |> EndHeader
                |> BeginHorizontalRule |> EndHorizontalRule
            |> EndDivVertical
            |> Label "Music"          // shorthand for BeginLabel |> EndLabel
            |> Slider (0f, 100f) 1.0f 50f (fun _ -> ())  // shorthand for BeginSlider |> EndSlider
        |> EndChildren
    |> EndPanel
|> EndFrame
```

Notes:
- Shorthands should be limited to components that do not require nested children.
- `Begin<Component>`/`End<Component>` are the primary API; shorthands are syntactic sugar.

## Memory Strategy (No Shared Pool)
- Keep `ResizeArray` buffers per component but clear them per frame (do not reallocate).
- Add a `ClearChildrenForFrame` pass to reset `Count` and clear child lists only for active components.
- Avoid allocations by pre-allocating `ResizeArray` capacity when a component is first created.

## Input Handling (Allocation-Friendly)
Immediate-mode UI can stay allocation-free if input is handled as a second pass that writes into preallocated buffers, rather than wiring callbacks per component.

### Goals
- No per-frame allocations for input dispatch.
- No closures required in builder calls.
- Stable, pollable input state keyed by `UIComponentIdV2`.

### Proposed shape
- Add per-frame input arrays (or a reusable event queue) to `NoobishComponentsV2`:
  - `Clicked: bool[]`
  - `Pressed: bool[]`
  - `TextChanged: bool[]`
  - Optional payload storage: `TextValue: string[]`, `KeyPressed: Keys[]`, etc.
- During input processing, write into these arrays (single pass) based on `Wants*` flags.
- The fluent API returns `UIComponentIdV2`, and callers poll input state by index:
```fsharp
let struct (ctx, cid) = NoobishV2.button "Save" ctx
if components.Clicked.[int cid.Index] then
    save()
```

### Event queue option (if payloads are needed)
Use a preallocated ring buffer for events to avoid allocations while supporting payloads:
```fsharp
type InputEventKind =
    | Clicked
    | Pressed
    | TextChanged

[<Struct>]
type InputEvent = {
    Id: UIComponentIdV2
    Kind: InputEventKind
    PayloadIndex: int
}
```
Payloads can be stored in parallel arrays (e.g., `TextPayload: string[]`) and indexed by `PayloadIndex`.

### Integration sketch
- `ProcessInputV2` walks components once, sets `Clicked/Pressed/TextChanged`, and fills the queue.
- `BeginFrame` clears the input buffers for active indices (not full array clears).
- Existing `Wants*` flags remain the opt-in for which components participate.

## Incremental Steps (V2-first migration)
1. Implement test coverage with coverlet and wire it into the build/test flow.
2. Add `docs/allocation-friendly-ui-plan.md` (this document).
3. Introduce V2 types alongside existing ones (`UIComponentIdV2`, `NoobishComponentsV2`, `ComponentContextV2`, `BuilderV2`, etc.).
4. Implement the new `UIComponentIdV2` struct and wire it through `NoobishComponentsV2`.
5. Add the V2 frame lifecycle (`BeginFrame`, `EndFrame`) and `ComponentContextV2` plumbing.
6. Build `BuilderV2` that mirrors the current APIs (e.g., `Panel`, `Label`, `SetFill`) but returns `struct(ComponentContextV2 * UIComponentIdV2)` and operates on the V2 ECS.
7. Introduce `BeginChildren`/`EndChildren` in `BuilderV2` and migrate `SetChildren` call sites to V2.
8. Update the project to use V2 types end-to-end (demo + tests).
9. Remove old types once they are no longer referenced.
10. Drop the `V2` suffixes after the project runs fully on the new path.

## Implementation Status
- Done: coverlet collector added to `src/Noobish.Test/Noobish.Test.fsproj`.
- Done: `test.sh` added to run unit tests with coverage.
- Done: branch coverage goal set to 90% for new code in `AGENTS.md`.
- Done: added initial V2 types/helpers and tests (`src/Noobish/TypesV2.fs`, `src/Noobish.Test/TypesV2Tests.fs`).
- Done: added `ComponentContextV2` and `NoobishComponentsV2` with tests (`src/Noobish/NoobishComponentsV2.fs`, `src/Noobish.Test/NoobishComponentsV2Tests.fs`).
- Done: added `NoobishV2` API shell and tests (`src/Noobish/NoobishV2.fs`, `src/Noobish.Test/NoobishV2Tests.fs`).
- Done: added context pooling interface + implementation and ensured `ComponentContextV2` implements the context interface.
- Done: added initial V2 builder helpers for header/label/paragraph/textbox/button/space/div/grid/panel/canvas plus storage fields and tests.
- Done: added V2 slider builder API and backing storage fields with tests.
- Done: added minimal V2 layout pass (`NoobishLayoutV2`) with stack/grid/relative handling and tests.
- Done: added MonoGame renderer V2 (`src/Noobish/NoobishRenderV2.fs`) with helper tests.
- Done: cached V2 namespace hash per frame; current hash is case/whitespace sensitive to avoid allocations.
- Done: drafted V2 input API (`docs/input-api-v2.md`) for tick-time localId queries.
- Done: added V2 input buffer scaffolding (`src/Noobish/NoobishInputV2.fs`) with basic tests.
- Done: added slider input polling and drag processing in V2 input buffer.
- Done: added V2 slider render pin placement and bounds helper with tests.
- Done: V2 input API uses engine-agnostic interfaces; platform backends should remain separate from core.
- Done: added V2 measure pass (`src/Noobish/NoobishMeasureV2.fs`) and content size storage for layout.
- Done: added `NoobishComponentsV2.Clear()` and `beginFrame` guard for cleared component state.
- Done: added V2 grid span builder helpers with tests.
- Done: applied V2 style-driven padding/margin defaults with overrides.
- Done: added V2 progress bar component with segmented render support.
- Done: refactored V2 input processing helpers with pure hit testing and floor-based slider stepping, plus added tests.
- Done: added V2 scroll container support (storage, input wheel handling, layout bounds, render offsets).
- Done: documented V1/V2 component and behavior parity snapshot with noted intentional differences.
- Done: added InputBufferV2 tests to cover missing branches for text/slider change queries.
- Done: added NoobishV2 progress segment clamp coverage for non-positive values.
- Done: added NoobishV2 progress segment coverage for positive values.
- Done: added post-layout measure pass for wrapped text and wired it into the demo layout flow.
- Done: refactored `NoobishMeasureV2` helpers for testability and added coverage for wrap width and layout sizing branches.
- Done: refactored `NoobishLayoutV2` helpers for testability and expanded layout helper test coverage.
- Done: added V2 textbox focus, caret, and text input handling with render-time cursor support.
- Done: added `NoobishV2MonoGame.processFrame` convenience helper for measure/layout/input pipeline in the MonoGame assembly.
- Done: enabled horizontal scrolling when both axes are active, anchored slider stepping to range starts, extracted text-input helper, and expanded V2 input tests.
- Done: added MonoGame test project plus StyleSheetReader helper parsing functions with tests.

## V1/V2 Parity Snapshot (Component + Behavior)
V2 is not aiming for 1:1 parity. Some behaviors are intentionally redesigned (for example: any container can scroll, and there is no dedicated scrollable component).

### Components
| Component/Feature | V1 | V2 | Notes |
| --- | --- | --- | --- |
| Header | Yes | Yes | V2 builder in `NoobishV2` |
| Label | Yes | Yes | V2 builder in `NoobishV2` |
| Paragraph | Yes | Yes | Text wrap + align supported |
| Textbox | Yes | Yes | V2 supports focus, editing, and caret rendering |
| Button | Yes | Yes | Click/press flags in V2 input buffer |
| Checkbox | Yes | Partial | V2 has builder + toggle state; visuals/behavior still minimal |
| Slider | Yes | Yes | V2 has builder, render pin, and input drag |
| ProgressBar | Yes | Yes | V2 supports segmented progress |
| Space | Yes | Yes | V2 builder in `NoobishV2` |
| Panel | Yes | Partial | V2 has panel + stack/grid containers, but no window helpers |
| Division/Stack | Yes | Yes | V1 `Div*` vs V2 `beginStack*` with `Division` theme |
| Grid | Yes | Yes | V2 supports grid + spans |
| Canvas/Relative | Yes | Yes | V2 canvas uses relative layout |
| HorizontalRule | Yes | Yes | V2 builder in `NoobishV2` |
| Image | Yes | No | V2 render has no image support yet |
| List | Yes | No | To be redesigned for scrollable containers |
| Combobox | Yes | No | Overlay/menu behavior not implemented in V2 |
| Overlaypane/Window | Yes | Partial | V2 has overlay helpers + layer-aware hit test; no window helpers yet |
| Scroll component | Implicit | No | V2 uses scroll flags on any container |

### Behavior/Functionality
| Behavior | V1 | V2 | Notes |
| --- | --- | --- | --- |
| Click/Press input | Yes | Yes | V2 uses `InputBufferV2` |
| Hover state | Yes | Yes | V2 tracks `Hovered` and renders hover state |
| Toggle state | Yes | Yes | V2 uses `Toggled` + `WantsToggle` |
| Text input/editing | Yes | Yes | V2 consumes text input into focused textboxes |
| Focus/cursor caret | Yes | Yes | V2 tracks focus + caret and renders cursor |
| Scroll wheel | Yes | Yes | V2 scrolls nearest scrollable ancestor |
| Scroll bars visuals | Yes | No | V2 does not render scroll bars yet |
| Layout: stack/grid/relative | Yes | Yes | V2 includes basic layout pass |
| Padding/margin defaults | Yes | Yes | V2 applies style defaults + overrides |
| Image rendering | Yes | No | Pending V2 render support |

## Open Questions
- Do you want `localId` to be user-defined or derived from call-site order?
- Should `generation` be per-frame or per namespace?
- What should the collision strategy be for FNV-1a namespace hashes (e.g. secondary id map, linear probe)?
- Should `ParentId` use `UIComponentId.empty` or `0` as the sentinel for “no parent”?
- How should frame resets handle stale component state (clear all vs. clear only active indices)?

## ToDo (By Component + Dependencies)
- Gap analysis check: review the V1/V2 parity snapshot after each component milestone and update gaps.
- Theme parity: respect V1 theme/style lookups in V2 (colors/fonts/nine-patch/spacing); plan: add a per-frame resolved-style cache keyed by `UIComponentId`+state, reuse precomputed atlas/font indices, and route all render/layout defaults through cached lookups to avoid allocations.
- Input Core: remaining key handling (selection/edit shortcuts), drag interactions; depends on input buffer + input state APIs.
- Layout Core: grid span/alignment, margin/padding overrides, percent sizing, style-driven min size defaults.
- Rendering Core: scissor/clip parity, debug overlays, pressed color blend; depends on layout bounds + style states.
- Text: wrap + align overrides from styles, text bounds selection; depends on layout + style defaults.
- Image Component: basic/atlas/nine-patch rendering; depends on rendering core + style lookups.
- Slider: drag behavior + fill rendering; depends on input core + rendering core.
- List/Combobox: selection + input behavior; depends on input core + rendering core + scroll container.
- State carry: minimal per-frame carry for hover/press/scroll; depends on input core.
