namespace Noobish

open System.Collections.Generic
open Noobish.Internal

type InputBufferV2(capacity: int) =
    let clicked = Array.create capacity false
    let pressed = Array.create capacity false
    let textChanged = Array.create capacity false
    let textPayload = Array.create capacity ""
    let activeFlags = Array.create capacity false
    let activeIndices = ResizeArray<int>()
    let localIdToIndex = Dictionary<uint16, int>()

    let ensureActive index =
        if not activeFlags.[index] then
            activeFlags.[index] <- true
            activeIndices.Add index

    member _.Clicked = clicked
    member _.Pressed = pressed
    member _.TextChanged = textChanged
    member _.TextPayload = textPayload
    member _.ActiveIndices = activeIndices
    member _.LocalIdToIndex = localIdToIndex

    member _.EnsureCapacity(count: int) =
        if count > clicked.Length then
            invalidArg "count" "InputBufferV2 capacity too small for component count."

    member this.Reset(components: NoobishComponentsV2) =
        this.EnsureCapacity components.Count
        for i = 0 to activeIndices.Count - 1 do
            let index = activeIndices.[i]
            clicked.[index] <- false
            pressed.[index] <- false
            textChanged.[index] <- false
            textPayload.[index] <- ""
            activeFlags.[index] <- false
        activeIndices.Clear()
        localIdToIndex.Clear()
        for i = 0 to components.Count - 1 do
            let localId = components.Id.[i].LocalId
            if localId <> 0us then
                localIdToIndex.[localId] <- i

    member this.MarkClicked(index: int) =
        ensureActive index
        clicked.[index] <- true

    member this.MarkPressed(index: int) =
        ensureActive index
        pressed.[index] <- true

    member this.MarkTextChanged(index: int, text: string) =
        ensureActive index
        textChanged.[index] <- true
        textPayload.[index] <- text

    member this.WasClicked(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index -> clicked.[index]
        | false, _ -> false

    member this.WasPressed(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index -> pressed.[index]
        | false, _ -> false

    member this.TryGetTextChanged(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index when textChanged.[index] -> ValueSome textPayload.[index]
        | _ -> ValueNone

module NoobishInputV2 =
    let private contains (bounds: NoobishRectangle) (x: float32) (y: float32) =
        x >= bounds.X && x <= bounds.X + bounds.Width
        && y >= bounds.Y && y <= bounds.Y + bounds.Height

    let process (input: INoobishInputState) (components: NoobishComponentsV2) (buffer: InputBufferV2) =
        buffer.Reset components
        if input.IsPrimaryClick() then
            let x = input.PointerX
            let y = input.PointerY
            let mutable found = false
            let mutable i = components.Count - 1
            while i >= 0 && not found do
                if components.Visible.[i] && components.Enabled.[i] && components.WantsOnClick.[i] then
                    let bounds = components.Bounds.[i]
                    if bounds.Width > 0f && bounds.Height > 0f && contains bounds x y then
                        buffer.MarkClicked i
                        found <- true
                i <- i - 1
