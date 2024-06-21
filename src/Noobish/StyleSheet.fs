namespace Noobish.Styles

open Noobish

open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics




[<RequireQualifiedAccess>]
type NoobishDrawable=
| NinePatch of string
| NinePatchWithColor of string*Color
| Texture of string


[<Struct>]
type NoobishMargin = {
    Top: float32
    Right: float32
    Bottom: float32
    Left: float32
}

module NoobishMargin = 
    let empty: NoobishMargin = {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

[<Struct>]
type NoobishPadding = {
    Top: float32
    Right: float32
    Bottom: float32
    Left: float32
}

module NoobishPadding = 
    let empty: NoobishPadding = {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

type NoobishStyleSheet = {
    Name: string
    TextureAtlasId: string
    Widths: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
    Heights: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
    Paddings: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishPadding>>
    Margins: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishMargin>>
    Colors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, Color>>
    Fonts: IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>
    FontSizes: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>
    FontColors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, Color>>
    TextAlignments: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishAlignment>>
    Drawables: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishDrawable[]>>
} with

    static member private GetDefault (d: IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>) (themeId: string) (state: string) (fallback: 'T): 'T =
        
        let mutable defaultDict = Unchecked.defaultof<IReadOnlyDictionary<string, 'T>>
        let success = d.TryGetValue(themeId, &defaultDict)
        if success then
            let mutable v = Unchecked.defaultof<'T>
            let success' = defaultDict.TryGetValue (state, &v)
            if success' then
                v
            else
                NoobishStyleSheet.GetDefault d "Default" "default" fallback
        else
            fallback

    static member private GetValue (d: IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>) (themeId: string) (state: string) (fallback: 'T): 'T =
        let mutable defaultDict = Unchecked.defaultof<IReadOnlyDictionary<string, 'T>>
        let success = d.TryGetValue(themeId, &defaultDict)
        if success then
            let mutable v = Unchecked.defaultof<'T>
            let success' = defaultDict.TryGetValue (state, &v)
            if success' then
                v
            else
                NoobishStyleSheet.GetDefault d themeId "default" fallback
        else
            NoobishStyleSheet.GetDefault d "Default" state fallback

    member t.GetWidth (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Widths cid state 0f

    member t.GetHeight (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Heights cid state 0f

    member t.GetFont (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Fonts cid state "None"

    member t.GetFontSize (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.FontSizes cid state 25

    member t.GetFontColor (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.FontColors cid state Color.White

    member t.GetColor (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Colors cid state Color.White

    member t.GetPadding (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Paddings cid state NoobishPadding.empty

    member t.GetMargin (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Margins cid state NoobishMargin.empty

    member t.GetTextAlignment (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.TextAlignments cid state NoobishAlignment.TopLeft

    member t.GetDrawables (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Drawables cid state [||]

