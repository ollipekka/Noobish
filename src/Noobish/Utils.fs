module Noobish.Utils


let pi = float32 System.Math.PI
let clamp n minVal maxVal = max (min n maxVal) minVal
let inline toDegrees angle = (float32 angle) * 180.0f / pi
let inline toRadians angle = (float32 angle) * pi / 180.0f