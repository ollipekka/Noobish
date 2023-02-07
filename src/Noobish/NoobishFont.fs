namespace Noobish

open System.Collections.Generic
open Microsoft.Xna.Framework.Graphics

type NoobishFontAtlas = {
    FontType: string
    DistanceRange: float32
    Size: float32
    Width: int
    Height: int
    yOrigin: string
}

type NoobishFontMetrics = {
    EmSize: int32
    LineHeight: float32
    Ascender: float32
    Descender: float32
    UnderlineY: float32
    UnderlineThickness: float32
}

type NoobishGlyph = {
    Unicode: int64
    Advance: float32
    AtlasBounds: struct(float32*float32*float32*float32)
    PlaneBounds: struct(float32*float32*float32*float32)
}

type NoobishFont = {
    Atlas: NoobishFontAtlas
    Metrics: NoobishFontMetrics
    Glyphs: IReadOnlyDictionary<int64, NoobishGlyph>
    Kerning: IReadOnlyDictionary<int64, IReadOnlyDictionary<int64, float32>>
    Texture: Texture2D
}