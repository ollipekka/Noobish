namespace Noobish

open System
open System.Collections.Generic

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics


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

        let fontData = font.Font
        let mutable nextPosX = position.X

        for i = 0 to text.Length - 1 do
            let c = text.[i]
            let glyph = NoobishFont.getGlyph fontData c

            let struct(advance, xOffset, yOffset, glyphWidth, glyphHeight) =
                NoobishGlyph.getGlyphMetricsInPx size glyph

            let kern =
                if i + 1 < text.Length then
                    glyph.Kerning.GetValueOrDefault (text.[i + 1], 0f) * size
                else
                    0f

            let x = nextPosX + xOffset
            let y = position.Y + (size * fontData.Metrics.LineHeight - glyphHeight) - yOffset

            let glyphHalfSize = Vector2(glyphWidth / 2f, glyphHeight / 2f)
            let position = Vector2(x, y) + glyphHalfSize

            s.DrawGlyph font position glyphHalfSize layer glyph

            nextPosX <- nextPosX + advance + kern

    member s.DrawSingleLine
        (font: NoobishMonoGameFont)
        (size: int)
        (position: Vector2)
        (layer: float32)
        (color: Color)
        (text: string) =

        let fontData = font.Font
        let size = float32 size * 4f / 3f / float32 fontData.Metrics.EmSize

        let wvp = s.World * s.View * s.Projection
        effect.Parameters["WorldViewProjection"].SetValue(wvp)
        effect.Parameters["GlyphTexture"].SetValue(font.Texture)
        effect.Parameters["PxRange"].SetValue(float32 fontData.Atlas.DistanceRange)

        let atlasSize = Vector2(float32 font.Texture.Width, float32 font.Texture.Height)
        effect.Parameters["TextureSize"].SetValue(atlasSize)
        effect.Parameters["ForegroundColor"].SetValue(color.ToVector4())
        effect.CurrentTechnique <-
            if size > 10.0f then
                effect.Techniques["LargeText"]
            else
                effect.Techniques["SmallText"]

        let position = position + Vector2(0f, fontData.Metrics.Descender * size)
        s.DrawSubstring(font, size, position, layer, text.AsSpan())

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
        let size = float32 sizeInPt * 4f / 3f / float32 fontData.Metrics.EmSize

        let wvp = s.World * s.View * s.Projection
        effect.Parameters["WorldViewProjection"].SetValue(wvp)
        effect.Parameters["GlyphTexture"].SetValue(font.Texture)
        effect.Parameters["PxRange"].SetValue(float32 fontData.Atlas.DistanceRange)

        let atlasSize = Vector2(float32 font.Texture.Width, float32 font.Texture.Height)
        effect.Parameters["TextureSize"].SetValue(atlasSize)
        effect.Parameters["ForegroundColor"].SetValue(color.ToVector4())
        effect.CurrentTechnique <-
            if size > 10.0f then
                effect.Techniques["LargeText"]
            else
                effect.Techniques["SmallText"]

        let position = position + Vector2(0f, fontData.Metrics.Descender * size)
        let mutable nextPosX = 0f
        let mutable nextPosY = 0f

        let mutable i = 0
        while i < text.Length - 1 do
            let struct(wsWidth, wsNewLinePos, wsCount) = NoobishFont.measureLeadingWhiteSpace fontData size text i

            if wsNewLinePos <> -1 then
                nextPosX <- 0.0f
                nextPosY <- nextPosY + fontData.Metrics.LineHeight * size
                i <- i + wsCount
            else
                let struct(wordWidth, wordCount) = NoobishFont.measureNextWord fontData size text (i + wsCount)

                if nextPosX + wsWidth + wordWidth > maxWidth then
                    if nextPosX <= Single.Epsilon then
                        failwith "Word is larger than line width. Use smaller font."
                    nextPosX <- 0.0f
                    nextPosY <- nextPosY + fontData.Metrics.LineHeight * size
                else
                    let struct(startPos, endPos, adjustedWidth) =
                        if nextPosX < Single.Epsilon && wsCount > 0 then
                            struct(i + wsCount, i + wsCount + wordCount, 0f)
                        else
                            struct(i, i + wsCount + wordCount, wsWidth)

                    let nextPos = position + Vector2(nextPosX, nextPosY)
                    let textSpan = text.AsSpan(startPos, endPos - startPos)
                    s.DrawSubstring(font, size, nextPos, layer, textSpan)

                    nextPosX <- nextPosX + adjustedWidth + wordWidth

                    i <- endPos

        s.Flush()

    interface IDisposable with
        member _.Dispose() =
            effect.Dispose()
