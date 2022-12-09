namespace Noobish

[<RequireQualifiedAccess>]
type NoobishLayout =
| Default
| Grid of cols: int * rows: int
| OverlaySource
| Absolute
| None

type NoobishTextAlign =
| TopLeft | TopCenter | TopRight
| Left  | Center | Right
| BottomLeft | BottomCenter | BottomRight

[<RequireQualifiedAccess>]
type NoobishTextureSize = Stretch | BestFitMax | BestFitMin | Original

[<RequireQualifiedAccess>]
type NoobishKeyId =
    | Escape
    | Enter
    | None

type FontSettings = {
    Small: string
    Normal: string
    Large: string
}

type NoobishSettings = {
    Scale: float32
    Pixel: string
    FontSettings: FontSettings
}

[<RequireQualifiedAccess>]
type NoobishTexture =
    | None
    | NinePatch of string
    | Basic of string
    | Atlas of id: string * sx: int * sy: int * sw: int * sh: int

[<RequireQualifiedAccess>]
type NoobishTextureEffect =
    | None
    | FlipHorizontally
    | FlipVertically



namespace Noobish.Internal
    open Noobish
    open System
    open System.Collections.Generic

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

    type NoobishComponentModel =
        | Slider of SliderModel
        | Combobox of ComboboxModel

    type ComponentMessage =
        | Show
        | Hide
        | ToggleVisibility
        | SetScrollX of float32
        | SetScrollY of float32
        | ChangeModel of (NoobishComponentModel -> NoobishComponentModel)

    type ComponentChangeDispatch = (ComponentMessage -> unit)

    type LayoutComponentState = {
        Id: string
        Name: string
        mutable Toggled: bool
        mutable Visible: bool
        mutable PressedTime: TimeSpan
        mutable ScrolledTime: TimeSpan

        mutable ScrollX: float32
        mutable ScrollY: float32

        Version: Guid
        KeyboardShortcut: NoobishKeyId

        Model: option<NoobishComponentModel>
    }

    type NoobishId = | NoobishId of string

    type NoobishState = {
        State: Dictionary<string, LayoutComponentState>
        StateByName: Dictionary<string, LayoutComponentState>
        TempState: Dictionary<string, LayoutComponentState>
    } with

        member private s.UpdateState (name: string) (state: LayoutComponentState) =
            s.State.[state.Id] <- state
            s.StateByName.[name] <- state

        member s.Update (name: string) (message:ComponentMessage) =
            let (success, cs) = s.StateByName.TryGetValue(name)
            if success then
                match message with
                | Show ->
                    cs.Visible <- true
                | Hide ->
                    cs.Visible <- false
                | ToggleVisibility ->
                    cs.Visible <- not cs.Visible
                | SetScrollX (v) ->
                    cs.ScrollX <- v
                | SetScrollY(v) ->
                    cs.ScrollY <- v
                | ChangeModel(cb) ->
                    cs.Model |> Option.iter (fun model ->
                        let model' = cb model
                        let cs = s.State.[cs.Id]
                        s.UpdateState name {cs with Model = Some(model') })
