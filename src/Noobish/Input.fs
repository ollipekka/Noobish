module Noobish.Input

open System
open System.Collections.Generic

open Noobish.Utils

let rec press
    (state: IReadOnlyDictionary<string, LayoutComponentState>)
    (components: LayoutComponent[])
    (time: TimeSpan)
    (positionX: float32)
    (positionY: float32)
    (scrollX: float32)
    (scrollY: float32) =
    let mutable handled = false
    let mutable i = 0

    while not handled && i < components.Length do
        let c = components.[i]
        let cs = state.[c.Id]
        if c.Enabled && cs.State <> ComponentState.Toggled && c.Contains positionX positionY scrollX scrollY  then
            let handledByChild =
                if c.Children.Length > 0 then
                    press state c.Children time positionX positionY (scrollX + cs.ScrollX) (scrollY + cs.ScrollY)
                else
                    false
            if not handledByChild then
                let cs = state.[c.Id]
                cs.PressedTime <- time
                handled <- true

                match c.Slider with
                | Some (slider') ->
                    let bounds = c.RectangleWithPadding
                    let relative = (positionX - bounds.X) / (bounds.Width)
                    let newValue = slider'.Min + (relative * slider'.Max - slider'.Min)
                    slider'.OnValueChanged (clamp newValue slider'.Min slider'.Max)
                | None -> ()
            else
                handled <- true

        i <- i + 1
    handled
let rec click
    (state: IReadOnlyDictionary<string, LayoutComponentState>)
    (components: LayoutComponent[])
    (time: TimeSpan)
    (positionX: float32)
    (positionY: float32)
    (scrollX: float32)
    (scrollY: float32) =

    let mutable handled = false
    let mutable i = 0

    while not handled && i < components.Length do
        let c = components.[i]
        if c.Enabled && c.Contains positionX positionY scrollX scrollY then
            let cs = state.[c.Id]
            let handledByChild =
                if c.Children.Length > 0 then
                    click state c.Children time positionX positionY (scrollX + cs.ScrollX) (scrollY + cs.ScrollY)
                else
                    false
            if not handledByChild then
                let cs = state.[c.Id]
                cs.PressedTime <- time
                c.OnClick()
                handled <- true
            else
                handled <- true

        i <- i + 1
    handled

let rec scroll
    (state: IReadOnlyDictionary<string, LayoutComponentState>)
    (components: LayoutComponent[])
    (positionX: float32)
    (positionY: float32)
    (scale: float32)
    (time: TimeSpan)
    (scrollX: float32)
    (scrollY: float32): bool =

    let scaleValue v = v * scale

    let mutable handled = false;
    for c in components do
        if c.Enabled && c.Contains positionX positionY scrollX scrollY then

            let handledByChild =
                if c.Children.Length > 0 then
                    scroll state c.Children positionX positionY scale time scrollX scrollY
                else
                    false

            let cs = state.[c.Id]

            if not handledByChild then
                if c.ScrollHorizontal && c.OverflowWidth > c.Width then
                    cs.ScrollX <- cs.ScrollX + scrollX
                    cs.ScrolledTime <- time
                    handled <- true
                if c.ScrollVertical && c.OverflowHeight > c.Height then
                    let scaledScroll = scaleValue scrollY
                    let nextScroll = cs.ScrollY + scaledScroll
                    let minScroll = c.PaddedHeight - c.OverflowHeight

                    cs.ScrollY <- clamp nextScroll minScroll 0.0f
                    cs.ScrolledTime <- time
                    handled <- true
            else
                handled <- true
    handled