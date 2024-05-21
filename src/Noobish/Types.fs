namespace Noobish

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
| TopLeft | TopCenter | TopRight
| Left  | Center | Right
| BottomLeft | BottomCenter | BottomRight

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

    let pi = float32 System.Math.PI
    let clamp n minVal maxVal = max (min n maxVal) minVal
    let inline toDegrees angle = (float32 angle) * 180.0f / pi
    let inline toRadians angle = (float32 angle) * pi / 180.0f