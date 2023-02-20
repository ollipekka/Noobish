namespace Noobish

[<RequireQualifiedAccess>]
type NoobishLayout =
| Default
| Grid of cols: int * rows: int
| OverlaySource
| Absolute
| None

[<RequireQualifiedAccess>]
type NoobishTextureSize = Stretch | BestFitMax | BestFitMin | Original

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

    type ComponentMessage =
        | Show
        | Hide
        | ToggleVisibility
        | SetScrollX of float32
        | SetScrollY of float32
        | ChangeModel of (NoobishComponentModel -> NoobishComponentModel)

    type ComponentChangeDispatch = (ComponentMessage -> unit)

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
        member val ElementsById = Dictionary<string, NoobishLayoutElementState>()
        member val TempElements = Dictionary<string, NoobishLayoutElementState>()
        member val FocusedElementId: Option<string> = None with get, set

        member private s.UpdateState (state: NoobishLayoutElementState) =
            s.ElementsById.[state.Id] <- state

        member s.Item
            with get (tid: string) = s.ElementsById.[tid]

        member s.Update (cid: string) (message:ComponentMessage) =
            let (success, cs) = s.ElementsById.TryGetValue(cid)
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
                        let cs: NoobishLayoutElementState = s.ElementsById.[cid]
                        s.UpdateState {cs with Model = Some(model') })

        member s.SetFocus (id: string) (time: TimeSpan)=
            s.FocusedElementId |> Option.iter (
                fun id ->
                    let cs = s.ElementsById.[id]
                    cs.Focused <- false
                    cs.FocusedTime <- TimeSpan.Zero
            )
            if id <> "" then

                s.FocusedElementId <- Some(id)
                let cs = s.ElementsById.[id]
                cs.Focused <- true
                cs.FocusedTime <- time

                cs.Model <-
                    cs.Model
                    |> Option.map(
                        fun model' ->
                            match model' with
                            | Textbox (model'') -> Textbox {model'' with Text = ""; Cursor = 0}
                            | _ -> model'
                    )


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
            else
                s.FocusedElementId <- None


    let pi = float32 System.Math.PI
    let clamp n minVal maxVal = max (min n maxVal) minVal
    let inline toDegrees angle = (float32 angle) * 180.0f / pi
    let inline toRadians angle = (float32 angle) * pi / 180.0f