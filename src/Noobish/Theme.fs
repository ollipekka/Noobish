module Noobish.Theme

open System.Collections.Generic

module private DictionaryExtensions =
    type Dictionary<'TKey, 'TValue> with

        member d.GetOrAdd (key: 'TKey) (init: unit -> 'TValue) =

            let (success, value) = d.TryGetValue(key)

            if success then
                value
            else
                let value = init()
                d.[key] <- value
                value

open DictionaryExtensions

[<RequireQualifiedAccess>]
type NoobishDrawable=
| NinePatch of string
| Texture of string

[<RequireQualifiedAccess>]
type NoobishStyle =

// Text related styles.
| Font of string
| FontColor of int

| Color of int
| Drawables of list<NoobishDrawable>
| Padding of (int*int*int*int)
| Margin of (int*int*int*int)

let font f = NoobishStyle.Font
let fontColor c = NoobishStyle.FontColor c

let color c = NoobishStyle.Color(c)
let drawables d = NoobishStyle.Drawables d
let texture t = NoobishDrawable.Texture t
let ninePatch t = NoobishDrawable.NinePatch t
let padding t r b l = NoobishStyle.Padding(t,r,b,l)
let margin t r b l = NoobishStyle.Margin(t,r,b,l)

let styles2 = [
    "Panel", [
        "default", [
            padding 5 5 5 5
            margin 2 2 2 2
            drawables [
                ninePatch "panel-default.9"
            ]
        ]
    ]
    "Button", [
        "default", [
            color 0xFFFFFFFF
            fontColor 0xAAAAAA
            drawables [
                ninePatch "button-default.9"
            ]
        ];
        "toggled", [
            color 0xFFFFFFFF
            drawables [
                ninePatch "button-toggled.9"
            ]
        ]
        "disabled", [
            color 0xFFFFFFFF
            drawables [
                ninePatch "button-disabled.9"
            ]
        ]
    ];
    "TextBox", [
        "default", [
            color 0xFF0000FF
            fontColor 0xAAAAAA
            drawables [
                ninePatch "panel-default.9"
            ]
        ];
        "focused", [
            color 0xFFFF00FF
            drawables [
                ninePatch "panel-default.9"
                ninePatch "panel-default.9"
            ]
        ]
    ];
    "checkbox", [
        "default", [
            drawables [
                texture "checkbox"
            ]
        ];
        "checked", [
            drawables [
                texture "checkbox"
                texture "checkbox-checked"
            ]
        ]
    ]
]

type Theme = {
    FontSettings: FontSettings
    Paddings: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int*int*int*int>>
    Margins: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int*int*int*int>>
    Colors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>
    Fonts: IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>
    FontColors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>
    Drawables: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishDrawable[]>>
} with

    member t.GetFont (cid: string) (state: string) =
        let (success, fonts) = t.Fonts.TryGetValue(cid)

        if success then
            let success', font = fonts.TryGetValue(state)
            if success' then
                font
            else
                t.FontSettings.Normal
        else
            t.FontSettings.Normal

    member t.GetFontColor (cid: string) (state: string) =
        let (success, fontColors) = t.FontColors.TryGetValue(cid)

        if success then
            let success', color = fontColors.TryGetValue(state)
            if success' then
                color
            else
                0xFFFFFFFF
        else
            0xFFFFFFFF


    member t.GetColor (cid: string) (state: string) =
        let (success, colors) = t.Colors.TryGetValue(cid)

        if success then
            let success', color = colors.TryGetValue(state)
            if success' then
                color
            else
                0xFFFFFFFF
        else
            0xFFFFFFFF


    member t.GetPadding (cid: string) (state: string) =
        let (success, paddings) = t.Paddings.TryGetValue(cid)

        if success then
            let success', padding = paddings.TryGetValue(state)
            if success' then
                padding
            else
                (0,0,0,0)
        else
            (0,0,0,0)

    member t.GetMargin (cid: string) (state: string) =
        let (success, margins) = t.Margins.TryGetValue(cid)

        if success then
            let success', margin = margins.TryGetValue(state)
            if success' then
                margin
            else
                (0,0,0,0)
        else
            (0,0,0,0)

    member t.GetDrawables (cid: string) (state: string) =
        let (success, drawables) = t.Drawables.TryGetValue(cid)

        if success then
            let success', drawables = drawables.TryGetValue(state)
            if success' then
                drawables
            else
                [||]
        else
            [||]


(*
let empty defaultFont =
    {
        TextFont = defaultFont
        TextColor = 0x00000000
        TextColorDisabled = 0x00000000
        TextColorFocused = 0x00000000
        TextAlignment = NoobishTextAlign.Left

        TextureColor = 0x00000000
        TextureColorDisabled = 0x00000000
        TextureColorFocused = 0x00000000

        Color = 0x00000000
        ColorDisabled = 0x00000000
        ColorFocused = 0x00000000

        PressedColor = 0x00000000
        HoverColor = 0x00000000

        Padding = (0, 0, 0, 0)
        Margin =  (0, 0, 0, 0)

        ScrollBarColor = 0x00000000
        ScrollPinColor = 0x00000000
        ScrollBarThickness = 2
        ScrollPinThickness = 2
    }

let createDefaultTheme (fontSettings: FontSettings): Theme=
    let defaultFont = fontSettings.Normal
    let textColor = 0xbbbbbbFF
    let textInputColor = 0xccccccFF
    let textColorDisabled = 0x806d5fff
    let backgroundDisabled = 0x4d4b39ff
    let backgroundColorDark = 0x262b33ff
    let backgroundColor = 0x39404dff
    let backgroundColorLight = 0x5f6b80ff
    let borderColor = 0x4c5666ff
    let borderColorFocused = 0xffd700ff
    let borderColorDisabled = 0x806d5fff

    let textureColor = 0xffffffff
    let textureColorDisabled = 0xccccccff
    let NoobishElementStyles =
        dict [
            "Label", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = 0x00000000
                TextAlignment = NoobishTextAlign.Left

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000
                ColorFocused = 0x000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (0, 0, 0, 0)
                Margin =  (5, 5, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "TextBox", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.Left

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000
                ColorFocused = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (5, 5, 5, 5)
                Margin =  (2, 2, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Paragraph", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.TopLeft


                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = textureColor

                Color = 0x00000000
                ColorDisabled = 0x00000000
                ColorFocused = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (0, 0, 0, 0)
                Margin =  (5, 5, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Header", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.Left

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = textureColor

                Color = 0x00000000
                ColorDisabled = 0x00000000
                ColorFocused = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (0, 0, 0, 0)
                Margin =  (5, 5, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Button", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.Center

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = textureColor

                Color = backgroundColor
                ColorDisabled = backgroundDisabled
                ColorFocused= backgroundColor

                PressedColor = backgroundColorLight
                HoverColor = backgroundColorLight

                Padding = (5, 5, 5, 5)
                Margin =  (2, 2, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Combobox", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColorDisabled
                TextAlignment = NoobishTextAlign.Center

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = textureColor

                Color = backgroundColor
                ColorDisabled = backgroundDisabled
                ColorFocused = backgroundColor

                PressedColor = backgroundColorLight
                HoverColor = backgroundColorLight

                Padding = (5, 5, 5, 5)
                Margin =  (2, 2, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Panel", {
                TextFont = defaultFont
                TextColor = 0xffffffff
                TextColorDisabled = 0x00000000
                TextColorFocused = 0x00000000
                TextAlignment = NoobishTextAlign.Center

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = textureColor

                Color = backgroundColorDark
                ColorDisabled = backgroundColorDark
                ColorFocused = backgroundColorDark

                PressedColor = backgroundColorDark
                HoverColor = backgroundColorDark

                Padding = (5, 5, 5, 5)
                Margin =  (2, 2, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Division", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.Center

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = textureColor

                Color = 0x00000000
                ColorDisabled = 0x00000000
                ColorFocused = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (0, 0, 0, 0)
                Margin =  (0, 0, 0, 0)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "HorizontalRule", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.Center

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = textureColor

                Color = borderColor
                ColorDisabled = borderColorDisabled
                ColorFocused = borderColor


                PressedColor = 0xccaaaaff
                HoverColor = 0xccaaaaff

                Padding = (5, 5, 0, 0)
                Margin =  (5, 5, 5, 5)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 0
                ScrollPinThickness = 0
            }
            "Image", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.Center

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled
                TextureColorFocused = textureColor


                Color = 0x00000000
                ColorDisabled = 0x00000000
                ColorFocused = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (5, 5, 5, 5)
                Margin =  (5, 5, 0, 0)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Scroll", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.Center

                TextureColor = 0x00000000
                TextureColorDisabled = 0x00000000
                TextureColorFocused = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000
                ColorFocused = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (2, 2, 2, 2)
                Margin =  (0, 0, 0, 0)

                ScrollBarColor = 0x4d4139aa
                ScrollPinColor = 0xdf7126aa
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Slider", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextColorFocused = textColor
                TextAlignment = NoobishTextAlign.Center

                TextureColor = 0x00000000
                TextureColorDisabled = 0x00000000
                TextureColorFocused = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000
                ColorFocused = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (2, 2, 2, 2)
                Margin =  (0, 0, 4, 4)

                ScrollBarColor = 0x4d4139aa
                ScrollPinColor = 0xdf7126aa
                ScrollBarThickness = 6
                ScrollPinThickness = 12
            }
            "Empty", empty defaultFont
        ]
*)


let toReadOnlyDictionary (dictionary: Dictionary<string, Dictionary<string, 'T>>) =
    dictionary
        |> Seq.map(fun kvp -> KeyValuePair(kvp.Key, kvp.Value :> IReadOnlyDictionary<string, 'T>))
        |> Dictionary
        :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>

let createDefaultTheme (fontSettings: FontSettings)  =

    let fonts = Dictionary<string, Dictionary<string, string>>()
    let fontColors = Dictionary<string, Dictionary<string, int>>()
    let colors = Dictionary<string, Dictionary<string, int>>()
    let drawables = Dictionary<string, Dictionary<string, NoobishDrawable[]>>()
    let paddings = Dictionary<string, Dictionary<string, (int*int*int*int)>>()
    let margins = Dictionary<string, Dictionary<string, (int*int*int*int)>>()

    for (name, componentStyles) in styles2 do
        for (stateId, styles) in componentStyles do
            for style in styles do
                match style with
                | NoobishStyle.Font (font) ->
                    let fontsByComponent = fonts.GetOrAdd name (fun () -> Dictionary())
                    fontsByComponent.[stateId] <- font
                | NoobishStyle.FontColor (c) ->
                    let fontColorsByComponent = fontColors.GetOrAdd name (fun () -> Dictionary())
                    fontColorsByComponent.[stateId] <- c
                | NoobishStyle.Color (c) ->
                    let colorsByComponent = colors.GetOrAdd name (fun () -> Dictionary())
                    colorsByComponent.[stateId] <- c
                | NoobishStyle.Drawables (d) ->
                    let drawablesByComponent = drawables.GetOrAdd name (fun () -> Dictionary())
                    drawablesByComponent.[stateId] <- d |> List.toArray
                | NoobishStyle.Padding (p) ->
                    let paddingsByComponent = paddings.GetOrAdd name (fun () -> Dictionary())
                    paddingsByComponent.[stateId] <- p
                | NoobishStyle.Margin (m) ->
                    let marginsByComponent = margins.GetOrAdd name (fun () -> Dictionary())
                    marginsByComponent.[stateId] <- m



    {
        FontSettings = fontSettings
        Fonts = fonts |> toReadOnlyDictionary
        FontColors = fontColors |> toReadOnlyDictionary
        Colors = colors |> toReadOnlyDictionary
        Drawables = drawables |> toReadOnlyDictionary
        Paddings = paddings |> toReadOnlyDictionary
        Margins = margins |> toReadOnlyDictionary


    }
