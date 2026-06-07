namespace Noobish

open System
open System.Collections.Generic

[<Struct>]
type UIComponentIdV2 = {
    Namespace: uint16
    Version: uint16
    Index: uint16
    LocalId: uint16
}

module UIComponentIdV2 =
    let empty: UIComponentIdV2 = {
        Namespace = UInt16.MaxValue
        Version = UInt16.MaxValue
        Index = UInt16.MaxValue
        LocalId = UInt16.MaxValue
    }

    let create (ns: uint16) (version: uint16) (index: uint16) (localId: uint16) : UIComponentIdV2 = {
        Namespace = ns
        Version = version
        Index = index
        LocalId = localId
    }

    let isEmpty (id: UIComponentIdV2) =
        id.Namespace = UInt16.MaxValue
        && id.Version = UInt16.MaxValue
        && id.Index = UInt16.MaxValue
        && id.LocalId = UInt16.MaxValue

module NamespaceHash =
    let fnv1a32 (value: string) =
        let mutable hash = 2166136261u
        for i = 0 to value.Length - 1 do
            hash <- hash ^^^ uint32 value.[i]
            hash <- hash * 16777619u
        hash

    let toNamespace (hash: uint32) = uint16 (hash &&& 0xFFFFu)

    let fromPage (page: string) =
        page
        |> fnv1a32
        |> toNamespace

[<Struct>]
type TableSpan = {
    Rowspan: int
    Colspan: int
}

[<Struct>]
type Fill = {
    Horizontal: bool
    Vertical: bool
}

[<Struct>]
type Scroll = {
    Horizontal: bool
    Vertical: bool
}

[<RequireQualifiedAccess>]
type NoobishTextDisplayMode =
| Plain
| Masked

module NoobishTextDisplay =
    let private maskCharacter = '*'
    let private maskedByLength = Dictionary<int, string>()

    let maskedText length =
        if length <= 0 then
            ""
        else
            let mutable text = ""
            if maskedByLength.TryGetValue(length, &text) then
                text
            else
                text <- String(maskCharacter, length)
                maskedByLength.[length] <- text
                text

    let resolve mode (text: string) =
        match mode with
        | NoobishTextDisplayMode.Plain -> text
        | NoobishTextDisplayMode.Masked ->
            if String.IsNullOrWhiteSpace text then
                text
            else
                maskedText text.Length

[<Struct>]
type NoobishSize = {Width: float32; Height: float32}

module NoobishSize =
    let hasArea (size: NoobishSize) =
        size.Width > 0f && size.Height > 0f
    let hasExtent (size: NoobishSize) =
        size.Width > 0f || size.Height > 0f

[<Struct>]
type NoobishPosition = {X: float32; Y: float32}

[<Struct>]
type NoobishColor = { R: byte; G: byte; B: byte; A: byte }

module NoobishColor =
    let fromRgba32 (rgba: uint32) =
        {
            R = byte (rgba >>> 24)
            G = byte (rgba >>> 16)
            B = byte (rgba >>> 8)
            A = byte rgba
        }

    let toRgba32 (color: NoobishColor) =
        (uint32 color.R <<< 24)
        ||| (uint32 color.G <<< 16)
        ||| (uint32 color.B <<< 8)
        ||| uint32 color.A

    let transparent: NoobishColor = { R = 0uy; G = 0uy; B = 0uy; A = 0uy }
    let white: NoobishColor = { R = 255uy; G = 255uy; B = 255uy; A = 255uy }


[<RequireQualifiedAccess>]
type NoobishLayout =
| Default
| Grid of cols: int * rows: int
| OverlaySource
| Absolute
| None

[<RequireQualifiedAccess>]
type NoobishTextureId =
    | NinePatch of atlasId: string * ninePatchId: string
    | Basic of string
    | Atlas of atlasId: string * textureId: string

[<RequireQualifiedAccess>]
type NoobishTextureEffect =
    | None
    | FlipHorizontally
    | FlipVertically

[<RequireQualifiedAccess>]
type NoobishImageSize = Stretch | BestFitMax | BestFitMin | Original

[<RequireQualifiedAccess>]
type NoobishAlignment =
| None
| TopLeft | TopCenter | TopRight
| Left  | Center | Right
| BottomLeft | BottomCenter | BottomRight

[<Struct>]
type NoobishRectangle = {
    X: float32
    Y: float32
    Width: float32
    Height: float32
} with
    member r.Left with get() = r.X
    member r.Right with get() = r.X + r.Width
    member r.Top with get() = r.Y
    member r.Bottom with get() = r.Y + r.Height

    member this.Clamp (bounds: NoobishRectangle) =
        let x = 
            if this.X < bounds.X then bounds.X else this.X
        let y = 
            if this.Y < bounds.Y then bounds.Y else this.Y

        let right = if this.Right > bounds.Right then bounds.Right else this.Right
        let bottom = if this.Bottom > bounds.Bottom then bounds.Bottom else this.Bottom

        {
            X = x 
            Y = y
            Width = max 0f (right - x)
            Height = max 0f (bottom - y)
        }

    
    member this.Contains  (x: float32) (y: float32) =
        x >= this.X && x <= this.X + this.Width
        && y >= this.Y && y <= this.Y + this.Height

module NoobishRectangle =
    let hasArea (bounds: NoobishRectangle) =
        bounds.Width > 0f && bounds.Height > 0f
    let hasExtent (bounds: NoobishRectangle) =
        bounds.Width > 0f || bounds.Height > 0f

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

[<RequireQualifiedAccess>]
type NoobishKeyId =
| Escape
| Enter
| Space
| A
| B
| C
| D
| E
| F
| G
| H
| I
| J
| K
| L
| M
| N
| O
| P
| Q
| R
| S
| T
| U
| V
| W
| X
| Y
| Z
| Left
| Right
| None

[<RequireQualifiedAccess>]
type NoobishMouseButtonId =
| Left
| Right
| Middle
| XButton1
| XButton2
| None

[<RequireQualifiedAccess>]
type NoobishProgressStyle =
| None
| Bar
| Radial
| RadialSquare

[<RequireQualifiedAccess>]
type NoobishKeyboardShortcut =
| KeyPressed of pressed: NoobishKeyId
| CtrlKeyPressed of pressed: NoobishKeyId
| AltKeyPressed of pressed: NoobishKeyId
| NoShortcut

module Internal =

    let pi = float32 System.Math.PI


#if DEBUG
    let max0 (value: float32) = if value < 0f then 0f else value
#else 
    let inline max0 (value: float32) = if value < 0f then 0f else value
#endif
    let inline toDegrees angle = (float32 angle) * 180.0f / pi
    let inline toRadians angle = (float32 angle) * pi / 180.0f
