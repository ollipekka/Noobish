namespace Noobish

open System

type INoobishMeasureProvider =
    abstract GetFontSize: themeId: string -> state: string -> int
    abstract MeasureSingleLine: themeId: string -> state: string -> fontSize: int -> text: string -> struct(float32 * float32)
    abstract MeasureMultiLine: themeId: string -> state: string -> fontSize: int -> maxWidth: float32 -> text: string -> struct(float32 * float32)
    abstract GetMargin: themeId: string -> state: string -> NoobishMargin
    abstract GetPadding: themeId: string -> state: string -> NoobishPadding
    abstract GetHeight: themeId: string -> state: string -> float32

module NoobishMeasureV2 =
    let private defaultState = "default"
    let internal computeCheckboxSquareSize (minSize: NoobishSize) (padding: NoobishPadding) =
        let paddingSize = max (padding.Left + padding.Right) (padding.Top + padding.Bottom)
        if minSize.Width > 0f && minSize.Height > 0f then
            max minSize.Width minSize.Height
        elif minSize.Height > 0f then
            minSize.Height
        elif minSize.Width > 0f then
            minSize.Width
        else
            paddingSize

    let internal computeContainerContentSize (layout: LayoutV2) (totalWidth: float32) (totalHeight: float32) (maxWidth: float32) (maxHeight: float32): NoobishSize =
        match layout with
        | LayoutV2.LinearHorizontal -> {Width = totalWidth; Height = maxHeight}
        | LayoutV2.LinearVertical -> {Width = maxWidth; Height = totalHeight}
        | LayoutV2.Stack
        | LayoutV2.Relative _ -> {Width = maxWidth; Height = maxHeight}
        | LayoutV2.Grid _ // Grid sizing is handled in layout; children are expected to fill cells.
        | LayoutV2.None -> {Width = 0f; Height = 0f}

    let internal resolveWrapWidth (minWidth: float32) (parentWidth: float32) (boundsWidth: float32) (padding: NoobishPadding) =
        if minWidth > 0f then
            minWidth
        elif parentWidth > 0f then
            parentWidth
        elif boundsWidth > 0f then
            Internal.max0 (boundsWidth - padding.Left - padding.Right)
        else
            0f

    let internal computeParentWrapWidth (components: NoobishComponentsV2) (index: int) =
        let parentId = components.ParentId.[index]
        if parentId <> UIComponentIdV2.empty then
            let parentIndex = int parentId.Index
            let parentBounds = components.Bounds.[parentIndex]
            let parentPadding = components.Padding.[parentIndex]
            let margin = components.Margin.[index]
            Internal.max0 (parentBounds.Width - parentPadding.Left - parentPadding.Right - margin.Left - margin.Right)
        else
            0f

    let private applyCheckboxMinSize (components: NoobishComponentsV2) =
        for i = 0 to components.Count - 1 do
            if components.ThemeId.[i] = "Checkbox" && not components.WantsText.[i] then
                let minSize = components.MinSize.[i]
                let padding = components.Padding.[i]
                let squareSize = computeCheckboxSquareSize minSize padding
                if squareSize > 0f then
                    components.MinSize.[i] <- {Width = squareSize; Height = squareSize}

    let recomputeContainerContentSizes (components: NoobishComponentsV2) =
        for i = components.Count - 1 downto 0 do
            let children = components.Children.[i]
            if children.Count > 0 then
                let mutable totalWidth = 0f
                let mutable totalHeight = 0f
                let mutable maxWidth = 0f
                let mutable maxHeight = 0f
                for c = 0 to children.Count - 1 do
                    let childIndex = int children.[c].Index
                    let minSize = components.MinSize.[childIndex]
                    let contentSize = components.ContentSize.[childIndex]
                    let padding = components.Padding.[childIndex]
                    let margin = components.Margin.[childIndex]
                    let childWidth = max minSize.Width contentSize.Width + padding.Left + padding.Right + margin.Left + margin.Right
                    let childHeight = max minSize.Height contentSize.Height + padding.Top + padding.Bottom + margin.Top + margin.Bottom
                    totalWidth <- totalWidth + childWidth
                    totalHeight <- totalHeight + childHeight
                    maxWidth <- max maxWidth childWidth
                    maxHeight <- max maxHeight childHeight
                let computed = computeContainerContentSize components.Layout.[i] totalWidth totalHeight maxWidth maxHeight
                if computed.Width > 0f || computed.Height > 0f then
                    let existing = components.ContentSize.[i]
                    components.ContentSize.[i] <- {
                        Width = max existing.Width computed.Width
                        Height = max existing.Height computed.Height
                    }

    let measureFrameWith
        (getFontSize: string -> int)
        (measureSingleLine: string -> int -> string -> struct(float32 * float32))
        (measureMultiLine: string -> int -> float32 -> string -> struct(float32 * float32))
        (components: NoobishComponentsV2) =
        applyCheckboxMinSize components
        for i = 0 to components.Count - 1 do
            let minSize = components.MinSize.[i]
            let text = components.Text.[i]
            let wantsText = components.WantsText.[i]
            if wantsText && not (String.IsNullOrWhiteSpace text) then
                let themeId = components.ThemeId.[i]
                let fontSize = getFontSize themeId
                let wrap = components.Textwrap.[i]
                let struct(textWidth, textHeight) =
                    if wrap then
                        let padding = components.Padding.[i]
                        let boundsWidth = components.Bounds.[i].Width
                        let parentWidth = computeParentWrapWidth components i
                        let wrapWidth = resolveWrapWidth minSize.Width parentWidth boundsWidth padding
                        if wrapWidth > 0f then
                            measureMultiLine themeId fontSize wrapWidth text
                        else
                            measureSingleLine themeId fontSize text
                    else
                        measureSingleLine themeId fontSize text
                components.ContentSize.[i] <- {
                    Width = max minSize.Width (ceil textWidth)
                    Height = max minSize.Height (ceil textHeight)
                }
            else
                components.ContentSize.[i] <- minSize
        recomputeContainerContentSizes components

    let measureFramePostLayoutWith
        (getFontSize: string -> int)
        (measureSingleLine: string -> int -> string -> struct(float32 * float32))
        (measureMultiLine: string -> int -> float32 -> string -> struct(float32 * float32))
        (components: NoobishComponentsV2) =
        for i = 0 to components.Count - 1 do
            let text = components.Text.[i]
            if components.WantsText.[i] && components.Textwrap.[i] && not (String.IsNullOrWhiteSpace text) then
                let themeId = components.ThemeId.[i]
                let fontSize = getFontSize themeId
                let bounds = components.Bounds.[i]
                let padding = components.Padding.[i]
                let wrapWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
                let struct(textWidth, textHeight) =
                    if wrapWidth > 0f then
                        measureMultiLine themeId fontSize wrapWidth text
                    else
                        measureSingleLine themeId fontSize text
                let minSize = components.MinSize.[i]
                components.ContentSize.[i] <- {
                    Width = max minSize.Width (ceil textWidth)
                    Height = max minSize.Height (ceil textHeight)
                }
        recomputeContainerContentSizes components

    let measureFrame (provider: INoobishMeasureProvider) (components: NoobishComponentsV2) =
        let getFontSize themeId = provider.GetFontSize themeId defaultState
        let measureSingleLine themeId fontSize text =
            provider.MeasureSingleLine themeId defaultState fontSize text
        let measureMultiLine themeId fontSize maxWidth text =
            provider.MeasureMultiLine themeId defaultState fontSize maxWidth text
        for i = 0 to components.Count - 1 do
            let themeId = components.ThemeId.[i]
            if not components.MarginOverride.[i] then
                components.Margin.[i] <- provider.GetMargin themeId defaultState
            if not components.PaddingOverride.[i] then
                components.Padding.[i] <- provider.GetPadding themeId defaultState
        measureFrameWith getFontSize measureSingleLine measureMultiLine components
        for i = 0 to components.Count - 1 do
            if components.WantsSlider.[i] then
                let minSize = components.MinSize.[i]
                let sliderHeight = provider.GetHeight components.ThemeId.[i] defaultState
                let pinHeight = provider.GetHeight "SliderPin" defaultState
                let desiredHeight = max sliderHeight pinHeight
                if desiredHeight > 0f then
                    let size = components.ContentSize.[i]
                    components.ContentSize.[i] <- {
                        Width = max size.Width minSize.Width
                        Height = max size.Height desiredHeight
                    }
            elif components.WantsProgress.[i] then
                let minSize = components.MinSize.[i]
                let themeId = components.ThemeId.[i]
                let baseHeight = provider.GetHeight themeId defaultState
                let dashHeight = provider.GetHeight "ProgressBar-Dash" defaultState
                let progressHeight = provider.GetHeight "ProgressBar-Progress" defaultState
                let height = max baseHeight (max dashHeight progressHeight)
                if height > 0f then
                    let size = components.ContentSize.[i]
                    components.ContentSize.[i] <- {
                        Width = max size.Width minSize.Width
                        Height = max size.Height height
                    }
        recomputeContainerContentSizes components

    let measureFramePostLayout (provider: INoobishMeasureProvider) (components: NoobishComponentsV2) =
        let getFontSize themeId = provider.GetFontSize themeId defaultState
        let measureSingleLine themeId fontSize text =
            provider.MeasureSingleLine themeId defaultState fontSize text
        let measureMultiLine themeId fontSize maxWidth text =
            provider.MeasureMultiLine themeId defaultState fontSize maxWidth text
        measureFramePostLayoutWith getFontSize measureSingleLine measureMultiLine components
