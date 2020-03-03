namespace Noobish

type Theme = {
    TextColor: int
    TextColorDisabled: int
    TextHorizontalAlignment: NoobishHorizontalTextAlign
    TextVerticalAlignment: NoobishVerticalTextAlign

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
}


module Theme =
    open System.Collections.Generic
    let empty =
        {
            TextColor = 0x00000000
            TextColorDisabled = 0x00000000
            TextHorizontalAlignment = NoobishHorizontalTextAlign.Left
            TextVerticalAlignment = NoobishVerticalTextAlign.Center

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
        }

    let createDefaultTheme(): IDictionary<string, Theme> =
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
        dict [
            "Label", {
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Left
                TextVerticalAlignment = NoobishVerticalTextAlign.Center

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
            }
            "Paragraph", {
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Left
                TextVerticalAlignment = NoobishVerticalTextAlign.Top

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
            }
            "Header", {
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Left
                TextVerticalAlignment = NoobishVerticalTextAlign.Center

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
            }
            "Button", {
                TextColor = textColor
                TextColorDisabled = textColorDisabled
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Center
                TextVerticalAlignment = NoobishVerticalTextAlign.Center

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
            }
            "Panel", {
                TextColor = 0xffffffff
                TextColorDisabled = 0x00000000
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Center
                TextVerticalAlignment = NoobishVerticalTextAlign.Center

                TextureColor = textureColor
                TextureColorDisabled = textureColorDisabled

                BorderSize = 2
                BorderColor = borderColor
                BorderColorDisabled = borderColorDisabled

                Color = backgroundColorDark
                ColorDisabled = 0x00000000

                PressedColor = 0x00000000
                HoverColor = 0x00000000

                Padding = (5, 5, 5, 5)
                Margin =  (2, 2, 2, 2)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
            }
            "Division", {
                TextColor = 0xffffffff
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Center
                TextVerticalAlignment = NoobishVerticalTextAlign.Center
                TextColorDisabled = 0x00000000

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
            }
            "HorizontalRule", {
                TextColor = 0x00000000
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Center
                TextVerticalAlignment = NoobishVerticalTextAlign.Center
                TextColorDisabled = 0x00000000

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
                Margin =  (5, 5, 0, 0)

                ScrollBarColor = 0x00000000
                ScrollPinColor = 0x00000000
            }
            "Image", {
                TextColor = 0x00000000
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Center
                TextVerticalAlignment = NoobishVerticalTextAlign.Center
                TextColorDisabled = 0x00000000

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
            }
            "Scroll", {
                TextColor = 0xffffffff
                TextHorizontalAlignment = NoobishHorizontalTextAlign.Center
                TextVerticalAlignment = NoobishVerticalTextAlign.Center
                TextColorDisabled = 0x00000000

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

                ScrollBarColor = backgroundColor
                ScrollPinColor = backgroundColorLight
            }
        ]
