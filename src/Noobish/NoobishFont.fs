namespace Noobish

open System.Collections.Generic
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework

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


type TextBatch (batchSize: int) =

    member val Vertices = Array.create batchSize (VertexPositionColorTexture())
    member s.BatchSize with get() = s.Vertices.Length



    member s.Draw (spriteBatch: SpriteBatch) (text:string) (font: NoobishFont) (position: Vector2) =

        let mutable nextPosX = position.X
        for c in text do
            let glyph = font.Glyphs.[int64(c)]

            let sourceRect =
                let struct(top, right, bottom, left) = glyph.AtlasBounds
                let width = int (right - left)
                let height = int (top - bottom)
                Rectangle(
                    int left,
                    font.Atlas.Height - height - int bottom,
                    int width,
                    int height
                )
            let struct(oTop, oRight, oBottom, oLeft) = glyph.PlaneBounds

            let x = oLeft + nextPosX
            let y = position.Y + oBottom

            spriteBatch.Draw(font.Texture, Rectangle(int(x), int(y), sourceRect.Width, sourceRect.Height), sourceRect, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f)

            nextPosX <- x + float32 (sourceRect.Width) + glyph.Advance




