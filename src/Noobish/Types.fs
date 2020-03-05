namespace Noobish

[<RequireQualifiedAccess>]
type NoobishLayout = Default | Grid of cols: int * rows: int


type NoobishTextAlign =
    | TopLeft | TopCenter | TopRight
    | Left  | Center | Right
    | BottomLeft | BottomCenter | BottomRight

type NoobishTextureSize = Stretch | BestFitMax | BestFitMin | Original | Custom of width: int * height: int
