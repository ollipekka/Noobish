namespace Noobish

open System

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

[<Struct>]
type NoobishSize = {Width: float32; Height: float32}

[<Struct>]
type NoobishPosition = {X: float32; Y: float32}


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
| Middle
| Right
| None

[<RequireQualifiedAccess>]
type NoobishKeyboardShortcut =
| KeyPressed of pressed: NoobishKeyId
| CtrlKeyPressed of pressed: NoobishKeyId
| AltKeyPressed of pressed: NoobishKeyId
| NoShortcut

module Internal =

    let pi = float32 System.Math.PI

    let inline max0 (value: float32) = if value < 0f then 0f else value

    let inline toDegrees angle = (float32 angle) * 180.0f / pi
    let inline toRadians angle = (float32 angle) * pi / 180.0f
