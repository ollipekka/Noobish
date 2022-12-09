namespace NoobishTypes

[<RequireQualifiedAccess>]
type NoobishLayout =
| Default
| Grid of cols: int * rows: int
| OverlaySource
| Absolute
| None


[<RequireQualifiedAccess>]
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
type NoobishTextureId =
    | None
    | NinePatch of string
    | Basic of string
    | Atlas of id: string * sx: int * sy: int * sw: int * sh: int

[<RequireQualifiedAccess>]
type NoobishTextureEffect =
    | None
    | FlipHorizontally
    | FlipVertically



module Internal =

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

    type NoobishTexture = {
        Texture: NoobishTextureId
        TextureEffect: NoobishTextureEffect
        TextureColor: int
        TextureColorDisabled:int
        TextureSize: NoobishTextureSize
        Rotation: int
    }


    type LayoutComponent = {
        Id: string
        Path: string
        Name: string
        ThemeId: string
        Enabled: bool
        Visible: bool
        Toggled: bool
        ZIndex: int
        Overlay: bool

        FillVertical: bool
        FillHorizontal: bool

        TextAlignment: NoobishTextAlign
        Text: string[]
        TextFont: string
        TextColor: int
        TextColorDisabled: int
        TextWrap: bool
        Texture: option<NoobishTexture>
        Color: int
        ColorDisabled: int
        PressedColor: int
        HoverColor: int
        StartX: float32
        StartY: float32
        RelativeX: float32
        RelativeY: float32
        OuterWidth: float32
        OuterHeight: float32
        IsBlock: bool
        PaddingLeft: float32
        PaddingRight: float32
        PaddingTop: float32
        PaddingBottom: float32
        MarginLeft: float32
        MarginRight: float32
        MarginTop: float32
        MarginBottom: float32

        BorderSize: float32
        BorderColor: int
        BorderColorDisabled: int

        ScrollBarColor: int
        ScrollPinColor: int
        ScrollBarThickness: float32
        ScrollPinThickness: float32
        ScrollHorizontal: bool
        ScrollVertical: bool
        OverflowWidth: float32
        OverflowHeight: float32

        Model: option<NoobishComponentModel>

        KeyboardShortcut: NoobishKeyId

        OnClickInternal: unit -> unit
        OnPressInternal: struct(int*int) -> LayoutComponent -> unit
        OnChange: string -> unit

        Layout: NoobishLayout
        ColSpan: int
        RowSpan: int

        Children: LayoutComponent[]
    } with
        member l.MarginVertical with get() = l.MarginTop + l.MarginBottom
        member l.MarginHorizontal with get() = l.MarginLeft + l.MarginRight
        member l.PaddingVertical with get() = l.PaddingTop + l.PaddingBottom
        member l.PaddingHorizontal with get() = l.PaddingRight+ l.PaddingLeft

        member l.Hidden with get() = not l.Visible
        member l.ContentStartX with get() = l.X + l.PaddingLeft + l.MarginLeft + l.BorderSize
        member l.ContentStartY with get() = l.Y + l.PaddingTop + l.MarginTop + l.BorderSize
        member l.ContentWidth with get() = l.OuterWidth - l.MarginHorizontal - l.PaddingHorizontal - 2f * l.BorderSize
        member l.ContentHeight with get() = l.OuterHeight - l.MarginVertical - l.PaddingVertical - 2f * l.BorderSize
        member l.X with get() = l.StartX + l.RelativeX
        member l.Y with get() = l.StartY + l.RelativeY
        member l.Width with get() = l.OuterWidth - l.MarginHorizontal
        member l.Height with get() = l.OuterHeight - l.MarginVertical

        member l.Content =
            {
                X = l.ContentStartX
                Y = l.ContentStartY
                Width = l.ContentWidth
                Height = l.ContentHeight
            }

        member l.ContentWithBorder with get() =
            {
                X = l.X + l.MarginLeft
                Y = l.Y + l.MarginTop
                Width = l.Width
                Height = l.Height
            }

        member l.ContentWithBorderAndMargin with get() =
            {
                X = l.X
                Y = l.Y
                Width = l.OuterWidth
                Height = l.OuterHeight
            }

        member l.Contains x y scrollX scrollY =
            let startX = l.X + l.MarginLeft + scrollX
            let endX = startX + l.Width
            let startY = l.Y + l.MarginTop + scrollY
            let endY = startY + l.Height
            not (x < startX || x > endX || y < startY || y > endY)

    type Attribute =
        | Name of string
        | Padding of left: int * right: int * top: int * bottom: int
        | PaddingLeft of int
        | PaddingRight of int
        | PaddingTop of int
        | PaddingBottom of int

        | Margin of left: int * right: int * top: int * bottom: int
        | MarginLeft of int
        | MarginRight of int
        | MarginTop of int
        | MarginBottom of int
        | ZIndex of int
        | Overlay

        | Text of string
        | TextFont of string
        | TextSmall
        | TextLarge
        | TextAlign of NoobishTextAlign
        | TextColor of int
        | TextWrap

        | SliderRange of min:float32 * max:float32
        | SliderValue of float32
        | SliderStep of float32
        | SliderValueChanged of (float32 -> unit)

        | Fill
        | FillHorizontal
        | FillVertical

        | OnClick of (unit -> unit)
        | OnClickInternal of ((string -> ComponentMessage -> unit) -> unit)
        | OnPress of (struct(int*int) -> unit)
        | OnPressInternal of ((string -> ComponentMessage -> unit) -> struct(int*int) -> LayoutComponent -> unit)
        | OnChange of (string -> unit)
        | Toggled of bool
        | Block
        | MinSize of width: int * height: int
        | FgColor of int
        | Visible of bool
        | Enabled of bool
        | DisabledColor of int
        | BorderColor of int
        | BorderSize of int
        | Texture of NoobishTextureId
        | TextureEffect of NoobishTextureEffect
        | TextureColor of int
        | TextureColorDisabled of int
        | TextureSize of NoobishTextureSize
        | TextureRotation of int
        | ScrollHorizontal
        | ScrollVertical
        | Scroll
        | ScrollBarColor of int
        | ScrollPinColor of int
        | ScrollBarThickness of int
        | ScrollPinThickness of int
        | Layout of NoobishLayout
        | RowSpan of int
        | ColSpan of int
        | RelativePosition of x: int * y: int
        | KeyboardShortcut of NoobishKeyId

    type Component = {
        ThemeId: string
        Children: list<Component>
        Attributes: list<Attribute>
    }

    let pi = float32 System.Math.PI
    let clamp n minVal maxVal = max (min n maxVal) minVal
    let inline toDegrees angle = (float32 angle) * 180.0f / pi
    let inline toRadians angle = (float32 angle) * pi / 180.0f