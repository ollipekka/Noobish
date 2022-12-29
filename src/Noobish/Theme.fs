module Noobish.Theme

open System.Collections.Generic


type NoobishElementStyle = {
    TextFont: string
    TextColor: int
    TextColorDisabled: int
    TextAlignment: NoobishTextAlign

    Width: int
    Height: int

    TextureColor: int
    TextureColorDisabled: int

    BorderSize: int
    BorderColor: int
    BorderColorDisabled: int

    Color: int
    PressedColor: int
    HoverColor: int
    ColorDisabled: int

    Padding: int*int*int*int
    Margin: int*int*int*int

    ScrollBarColor: int
    ScrollPinColor: int
    ScrollBarThickness: int
    ScrollPinThickness: int

}

type Theme = {
    FontSettings: FontSettings
    Styles: IDictionary<string, NoobishElementStyle>
}
let empty defaultFont =
    {
        TextFont = defaultFont
        TextColor = 0x00000000
        TextColorDisabled = 0x00000000
        TextAlignment = NoobishTextAlign.Left

        Width = 0
        Height = 0

        TextureColor = 0x00000000
        TextureColorDisabled = 0x00000000

        BorderSize = 0
        BorderColor = 0x00000000
        BorderColorDisabled = 0x00000000

        Color = 0x00000000
        ColorDisabled = 0x00000000

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
    let textColorDisabled = 0x806d5fff
    let backgroundDisabled = 0x4d4b39ff
    let backgroundColorDark = 0x262b33ff
    let backgroundColor = 0x39404dff
    let backgroundColorLight = 0x5f6b80ff
    let borderColor = 0x4c5666ff
    let borderColorDisabled = 0x806d5fff

    let textureColor = 0xffffffff
    let textureColorDisabled = 0xccccccff
    let NoobishElementStyles =
        dict [
            "Label", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextAlignment = NoobishTextAlign.Left

                Width = 0
                Height = 0

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 0
                BorderColor = 0x00000000
                BorderColorDisabled = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000

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
                TextAlignment = NoobishTextAlign.Left

                Width = 0
                Height = 0

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 1
                BorderColor = 0xFFFFFFFF
                BorderColorDisabled = 0xCCCCCCFF

                Color = 0x00000000
                ColorDisabled = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (0, 0, 0, 0)
                Margin =  (5, 5, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
                ScrollBarThickness = 2
                ScrollPinThickness = 2
            }
            "Paragraph", {
                TextFont = defaultFont
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextAlignment = NoobishTextAlign.TopLeft


                Width = 0
                Height = 0

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 0
                BorderColor = 0x00000000
                BorderColorDisabled = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000

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
                TextAlignment = NoobishTextAlign.Left

                Width = 0
                Height = 0

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 0
                BorderColor = 0x00000000
                BorderColorDisabled = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000

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
                TextAlignment = NoobishTextAlign.Center

                Width = 0
                Height = 0

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 2
                BorderColor = borderColor
                BorderColorDisabled = borderColorDisabled

                Color = backgroundColor
                ColorDisabled = backgroundDisabled

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
                TextAlignment = NoobishTextAlign.Center

                Width = 0
                Height = 0

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 2
                BorderColor = borderColor
                BorderColorDisabled = borderColorDisabled

                Color = backgroundColor
                ColorDisabled = backgroundDisabled

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
                TextAlignment = NoobishTextAlign.Center

                Width = 0
                Height = 0

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 2
                BorderColor = borderColor
                BorderColorDisabled = borderColorDisabled

                Color = backgroundColorDark
                ColorDisabled = backgroundColorDark

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
                TextColor = 0xffffffff
                TextAlignment = NoobishTextAlign.Center
                TextColorDisabled = 0x00000000

                Width = 0
                Height = 0

                TextureColor = 0x00000000
                TextureColorDisabled = 0x00000000

                BorderSize = 0
                BorderColor = 0x00000000
                BorderColorDisabled = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000

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
                TextColor = 0x00000000
                TextAlignment = NoobishTextAlign.Center
                TextColorDisabled = 0x00000000

                Width = 0
                Height = 3

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 0
                BorderColor = 0x00000000
                BorderColorDisabled = borderColorDisabled

                Color = borderColor
                ColorDisabled = borderColorDisabled

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
                TextColor = 0x00000000
                TextAlignment = NoobishTextAlign.Center
                TextColorDisabled = 0x00000000

                Width = 0
                Height = 0

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 0
                BorderColor = 0x00000000
                BorderColorDisabled = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000

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
                TextColor = 0xffffffff
                TextAlignment = NoobishTextAlign.Center
                TextColorDisabled = 0x00000000

                Width = 0
                Height = 0

                TextureColor = 0x00000000
                TextureColorDisabled = 0x00000000

                BorderSize = 0
                BorderColor = 0x00000000
                BorderColorDisabled = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000

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
                TextColor = 0xffffffff
                TextAlignment = NoobishTextAlign.Center
                TextColorDisabled = 0x00000000

                Width = 0
                Height = 0

                TextureColor = 0x00000000
                TextureColorDisabled = 0x00000000

                BorderSize = 0
                BorderColor = 0x00000000
                BorderColorDisabled = 0x00000000

                Color = 0x00000000
                ColorDisabled = 0x00000000

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
    {
        FontSettings = fontSettings
        Styles = NoobishElementStyles
    }
