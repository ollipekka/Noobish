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

module NoobishGlyph =
    let getTextureCoordinates (textureWidth: float32) (textureHeight: float32) (glyph: NoobishGlyph) =
        let struct(top, right, bottom, left) = glyph.AtlasBounds
        let u = left / textureWidth
        let u2 = right / textureWidth
        let v = 1f - top / textureHeight
        let v2 = v  + (top - bottom) / textureHeight
        struct(u, u2, v, v2)

    let getGlyphMetricsInPx (size: float32) (glyph: NoobishGlyph) =
        let struct(top, right, bottom, left) = glyph.PlaneBounds

        let advance = glyph.Advance * size
        let xOffset = left * size
        let yOffset = bottom * size

        let width = (right - left) * size
        let height = (top - bottom) * size

        struct(advance, xOffset, yOffset, width, height)


    let getSize (scale: float32) (glyph: NoobishGlyph) =
        let struct(top, right, bottom, left) = glyph.AtlasBounds

        struct((right - left) * scale, (top - bottom) * scale)


type NoobishFont = {
    Atlas: NoobishFontAtlas
    Metrics: NoobishFontMetrics
    Glyphs: IReadOnlyDictionary<int64, NoobishGlyph>
    Kerning: IReadOnlyDictionary<int64, IReadOnlyDictionary<int64, float32>>
    Texture: Texture2D
}

module NoobishFont =

    let measureText (font: NoobishFont) (size: float32) (text: string) =
        let mutable width = 0f
        let mutable height = font.Metrics.LineHeight * size

        for c in text do
            let glyph = font.Glyphs.[int64 c]
            let struct(advance, xOffset, yOffset, glyphWidth, glyphHeight) = NoobishGlyph.getGlyphMetricsInPx size glyph

            width <- width + advance + xOffset
        struct(width, height)

type TextBatch (graphics: GraphicsDevice, effect: Effect, batchSize: int) =

    let mutable vertexCount = 0
    let vertices = Array.create batchSize (VertexPositionTexture())
    let indices = Array.create (batchSize * 6) (int16 0)


    let addVertex (v:Vector3) (t: Vector2) =
        vertices.[vertexCount] <- VertexPositionTexture(v, t)
        vertexCount <- vertexCount + 1

    let addDegenerate () =
        let v = vertices.[vertexCount - 1]
        addVertex v.Position v.TextureCoordinate

    member val World = Matrix.Identity
    member val View = Matrix.Identity

    member val Projection =
        let vp = graphics.Viewport
        #if PSM || DIRECTX
        Matrix.CreateOrthographicOffCenter(0.0f, float32 vp.Width, float32 vp.Height, 0.0f, -1.0f, 0.0f)
        #else
        Matrix.CreateOrthographicOffCenter(0.0f, float32 vp.Width, float32 vp.Height, 0.0f, 0.0f, 1.0f)
        #endif

    member s.Flush () =
        if vertexCount > 0 then

            for pass in effect.CurrentTechnique.Passes do
                pass.Apply()
                graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertexCount - 2)

            vertexCount <- 0

    member s.DrawGlyph (font: NoobishFont) (center: Vector2) (halfSize: Vector2) (glyph: NoobishGlyph) =

        if vertexCount + 4 > vertices.Length then
            s.Flush()

        let halfWidth = halfSize.X
        let halfHeight = halfSize.Y

        let struct(u, u2, v, v2) = NoobishGlyph.getTextureCoordinates (float32 font.Texture.Width) (float32 font.Texture.Height) glyph

        let t1 = Vector2(u, v)
        let p1 = center + Vector2(-halfWidth, -halfHeight)
        let t2 = Vector2(u2, v)
        let p2 = center + Vector2(halfWidth, -halfHeight)
        let t3 = Vector2(u, v2)
        let p3 = center + Vector2(-halfWidth, halfHeight)
        let t4 = Vector2(u2, v2)
        let p4 = center + Vector2(halfWidth, halfHeight)

        let layer = 0f

        if(vertexCount > 0) then
            addVertex (Vector3(p1.X, p1.Y, layer)) (Vector2(u2, v2))


        addVertex (Vector3(p1.X, p1.Y, layer)) t1
        addVertex (Vector3(p2.X, p2.Y, layer)) t2
        addVertex (Vector3(p3.X, p3.Y, layer)) t3
        addVertex (Vector3(p4.X, p4.Y, layer)) t4


        addDegenerate()



    member s.Draw (font: NoobishFont) (sizeInPt: int) (position: Vector2) (color: Color) (text:string) =

        let size = float32 sizeInPt * 4f / 3f / float32 font.Metrics.EmSize // Size in PX


        let wvp = s.World * s.View * s.Projection
        effect.Parameters["WorldViewProjection"].SetValue(wvp)
        effect.Parameters["GlyphTexture"].SetValue(font.Texture)
        effect.Parameters["PxRange"].SetValue(2f)

        let atlasSize = Vector2(float32 font.Texture.Width, float32 font.Texture.Height)
        effect.Parameters["TextureSize"].SetValue(atlasSize)
        effect.Parameters["ForegroundColor"].SetValue(color.ToVector4())
        effect.CurrentTechnique <- if sizeInPt > 10 then effect.Techniques["LargeText"] else effect.Techniques["SmallText"]

        let mutable nextPosX = position.X
        for c in text do
            let glyph = font.Glyphs.[int64(c)]

            let struct(advance, xOffset, yOffset, glyphWidth, glyphHeight) = NoobishGlyph.getGlyphMetricsInPx size glyph

            let x = nextPosX + xOffset
            let y = position.Y + (size - glyphHeight) - yOffset

            let glyphHalfSize = Vector2(glyphWidth / 2f, glyphHeight / 2f)
            let position = Vector2(x, y) + glyphHalfSize

            s.DrawGlyph font position glyphHalfSize glyph

            nextPosX <- x + advance

        s.Flush()


    interface System.IDisposable with
        member s.Dispose() =
            effect.Dispose()