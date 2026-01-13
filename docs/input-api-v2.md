# Input API Draft (V2)

Goal: decouple input processing from rendering while allowing tight, allocation-free queries by `localId` during `tick`.

## Desired usage
```fsharp
let frameCtx = NoobishV2.beginFrame "Demo/Simple" components
// build UI using localId on interactive components
NoobishV2.beginButton "Get Started" 1us frameCtx |> NoobishV2.endButton |> ignore
NoobishV2.endFrame width height frameCtx

NoobishInputV2.process inputState components inputBuffer

if inputBuffer.WasClicked 1us then
    // handle click in tick
```

## Proposed data structures
```fsharp
type InputBufferV2(capacity: int) =
    member Clicked: bool[]
    member Pressed: bool[]
    member TextChanged: bool[]
    member TextPayload: string[]
    member ActiveIndices: ResizeArray<int>
    member LocalIdToIndex: System.Collections.Generic.Dictionary<uint16, int>

    member Reset: NoobishComponentsV2 -> unit
    member WasClicked: uint16 -> bool
    member WasPressed: uint16 -> bool
    member TryGetTextChanged: uint16 -> voption<string>
```

## Proposed flow
1. **Build UI** (V2 builder calls) and assign `localId` to any interactive element.
2. **Prepare mapping**: `Reset` clears only the active indices and rebuilds `LocalIdToIndex` from current components (no allocations when capacity is stable).
3. **Process input**: `NoobishInputV2.process` walks visible components, computes hit tests, and marks `Clicked/Pressed/TextChanged`.
4. **Tick**: game logic calls `WasClicked localId`, `WasPressed localId`, `TryGetTextChanged localId` in a tight loop.

## Proposed API surface (module)
```fsharp
module NoobishInputV2 =
    val process: NoobishInputState -> NoobishComponentsV2 -> InputBufferV2 -> unit
```

Notes:
- `LocalIdToIndex` only includes components with `LocalId <> 0us`.
- `InputBufferV2.Reset` should not clear full arrays each frame; only clear indices in `ActiveIndices`.
- `TextPayload` is indexed by component index for `TryGetTextChanged`.
- Avoid lambdas or per-frame allocation in input processing.
