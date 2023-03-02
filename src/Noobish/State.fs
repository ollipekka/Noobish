namespace Noobish
    

open Microsoft.Xna.Framework

open Noobish

open Noobish.TextureAtlas
open Noobish.Styles
open Noobish.Internal

open System
open System.Collections.Generic

type NoobishImage = {
    Texture: NoobishTextureId
    TextureEffect: NoobishTextureEffect
    Color: Color
    ImageSize: NoobishImageSize
    Rotation: int
}

type NoobishLayoutElement = {
    Id: string
    ParentId: string
    Path: string
    ThemeId: string
    Enabled: bool
    Visible: bool
    Toggled: bool
    ZIndex: int
    Overlay: bool

    FillVertical: bool
    FillHorizontal: bool

    TextAlignment: NoobishTextAlignment
    Text: string
    TextWrap: bool
    Image: option<NoobishImage>
    Color: Color
    ColorDisabled: Color

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

    ScrollHorizontal: bool
    ScrollVertical: bool
    OverflowWidth: float32
    OverflowHeight: float32

    Model: option<NoobishComponentModel>

    KeyboardShortcut: NoobishKeyId

    ConsumedMouseButtons: NoobishMouseButtonId[]
    ConsumedKeys: NoobishKeyId[]
    KeyTypedEnabled: bool
    OnClickInternal: NoobishLayoutElement -> unit
    OnPressInternal: struct(int*int) -> NoobishLayoutElement -> unit
    OnChange: string -> unit

    Layout: NoobishLayout
    ColSpan: int
    RowSpan: int

    Children: NoobishLayoutElement[]
} with
    member l.MarginVertical with get() = l.MarginTop + l.MarginBottom
    member l.MarginHorizontal with get() = l.MarginLeft + l.MarginRight
    member l.PaddingVertical with get() = l.PaddingTop + l.PaddingBottom
    member l.PaddingHorizontal with get() = l.PaddingRight+ l.PaddingLeft

    member l.Hidden with get() = not l.Visible
    member l.ContentStartX with get() = l.X + l.PaddingLeft + l.MarginLeft
    member l.ContentStartY with get() = l.Y + l.PaddingTop + l.MarginTop
    member l.ContentWidth with get() = l.OuterWidth - l.MarginHorizontal - l.PaddingHorizontal
    member l.ContentHeight with get() = l.OuterHeight - l.MarginVertical - l.PaddingVertical
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

    member l.ContentWithPadding with get() =
        {
            X = l.X + l.MarginLeft
            Y = l.Y + l.MarginTop
            Width = l.Width
            Height = l.Height
        }

    member l.ContentWithMargin with get() =
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


type NoobishLayoutElementState = {
    Id: string
    ParentId: string
    mutable Focused: bool
    mutable Toggled: bool
    mutable Visible: bool

    mutable FocusedTime: TimeSpan
    mutable PressedTime: TimeSpan
    mutable ScrolledTime: TimeSpan

    mutable ScrollX: float32
    mutable ScrollY: float32

    mutable Model: option<NoobishComponentModel>

    Version: Guid
    KeyboardShortcut: NoobishKeyId


    Children: string[]
} with
    member s.CanFocus with get() =
        match s.Model with
        | Some(model') ->
            match model' with
            | Textbox (_) -> true
            | _ -> false
        | None -> false

type NoobishId = | NoobishId of string

type NoobishState () =
    member val ElementsById = Dictionary<string, NoobishLayoutElement>()
    member val ElementStateById = Dictionary<string, NoobishLayoutElementState>()
    member val TempElements = Dictionary<string, NoobishLayoutElementState>()
    member val FocusedElementId: Option<string> = None with get, set

    member private s.UpdateState (state: NoobishLayoutElementState) =
        s.ElementStateById.[state.Id] <- state

    member s.Item
        with get (tid: string) = s.ElementStateById.[tid]

    member s.Update (cid: string) (message:ComponentMessage) =
        let (success, cs) = s.ElementStateById.TryGetValue(cid)
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
                    let cs: NoobishLayoutElementState = s.ElementStateById.[cid]
                    s.UpdateState {cs with Model = Some(model') })


    member s.Unfocus () = 
        s.FocusedElementId |> Option.iter (
            fun id ->
                let cs = s.ElementStateById.[id]
                cs.Focused <- false
                cs.FocusedTime <- TimeSpan.Zero
        )
        s.FocusedElementId <- None

    member s.SetFocus (id: string) (time: TimeSpan)=
        s.Unfocus()

        s.FocusedElementId <- Some(id)
        let cs = s.ElementStateById.[id]
        cs.Focused <- true
        cs.FocusedTime <- time

        let setText (text: string) =
            let model =
                cs.Model
                |> Option.map(
                    fun model' ->
                        match model' with
                        | Textbox (model'') -> Textbox {model'' with Text = text}
                        | _ -> failwith "Not a textbox"
                )

            cs.Model <- model

        // Send open keyboard event.
        cs.Model |> Option.iter (
            function
            | Textbox (model') ->
                model'.OnOpenKeyboard(setText)
            | _ -> ()
        )
