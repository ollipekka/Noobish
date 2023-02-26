module Noobish.Cursor

open System
let fadeInterval = TimeSpan.FromSeconds 1.2
let fade (time: TimeSpan) =
    MathF.Pow(float32 (time.TotalSeconds % fadeInterval.TotalSeconds), 5f)

let blinkInterval = TimeSpan.FromSeconds 1.2
let blink (time: TimeSpan) =
    let remainder = time.TotalSeconds % blinkInterval.TotalSeconds
    if remainder < 0.5 then
        0f
    else
            1f


