namespace Noobish

open System
open Microsoft.Xna.Framework.Content
open Noobish.Styles

module NoobishMeasureV2 =
    let private max0 value =
        if value < 0f then 0f else value

    let private applyCheckboxMinSize (components: NoobishComponentsV2) =
        for i = 0 to components.Count - 1 do
            if components.ThemeId.[i] = "Checkbox" && not components.WantsText.[i] then
                let minSize = components.MinSize.[i]
                let padding = components.Padding.[i]
                let paddingSize = max (padding.Left + padding.Right) (padding.Top + padding.Bottom)
                let squareSize =
                    if minSize.Width > 0f && minSize.Height > 0f then
                        max minSize.Width minSize.Height
                    elif minSize.Height > 0f then
                        minSize.Height
                    elif minSize.Width > 0f then
                        minSize.Width
                    else
                        paddingSize
                if squareSize > 0f then
                    components.MinSize.[i] <- {Width = squareSize; Height = squareSize}

    let private computeContainerContentSizes (components: NoobishComponentsV2) =
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
                let computed =
                    match components.Layout.[i] with
                    | LayoutV2.LinearHorizontal -> {Width = totalWidth; Height = maxHeight}
                    | LayoutV2.LinearVertical -> {Width = maxWidth; Height = totalHeight}
                    | LayoutV2.Stack
                    | LayoutV2.Relative _ -> {Width = maxWidth; Height = maxHeight}
                    | LayoutV2.Grid _ -> {Width = 0f; Height = 0f} // Grid sizing is handled in layout; children are expected to fill cells.
                    | LayoutV2.None -> {Width = 0f; Height = 0f}
                if computed.Width > 0f || computed.Height > 0f then
                    let existing = components.ContentSize.[i]
                    components.ContentSize.[i] <- {
                        Width = max existing.Width computed.Width
                        Height = max existing.Height computed.Height
                    }

    let measureFrameWith (getFont: string -> NoobishFont) (getFontSize: string -> int) (components: NoobishComponentsV2) =
        applyCheckboxMinSize components
        for i = 0 to components.Count - 1 do
            let minSize = components.MinSize.[i]
            let text = components.Text.[i]
            let wantsText = components.WantsText.[i]
            if wantsText && not (String.IsNullOrWhiteSpace text) then
                let themeId = components.ThemeId.[i]
                let font = getFont themeId
                let fontSize = getFontSize themeId
                let wrap = components.Textwrap.[i]
                let struct(textWidth, textHeight) =
                    if wrap then
                        let padding = components.Padding.[i]
                        let margin = components.Margin.[i]
                        let boundsWidth = components.Bounds.[i].Width
                        let parentId = components.ParentId.[i]
                        let parentWidth =
                            if parentId <> UIComponentIdV2.empty then
                                let parentIndex = int parentId.Index
                                let parentBounds = components.Bounds.[parentIndex]
                                let parentPadding = components.Padding.[parentIndex]
                                max0 (parentBounds.Width - parentPadding.Left - parentPadding.Right - margin.Left - margin.Right)
                            else
                                0f
                        let wrapWidth =
                            if minSize.Width > 0f then
                                minSize.Width
                            elif parentWidth > 0f then
                                parentWidth
                            elif boundsWidth > 0f then
                                max0 (boundsWidth - padding.Left - padding.Right)
                            else
                                0f
                        if wrapWidth > 0f then
                            NoobishFont.measureMultiLine font fontSize wrapWidth text
                        else
                            NoobishFont.measureSingleLine font fontSize text
                    else
                        NoobishFont.measureSingleLine font fontSize text
                components.ContentSize.[i] <- {
                    Width = max minSize.Width (ceil textWidth)
                    Height = max minSize.Height (ceil textHeight)
                }
            else
                components.ContentSize.[i] <- minSize
        computeContainerContentSizes components

    let measureFramePostLayoutWith (getFont: string -> NoobishFont) (getFontSize: string -> int) (components: NoobishComponentsV2) =
        for i = 0 to components.Count - 1 do
            let text = components.Text.[i]
            if components.WantsText.[i] && components.Textwrap.[i] && not (String.IsNullOrWhiteSpace text) then
                let themeId = components.ThemeId.[i]
                let font = getFont themeId
                let fontSize = getFontSize themeId
                let bounds = components.Bounds.[i]
                let padding = components.Padding.[i]
                let wrapWidth = max0 (bounds.Width - padding.Left - padding.Right)
                let struct(textWidth, textHeight) =
                    if wrapWidth > 0f then
                        NoobishFont.measureMultiLine font fontSize wrapWidth text
                    else
                        NoobishFont.measureSingleLine font fontSize text
                let minSize = components.MinSize.[i]
                components.ContentSize.[i] <- {
                    Width = max minSize.Width (ceil textWidth)
                    Height = max minSize.Height (ceil textHeight)
                }
        computeContainerContentSizes components

    let measureFrame (content: ContentManager) (styleSheet: NoobishStyleSheet) (components: NoobishComponentsV2) =
        let getFont themeId =
            let fontId = styleSheet.GetFont themeId "default"
            content.Load<NoobishFont> fontId
        let getFontSize themeId = styleSheet.GetFontSize themeId "default"
        for i = 0 to components.Count - 1 do
            let themeId = components.ThemeId.[i]
            if not components.MarginOverride.[i] then
                components.Margin.[i] <- styleSheet.GetMargin themeId "default"
            if not components.PaddingOverride.[i] then
                components.Padding.[i] <- styleSheet.GetPadding themeId "default"
        measureFrameWith getFont getFontSize components
        for i = 0 to components.Count - 1 do
            if components.WantsSlider.[i] then
                let minSize = components.MinSize.[i]
                let sliderHeight = styleSheet.GetHeight components.ThemeId.[i] "default"
                let pinHeight = styleSheet.GetHeight "SliderPin" "default"
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
                let baseHeight = styleSheet.GetHeight themeId "default"
                let dashHeight = styleSheet.GetHeight "ProgressBar-Dash" "default"
                let progressHeight = styleSheet.GetHeight "ProgressBar-Progress" "default"
                let height = max baseHeight (max dashHeight progressHeight)
                if height > 0f then
                    let size = components.ContentSize.[i]
                    components.ContentSize.[i] <- {
                        Width = max size.Width minSize.Width
                        Height = max size.Height height
                    }
        computeContainerContentSizes components

    let measureFramePostLayout (content: ContentManager) (styleSheet: NoobishStyleSheet) (components: NoobishComponentsV2) =
        let getFont themeId =
            let fontId = styleSheet.GetFont themeId "default"
            content.Load<NoobishFont> fontId
        let getFontSize themeId = styleSheet.GetFontSize themeId "default"
        measureFramePostLayoutWith getFont getFontSize components
