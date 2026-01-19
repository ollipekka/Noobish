module Noobish.Cursor

open System
let blinkInterval = TimeSpan.FromSeconds 1.2
let blink (time: TimeSpan) =
    let remainder = time.TotalSeconds % blinkInterval.TotalSeconds
    let visibleWindow = blinkInterval.TotalSeconds * 0.6
    if remainder < visibleWindow then
        0f
    else
        1f

