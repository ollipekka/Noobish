namespace Noobish


open Microsoft.Xna.Framework

open Noobish

open Noobish.Internal

open System
open System.Collections.Generic

[<Flags>]
[<Struct>]
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

    let inline isSet (self: NoobishElementState) (flag: NoobishElementState) =
        self &&& flag = flag

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

    let deselect s =
        s &&& ~~~NoobishElementState.Selected

    let enable s =
        s &&& ~~~NoobishElementState.Selected

    let disable s =
        s ||| NoobishElementState.Disabled

    let toggle s =
        s |||  NoobishElementState.Toggled

    let detoggle s =
        s &&& ~~~NoobishElementState.Toggled

type NoobishImage = {
    Texture: NoobishTextureId
    TextureEffect: NoobishTextureEffect
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

    KeyboardShortcut: NoobishKeyboardShortcut

    ConsumedMouseButtons: NoobishMouseButtonId[]
    ConsumedKeys: NoobishKeyId[]
    KeyTypedEnabled: bool
    OnClickInternal: NoobishLayoutElement -> GameTime -> unit
    OnPressInternal: Vector2 -> NoobishLayoutElement -> GameTime -> unit
    OnTextChange: string -> unit
    OnCheckBoxValueChange: bool -> unit

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

    KeyboardShortcut: NoobishKeyboardShortcut


    Children: string[]
} with
    member s.Disabled with get() = (NoobishElementState.isSet s.State NoobishElementState.Disabled)
    member s.Enabled with get() = not s.Disabled
    member s.Focused with get() =  (NoobishElementState.isSet s.State NoobishElementState.Focused)
    member s.Visible with get() = not ( (NoobishElementState.isSet s.State NoobishElementState.Hidden))
    member s.Toggled with get() =  (NoobishElementState.isSet s.State NoobishElementState.Toggled)
    member s.Hovered with get() =  (NoobishElementState.isSet s.State NoobishElementState.Hovered)
    member s.Selected with get() =  (NoobishElementState.isSet s.State NoobishElementState.Selected)
    member s.Pressed with get() =  (NoobishElementState.isSet s.State NoobishElementState.Pressed)
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
    | Toggle
    | SetScrollX of float32
    | SetScrollY of float32
    | ChangeModel of NoobishComponentModel
    | ChangeSliderValue of float32
    | InvokeAction of (unit -> unit)
    | InvokeTextChange of string
    | InvokeClick
    | InvokePress of Vector2
    | InvokeSliderValueChange of float32
    | InvokeCheckBoxValueChange

type NoobishState () =

    let elementsById = Dictionary<string, NoobishLayoutElement>()
    let elementStateById = Dictionary<string, NoobishLayoutElementState>()
    let tempElementStateById = Dictionary<string, NoobishLayoutElementState>()
    member val ElementsById = (elementsById :> IReadOnlyDictionary<string, NoobishLayoutElement>)
    member val ElementStateById = (elementStateById :> IReadOnlyDictionary<string, NoobishLayoutElementState>)
    member val FocusedElementId: Option<string> = None with get, set

    member val Events = Queue<struct(string*ComponentMessage)>()

    member s.Item
        with get (tid: string) = s.ElementStateById.[tid]

    member this.GetById (stateId: string) = 
        let mutable value = Unchecked.defaultof<NoobishLayoutElementState>
        let success = this.ElementStateById.TryGetValue(stateId, &value)
        if not success then failwith $"No such state: {stateId}"
        value


    member s.QueueEvent (cid: string) (message:ComponentMessage) =
        s.Events.Enqueue(struct(cid, message))

    member s.BeginUpdate() =
        for kvp in elementStateById do
            tempElementStateById.[kvp.Key] <- kvp.Value

        elementsById.Clear()
        elementStateById.Clear()

    member s.EndUpdate() =
        tempElementStateById.Clear()

    member s.UpdateState (e: NoobishLayoutElement) (state: NoobishElementState)  =
        elementsById.[e.Id] <- e

        let mutable es = Unchecked.defaultof<NoobishLayoutElementState>
        let success = tempElementStateById.TryGetValue(e.Id, &es)
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

    member s.ProcessEvents(gameTime: GameTime) =

        while s.Events.Count > 0 do
            let struct(cid, message) = s.Events.Dequeue()
            let mutable c = Unchecked.defaultof<NoobishLayoutElement>
            let success = s.ElementsById.TryGetValue(cid, &c)
            
            let mutable cs = Unchecked.defaultof<NoobishLayoutElementState>
            let successState= s.ElementStateById.TryGetValue(cid, &cs)
            if success && successState then
                match message with
                | Show ->
                    let es = elementStateById.[cid]
                    elementStateById.[cid] <- {es with State = NoobishElementState.show es.State }
                | Hide ->
                    let es = elementStateById.[cid]
                    elementStateById.[cid] <- {es with State = NoobishElementState.hide es.State}
                | ToggleVisibility ->

                    let es = elementStateById.[cid]
                    let state =
                        if NoobishElementState.isSet es.State NoobishElementState.Hidden then
                            NoobishElementState.show es.State
                        else
                            NoobishElementState.hide es.State

                    elementStateById.[cid] <- {es with State = state}

                | Toggle ->

                    let es = elementStateById.[cid]

                    let toggled = NoobishElementState.isSet es.State NoobishElementState.Toggled
                    let state =
                        if toggled then
                            NoobishElementState.detoggle es.State
                        else
                            NoobishElementState.toggle es.State
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
                    c.OnClickInternal c gameTime
                | InvokePress v ->
                    c.OnPressInternal v c gameTime
                | InvokeTextChange t ->
                    c.OnTextChange t
                | InvokeCheckBoxValueChange ->
                    let es = elementStateById.[cid]
                    c.OnCheckBoxValueChange es.Toggled
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
