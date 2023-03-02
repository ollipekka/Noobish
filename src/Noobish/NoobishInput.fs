module Noobish.Input

open System
open System.Collections.Generic

open Noobish.Internal
open Noobish.Styles
open Microsoft.Xna.Framework.Content

let rec press
    (version: Guid)
    (state: NoobishState)
    (elements: NoobishLayoutElement[])
    (time: TimeSpan)
    (positionX: float32)
    (positionY: float32)
    (scrollX: float32)
    (scrollY: float32) =
    let mutable handled = false
    let mutable i = 0

    while not handled && i < elements.Length do
        let c = elements.[i]
        let cs = state.ElementStateById.[c.Id]
        if cs.Version <> version then failwith "Version mismatch!"
        if c.Enabled && cs.Visible && (not cs.Toggled) && c.Contains positionX positionY scrollX scrollY  then
            let handledByChild =
                if c.Children.Length > 0 then
                    press version state c.Children time positionX positionY (scrollX + cs.ScrollX) (scrollY + cs.ScrollY)
                else
                    false
            if not handledByChild then
                let cs = state.ElementStateById.[c.Id]
                cs.PressedTime <- time
                handled <- true

                c.OnPressInternal (struct(int positionX, int positionY)) c

            else
                handled <- true

        i <- i + 1
    handled

let rec click
    (version: Guid)
    (content: ContentManager)
    (state: NoobishState)
    (styles: NoobishStyleSheet)
    (elements: NoobishLayoutElement[])
    (time: TimeSpan)
    (positionX: float32)
    (positionY: float32)
    (scrollX: float32)
    (scrollY: float32) =

    let mutable handled = false
    let mutable i = 0

    while not handled && i < elements.Length do
        let c = elements.[i]
        let cs = state.ElementStateById.[c.Id]
        if cs.Version <> version then failwith "Version mismatch!"
        if c.Enabled && cs.Visible && c.Contains positionX positionY scrollX scrollY then

            let handledByChild =
                if c.Children.Length > 0 then
                    click version content state styles c.Children time positionX positionY (scrollX + cs.ScrollX) (scrollY + cs.ScrollY)
                else
                    false
            if not handledByChild then

                cs.PressedTime <- time
                printfn $"Pressed time set for %s{c.ThemeId} new time %A{time}."

                if cs.CanFocus && not cs.Focused then
                    state.SetFocus cs.Id time

                    cs.Model
                        |> Option.map(fun m ->
                            match m with
                            | Textbox (m') ->
                                let bounds = c.Content

                                let fontId = styles.GetFont c.ThemeId "default"
                                let font = content.Load<NoobishFont> fontId
                                let fontSize = styles.GetFontSize c.ThemeId "default"
                                let cursorIndex = NoobishFont.calculateCursorIndex font fontSize false bounds scrollX scrollY c.TextAlignment positionX positionY m'.Text
                                Textbox {m' with Cursor = cursorIndex }
                            | _ -> m
                        ) |> Option.iter (fun m ->
                            state.Update c.Id (ChangeModel (fun _ -> m))
                        )

                else
                    c.OnClickInternal c

                handled <- true
            else
                handled <- true



        i <- i + 1
    handled

let rec keyTyped
    (version: Guid)
    (state: NoobishState)
    (elements: NoobishLayoutElement[])
    (typed: char): bool =

    let mutable handled = false
    let mutable i = 0

    let focusedElementId = state.FocusedElementId |> Option.defaultValue ""

    while not handled && i < elements.Length do
        let e = elements.[i]
        let es = state.ElementStateById.[e.Id]

        if es.Version <> version then failwith "Version mismatch!"
        if e.Id = focusedElementId then
            let model'' = es.Model |> Option.map (
                fun model' ->
                    match model' with
                    | Textbox model'' ->
                        let (text, cursor) =
                            if int typed = 8 then // backspace
                                if model''.Text.Length > 0 && model''.Cursor > 0 then
                                    model''.Text.Remove(model''.Cursor - 1, 1), model''.Cursor - 1
                                else
                                    "", 0
                            elif int typed = 13 then
                                state.Unfocus()
                                model''.Text, 0
                            elif int typed = 127 then // deleted
                                if model''.Text.Length > 0 && model''.Cursor < model''.Text.Length - 1 then
                                    model''.Text.Remove(model''.Cursor, 1), model''.Cursor
                                else
                                    "", 0
                            else
                                model''.Text.Insert(model''.Cursor, typed.ToString()), model''.Cursor + 1
                        Textbox {model'' with Text = text; Cursor = cursor}
                    | _ -> model'
            )
            model'' |> Option.iter (fun m ->
                state.Update focusedElementId (ChangeModel (fun _ -> m))
            )
            handled <- true
        else
            handled <- keyTyped version state e.Children typed

        i <- i + 1
    handled

let rec scroll
    (version: Guid)
    (state: IReadOnlyDictionary<string, NoobishLayoutElementState>)
    (elements: NoobishLayoutElement[])
    (positionX: float32)
    (positionY: float32)
    (scale: float32)
    (time: TimeSpan)
    (scrollX: float32)
    (scrollY: float32): bool =

    let scaleValue v = v * scale

    let mutable handled = false;
    for c in elements do
        let cs = state.[c.Id]
        if cs.Version <> version then failwith "Version mismatch!"
        if c.Enabled && cs.Visible && c.Contains positionX positionY scrollX scrollY then

            let handledByChild =
                if c.Children.Length > 0 then
                    scroll version state c.Children positionX positionY scale time scrollX scrollY
                else
                    false


            if not handledByChild then
                if c.ScrollHorizontal && c.OverflowWidth > c.Width then
                    cs.ScrollX <- cs.ScrollX + scrollX
                    cs.ScrolledTime <- time
                    handled <- true
                if c.ScrollVertical && c.OverflowHeight > c.Height then
                    let scaledScroll = scaleValue scrollY
                    let nextScroll = cs.ScrollY + scaledScroll
                    let minScroll = c.ContentHeight - c.OverflowHeight

                    cs.ScrollY <- clamp nextScroll minScroll 0.0f
                    cs.ScrolledTime <- time
                    handled <- true
            else
                handled <- true
    handled

