namespace Noobish

[<RequireQualifiedAccess>]
type NoobishLayout = Default | Grid of cols: int * rows: int


[<RequireQualifiedAccess>]
type NoobishHorizontalTextAlign = Left | Right | Center

[<RequireQualifiedAccess>]
type NoobishVerticalTextAlign = Top | Bottom | Center


type NoobishTextureSize = Stretch | BestFitMax | BestFitMin | Original | Custom of width: int * height: int
