# Input API Draft (V2)

Goal: decouple input processing from rendering while allowing tight, allocation-free queries by `localId` during `tick`.

## Desired usage
```fsharp
let frameCtx = NoobishV2.beginFrame "Demo/Simple" components
// build UI using localId on interactive components
NoobishV2.beginButton "Get Started" 1us frameCtx |> NoobishV2.endButton |> ignore
NoobishV2.endFrame width height frameCtx

NoobishInputV2.process inputState components inputBuffer

if inputBuffer.WasClicked(1us, NoobishMouseButtonId.Left) then
    // handle click in tick
```

### C#-friendly wrapper
```fsharp
let ui = NoobishUserInterface(64)
let frameCtx = ui.BeginFrame "Demo/Simple"
NoobishV2.beginButton "Get Started" 1us frameCtx |> ui.ReleaseContext |> ignore
ui.EndFrame(width, height, frameCtx)
ui.ProcessInput inputState

match ui.TryGetBounds 1us with
| ValueSome bounds -> bounds.Width |> ignore
| ValueNone -> ()
```

## Proposed data structures
```fsharp
type InputBufferV2(capacity: int) =
    member Clicked: bool[]
    member Pressed: bool[]
    member TextChanged: bool[]
    member TextPayload: string[]
    member PointerConsumed: bool
    member KeyboardConsumed: bool
    member ActiveIndices: ResizeArray<int>
    member LocalIdToIndex: System.Collections.Generic.Dictionary<uint16, int>

    member Reset: NoobishComponentsV2 -> unit
    member WasClicked: uint16 * NoobishMouseButtonId -> bool
    member WasPressed: uint16 * NoobishMouseButtonId -> bool
    member WasReleased: uint16 * NoobishMouseButtonId -> bool
    member IsDown: uint16 * NoobishMouseButtonId -> bool
    member TryGetTextChanged: uint16 -> voption<string>
    member TryGetBounds: NoobishComponentsV2 * uint16 -> voption<NoobishRectangle>
    member TryGetSize: NoobishComponentsV2 * uint16 -> voption<NoobishSize>

type NoobishUserInterface(capacity: int) =
    member Components: NoobishComponentsV2
    member InputBuffer: InputBufferV2
    member BeginFrame: string -> ComponentContextV2
    member EndFrame: float32 * float32 * ComponentContextV2 -> unit
    member ProcessInput: INoobishInputState -> unit
    member ReleaseContext: ComponentContextV2 -> unit
    member WasClicked: uint16 * NoobishMouseButtonId -> bool
    member WasPressed: uint16 * NoobishMouseButtonId -> bool
    member WasReleased: uint16 * NoobishMouseButtonId -> bool
    member IsDown: uint16 * NoobishMouseButtonId -> bool
    member TryGetTextChanged: uint16 -> voption<string>
    member TryGetSliderChanged: uint16 -> voption<float32>
    member TryGetBounds: uint16 -> voption<NoobishRectangle>
    member TryGetSize: uint16 -> voption<NoobishSize>
```

## Proposed flow
1. **Build UI** (V2 builder calls) and assign `localId` to any interactive element.
2. **Prepare mapping**: `Reset` clears only the active indices and rebuilds `LocalIdToIndex` from current components (no allocations when capacity is stable).
3. **Process input**: `NoobishInputV2.process` walks visible components, computes hit tests, consumes text input, and marks `Clicked/Pressed/TextChanged`.
4. **Tick**: game logic calls `WasClicked(localId, NoobishMouseButtonId.Left)`, `WasPressed(localId, NoobishMouseButtonId.Left)`, `TryGetTextChanged localId` in a tight loop.
   - Layout queries can call `TryGetBounds(components, localId)` to read `Left/Right/Top/Bottom/Width/Height` after layout.
   - When using `NoobishUserInterface`, call `ProcessInput` before queries to refresh the localId map.
5. **Consume flags**: `PointerConsumed`/`KeyboardConsumed` indicate whether input was handled by components that opted in via `Wants*`.

## Proposed API surface (module)
```fsharp
module NoobishInputV2 =
    val process: NoobishInputState -> NoobishComponentsV2 -> InputBufferV2 -> unit
```

Notes:
- `LocalIdToIndex` only includes components with `LocalId <> 0us`.
- `InputBufferV2.Reset` should not clear full arrays each frame; only clear indices in `ActiveIndices`.
- `TextPayload` is indexed by component index for `TryGetTextChanged`.
- `INoobishInputState.ScrollWheelDelta` reports the per-frame mouse wheel delta for scroll containers.
- `INoobishInputState.ConsumeTextInput` returns the current text buffer and clears it.
- Avoid lambdas or per-frame allocation in input processing.

## Optional: Event Queue (for dynamic content)
An additional event queue can coexist with the input buffer to support list/dynamic content without polling.

### Shape
```fsharp
type InputEventType =
    | Clicked
    | Pressed
    | Released
    | TextChanged

type InputEvent = {
    ComponentId: UIComponentIdV2
    EventType: InputEventType
    PayloadIndex: int
}
```

### Flow
- `process` fills `InputBufferV2` as before, and optionally appends `InputEvent` to a reusable `ResizeArray`.
- For text changes, store payloads in a parallel array and set `PayloadIndex`.
- Consumers can use `match` on events for dynamic lists without predeclared ids.

### Trade-offs
- Queue is better for dynamic content or “any clicked” logic.
- Buffer polling is cheaper and ideal for fixed, known controls.
