namespace Noobish

open System.Collections.Generic
open Noobish.Internal

type InputBufferV2(capacity: int) =
    let clicked = Array.create capacity false
    let pressed = Array.create capacity false
    let released = Array.create capacity false
    let down = Array.create capacity false
    let textChanged = Array.create capacity false
    let textPayload = Array.create capacity ""
    let activeFlags = Array.create capacity false
    let activeIndices = ResizeArray<int>()
    let localIdToIndex = Dictionary<uint16, int>()
    let mutable downIndex = -1
    let mutable hoveredIndex = -1

    let ensureActive index =
        if not activeFlags.[index] then
            activeFlags.[index] <- true
            activeIndices.Add index

    member _.Clicked = clicked
    member _.Pressed = pressed
    member _.Released = released
    member _.Down = down
    member _.TextChanged = textChanged
    member _.TextPayload = textPayload
    member _.ActiveIndices = activeIndices
    member _.LocalIdToIndex = localIdToIndex
    member _.DownIndex = downIndex
    member _.HoveredIndex
        with get() = hoveredIndex
        and set value = hoveredIndex <- value

    member _.EnsureCapacity(count: int) =
        if count > clicked.Length then
            invalidArg "count" "InputBufferV2 capacity too small for component count."

    member this.Reset(components: NoobishComponentsV2) =
        this.EnsureCapacity components.Count
        for i = 0 to activeIndices.Count - 1 do
            let index = activeIndices.[i]
            clicked.[index] <- false
            pressed.[index] <- false
            released.[index] <- false
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

    member this.MarkReleased(index: int) =
        ensureActive index
        released.[index] <- true

    member this.MarkTextChanged(index: int, text: string) =
        ensureActive index
        textChanged.[index] <- true
        textPayload.[index] <- text

    member this.SetDown(index: int) =
        if downIndex <> index then
            if downIndex >= 0 then
                down.[downIndex] <- false
            downIndex <- index
            down.[index] <- true

    member this.ClearDown() =
        if downIndex >= 0 then
            let index = downIndex
            down.[index] <- false
            downIndex <- -1
            this.MarkReleased index

    member this.WasClicked(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index -> clicked.[index]
        | false, _ -> false

    member this.WasPressed(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index -> pressed.[index]
        | false, _ -> false

    member this.WasReleased(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index -> released.[index]
        | false, _ -> false

    member this.IsDown(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index -> down.[index]
        | false, _ -> false

    member this.TryGetTextChanged(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index when textChanged.[index] -> ValueSome textPayload.[index]
        | _ -> ValueNone

module NoobishInputV2 =
    let private contains (bounds: NoobishRectangle) (x: float32) (y: float32) =
        x >= bounds.X && x <= bounds.X + bounds.Width
        && y >= bounds.Y && y <= bounds.Y + bounds.Height

    let private clippedBounds (components: NoobishComponentsV2) (index: int) =
        let mutable bounds = components.Bounds.[index]
        let mutable parentId = components.ParentId.[index]
        while parentId <> UIComponentIdV2.empty do
            let parentIndex = int parentId.Index
            bounds <- bounds.Clamp components.Bounds.[parentIndex]
            parentId <- components.ParentId.[parentIndex]
        bounds

    let process (input: INoobishInputState) (components: NoobishComponentsV2) (buffer: InputBufferV2) =
        buffer.Reset components
        if buffer.DownIndex >= components.Count then
            buffer.ClearDown()
        let x = input.PointerX
        let y = input.PointerY
        if buffer.HoveredIndex >= components.Count then
            if buffer.HoveredIndex >= 0 && buffer.HoveredIndex < components.Hovered.Length then
                components.Hovered.[buffer.HoveredIndex] <- false
            buffer.HoveredIndex <- -1
        let mutable foundHover = false
        let mutable i = components.Count - 1
        while i >= 0 && not foundHover do
            if components.Visible.[i] && components.Enabled.[i] then
                let bounds = clippedBounds components i
                if bounds.Width > 0f && bounds.Height > 0f && contains bounds x y then
                    if buffer.HoveredIndex <> i then
                        if buffer.HoveredIndex >= 0 then
                            components.Hovered.[buffer.HoveredIndex] <- false
                        components.Hovered.[i] <- true
                        buffer.HoveredIndex <- i
                    foundHover <- true
            i <- i - 1
        if not foundHover && buffer.HoveredIndex >= 0 then
            components.Hovered.[buffer.HoveredIndex] <- false
            buffer.HoveredIndex <- -1
        if input.IsPrimaryDown() then
            if buffer.DownIndex < 0 then
                let mutable found = false
                let mutable i = components.Count - 1
                while i >= 0 && not found do
                    if components.Visible.[i] && components.Enabled.[i] && components.WantsOnPress.[i] then
                        let bounds = clippedBounds components i
                        if bounds.Width > 0f && bounds.Height > 0f && contains bounds x y then
                            buffer.MarkPressed i
                            buffer.SetDown i
                            found <- true
                    i <- i - 1
        else
            let downIndex = buffer.DownIndex
            if downIndex >= 0 then
                if components.Visible.[downIndex] && components.Enabled.[downIndex] && components.WantsOnClick.[downIndex] then
                    let bounds = clippedBounds components downIndex
                    if bounds.Width > 0f && bounds.Height > 0f && contains bounds x y then
                        buffer.MarkClicked downIndex
                        if components.WantsToggle.[downIndex] then
                            components.Toggled.[downIndex] <- not components.Toggled.[downIndex]
                buffer.ClearDown()
