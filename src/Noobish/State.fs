namespace Noobish


open Microsoft.Xna.Framework

open Noobish

open Noobish.Internal

open System
open System.Collections.Generic

[<Flags>]
[<RequireQualifiedAccess>]
type NoobishElementState =
| Empty    =  0
| Selected =  1
| Toggled  =  2
| Hovered  =  4
| Pressed  =  8
| Disabled = 16
| Hidden   = 32
| Focused  = 64

module NoobishElementState =

    let hide s =
        s ||| NoobishElementState.Hidden

    let show s =
        s &&& ~~~NoobishElementState.Hidden

    let focus s =
        s ||| NoobishElementState.Focused

    let unfocus s =
        s &&& ~~~NoobishElementState.Focused

    let select s =
        s ||| NoobishElementState.Selected

    let unselect s =
        s &&& ~~~NoobishElementState.Selected

    let enable s =
        s &&& ~~~NoobishElementState.Selected

    let disable s =
        s ||| NoobishElementState.Disabled

    let toggle s =
        s |||  NoobishElementState.Toggled

    let untoggle s =
        s &&& ~~~NoobishElementState.Toggled

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
    OnPressInternal: Vector2 -> NoobishLayoutElement -> unit
    OnTextChange: string -> unit

    Layout: NoobishLayout
    ColSpan: int
    RowSpan: int

    Children: NoobishLayoutElement[]
} with
    member l.MarginVertical with get() = l.MarginTop + l.MarginBottom
    member l.MarginHorizontal with get() = l.MarginLeft + l.MarginRight
    member l.PaddingVertical with get() = l.PaddingTop + l.PaddingBottom
    member l.PaddingHorizontal with get() = l.PaddingRight+ l.PaddingLeft

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

    State: NoobishElementState

    mutable FocusedTime: TimeSpan
    mutable PressedTime: TimeSpan
    mutable ScrolledTime: TimeSpan

    mutable ScrollX: float32
    mutable ScrollY: float32

    mutable Model: option<NoobishComponentModel>

    KeyboardShortcut: NoobishKeyId


    Children: string[]
} with
    member s.Disabled with get() = s.State.HasFlag NoobishElementState.Disabled
    member s.Enabled with get() = not s.Disabled
    member s.Focused with get() = (s.State.HasFlag NoobishElementState.Focused)
    member s.Visible with get() = not (s.State.HasFlag NoobishElementState.Hidden)
    member s.Toggled with get() = (s.State.HasFlag NoobishElementState.Toggled)
    member s.Pressed with get() = (s.State.HasFlag NoobishElementState.Pressed)
    member s.CanFocus with get() =
        match s.Model with
        | Some(model') ->
            match model' with
            | Textbox (_) -> true
            | _ -> false
        | None -> false

type NoobishId = | NoobishId of string

type ComponentMessage =
    | Show
    | Hide
    | ToggleVisibility
    | SetScrollX of float32
    | SetScrollY of float32
    | ChangeModel of NoobishComponentModel
    | ChangeSliderValue of float32
    | InvokeAction of (unit -> unit)
    | InvokeTextChange of string
    | InvokeClick
    | InvokePress of Vector2
    | InvokeSliderValueChange of float32

type NoobishState () =

    let elementsById = Dictionary<string, NoobishLayoutElement>()
    let elementStateById = Dictionary<string, NoobishLayoutElementState>()
    member val ElementsById = (elementsById :> IReadOnlyDictionary<string, NoobishLayoutElement>)
    member val ElementStateById = (elementStateById :> IReadOnlyDictionary<string, NoobishLayoutElementState>)
    member val FocusedElementId: Option<string> = None with get, set

    member val Events = Queue<struct(string*ComponentMessage)>()

    member s.Item
        with get (tid: string) = s.ElementStateById.[tid]

    member s.QueueEvent (cid: string) (message:ComponentMessage) =
        s.Events.Enqueue(struct(cid, message))

    member s.UpdateState (e: NoobishLayoutElement) (state: NoobishElementState)  =
        elementsById.[e.Id] <- e

        let success, es = s.ElementStateById.TryGetValue e.Id
        if success then
            elementStateById.[e.Id] <- { es with Model = e.Model; State = state }
        else
            elementStateById.[e.Id] <-
                {
                    Id = e.Id
                    ParentId = e.ParentId
                    State  = state
                    FocusedTime = TimeSpan.FromDays(-1)
                    PressedTime = TimeSpan.FromDays(-1)
                    ScrolledTime = TimeSpan.FromDays(-1)

                    ScrollX = 0.0f
                    ScrollY = 0.0f

                    KeyboardShortcut = e.KeyboardShortcut
                    Model = e.Model

                    Children = e.Children |> Array.map(fun child -> child.Id)
                }

    member s.ProcessEvents() =

        while s.Events.Count > 0 do
            let struct(cid, message) = s.Events.Dequeue()
            let (success, c) = s.ElementsById.TryGetValue cid
            let (successState, cs) = s.ElementStateById.TryGetValue cid
            if success && successState then
                match message with
                | Show ->
                    let es = elementStateById.[cid]
                    elementStateById.[cid] <- {es with State = es.State &&& ~~~NoobishElementState.Hidden }
                | Hide ->
                    let es = elementStateById.[cid]
                    elementStateById.[cid] <- {es with State = es.State &&& NoobishElementState.Hidden }
                | ToggleVisibility ->

                    let es = elementStateById.[cid]
                    let state =
                        if (es.State.HasFlag NoobishElementState.Hidden) then
                            (es.State &&& ~~~NoobishElementState.Hidden)
                        else
                            (es.State &&& NoobishElementState.Hidden)

                    elementStateById.[cid] <- {es with State = state}
                | SetScrollX (v) ->
                    cs.ScrollX <- v
                | SetScrollY(v) ->
                    cs.ScrollY <- v
                | ChangeModel(m') ->
                    let es = elementStateById.[cid]
                    elementStateById.[cid] <- {es with Model = Some(m') }
                | ChangeSliderValue(v) ->

                    let e = elementsById.[cid]
                    let es = elementStateById.[cid]

                    let m = es.Model|> Option.map(
                            function
                            | Slider(s) ->
                                Slider {s with Value = v}
                            | _ -> failwith "Not a slider.")
                    elementStateById.[cid] <- {es with Model = m}

                    s.QueueEvent cid (InvokeSliderValueChange v)
                | InvokeAction (action) ->
                    action()
                | InvokeClick  ->
                    c.OnClickInternal c
                | InvokePress v ->
                    c.OnPressInternal v c
                | InvokeTextChange t ->
                    c.OnTextChange t
                | InvokeSliderValueChange v ->
                    let c = s.ElementsById[cid]
                    c.Model
                        |> Option.iter (fun m ->
                            match m with
                            | Slider(m') -> m'.OnValueChanged v
                            | _ -> ()
                        )

        s.Events.Clear()

    member s.Unfocus () =
        s.FocusedElementId |> Option.iter (
            fun id ->

                let c = s.ElementsById.[id]
                let cs = s.ElementStateById.[id]

                cs.Model
                    |> Option.iter (fun m ->
                        match m with
                        | Textbox m' ->
                            s.QueueEvent id (InvokeTextChange m'.Text)
                        | _ -> ()
                    )

                let cs = s.ElementStateById.[id]
                cs.FocusedTime <- TimeSpan.Zero

                elementStateById.[id] <- {cs with State = NoobishElementState.unfocus cs.State; FocusedTime = TimeSpan.Zero}
        )
        s.FocusedElementId <- None


    member s.SetFocus (id: string) (time: TimeSpan) =
        s.Unfocus()

        s.FocusedElementId <- Some(id)
        let cs = s.ElementStateById.[id]

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

        elementStateById.[id] <- {cs with State = NoobishElementState.focus cs.State; FocusedTime = time}
