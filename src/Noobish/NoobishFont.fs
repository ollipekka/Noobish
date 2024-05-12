namespace Noobish

open System.Collections.Generic

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework

open Noobish.Internal
open System


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
    Unicode: char
    Advance: float32
    AtlasBounds: struct(float32*float32*float32*float32)
    PlaneBounds: struct(float32*float32*float32*float32)
    Kerning: IReadOnlyDictionary<char, float32>
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

    let getKern (g: NoobishGlyph) (c: char) =
        g.Kerning.GetValueOrDefault c

    let getSize (scale: float32) (glyph: NoobishGlyph) =
        let struct(top, right, bottom, left) = glyph.AtlasBounds

        struct((right - left) * scale, (top - bottom) * scale)

[<Struct>]
type NoobishTextSegment = {
    Start: int
    End: int
    Text: string
}

module NoobishTextSegment =
    let until (until: int) (text: string) =
        {
            Start = 0
            End = until
            Text = text
        }

    let all (text: string) =
        {
            Start = 0
            End = text.Length - 1
            Text = text
        }

type NoobishFont = {
    Atlas: NoobishFontAtlas
    Metrics: NoobishFontMetrics
    Glyphs: IReadOnlyDictionary<char, NoobishGlyph>
    Kerning: IReadOnlyDictionary<char, IReadOnlyDictionary<char, float32>>
    Texture: Texture2D
}

module NoobishFont =
    open Noobish.Styles

    let getGlyph (f: NoobishFont) (c: char) = 
        let mutable v = Unchecked.defaultof<NoobishGlyph>
        let s = f.Glyphs.TryGetValue (c, &v)
        if s then 
            v
        else 
            f.Glyphs.['x']

    let truncate (size: int) (value:string) =
        let size = min value.Length size
        value.Substring(0, size)

    let measureLeadingWhiteSpace (font: NoobishFont) (size: float32) (text: string) (startPos: int) =
        let mutable newLinePos = -1
        let mutable nonWhiteSpaceFound = false
        let mutable i = startPos
        let mutable width = 0f
        while i < text.Length - 1 && not nonWhiteSpaceFound do
            let c = text.[i]
            if c = '\n' then
                newLinePos <- i
                i <- i + 1
                nonWhiteSpaceFound <- true
            elif c <> ' ' then
                nonWhiteSpaceFound <- true
            else
                let g = getGlyph font c

                let struct(a, xOffset, yOffset, gw, gh) = NoobishGlyph.getGlyphMetricsInPx size g
                width <- width + a + xOffset

                i <- i + 1
        struct(width, newLinePos, i - startPos)

    let measureNextWord (font:NoobishFont) (size: float32) (text: string) (startPos: int) =
        let size = float32 size
        let mutable wordFound = false
        let mutable width = 0f
        let mutable i = startPos

        while i < text.Length - 1 && not wordFound do
            let c = text.[i]
            if c = ' ' || c = '\n' then
                wordFound <- true
            else
                let g = getGlyph font c

                width <- width + g.Advance * size
                i <- i + 1

        struct(width, i - startPos)

    let measureSingleLineSegment (font: NoobishFont) (size: int) (startIndex: int) (count: int) (text: string) =

        let size = float32 size * 4f / 3f

        let mutable width = 0.0f
        for i = 0 to count - 1 do
            let c = text.[startIndex + i]
            if c = '\n' then
                ()
            else
                let g = getGlyph font c
                width <- width + g.Advance * size

        struct(width, font.Metrics.LineHeight * size)


    let measureSingleLine (font: NoobishFont) (size: int) (text: string) =
        measureSingleLineSegment font size 0 (text.Length) text

    let measureMultiLine (font: NoobishFont) (size: int) (maxWidth: float32) (text: string) =
        let size = float32 size * 4f / 3f
        let lineHeight = font.Metrics.LineHeight * size

        let mutable x = 0f
        let mutable lineCount = 1

        let mutable i = 0

        while i < text.Length - 1 do

            let struct(wsWidth, wsLineEndPos, wsCount) = measureLeadingWhiteSpace font size text i

            if wsLineEndPos > -1 then
                x <- 0f
                lineCount <- lineCount + 1
                i <- i + wsCount
            else
                let struct(wordWidth, wordCount) = measureNextWord font size text (i + wsCount)

                // Start of the line, ignore whitespace.
                if x < System.Single.Epsilon && wsWidth > 0f then
                    i <- i + wsCount
                // End of the line.
                elif x + wsWidth + wordWidth > maxWidth then
                    x <- 0f
                    lineCount <- lineCount + 1
                // Start of the line with no whitespace
                // Middle of the line.
                else
                    x <- x + wsWidth + wordWidth
                    i <- i + wsCount + wordCount


        let height = float32 lineCount * lineHeight
        struct(maxWidth, height)


    let calculateCursorPosition
        (font: NoobishFont)
        (fontSize: int)
        (wrap: bool)
        (bounds: NoobishRectangle)
        (scrollX: float32)
        (scrollY: float32)
        (textAlign: NoobishTextAlignment)
        (cursorPosition: int)
        (text: string) =

        let size = float32 fontSize * 4f / 3f
        let struct(textSizeX, _) =
            if wrap then
                failwith "Multiline text not supported yet."
            else
                measureSingleLineSegment font fontSize 0 cursorPosition text

        let textSizeY = size * font.Metrics.LineHeight

        let inline leftX () = bounds.X
        let inline rightX () = bounds.X + bounds.Width - textSizeX
        let inline topY () = bounds.Y
        let inline bottomY () = bounds.Y + bounds.Height - textSizeY
        let inline centerX () = bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f
        let inline centerY () = bounds.Y  + bounds.Height / 2.0f - textSizeY / 2.0f

        let struct(textStartX, textStartY) =
            match textAlign with
            | NoobishTextAlignment.TopLeft -> struct(leftX(), topY())
            | NoobishTextAlignment.TopCenter -> struct(centerX(), topY())
            | NoobishTextAlignment.TopRight -> struct(rightX(), topY())
            | NoobishTextAlignment.Left -> struct(leftX(), centerY())
            | NoobishTextAlignment.Center -> struct(centerX(), centerY())
            | NoobishTextAlignment.Right -> struct(rightX(), centerY())
            | NoobishTextAlignment.BottomLeft -> struct(leftX(), bottomY())
            | NoobishTextAlignment.BottomCenter -> struct(centerX(), bottomY())
            | NoobishTextAlignment.BottomRight -> struct(rightX(), bottomY())

        {X = (textStartX + scrollX); Y = (textStartY + scrollY); Width = textSizeX; Height = textSizeY}



    let calculateCursorIndex
        (font: NoobishFont)
        (fontSize: int)
        (wrap: bool)
        (bounds: NoobishRectangle)
        (scrollX: float32)
        (scrollY: float32)
        (textAlign: NoobishTextAlignment)
        (relativeX: float32)
        (relativeY: float32)
        (text: string) =
        if wrap then failwith "Not supported."
        let struct(textSizeX, _) =
                measureSingleLineSegment font fontSize 0 text.Length text

        let size = float32 fontSize * 4f / 3f
        let textSizeY = size * font.Metrics.LineHeight

        let inline leftX () = bounds.X
        let inline rightX () = bounds.X + bounds.Width - textSizeX
        let inline topY () = bounds.Y
        let inline bottomY () = bounds.Y + bounds.Height - textSizeY
        let inline centerX () = bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f
        let inline centerY () = bounds.Y  + bounds.Height / 2.0f - textSizeY / 2.0f

        let struct(textStartX, _textStartY) =
            match textAlign with
            | NoobishTextAlignment.TopLeft -> struct(leftX(), topY())
            | NoobishTextAlignment.TopCenter -> struct(centerX(), topY())
            | NoobishTextAlignment.TopRight -> struct(rightX(), topY())
            | NoobishTextAlignment.Left -> struct(leftX(), centerY())
            | NoobishTextAlignment.Center -> struct(centerX(), centerY())
            | NoobishTextAlignment.Right -> struct(rightX(), centerY())
            | NoobishTextAlignment.BottomLeft -> struct(leftX(), bottomY())
            | NoobishTextAlignment.BottomCenter -> struct(centerX(), bottomY())
            | NoobishTextAlignment.BottomRight -> struct(rightX(), bottomY())

        let mutable width = textStartX
        let mutable i = 0
        while i < text.Length && width < relativeX do
            let c = text.[i]
            if c = '\n' then
                ()
            else
                let g = getGlyph font c
                width <- width + g.Advance * size
            i <- i + 1
        i

    let inline leftX (bounds: NoobishRectangle) = bounds.X
    let inline rightX (bounds: NoobishRectangle) (textSizeX: float32) = bounds.X + bounds.Width - textSizeX
    let inline topY (bounds: NoobishRectangle) = bounds.Y
    let inline bottomY (bounds: NoobishRectangle) (textSizeY: float32) = bounds.Y + bounds.Height - textSizeY
    let inline centerX (bounds: NoobishRectangle) (textSizeX: float32) = bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f
    let inline centerY (bounds: NoobishRectangle) (textSizeY: float32) = bounds.Y + bounds.Height / 2.0f - textSizeY / 2.0f

    let calculateBounds
        (font: NoobishFont)
        (fontSize: int)
        (wrap: bool)
        (bounds: NoobishRectangle)
        (scrollX: float32)
        (scrollY: float32)
        (textAlign: NoobishTextAlignment)
        (text: string) =

        let struct(textSizeX, textSizeY) =
            if wrap then
                measureMultiLine font fontSize bounds.Width text
            else
                measureSingleLine font fontSize text



        let struct(textStartX, textStartY) =
            match textAlign with
            | NoobishTextAlignment.TopLeft -> struct(leftX bounds, topY(bounds))
            | NoobishTextAlignment.TopCenter -> struct(centerX(bounds) textSizeX, topY(bounds))
            | NoobishTextAlignment.TopRight -> struct(rightX bounds textSizeX, topY(bounds))
            | NoobishTextAlignment.Left -> struct(leftX bounds, centerY bounds textSizeY)
            | NoobishTextAlignment.Center -> struct(centerX bounds textSizeX, centerY bounds textSizeY)
            | NoobishTextAlignment.Right -> struct(rightX bounds textSizeX, centerY bounds textSizeY)
            | NoobishTextAlignment.BottomLeft -> struct(leftX bounds, bottomY bounds textSizeY)
            | NoobishTextAlignment.BottomCenter -> struct(centerX(bounds) textSizeX, bottomY bounds textSizeY)
            | NoobishTextAlignment.BottomRight -> struct(rightX bounds textSizeX, bottomY bounds textSizeY)

        {X = (textStartX + scrollX); Y = (textStartY + scrollY); Width = textSizeX; Height = textSizeY}

type TextBatch (graphics: GraphicsDevice, Resolution: struct(int*int), effect: Effect, batchSize: int) =

    let mutable vertexCount = 0
    let vertices = Array.create batchSize (VertexPositionTexture())

    let addVertex (v:Vector3) (t: Vector2) =
        vertices.[vertexCount] <- VertexPositionTexture(v, t)
        vertexCount <- vertexCount + 1

    let addDegenerate () =
        let v = vertices.[vertexCount - 1]
        addVertex v.Position v.TextureCoordinate

    member val World = Matrix.Identity
    member val View = Matrix.Identity

    member val Projection =
        let struct(screenWidth, screenHeight) = Resolution
        Matrix.CreateOrthographicOffCenter(0.0f, float32 screenWidth, float32 screenHeight, 0.0f, 0.0f, -1.0f)


    member s.Flush () =
        if vertexCount > 0 then

            graphics.SamplerStates.[0] <- SamplerState.LinearClamp
            graphics.BlendState <- BlendState.AlphaBlend;
            graphics.DepthStencilState <- DepthStencilState.None;
            for pass in effect.CurrentTechnique.Passes do
                pass.Apply()
                graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertexCount - 2)

            vertexCount <- 0

    member s.DrawGlyph (font: NoobishFont) (center: Vector2) (halfSize: Vector2) (layer: float32) (glyph: NoobishGlyph) =
        if vertexCount + 6 > vertices.Length then
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

        if(vertexCount > 0) then
            addVertex (Vector3(p1.X, p1.Y, layer)) (Vector2(u2, v2))

        addVertex (Vector3(p1.X, p1.Y, layer)) t1
        addVertex (Vector3(p2.X, p2.Y, layer)) t2
        addVertex (Vector3(p3.X, p3.Y, layer)) t3
        addVertex (Vector3(p4.X, p4.Y, layer)) t4

        addDegenerate()

    member s.DrawSubstring ((font: NoobishFont), (size: float32), (position: Vector2), (layer: float32), (color: Color), (text: ReadOnlySpan<Char>)) =

        let mutable nextPosX = position.X

        for i = 0 to text.Length - 1 do
            let c = text.[i]
            let glyph = NoobishFont.getGlyph font c

            let struct(advance, xOffset, yOffset, glyphWidth, glyphHeight) = NoobishGlyph.getGlyphMetricsInPx size glyph

            let kern =
                if i + 1 < text.Length then
                    glyph.Kerning.GetValueOrDefault (text.[i + 1], 0f) * size
                else
                    0f

            let x = nextPosX + xOffset
            let y = position.Y + (size * font.Metrics.LineHeight - glyphHeight) - yOffset

            let glyphHalfSize = Vector2(glyphWidth / 2f, glyphHeight / 2f)
            let position = Vector2(x, y) + glyphHalfSize

            s.DrawGlyph font position glyphHalfSize layer glyph

            nextPosX <- nextPosX + advance + kern


    member s.DrawSingleLine (font: NoobishFont) (size: int) (position: Vector2) (layer: float32) (color: Color) (text:string) =

        let size = float32 size * 4f / 3f / float32 font.Metrics.EmSize // Size in PX


        let wvp = s.World * s.View * s.Projection
        effect.Parameters["WorldViewProjection"].SetValue(wvp)
        effect.Parameters["GlyphTexture"].SetValue(font.Texture)
        effect.Parameters["PxRange"].SetValue(float32 font.Atlas.DistanceRange)

        let atlasSize = Vector2(float32 font.Texture.Width, float32 font.Texture.Height)
        effect.Parameters["TextureSize"].SetValue(atlasSize)
        effect.Parameters["ForegroundColor"].SetValue(color.ToVector4())
        effect.CurrentTechnique <- if size > 10.0f then effect.Techniques["LargeText"] else effect.Techniques["SmallText"]

        let position = position + Vector2(0f, font.Metrics.Descender * size)
        s.DrawSubstring(font, size, position, layer, color, text.AsSpan())

        s.Flush()

    member s.DrawMultiLine (font: NoobishFont) (sizeInPt: int) (maxWidth: float32) (position: Vector2) (layer: float32) (color: Color) (text:string) =

        let size = float32 sizeInPt * 4f / 3f / float32 font.Metrics.EmSize // Size in PX

        let wvp = s.World * s.View * s.Projection
        effect.Parameters["WorldViewProjection"].SetValue(wvp)
        effect.Parameters["GlyphTexture"].SetValue(font.Texture)
        effect.Parameters["PxRange"].SetValue(float32 font.Atlas.DistanceRange)

        let atlasSize = Vector2(float32 font.Texture.Width, float32 font.Texture.Height)
        effect.Parameters["TextureSize"].SetValue(atlasSize)
        effect.Parameters["ForegroundColor"].SetValue(color.ToVector4())
        effect.CurrentTechnique <- if size > 10.0f then effect.Techniques["LargeText"] else effect.Techniques["SmallText"]


        let position = position + Vector2(0f, font.Metrics.Descender * size)
        let mutable nextPosX = 0f
        let mutable nextPosY = 0f

        let mutable circuitBreaker = 0

        let mutable i = 0
        while i < text.Length - 1 do
            let struct(wsWidth, wsNewLinePos, wsCount) = NoobishFont.measureLeadingWhiteSpace font size text i

            if wsNewLinePos <> -1 then

                nextPosX <- 0.0f
                nextPosY <- nextPosY + font.Metrics.LineHeight * size
                i <- i + wsCount
            else
                let struct(wordWidth, wordCount) = NoobishFont.measureNextWord font size text (i + wsCount)

                if nextPosX + wsWidth + wordWidth > maxWidth then

                    if circuitBreaker > 0 then failwith "Word is larger than line width. Use smalelr font."
                    circuitBreaker <- circuitBreaker + 1
                    nextPosX <- 0.0f
                    nextPosY <- nextPosY + font.Metrics.LineHeight * size
                else
                    circuitBreaker <- 0 
                    // Handle the case of leading whitespace by skipping whitespace at the start of line.
                    let struct(startPos, endPos, wsWidth) =
                        if nextPosX < System.Single.Epsilon && wsCount > 0 then
                            struct(i + wsCount, i + wsCount + wordCount, 0f)
                        else
                            struct(i, i + wsCount + wordCount, wsWidth)

                    let nextPos = position + Vector2(nextPosX, nextPosY)
                    let textSpan = (text.AsSpan(startPos, (endPos - startPos)))
                    s.DrawSubstring(font, size, nextPos, layer, color, textSpan)

                    nextPosX <- nextPosX + wsWidth + wordWidth

                    i <- endPos

        s.Flush()


    interface System.IDisposable with
        member s.Dispose() =
            effect.Dispose()