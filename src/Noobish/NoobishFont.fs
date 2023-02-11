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


type TextBatch (graphics: GraphicsDevice, batchSize: int) =

    let mutable vertexCount = 0
    let vertices = Array.create batchSize (VertexPositionColorTexture())


    let effect = new BasicEffect(graphics)

    let addVertex (v:Vector2) (c:Color) (t: Vector2) =
        vertices.[vertexCount] <- VertexPositionColorTexture(Vector3(v.X, v.Y, 0.0f), c, t)
        vertexCount <- vertexCount + 1

    let addDegenerate () =
        let v = vertices.[vertexCount - 1]
        addVertex (Vector2(v.Position.X, v.Position.Y)) Color.Pink v.TextureCoordinate


    let flush () =
        if vertexCount > 0 then
            effect.VertexColorEnabled <- true
            effect.TextureEnabled <- true
            for pass in effect.CurrentTechnique.Passes do
                pass.Apply()
                graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertexCount - 2)

            vertexCount <- 0


    member s.Glyph (center: Vector2) (halfSize: Vector2) (color: Color) (glyph: NoobishGlyph) =

        if vertexCount + 8 > vertices.Length then
            flush()

        let startPos = center - halfSize
        let endPos = center + halfSize

        let textureWidth = halfSize.X * 2f
        let textureHeight = halfSize.Y * 2f

        let struct(top, right, bottom, left) = glyph.AtlasBounds
        let u = left / textureWidth
        let u2 = right / textureWidth
        let v = bottom / textureHeight
        let v2 = top / textureHeight


        if(vertexCount > 0) then
            addVertex (Vector2(startPos.X, startPos.Y)) Color.Pink (Vector2(u, v))

        addVertex (Vector2(startPos.X, startPos.Y)) color (Vector2(u, v))
        addVertex (Vector2(endPos.X, startPos.Y)) color (Vector2(u, v2))
        addVertex (Vector2(startPos.X, endPos.Y)) color (Vector2(u2, v))
        addVertex (Vector2(endPos.X, endPos.Y)) color (Vector2(u2, v2))

        addDegenerate()



    member s.Draw (text:string) (font: NoobishFont) (position: Vector2) =

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

            let xOffset = oLeft * font.Atlas.Size
            let yOffset = oBottom * font.Atlas.Size
            let y = position.Y + (font.Metrics.LineHeight * font.Atlas.Size - float32 sourceRect.Height) - yOffset


            s.Glyph (Vector2(x, y)) (Vector2(float32 sourceRect.Width / 2f, float32 sourceRect.Height / 2f)) Color.White glyph
            //spriteBatch.Draw(font.Texture, Rectangle(int(x + xOffset), int(y), sourceRect.Width, sourceRect.Height), sourceRect, Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.0f)

            let advance = glyph.Advance * font.Atlas.Size / float32 font.Metrics.EmSize
            nextPosX <- x + advance

        flush()


