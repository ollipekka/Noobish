namespace Noobish

open System
open System.Collections.Generic

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

module internal TextBatchHelpers =
    type TextRenderInfo =
        { Size: float32
          Technique: string
          Position: Vector2
          AtlasSize: Vector2
          ForegroundColor: Vector4
          PxRange: float32
          WorldViewProjection: Matrix }

    let resolveTextRenderInfo
        (font: NoobishFont)
        (sizeInPt: int)
        (position: Vector2)
        (color: Color)
        (textureWidth: int)
        (textureHeight: int)
        (worldViewProjection: Matrix) =
        let size = float32 sizeInPt * 4f / 3f / float32 font.Metrics.EmSize
        let technique = if size > 10.0f then "LargeText" else "SmallText"
        let atlasSize = Vector2(float32 textureWidth, float32 textureHeight)
        let position = position + Vector2(0f, font.Metrics.Descender * size)
        { Size = size
          Technique = technique
          Position = position
          AtlasSize = atlasSize
          ForegroundColor = color.ToVector4()
          PxRange = float32 font.Atlas.DistanceRange
          WorldViewProjection = worldViewProjection }

    let iterateGlyphPlacements
        (font: NoobishFont)
        (size: float32)
        (position: Vector2)
        (text: ReadOnlySpan<char>)
        (handler: Vector2 -> Vector2 -> NoobishGlyph -> unit) =
        let mutable nextPosX = position.X
        for i = 0 to text.Length - 1 do
            let c = text.[i]
            let glyph = NoobishFont.getGlyph font c

            let struct(advance, xOffset, yOffset, glyphWidth, glyphHeight) =
                NoobishGlyph.getGlyphMetricsInPx size glyph

            let kern =
                if i + 1 < text.Length then
                    glyph.Kerning.GetValueOrDefault (text.[i + 1], 0f) * size
                else
                    0f

            let x = nextPosX + xOffset
            let y = position.Y + (size * font.Metrics.LineHeight - glyphHeight) - yOffset

            let glyphHalfSize = Vector2(glyphWidth / 2f, glyphHeight / 2f)
            let center = Vector2(x, y) + glyphHalfSize

            handler center glyphHalfSize glyph

            nextPosX <- nextPosX + advance + kern

    let iterateMultiLineSegments
        (font: NoobishFont)
        (size: float32)
        (maxWidth: float32)
        (text: string)
        (handler: int -> int -> Vector2 -> unit) =
        let mutable nextPosX = 0f
        let mutable nextPosY = 0f
        let mutable i = 0

        while i < text.Length do
            let struct(wsWidth, wsNewLinePos, wsCount) = NoobishFont.measureLeadingWhiteSpace font size text i

            if wsNewLinePos <> -1 then
                nextPosX <- 0.0f
                nextPosY <- nextPosY + font.Metrics.LineHeight * size
                i <- i + wsCount
            else
                let struct(wordWidth, wordCount) = NoobishFont.measureNextWord font size text (i + wsCount)

                if nextPosX + wsWidth + wordWidth > maxWidth then
                    if nextPosX <= Single.Epsilon then
                        failwith "Word is larger than line width. Use smaller font."
                    nextPosX <- 0.0f
                    nextPosY <- nextPosY + font.Metrics.LineHeight * size
                else
                    let struct(startPos, endPos, adjustedWidth) =
                        if nextPosX < Single.Epsilon && wsCount > 0 then
                            struct(i + wsCount, i + wsCount + wordCount, 0f)
                        else
                            struct(i, i + wsCount + wordCount, wsWidth)

                    let length = endPos - startPos
                    if length > 0 then
                        handler startPos length (Vector2(nextPosX, nextPosY))

                    nextPosX <- nextPosX + adjustedWidth + wordWidth
                    i <- endPos


[<Struct>]
type NoobishMonoGameFont = {
    Font: NoobishFont
    Texture: Texture2D
}

type TextBatch (graphics: GraphicsDevice, resolution: struct(int*int), effect: Effect, batchSize: int) =

    let mutable vertexCount = 0
    let vertices = Array.create batchSize (VertexPositionTexture())

    let addVertex (v: Vector3) (t: Vector2) =
        vertices.[vertexCount] <- VertexPositionTexture(v, t)
        vertexCount <- vertexCount + 1

    let addDegenerate () =
        let v = vertices.[vertexCount - 1]
        addVertex v.Position v.TextureCoordinate

    member val World = Matrix.Identity
    member val View = Matrix.Identity

    member val Projection =
        let struct(screenWidth, screenHeight) = resolution
        Matrix.CreateOrthographicOffCenter(0.0f, float32 screenWidth, float32 screenHeight, 0.0f, 0.0f, -1.0f)

    member s.Flush () =
        if vertexCount > 0 then
            graphics.SamplerStates.[0] <- SamplerState.LinearClamp
            graphics.BlendState <- BlendState.AlphaBlend
            graphics.DepthStencilState <- DepthStencilState.None
            for pass in effect.CurrentTechnique.Passes do
                pass.Apply()
                graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertexCount - 2)
            vertexCount <- 0

    member s.DrawGlyph (font: NoobishMonoGameFont) (center: Vector2) (halfSize: Vector2) (layer: float32) (glyph: NoobishGlyph) =
        if vertexCount + 6 > vertices.Length then
            s.Flush()

        let halfWidth = halfSize.X
        let halfHeight = halfSize.Y

        let struct(u, u2, v, v2) =
            NoobishGlyph.getTextureCoordinates (float32 font.Texture.Width) (float32 font.Texture.Height) glyph

        let t1 = Vector2(u, v)
        let p1 = center + Vector2(-halfWidth, -halfHeight)
        let t2 = Vector2(u2, v)
        let p2 = center + Vector2(halfWidth, -halfHeight)
        let t3 = Vector2(u, v2)
        let p3 = center + Vector2(-halfWidth, halfHeight)
        let t4 = Vector2(u2, v2)
        let p4 = center + Vector2(halfWidth, halfHeight)

        if vertexCount > 0 then
            addVertex (Vector3(p1.X, p1.Y, layer)) (Vector2(u2, v2))

        addVertex (Vector3(p1.X, p1.Y, layer)) t1
        addVertex (Vector3(p2.X, p2.Y, layer)) t2
        addVertex (Vector3(p3.X, p3.Y, layer)) t3
        addVertex (Vector3(p4.X, p4.Y, layer)) t4

        addDegenerate()

    member s.DrawSubstring
        ((font: NoobishMonoGameFont), (size: float32), (position: Vector2), (layer: float32), (text: ReadOnlySpan<char>)) =
        TextBatchHelpers.iterateGlyphPlacements font.Font size position text (fun center halfSize glyph ->
            s.DrawGlyph font center halfSize layer glyph)

    member s.DrawSingleLine
        (font: NoobishMonoGameFont)
        (size: int)
        (position: Vector2)
        (layer: float32)
        (color: Color)
        (text: string) =

        let fontData = font.Font
        let wvp = s.World * s.View * s.Projection
        let renderInfo =
            TextBatchHelpers.resolveTextRenderInfo fontData size position color font.Texture.Width font.Texture.Height wvp

        effect.Parameters["WorldViewProjection"].SetValue(renderInfo.WorldViewProjection)
        effect.Parameters["GlyphTexture"].SetValue(font.Texture)
        effect.Parameters["PxRange"].SetValue(renderInfo.PxRange)
        effect.Parameters["TextureSize"].SetValue(renderInfo.AtlasSize)
        effect.Parameters["ForegroundColor"].SetValue(renderInfo.ForegroundColor)
        effect.CurrentTechnique <- effect.Techniques.[renderInfo.Technique]

        s.DrawSubstring(font, renderInfo.Size, renderInfo.Position, layer, text.AsSpan())

        s.Flush()

    member s.DrawMultiLine
        (font: NoobishMonoGameFont)
        (sizeInPt: int)
        (maxWidth: float32)
        (position: Vector2)
        (layer: float32)
        (color: Color)
        (text: string) =

        let fontData = font.Font
        let wvp = s.World * s.View * s.Projection
        let renderInfo =
            TextBatchHelpers.resolveTextRenderInfo fontData sizeInPt position color font.Texture.Width font.Texture.Height wvp

        effect.Parameters["WorldViewProjection"].SetValue(renderInfo.WorldViewProjection)
        effect.Parameters["GlyphTexture"].SetValue(font.Texture)
        effect.Parameters["PxRange"].SetValue(renderInfo.PxRange)
        effect.Parameters["TextureSize"].SetValue(renderInfo.AtlasSize)
        effect.Parameters["ForegroundColor"].SetValue(renderInfo.ForegroundColor)
        effect.CurrentTechnique <- effect.Techniques.[renderInfo.Technique]

        TextBatchHelpers.iterateMultiLineSegments fontData renderInfo.Size maxWidth text (fun startPos length offset ->
            let nextPos = renderInfo.Position + offset
            let textSpan = text.AsSpan(startPos, length)
            s.DrawSubstring(font, renderInfo.Size, nextPos, layer, textSpan))

        s.Flush()

    interface IDisposable with
        member _.Dispose() =
            effect.Dispose()
