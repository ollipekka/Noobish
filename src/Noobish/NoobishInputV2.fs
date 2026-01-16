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
    let sliderChanged = Array.create capacity false
    let sliderPayload = Array.create capacity 0f
    let activeFlags = Array.create capacity false
    let activeIndices = ResizeArray<int>()
    let localIdToIndex = Dictionary<uint16, int>()
    let mutable downIndex = -1
    let mutable hoveredIndex = -1
    let mutable lastClickedLocalId = 0us
    let mutable lastPressedLocalId = 0us

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
    member _.SliderChanged = sliderChanged
    member _.SliderPayload = sliderPayload
    member _.ActiveIndices = activeIndices
    member _.LocalIdToIndex = localIdToIndex
    member _.DownIndex = downIndex
    member _.HoveredIndex
        with get() = hoveredIndex
        and set value = hoveredIndex <- value
    member _.LastClickedLocalId
        with get() = lastClickedLocalId
        and set value = lastClickedLocalId <- value
    member _.LastPressedLocalId
        with get() = lastPressedLocalId
        and set value = lastPressedLocalId <- value

    member _.EnsureCapacity(count: int) =
        if count > clicked.Length then
            invalidArg "count" "InputBufferV2 capacity too small for component count."

    member this.Reset(components: NoobishComponentsV2) =
        this.EnsureCapacity components.Count
        lastClickedLocalId <- 0us
        lastPressedLocalId <- 0us
        for i = 0 to activeIndices.Count - 1 do
            let index = activeIndices.[i]
            clicked.[index] <- false
            pressed.[index] <- false
            released.[index] <- false
            textChanged.[index] <- false
            textPayload.[index] <- ""
            sliderChanged.[index] <- false
            sliderPayload.[index] <- 0f
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

    member this.MarkSliderChanged(index: int, value: float32) =
        ensureActive index
        sliderChanged.[index] <- true
        sliderPayload.[index] <- value

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

    member this.TryGetSliderChanged(localId: uint16) =
        match localIdToIndex.TryGetValue localId with
        | true, index when sliderChanged.[index] -> ValueSome sliderPayload.[index]
        | _ -> ValueNone

    member _.GetClicked() = lastClickedLocalId

    member _.GetPressed() = lastPressedLocalId

    member _.TryGetClicked() =
        if lastClickedLocalId = 0us then ValueNone else ValueSome lastClickedLocalId

    member _.TryGetPressed() =
        if lastPressedLocalId = 0us then ValueNone else ValueSome lastPressedLocalId

    member this.UpdateHover(components: NoobishComponentsV2, hitIndex: int) =
        let nextIndex = if hitIndex >= 0 && hitIndex < components.Count then hitIndex else -1
        if nextIndex <> hoveredIndex then
            if hoveredIndex >= 0 && hoveredIndex < components.Count then
                components.Hovered.[hoveredIndex] <- false
            hoveredIndex <- nextIndex
            if hoveredIndex >= 0 then
                components.Hovered.[hoveredIndex] <- true

    member this.UpdateDown(components: NoobishComponentsV2, hitIndex: int) =
        if downIndex < 0 && hitIndex >= 0 && hitIndex < components.Count then
            this.MarkPressed hitIndex
            this.SetDown hitIndex
            let localId = components.Id.[hitIndex].LocalId
            if localId <> 0us then
                lastPressedLocalId <- localId

    member this.Release(components: NoobishComponentsV2, hitIndex: int) =
        if downIndex >= 0 && downIndex < components.Count then
            if hitIndex = downIndex then
                if NoobishComponentsV2.isClickable components downIndex then
                    let localId = components.Id.[downIndex].LocalId
                    this.MarkClicked downIndex
                    if localId <> 0us then
                        lastClickedLocalId <- localId
                    if components.WantsToggle.[downIndex] then
                        components.Toggled.[downIndex] <- not components.Toggled.[downIndex]
            this.ClearDown()

module NoobishInputV2 =
    let internal contains (bounds: NoobishRectangle) (x: float32) (y: float32) =
        x >= bounds.X && x <= bounds.X + bounds.Width
        && y >= bounds.Y && y <= bounds.Y + bounds.Height

    let internal clippedBounds (components: NoobishComponentsV2) (index: int) =
        let mutable bounds = components.Bounds.[index]
        let mutable parentId = components.ParentId.[index]
        while parentId <> UIComponentIdV2.empty do
            let parentIndex = int parentId.Index
            bounds <- bounds.Clamp components.Bounds.[parentIndex]
            parentId <- components.ParentId.[parentIndex]
        bounds

    let internal hitTest (components: NoobishComponentsV2) (x: float32) (y: float32) (predicate: int -> bool) =
        let mutable hit = -1
        let mutable i = components.Count - 1
        while i >= 0 && hit < 0 do
            if predicate i then
                let bounds = clippedBounds components i
                if bounds.Width > 0f && bounds.Height > 0f && contains bounds x y then
                    hit <- i
            i <- i - 1
        hit

    let process (input: INoobishInputState) (components: NoobishComponentsV2) (buffer: InputBufferV2) =
        buffer.Reset components
        if buffer.DownIndex >= components.Count then
            buffer.ClearDown()
        let x = input.PointerX
        let y = input.PointerY
        let hoverHit =
            hitTest components x y (fun i ->
                components.Visible.[i] && components.Enabled.[i])
        buffer.UpdateHover(components, hoverHit)
        if input.IsPrimaryDown() then
            if buffer.DownIndex < 0 then
                let pressHit =
                    hitTest components x y (fun i ->
                        NoobishComponentsV2.isPressable components i)
                buffer.UpdateDown(components, pressHit)
            if buffer.DownIndex >= 0 then
                let downIndex = buffer.DownIndex
                if components.WantsSlider.[downIndex] then
                    let bounds = components.Bounds.[downIndex]
                    let rangeStart = components.SliderMin.[downIndex]
                    let rangeEnd = components.SliderMax.[downIndex]
                    let step = components.SliderStep.[downIndex]
                    let width = bounds.Width
                    let relative =
                        if width <= 0f then 0f
                        else (x - bounds.X) / width
                    let unclamped = rangeStart + relative * (rangeEnd - rangeStart)
                    let stepped =
                        if step > 0f then
                            truncate (unclamped / step) * step
                        else
                            unclamped
                    let value = Noobish.Internal.clamp stepped rangeStart rangeEnd
                    if value <> components.SliderValue.[downIndex] then
                        components.SliderValue.[downIndex] <- value
                        buffer.MarkSliderChanged(downIndex, value)
        else
            let clickHit =
                hitTest components x y (fun i ->
                    NoobishComponentsV2.isClickable components i)
            buffer.Release(components, clickHit)
