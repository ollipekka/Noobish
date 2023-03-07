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
    | None
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
type NoobishTextAlignment =
| TopLeft | TopCenter | TopRight
| Left  | Center | Right
| BottomLeft | BottomCenter | BottomRight

[<RequireQualifiedAccess>]
type NoobishKeyId =
| Escape
| Enter
| Space
| None

[<RequireQualifiedAccess>]
type NoobishMouseButtonId =
| Left
| Right
| None


type NoobishSettings = {
    Pixel: string
}

module Internal =

    open System
    open System.Collections.Generic
    open Microsoft.Xna.Framework

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

    type SliderModel = {
        Min: float32
        Max: float32
        Step: float32
        OnValueChanged: float32 -> unit
        Value: float32
    }

    type ComboboxModel = {
        Values: string[]
        Value: string
    }

    type TextboxModel = {
        Text: string
        Cursor: int
        OnOpenKeyboard: (string -> unit) -> unit
    }

    type NoobishComponentModel =
        | Slider of SliderModel
        | Combobox of ComboboxModel
        | Textbox of TextboxModel

    module NoobishComponentModel =
        let isTextbox (m: option<NoobishComponentModel>) =
            m |> Option.exists(
                function
                | Textbox (_) -> true
                | _ -> false
            )
        let isSlider (m: option<NoobishComponentModel>) =
            m |> Option.exists(
                function
                | Slider (_) -> true
                | _ -> false
            )

        let isCombobox (m: option<NoobishComponentModel>) =
            m |> Option.exists(
                function
                | Combobox (_) -> true
                | _ -> false
            )
    let pi = float32 System.Math.PI
    let clamp n minVal maxVal = max (min n maxVal) minVal
    let inline toDegrees angle = (float32 angle) * 180.0f / pi
    let inline toRadians angle = (float32 angle) * pi / 180.0f