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

[<RequireQualifiedAccess>]
type NoobishStyle =
| Width of int
| Height of int

// Text related styles.
| Font of string
| FontColor of Color

| Color of int
| Drawables of list<NoobishDrawable>
| Padding of (int*int*int*int)
| Margin of (int*int*int*int)

type NoobishStyleSheet = {
    Name: string
    TextureAtlasId: string
    Widths: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
    Heights: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
    Paddings: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int*int*int*int>>
    Margins: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int*int*int*int>>
    Colors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, Color>>
    Fonts: IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>
    FontSizes: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>
    FontColors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, Color>>
    TextAlignments: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishTextAlignment>>
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
        NoobishStyleSheet.GetValue t.Paddings cid state (0, 0, 0, 0)

    member t.GetMargin (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Margins cid state (0, 0, 0, 0)

    member t.GetTextAlignment (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.TextAlignments cid state NoobishTextAlignment.TopLeft

    member t.GetDrawables (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Drawables cid state [||]

