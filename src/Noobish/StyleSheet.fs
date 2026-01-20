namespace Noobish.Styles

open Noobish

open System.Collections.Generic



[<RequireQualifiedAccess>]
type NoobishDrawable=
| NinePatch of string
| NinePatchWithColor of string*NoobishColor
| Texture of string


type NoobishStyleSheet = {
    Name: string
    TextureAtlasId: string
    Widths: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
    Heights: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
    Paddings: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishPadding>>
    Margins: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishMargin>>
    Colors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishColor>>
    Fonts: IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>
    FontSizes: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>
    FontColors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishColor>>
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
        NoobishStyleSheet.GetValue t.FontColors cid state NoobishColor.white

    member t.GetColor (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Colors cid state NoobishColor.white

    member t.GetPadding (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Paddings cid state NoobishPadding.empty

    member t.GetMargin (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Margins cid state NoobishMargin.empty

    member t.GetTextAlignment (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.TextAlignments cid state NoobishAlignment.TopLeft

    member t.GetDrawables (cid: string) (state: string) =
        NoobishStyleSheet.GetValue t.Drawables cid state [||]
