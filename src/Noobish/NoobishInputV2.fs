namespace Noobish

open System
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
    let activeIndices = ResizeArray<int>(capacity)
    let localIdToIndex = Dictionary<uint16, int>(capacity)
    let mutable downIndex = -1
    let mutable hoveredIndex = -1
    let mutable lastHoveredLocalId = 0us
    let mutable lastClickedLocalId = 0us
    let mutable lastPressedLocalId = 0us
    let mutable focusedIndex = -1
    let mutable lastFocusedLocalId = 0us
    let mutable caretIndex = 0

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
    member _.LastHoveredLocalId
        with get() = lastHoveredLocalId
        and set value = lastHoveredLocalId <- value
    member _.LastClickedLocalId
        with get() = lastClickedLocalId
        and set value = lastClickedLocalId <- value
    member _.LastPressedLocalId
        with get() = lastPressedLocalId
        and set value = lastPressedLocalId <- value
    member _.FocusedIndex
        with get() = focusedIndex
        and set value = focusedIndex <- value
    member _.LastFocusedLocalId
        with get() = lastFocusedLocalId
        and set value = lastFocusedLocalId <- value
    member _.CaretIndex
        with get() = caretIndex
        and set value = caretIndex <- value

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
            components.Hovered.[i] <- false
        if focusedIndex >= 0 && focusedIndex < components.Count then
            components.Focused.[focusedIndex] <- false
        for i = 0 to components.Count - 1 do
            let localId = components.Id.[i].LocalId
            if localId <> 0us then
                localIdToIndex.[localId] <- i
        hoveredIndex <- -1
        if lastHoveredLocalId <> 0us then
            let mutable index = 0
            if localIdToIndex.TryGetValue(lastHoveredLocalId, &index) then
                hoveredIndex <- index
                components.Hovered.[index] <- true
            else
                lastHoveredLocalId <- 0us
        focusedIndex <- -1
        if lastFocusedLocalId <> 0us then
            let mutable index = 0
            if localIdToIndex.TryGetValue(lastFocusedLocalId, &index) then
                focusedIndex <- index
                components.Focused.[index] <- true
                let textLength = components.Text.[index].Length
                caretIndex <- min caretIndex textLength
                components.CaretIndex.[index] <- caretIndex
            else
                lastFocusedLocalId <- 0us
                caretIndex <- 0

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

    member this.SetFocus(components: NoobishComponentsV2, index: int, nextCaret: int) =
        if focusedIndex >= 0 && focusedIndex < components.Count then
            components.Focused.[focusedIndex] <- false
        focusedIndex <- index
        let localId = components.Id.[index].LocalId
        lastFocusedLocalId <- localId
        components.Focused.[index] <- true
        let textLength = components.Text.[index].Length
        caretIndex <- Math.Clamp(nextCaret, 0, textLength)
        components.CaretIndex.[index] <- caretIndex

    member this.ClearFocus(components: NoobishComponentsV2) =
        if focusedIndex >= 0 && focusedIndex < components.Count then
            components.Focused.[focusedIndex] <- false
        focusedIndex <- -1
        lastFocusedLocalId <- 0us
        caretIndex <- 0

    member this.ClearDown() =
        if downIndex >= 0 then
            let index = downIndex
            down.[index] <- false
            downIndex <- -1
            this.MarkReleased index

    member this.WasClicked(localId: uint16) =
        let mutable index = 0
        if localIdToIndex.TryGetValue(localId, &index) then
            clicked.[index]
        else
            false

    member this.WasPressed(localId: uint16) =
        let mutable index = 0
        if localIdToIndex.TryGetValue(localId, &index) then
            pressed.[index]
        else
            false

    member this.WasReleased(localId: uint16) =
        let mutable index = 0
        if localIdToIndex.TryGetValue(localId, &index) then
            released.[index]
        else
            false

    member this.IsDown(localId: uint16) =
        let mutable index = 0
        if localIdToIndex.TryGetValue(localId, &index) then
            down.[index]
        else
            false

    member this.TryGetTextChanged(localId: uint16) =
        let mutable index = 0
        if localIdToIndex.TryGetValue(localId, &index) && textChanged.[index] then
            ValueSome textPayload.[index]
        else
            ValueNone

    member this.TryGetSliderChanged(localId: uint16) =
        let mutable index = 0
        if localIdToIndex.TryGetValue(localId, &index) && sliderChanged.[index] then
            ValueSome sliderPayload.[index]
        else
            ValueNone

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
                lastHoveredLocalId <- components.Id.[hoveredIndex].LocalId
            else
                lastHoveredLocalId <- 0us

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

    let internal boundsWithAncestorScroll (components: NoobishComponentsV2) (index: int) =
        let mutable bounds = components.Bounds.[index]
        let mutable parentId = components.ParentId.[index]
        let mutable scrollX = 0f
        let mutable scrollY = 0f
        while parentId <> UIComponentIdV2.empty do
            let parentIndex = int parentId.Index
            let scroll = components.Scroll.[parentIndex]
            if scroll.Horizontal then
                scrollX <- scrollX + components.ScrollX.[parentIndex]
            if scroll.Vertical then
                scrollY <- scrollY + components.ScrollY.[parentIndex]
            parentId <- components.ParentId.[parentIndex]
        { bounds with X = bounds.X + scrollX; Y = bounds.Y + scrollY }

    let internal contentBounds (components: NoobishComponentsV2) (index: int) =
        let bounds = boundsWithAncestorScroll components index
        let padding = components.Padding.[index]
        {
            X = bounds.X + padding.Left
            Y = bounds.Y + padding.Top
            Width = Internal.max0 (bounds.Width - padding.Left - padding.Right)
            Height = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        }

    let internal tryFindScrollableAncestor (components: NoobishComponentsV2) (index: int) =
        let mutable current = index
        let mutable found = -1
        while current >= 0 && found < 0 do
            let scroll = components.Scroll.[current]
            if scroll.Horizontal || scroll.Vertical then
                found <- current
            else
                let parentId = components.ParentId.[current]
                if parentId = UIComponentIdV2.empty then
                    current <- -1
                else
                    current <- int parentId.Index
        found

    let internal getViewportSize (components: NoobishComponentsV2) (index: int) =
        let bounds = components.Bounds.[index]
        let padding = components.Padding.[index]
        let width = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let height = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        struct(width, height)

    let internal getContentExtent (components: NoobishComponentsV2) (index: int) =
        let bounds = components.Bounds.[index]
        let padding = components.Padding.[index]
        let contentX = bounds.X + padding.Left
        let contentY = bounds.Y + padding.Top
        let children = components.Children.[index]
        if children.Count > 0 then
            let mutable maxRight = contentX
            let mutable maxBottom = contentY
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let childBounds = components.Bounds.[childIndex]
                let right = childBounds.X + childBounds.Width
                let bottom = childBounds.Y + childBounds.Height
                if right > maxRight then
                    maxRight <- right
                if bottom > maxBottom then
                    maxBottom <- bottom
            struct(Internal.max0 (maxRight - contentX), Internal.max0 (maxBottom - contentY))
        else
            let contentSize = components.ContentSize.[index]
            struct(contentSize.Width, contentSize.Height)

    let internal applyScrollDelta
        (components: NoobishComponentsV2)
        (index: int)
        (delta: float32)
        (viewportWidth: float32)
        (viewportHeight: float32)
        (contentWidth: float32)
        (contentHeight: float32) =
        let scroll = components.Scroll.[index]
        if scroll.Vertical && contentHeight > viewportHeight then
            let minScroll = viewportHeight - contentHeight
            let nextScroll = components.ScrollY.[index] + delta
            components.ScrollY.[index] <- Math.Clamp(nextScroll, minScroll,0f)
        if scroll.Horizontal && contentWidth > viewportWidth then
            let minScroll = viewportWidth - contentWidth
            let nextScroll = components.ScrollX.[index] + delta
            components.ScrollX.[index] <- Math.Clamp(nextScroll, minScroll, 0f)

    let internal clippedBounds (components: NoobishComponentsV2) (index: int) =
        let mutable bounds = boundsWithAncestorScroll components index
        let mutable parentId = components.ParentId.[index]
        while parentId <> UIComponentIdV2.empty do
            let parentIndex = int parentId.Index
            let parentBounds = contentBounds components parentIndex
            bounds <- bounds.Clamp parentBounds
            parentId <- components.ParentId.[parentIndex]
        bounds

    let inline hitTestWith
        (count: int)
        ([<InlineIfLambda>] boundsAt: int -> NoobishRectangle)
        ([<InlineIfLambda>] layerAt: int -> int)
        (x: float32)
        (y: float32)
        ([<InlineIfLambda>] predicate: int -> bool) =
        let mutable hit = -1
        let mutable hitLayer = Int32.MinValue
        let mutable i = 0
        while i < count do
            if predicate i then
                let bounds = boundsAt i
                if NoobishRectangle.hasArea bounds && bounds.Contains x y then
                    let layer = layerAt i
                    if layer > hitLayer || (layer = hitLayer && i > hit) then
                        hit <- i
                        hitLayer <- layer
            i <- i + 1
        hit

    let internal hitTest (components: NoobishComponentsV2) (x: float32) (y: float32) (predicate: int -> bool) =
        hitTestWith components.Count (fun i -> clippedBounds components i) (fun i -> components.Layer.[i]) x y predicate

    let internal calculateSliderValue (bounds: NoobishRectangle) (rangeStart: float32) (rangeEnd: float32) (step: float32) (x: float32) =
        let width = bounds.Width
        let relative =
            if width <= 0f then 0f
            else (x - bounds.X) / width
        let unclamped = rangeStart + relative * (rangeEnd - rangeStart)
        let stepped =
            if step > 0f then
                MathF.Floor((unclamped - rangeStart) / step) * step + rangeStart
            else
                unclamped
        Math.Clamp(stepped, rangeStart, rangeEnd)

    let internal applyTextInput (text: string) (caret: int) (textBuffer: char[]) (textCount: int) =
        let mutable updated = text
        let mutable nextCaret = caret
        let mutable changed = false
        let mutable clearFocus = false

        for i = 0 to textCount - 1 do
            let c = textBuffer.[i]
            if c = '\b' then
                if nextCaret > 0 && updated.Length > 0 then
                    updated <- updated.Remove(nextCaret - 1, 1)
                    nextCaret <- nextCaret - 1
                    changed <- true
            elif c = '\r' || c = '\n' then
                clearFocus <- true
            elif c = '\u001b' then
                clearFocus <- true
            elif c = '\u007f' then
                if nextCaret < updated.Length then
                    updated <- updated.Remove(nextCaret, 1)
                    changed <- true
            elif not (Char.IsControl c) then
                updated <- updated.Insert(nextCaret, string c)
                nextCaret <- nextCaret + 1
                changed <- true

        struct(updated, nextCaret, changed, clearFocus)

    let internal updateHover (components: NoobishComponentsV2) (buffer: InputBufferV2) (x: float32) (y: float32) =
        let hoverHit =
            hitTestWith components.Count (fun i -> clippedBounds components i) (fun i -> components.Layer.[i]) x y (fun i ->
                components.Visible.[i] && components.Enabled.[i])
        buffer.UpdateHover(components, hoverHit)

    let internal updateSliderFromPointer (components: NoobishComponentsV2) (buffer: InputBufferV2) (x: float32) =
        let downIndex = buffer.DownIndex
        if components.WantsSlider.[downIndex] then
            let bounds = boundsWithAncestorScroll components downIndex
            let rangeStart = components.SliderMin.[downIndex]
            let rangeEnd = components.SliderMax.[downIndex]
            let step = components.SliderStep.[downIndex]
            let value = calculateSliderValue bounds rangeStart rangeEnd step x
            if value <> components.SliderValue.[downIndex] then
                components.SliderValue.[downIndex] <- value
                buffer.MarkSliderChanged(downIndex, value)

    let internal updateScroll (input: INoobishInputState) (components: NoobishComponentsV2) (x: float32) (y: float32) =
        let scrollDelta = input.ScrollWheelDelta
        if scrollDelta <> 0f then
            let hitIndex =
                hitTestWith components.Count (fun i -> clippedBounds components i) (fun i -> components.Layer.[i]) x y (fun i ->
                    components.Visible.[i] && components.Enabled.[i])
            let scrollHit = if hitIndex >= 0 then tryFindScrollableAncestor components hitIndex else -1
            if scrollHit >= 0 then
                let struct(viewportWidth, viewportHeight) = getViewportSize components scrollHit
                let struct(contentWidth, contentHeight) = getContentExtent components scrollHit
                let delta = -scrollDelta * 0.5f
                applyScrollDelta components scrollHit delta viewportWidth viewportHeight contentWidth contentHeight

    let internal updatePrimaryDown (components: NoobishComponentsV2) (buffer: InputBufferV2) (x: float32) (y: float32) =
        if buffer.DownIndex < 0 then
            let pressHit =
                hitTestWith components.Count (fun i -> clippedBounds components i) (fun i -> components.Layer.[i]) x y (fun i ->
                    NoobishComponentsV2.isPressable components i)
            buffer.UpdateDown(components, pressHit)
        if buffer.DownIndex >= 0 then
            updateSliderFromPointer components buffer x

    let internal updateRelease (components: NoobishComponentsV2) (buffer: InputBufferV2) (x: float32) (y: float32) =
        let clickHit =
            hitTestWith components.Count (fun i -> clippedBounds components i) (fun i -> components.Layer.[i]) x y (fun i ->
                NoobishComponentsV2.isClickable components i)
        buffer.Release(components, clickHit)

    let internal updateFocusFromClick (input: INoobishInputState) (components: NoobishComponentsV2) (buffer: InputBufferV2) (x: float32) (y: float32) =
        if input.IsPrimaryClick() then
            let focusHit =
                hitTestWith components.Count (fun i -> clippedBounds components i) (fun i -> components.Layer.[i]) x y (fun i ->
                    components.Visible.[i] && components.Enabled.[i] && components.WantsTextChanged.[i])
            if focusHit >= 0 then
                let textLength = components.Text.[focusHit].Length
                buffer.SetFocus(components, focusHit, textLength)
                components.CaretBlinkReset.[focusHit] <- true
            else
                buffer.ClearFocus components

    let internal updateTextInput (input: INoobishInputState) (components: NoobishComponentsV2) (buffer: InputBufferV2) =
        let focusedIndex = buffer.FocusedIndex
        let struct(textBuffer, textCount) = input.ConsumeTextInput()
        if focusedIndex >= 0 && focusedIndex < components.Count && textCount > 0 then
            let initialCaret = buffer.CaretIndex
            let text = components.Text.[focusedIndex]
            let struct(updatedText, nextCaret, changed, clearFocus) =
                applyTextInput text initialCaret textBuffer textCount
            if changed then
                components.Text.[focusedIndex] <- updatedText
                buffer.CaretIndex <- nextCaret
                components.CaretIndex.[focusedIndex] <- nextCaret
                buffer.MarkTextChanged(focusedIndex, updatedText)
                if nextCaret <> initialCaret then
                    components.CaretBlinkReset.[focusedIndex] <- true

            if clearFocus then
                buffer.ClearFocus components

    let internal updateCaretFromKeys (input: INoobishInputState) (components: NoobishComponentsV2) (buffer: InputBufferV2) =
        let focusedIndex = buffer.FocusedIndex
        if focusedIndex >= 0 && focusedIndex < components.Count then
            let textLength = components.Text.[focusedIndex].Length
            let mutable caret = buffer.CaretIndex
            if input.IsKeyPressed NoobishKeyId.Left then
                caret <- max 0 (caret - 1)
            elif input.IsKeyPressed NoobishKeyId.Right then
                caret <- min textLength (caret + 1)
            if caret <> buffer.CaretIndex then
                buffer.CaretIndex <- caret
                components.CaretIndex.[focusedIndex] <- caret
                components.CaretBlinkReset.[focusedIndex] <- true

    let ProcessInput (input: INoobishInputState) (components: NoobishComponentsV2) (buffer: InputBufferV2) =
        buffer.Reset components
        if buffer.DownIndex >= components.Count then
            buffer.ClearDown()
        let x = input.PointerX
        let y = input.PointerY
        updateScroll input components x y
        updateHover components buffer x y
        if input.IsPrimaryDown() then
            updatePrimaryDown components buffer x y
        else
            updateRelease components buffer x y
        updateFocusFromClick input components buffer x y
        updateTextInput input components buffer
        updateCaretFromKeys input components buffer
