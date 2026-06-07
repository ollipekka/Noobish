namespace Noobish

open System.Collections.Generic

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
}

module NoobishFont =

    let inline scaleFromFontSize (fontSize: int) =
        float32 fontSize * 4f / 3f

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
        while i < text.Length && not nonWhiteSpaceFound do
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
        let mutable wordFound = false
        let mutable width = 0f
        let mutable i = startPos

        while i < text.Length && not wordFound do
            let c = text.[i]
            if c = ' ' || c = '\n' then
                wordFound <- true
            else
                let g = getGlyph font c

                width <- width + g.Advance * size
                i <- i + 1

        struct(width, i - startPos)

    let measureSingleLineSegment (font: NoobishFont) (size: int) (startIndex: int) (count: int) (text: string) =

        let size = scaleFromFontSize size
        let lineHeight = font.Metrics.LineHeight * size

        if startIndex < 0 || startIndex >= text.Length || count <= 0 then
            struct(0.0f, lineHeight)
        else
            let safeCount = min count (text.Length - startIndex)
            let mutable width = 0.0f
            for i = 0 to safeCount - 1 do
                let c = text.[startIndex + i]
                if c = '\n' then
                    ()
                else
                    let g = getGlyph font c
                    width <- width + g.Advance * size

            struct(width, lineHeight)


    let measureSingleLine (font: NoobishFont) (size: int) (text: string) =
        measureSingleLineSegment font size 0 (text.Length) text

    let measureMultiLine (font: NoobishFont) (size: int) (maxWidth: float32) (text: string) =
        let size = scaleFromFontSize size
        let lineHeight = font.Metrics.LineHeight * size

        let mutable x = 0f
        let mutable lineCount = 1

        let mutable i = 0

        while i < text.Length do

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
                    if wordWidth > maxWidth && x < System.Single.Epsilon then
                        x <- 0f
                        lineCount <- lineCount + 1
                        i <- i + wsCount + wordCount
                    else
                        x <- 0f
                        lineCount <- lineCount + 1
                // Start of the line with no whitespace
                // Middle of the line.
                else
                    x <- x + wsWidth + wordWidth
                    i <- i + wsCount + wordCount


        let height = float32 lineCount * lineHeight
        struct(maxWidth, height)


    let calculateCaretPosition
        (font: NoobishFont)
        (fontSize: int)
        (wrap: bool)
        (bounds: NoobishRectangle)
        (scrollX: float32)
        (scrollY: float32)
        (textAlign: NoobishAlignment)
        (caretPosition: int)
        (text: string) =

        let size = scaleFromFontSize fontSize
        let struct(textSizeX, _) =
            if wrap then
                failwith "Multiline text not supported yet."
            else
                measureSingleLineSegment font fontSize 0 caretPosition text

        let textSizeY = size * font.Metrics.LineHeight

        let inline leftX () = bounds.X
        let inline rightX () = bounds.X + bounds.Width - textSizeX
        let inline topY () = bounds.Y
        let inline bottomY () = bounds.Y + bounds.Height - textSizeY
        let inline centerX () = bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f
        let inline centerY () = bounds.Y  + bounds.Height / 2.0f - textSizeY / 2.0f

        let struct(textStartX, textStartY) =
            match textAlign with
            | NoobishAlignment.TopLeft -> struct(leftX(), topY())
            | NoobishAlignment.TopCenter -> struct(centerX(), topY())
            | NoobishAlignment.TopRight -> struct(rightX(), topY())
            | NoobishAlignment.Left -> struct(leftX(), centerY())
            | NoobishAlignment.Center -> struct(centerX(), centerY())
            | NoobishAlignment.Right -> struct(rightX(), centerY())
            | NoobishAlignment.BottomLeft -> struct(leftX(), bottomY())
            | NoobishAlignment.BottomCenter -> struct(centerX(), bottomY())
            | NoobishAlignment.BottomRight -> struct(rightX(), bottomY())
            | NoobishAlignment.None -> failwith "Can't be none here."

        {X = (textStartX + scrollX); Y = (textStartY + scrollY); Width = textSizeX; Height = textSizeY}



    let calculateCaretIndex
        (font: NoobishFont)
        (fontSize: int)
        (wrap: bool)
        (bounds: NoobishRectangle)
        (scrollX: float32)
        (scrollY: float32)
        (textAlign: NoobishAlignment)
        (relativeX: float32)
        (relativeY: float32)
        (text: string) =
        if wrap then failwith "Not supported."
        let struct(textSizeX, _) =
                measureSingleLineSegment font fontSize 0 text.Length text

        let size = scaleFromFontSize fontSize
        let textSizeY = size * font.Metrics.LineHeight

        let inline leftX () = bounds.X
        let inline rightX () = bounds.X + bounds.Width - textSizeX
        let inline topY () = bounds.Y
        let inline bottomY () = bounds.Y + bounds.Height - textSizeY
        let inline centerX () = bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f
        let inline centerY () = bounds.Y  + bounds.Height / 2.0f - textSizeY / 2.0f

        let struct(textStartX, _textStartY) =
            match textAlign with
            | NoobishAlignment.TopLeft -> struct(leftX(), topY())
            | NoobishAlignment.TopCenter -> struct(centerX(), topY())
            | NoobishAlignment.TopRight -> struct(rightX(), topY())
            | NoobishAlignment.Left -> struct(leftX(), centerY())
            | NoobishAlignment.Center -> struct(centerX(), centerY())
            | NoobishAlignment.Right -> struct(rightX(), centerY())
            | NoobishAlignment.BottomLeft -> struct(leftX(), bottomY())
            | NoobishAlignment.BottomCenter -> struct(centerX(), bottomY())
            | NoobishAlignment.BottomRight -> struct(rightX(), bottomY())
            | NoobishAlignment.None -> failwith "Can't be none here."

        let adjustedTextStartX = textStartX + scrollX
        let mutable width = adjustedTextStartX
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
        (textAlign: NoobishAlignment)
        (text: string) =

        let struct(textSizeX, textSizeY) =
            if wrap then
                measureMultiLine font fontSize bounds.Width text
            else
                measureSingleLine font fontSize text



        let struct(textStartX, textStartY) =
            match textAlign with
            | NoobishAlignment.TopLeft -> struct(leftX bounds, topY(bounds))
            | NoobishAlignment.TopCenter -> struct(centerX(bounds) textSizeX, topY(bounds))
            | NoobishAlignment.TopRight -> struct(rightX bounds textSizeX, topY(bounds))
            | NoobishAlignment.Left -> struct(leftX bounds, centerY bounds textSizeY)
            | NoobishAlignment.Center -> struct(centerX bounds textSizeX, centerY bounds textSizeY)
            | NoobishAlignment.Right -> struct(rightX bounds textSizeX, centerY bounds textSizeY)
            | NoobishAlignment.BottomLeft -> struct(leftX bounds, bottomY bounds textSizeY)
            | NoobishAlignment.BottomCenter -> struct(centerX(bounds) textSizeX, bottomY bounds textSizeY)
            | NoobishAlignment.BottomRight -> struct(rightX bounds textSizeX, bottomY bounds textSizeY)
            | NoobishAlignment.None -> failwith "Can't be none here."

        {X = (textStartX + scrollX); Y = (textStartY + scrollY); Width = textSizeX; Height = textSizeY}
