namespace Noobish

open System
open System.Collections.Generic

module NoobishRenderV2 =
    let resolveState (enabled: bool) (toggled: bool) (hovered: bool) (focused: bool) =
        if not enabled then "disabled"
        elif focused then "focused"
        elif toggled && hovered then "toggledHovered"
        elif toggled then "toggled"
        elif hovered then "hovered"
        else "default"

    let computeTextBounds (bounds: NoobishRectangle) (padding: NoobishPadding) =
        {
            X = bounds.X + padding.Left
            Y = bounds.Y + padding.Top
            Width = Internal.max0 (bounds.Width - padding.Left - padding.Right)
            Height = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        }

    let offsetBounds (bounds: NoobishRectangle) (offsetX: float32) (offsetY: float32) =
        { bounds with X = bounds.X + offsetX; Y = bounds.Y + offsetY }

    let computeScissorBounds (bounds: NoobishRectangle) =
        let left = MathF.Floor bounds.X
        let top = MathF.Floor bounds.Y
        let right = MathF.Ceiling (bounds.X + bounds.Width)
        let bottom = MathF.Ceiling (bounds.Y + bounds.Height)
        let width = Internal.max0 (right - left)
        let height = Internal.max0 (bottom - top)
        {
            X = left
            Y = top
            Width = width
            Height = height
        }

    let public resolveCaretBlinkStart
        (byLocalId: Dictionary<uint32, TimeSpan>)
        (byIndex: Dictionary<int, TimeSpan>)
        (componentId: UIComponentIdV2)
        (index: int)
        (now: TimeSpan)
        (reset: bool) =
        let localId = componentId.LocalId
        if localId <> 0us then
            let key = (uint32 componentId.Namespace <<< 16) ||| uint32 localId
            if reset then
                byLocalId.[key] <- now
            match byLocalId.TryGetValue key with
            | true, value -> value
            | false, _ ->
                byLocalId.[key] <- now
                now
        else
            if reset then
                byIndex.[index] <- now
            match byIndex.TryGetValue index with
            | true, value -> value
            | false, _ ->
                byIndex.[index] <- now
                now

    let computeProgressSegmentWidth (contentWidth: float32) (segments: int) (gap: float32) =
        if segments <= 0 then
            0f
        else
            Internal.max0 ((contentWidth - gap * float32 (segments - 1)) / float32 segments)

    let computeProgressBounds (bounds: NoobishRectangle) (padding: NoobishPadding) (progress: float32) =
        let content = computeTextBounds bounds padding
        let clamped = Math.Clamp(progress, 0f, 1f)
        {
            X = content.X
            Y = content.Y
            Width = content.Width * clamped
            Height = content.Height
        }

    let computeSliderPinBounds
        (bounds: NoobishRectangle)
        (padding: NoobishPadding)
        (rangeStart: float32)
        (rangeEnd: float32)
        (value: float32)
        (pinWidth: float32)
        (pinHeight: float32) =
        let availableWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let availableHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        let clampedPinWidth = min pinWidth availableWidth
        let clampedPinHeight = min pinHeight availableHeight
        let span = rangeEnd - rangeStart
        let t =
            if span = 0f then
                0f
            else
                Math.Clamp ((value - rangeStart) / span, 0f, 1f)
        let usableWidth = Internal.max0 (availableWidth - clampedPinWidth)
        {
            X = bounds.X + padding.Left + t * usableWidth
            Y = bounds.Y + padding.Top + (availableHeight - clampedPinHeight) * 0.5f
            Width = clampedPinWidth
            Height = clampedPinHeight
        }

    let computeSliderTrackBounds
        (bounds: NoobishRectangle)
        (padding: NoobishPadding)
        (trackHeight: float32) =
        let availableWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let availableHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        let clampedHeight =
            if trackHeight > 0f then
                min trackHeight availableHeight
            else
                availableHeight
        {
            X = bounds.X + padding.Left
            Y = bounds.Y + padding.Top + (availableHeight - clampedHeight) * 0.5f
            Width = availableWidth
            Height = clampedHeight
        }
