namespace Noobish

[<RequireQualifiedAccess>]
type NoobishLayout =
| Default
| Grid of cols: int * rows: int
| None

type NoobishTextAlign =
| TopLeft | TopCenter | TopRight
| Left  | Center | Right
| BottomLeft | BottomCenter | BottomRight

[<RequireQualifiedAccess>]
type NoobishTextureSize = Stretch | BestFitMax | BestFitMin | Original
