module Noobish.Input

open System
open System.Collections.Generic




let private clamp n minVal maxVal = max (min n maxVal) minVal

let rec press
    (state: IReadOnlyDictionary<string, LayoutComponentState>)
    (components: LayoutComponent[])
    (time: TimeSpan)
    (positionX: float32)
    (positionY: float32)
    (scrollX: float32)
    (scrollY: float32) =
    for c in components do
        let cs = state.[c.Id]
        if c.Enabled && cs.State <> ComponentState.Toggled && c.Contains positionX positionY scrollX scrollY  then
            if c.Children.Length > 0 then
                press state c.Children time positionX positionY (scrollX + cs.ScrollX) (scrollY + cs.ScrollY)
            else
                let cs = state.[c.Id]
                cs.PressedTime <- time

let rec click
    (state: IReadOnlyDictionary<string, LayoutComponentState>)
    (components: LayoutComponent[])
    (time: TimeSpan)
    (positionX: float32)
    (positionY: float32)
    (scrollX: float32)
    (scrollY: float32) =

    for c in components do
        if c.Enabled && c.Contains positionX positionY scrollX scrollY then
            let cs = state.[c.Id]
            if c.Children.Length > 0 then
                click state c.Children time positionX positionY (scrollX + cs.ScrollX) (scrollY + cs.ScrollY)
            else
                let cs = state.[c.Id]
                cs.PressedTime <- time
                c.OnClick()


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
            if c.Children.Length > 0 then
                let handledByChild = scroll state c.Children positionX positionY scale time scrollX scrollY

                if handledByChild then
                    handled <- true

            let cs = state.[c.Id]

            if not handled then
                if c.ScrollHorizontal then
                    cs.ScrollX <- cs.ScrollX + scrollX
                    cs.ScrolledTime <- time
                    handled <- true
                if c.ScrollVertical then
                    let scaledScroll = scaleValue scrollY
                    let nextScroll = cs.ScrollY + scaledScroll
                    let minScroll = c.PaddedHeight - c.OverflowHeight

                    cs.ScrollY <- clamp nextScroll minScroll 0.0f
                    cs.ScrolledTime <- time
                    handled <- true
    handled