namespace Noobish

open Microsoft.Xna.Framework.Content
open Noobish.Styles

type NoobishMonoGameMeasureProvider(content: ContentManager, styleSheet: NoobishStyleSheet) =
    let getFont themeId state =
        let fontId = styleSheet.GetFont themeId state
        content.Load<NoobishMonoGameFont> fontId

    interface INoobishMeasureProvider with
        member _.GetFontSize themeId state =
            styleSheet.GetFontSize themeId state
        member _.MeasureSingleLine themeId state fontSize text =
            let font = getFont themeId state
            NoobishFont.measureSingleLine font.Font fontSize text
        member _.MeasureMultiLine themeId state fontSize maxWidth text =
            let font = getFont themeId state
            NoobishFont.measureMultiLine font.Font fontSize maxWidth text
        member _.GetMargin themeId state =
            styleSheet.GetMargin themeId state
        member _.GetPadding themeId state =
            styleSheet.GetPadding themeId state
        member _.GetHeight themeId state =
            styleSheet.GetHeight themeId state
