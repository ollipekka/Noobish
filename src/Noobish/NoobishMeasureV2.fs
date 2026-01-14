namespace Noobish

open System
open Microsoft.Xna.Framework.Content
open Noobish.Styles

module NoobishMeasureV2 =
    let measureFrameWith (getFont: string -> NoobishFont) (getFontSize: string -> int) (components: NoobishComponentsV2) =
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
                    if wrap && minSize.Width > 0f then
                        NoobishFont.measureMultiLine font fontSize minSize.Width text
                    else
                        NoobishFont.measureSingleLine font fontSize text
                components.ContentSize.[i] <- {
                    Width = max minSize.Width (ceil textWidth)
                    Height = max minSize.Height (ceil textHeight)
                }
            else
                components.ContentSize.[i] <- minSize

    let measureFrame (content: ContentManager) (styleSheet: NoobishStyleSheet) (components: NoobishComponentsV2) =
        let getFont themeId =
            let fontId = styleSheet.GetFont themeId "default"
            content.Load<NoobishFont> fontId
        let getFontSize themeId = styleSheet.GetFontSize themeId "default"
        measureFrameWith getFont getFontSize components
