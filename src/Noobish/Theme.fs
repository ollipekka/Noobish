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
| NinePatchWithColor of string*int
| Texture of string

[<RequireQualifiedAccess>]
type NoobishStyle =

| Width of int
| Height of int

// Text related styles.
| Font of string
| FontColor of int

| Color of int
| Drawables of list<NoobishDrawable>
| Padding of (int*int*int*int)
| Margin of (int*int*int*int)

let font f = NoobishStyle.Font f
let fontColor c = NoobishStyle.FontColor c

let width w = NoobishStyle.Width(w)
let height h = NoobishStyle.Height(h)
let color c = NoobishStyle.Color(c)
let drawables d = NoobishStyle.Drawables d
let texture t = NoobishDrawable.Texture t
let ninePatch t = NoobishDrawable.NinePatch t
let ninePatchWithColor t c = NoobishDrawable.NinePatchWithColor(t,c)
let padding t r b l = NoobishStyle.Padding(t,r,b,l)
let margin t r b l = NoobishStyle.Margin(t,r,b,l)

let styles2 = [
    "Default", [
        "default", [
            padding 5 5 5 5
            margin 2 2 2 2
            fontColor 0xbbbbbbFF
            color 0x39404dff
        ];
        "disabled", [
            padding 5 5 5 5
            margin 2 2 2 2
            fontColor 0x806d5fff
            color 0x39404dff
        ]
    ];
    "Panel", [
        "default", [
            drawables [
                ninePatch "panel-default.9"
            ]
        ]
        "toggled", [
            color 0xFF0000FF
            drawables [
                ninePatch "panel-default.9"
            ]

        ]
    ];
    "Division", [
        "default", [
        ]
    ];
    "Button", [
        "default", [
            drawables [
                ninePatch "button-default.9"
            ]
        ];
        "toggled", [
            color 0xFF0000F
            drawables [
                ninePatch "button-toggled.9"
            ]
        ]
        "disabled", [
            drawables [
                ninePatch "button-disabled.9"
            ]
        ]
    ];
    "Combobox", [
        "default", [
            drawables [
                ninePatch "button-default.9"
            ]
        ];
    ];
    "TextBox", [
        "default", [
            drawables [
                ninePatch "textbox-default.9"
            ]
        ];
        "focused", [
            drawables [
                ninePatch "textbox-default.9"
                ninePatchWithColor "textbox-focused.9" 0xdf7126aa
            ]
        ]
    ];
    "Checkbox", [
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
    ];
    "Cursor", [
        "default", [
            color 0xdf7126aa
            drawables [
                ninePatch "cursor.9"
            ]
        ]
    ];
    "SliderPin", [
        "default", [
            color 0xdf7126aa
            height 16
            drawables [
                ninePatch "slider-pin.9"
            ]
        ]
    ];
    "Slider", [
        "default", [
            color 0x5f6b80ff
            drawables [
                ninePatch "slider.9"
            ]
        ]
    ]
    "ScrollBarPin", [
        "default", [
            color 0xdf7126aa
            drawables [
                ninePatch "scrollbar-pin.9"
            ]
        ]
    ];
    "ScrollBar", [
        "default", [
            color 0x5f6b80ff
            drawables [
                ninePatch "scrollbar.9"
            ]
        ]
    ]
]

type Theme = {
    FontSettings: FontSettings
    Widths: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
    Heights: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
    Paddings: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int*int*int*int>>
    Margins: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int*int*int*int>>
    Colors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>
    Fonts: IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>
    FontColors: IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>
    Drawables: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishDrawable[]>>
} with

    static member private GetDefault (d: IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>) (themeId: string) (state: string) (fallback: 'T): 'T =
        let (success, defaultDict) = d.TryGetValue(themeId)
        if success then
            let (success', v) = defaultDict.TryGetValue state
            if success' then
                v
            else
                Theme.GetDefault d "Default" state fallback
        else
            fallback


    static member private GetValue (d: IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>) (themeId: string) (state: string) (fallback: 'T): 'T =
        let (success, defaultDict) = d.TryGetValue(themeId)
        if success then
            let (success', v) = defaultDict.TryGetValue state
            if success' then
                v
            else
                Theme.GetDefault d themeId "default" fallback
        else
            Theme.GetDefault d "Default" "default" fallback



    member t.GetWidth (cid: string) (state: string) =
        Theme.GetValue t.Widths cid state 0f

    member t.GetHeight (cid: string) (state: string) =
        Theme.GetValue t.Heights cid state 0f

    member t.GetFont (cid: string) (state: string) =
        Theme.GetValue t.Fonts cid state t.FontSettings.Normal

    member t.GetFontColor (cid: string) (state: string) =
        Theme.GetValue t.FontColors cid state 0xFFFFFFFF

    member t.GetColor (cid: string) (state: string) =
        Theme.GetValue t.Colors cid state 0xFFFFFFFF

    member t.GetPadding (cid: string) (state: string) =
        Theme.GetValue t.Paddings cid state (0, 0, 0, 0)

    member t.GetMargin (cid: string) (state: string) =
        Theme.GetValue t.Margins cid state (0, 0, 0, 0)

    member t.GetDrawables (cid: string) (state: string) =
        Theme.GetValue t.Drawables cid state [||]


let toReadOnlyDictionary (dictionary: Dictionary<string, Dictionary<string, 'T>>) =
    dictionary
        |> Seq.map(fun kvp -> KeyValuePair(kvp.Key, kvp.Value :> IReadOnlyDictionary<string, 'T>))
        |> Dictionary
        :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>

let createDefaultTheme (fontSettings: FontSettings) =
    let widths = Dictionary<string, Dictionary<string, float32>>()
    let heights = Dictionary<string, Dictionary<string, float32>>()

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
                | NoobishStyle.Width(w) ->
                    let widthsByComponent = widths.GetOrAdd name (fun () -> Dictionary())
                    widthsByComponent.[stateId] <- float32 w
                | NoobishStyle.Height(h) ->

                    let heightsByComponent = heights.GetOrAdd name (fun () -> Dictionary())
                    heightsByComponent.[stateId] <- float32 h
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
        Widths = widths |> toReadOnlyDictionary
        Heights = heights |> toReadOnlyDictionary
        Fonts = fonts |> toReadOnlyDictionary
        FontColors = fontColors |> toReadOnlyDictionary
        Colors = colors |> toReadOnlyDictionary
        Drawables = drawables |> toReadOnlyDictionary
        Paddings = paddings |> toReadOnlyDictionary
        Margins = margins |> toReadOnlyDictionary


    }
