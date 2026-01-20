module Noobish.Test.NoobishFontTests

open System.Collections.Generic
open NUnit.Framework
open Noobish

let private epsilon = 0.0001f

let private assertFloat expected actual =
    Assert.AreEqual(expected, actual)

let private createGlyph unicode advance atlasBounds planeBounds kerningPairs =
    let kerning = Dictionary<char, float32>()
    for (c, v) in kerningPairs do
        kerning.[c] <- v
    {
        NoobishGlyph.Unicode = unicode
        Advance = advance
        AtlasBounds = atlasBounds
        PlaneBounds = planeBounds
        Kerning = kerning :> IReadOnlyDictionary<char, float32>
    }

let private createTestFont () =
    let glyphs = Dictionary<char, NoobishGlyph>()
    let atlasBounds = struct(0f, 1f, 0f, 0f)
    let planeBounds = struct(1f, 1f, 0f, 0f)
    for c in [|'x'; 'a'; 'b'; ' '|] do
        glyphs.[c] <- createGlyph c 1.0f atlasBounds planeBounds []
    {
        NoobishFont.Atlas = {FontType = "Test"; DistanceRange = 0f; Size = 0f; Width = 0; Height = 0; yOrigin = "bottom"}
        Metrics = {EmSize = 1; LineHeight = 1.0f; Ascender = 0f; Descender = 0f; UnderlineY = 0f; UnderlineThickness = 0f}
        Glyphs = glyphs
        Kerning = Dictionary<char, IReadOnlyDictionary<char, float32>>() :> IReadOnlyDictionary<char, IReadOnlyDictionary<char, float32>>
    }

let private alignmentCases =
    [|
        NoobishAlignment.TopLeft
        NoobishAlignment.TopCenter
        NoobishAlignment.TopRight
        NoobishAlignment.Left
        NoobishAlignment.Center
        NoobishAlignment.Right
        NoobishAlignment.BottomLeft
        NoobishAlignment.BottomCenter
        NoobishAlignment.BottomRight
    |]

[<Test>]
let ``getTextureCoordinates computes UVs`` () =
    let glyph =
        createGlyph 'a' 1.0f (struct(20f, 30f, 10f, 5f)) (struct(0f, 0f, 0f, 0f)) []
    let struct(u, u2, v, v2) = NoobishGlyph.getTextureCoordinates 100f 200f glyph
    assertFloat 0.05f u
    assertFloat 0.30f u2
    assertFloat 0.90f v
    assertFloat 0.95f v2

[<Test>]
let ``getGlyphMetricsInPx scales bounds and advance`` () =
    let glyph =
        createGlyph 'a' 1.5f (struct(0f, 0f, 0f, 0f)) (struct(2f, 3f, -1f, 0.5f)) []
    let struct(advance, xOffset, yOffset, width, height) = NoobishGlyph.getGlyphMetricsInPx 2.0f glyph
    assertFloat 3.0f advance
    assertFloat 1.0f xOffset
    assertFloat -2.0f yOffset
    assertFloat 5.0f width
    assertFloat 6.0f height

[<Test>]
let ``getKern returns value or default`` () =
    let glyph =
        createGlyph 'a' 1.0f (struct(0f, 0f, 0f, 0f)) (struct(0f, 0f, 0f, 0f)) [ 'b', 0.25f ]
    assertFloat 0.25f (NoobishGlyph.getKern glyph 'b')
    assertFloat 0.0f (NoobishGlyph.getKern glyph 'c')

[<Test>]
let ``getSize scales atlas bounds`` () =
    let glyph = createGlyph 'a' 1.0f (struct(5f, 9f, 1f, 2f)) (struct(0f, 0f, 0f, 0f)) []
    let struct(width, height) = NoobishGlyph.getSize 2.0f glyph
    assertFloat 14.0f width
    assertFloat 8.0f height

[<Test>]
let ``text segment helpers create ranges`` () =
    let until = NoobishTextSegment.until 2 "abc"
    Assert.AreEqual(0, until.Start)
    Assert.AreEqual(2, until.End)
    Assert.AreEqual("abc", until.Text)

    let allEmpty = NoobishTextSegment.all ""
    Assert.AreEqual(0, allEmpty.Start)
    Assert.AreEqual(-1, allEmpty.End)
    Assert.AreEqual("", allEmpty.Text)

[<Test>]
let ``truncate handles smaller and larger sizes`` () =
    Assert.AreEqual("", NoobishFont.truncate 0 "abc")
    Assert.AreEqual("ab", NoobishFont.truncate 2 "abc")
    Assert.AreEqual("abc", NoobishFont.truncate 5 "abc")

[<Test>]
let ``getGlyph returns fallback`` () =
    let font = createTestFont ()
    let glyph = NoobishFont.getGlyph font 'z'
    Assert.AreEqual('x', glyph.Unicode)

[<Test>]
let ``measureLeadingWhiteSpace counts single space`` () =
    let font = createTestFont ()
    let struct(width, lineEndPos, count) = NoobishFont.measureLeadingWhiteSpace font 1.0f " " 0
    assertFloat 1.0f width
    Assert.AreEqual(-1, lineEndPos)
    Assert.AreEqual(1, count)

[<Test>]
let ``measureLeadingWhiteSpace stops at newline`` () =
    let font = createTestFont ()
    let struct(width, lineEndPos, count) = NoobishFont.measureLeadingWhiteSpace font 1.0f "\n" 0
    assertFloat 0.0f width
    Assert.AreEqual(0, lineEndPos)
    Assert.AreEqual(1, count)

[<Test>]
let ``measureLeadingWhiteSpace stops at non-space`` () =
    let font = createTestFont ()
    let struct(width, lineEndPos, count) = NoobishFont.measureLeadingWhiteSpace font 1.0f "a" 0
    assertFloat 0.0f width
    Assert.AreEqual(-1, lineEndPos)
    Assert.AreEqual(0, count)

[<Test>]
let ``measureNextWord includes last character`` () =
    let font = createTestFont ()
    let struct(width, count) = NoobishFont.measureNextWord font 1.0f "a" 0
    assertFloat 1.0f width
    Assert.AreEqual(1, count)

[<Test>]
let ``measureNextWord stops at space`` () =
    let font = createTestFont ()
    let struct(width, count) = NoobishFont.measureNextWord font 1.0f "ab a" 0
    assertFloat 2.0f width
    Assert.AreEqual(2, count)

[<Test>]
let ``measureSingleLineSegment clamps count to text length`` () =
    let font = createTestFont ()
    let struct(width, height) = NoobishFont.measureSingleLineSegment font 1 0 10 "a"
    assertFloat (4.0f / 3.0f) width
    assertFloat (4.0f / 3.0f) height

[<Test>]
let ``measureSingleLineSegment returns zero width when start is past end`` () =
    let font = createTestFont ()
    let struct(width, height) = NoobishFont.measureSingleLineSegment font 1 5 1 "a"
    assertFloat 0.0f width
    assertFloat (4.0f / 3.0f) height

[<Test>]
let ``measureSingleLineSegment returns zero width for non-positive count`` () =
    let font = createTestFont ()
    let struct(width, _) = NoobishFont.measureSingleLineSegment font 1 0 0 "a"
    assertFloat 0.0f width

[<Test>]
let ``measureSingleLineSegment ignores newline`` () =
    let font = createTestFont ()
    let struct(width, _) = NoobishFont.measureSingleLineSegment font 1 0 3 "a\nb"
    assertFloat (2.0f * (4.0f / 3.0f)) width

[<Test>]
let ``measureSingleLine uses full length`` () =
    let font = createTestFont ()
    let struct(width, _) = NoobishFont.measureSingleLine font 1 "ab"
    assertFloat (2.0f * (4.0f / 3.0f)) width

[<Test>]
let ``measureMultiLine counts newline as line break`` () =
    let font = createTestFont ()
    let struct(_, height) = NoobishFont.measureMultiLine font 1 100f "a\nb"
    assertFloat (2.0f * (4.0f / 3.0f)) height

[<Test>]
let ``measureMultiLine wraps when line overflows`` () =
    let font = createTestFont ()
    let size = NoobishFont.scaleFromFontSize 1
    let maxWidth = size * 1.5f
    let struct(_, height) = NoobishFont.measureMultiLine font 1 maxWidth "a a"
    assertFloat (2.0f * size) height

[<Test>]
let ``measureMultiLine handles long word on empty line`` () =
    let font = createTestFont ()
    let size = NoobishFont.scaleFromFontSize 1
    let struct(_, height) = NoobishFont.measureMultiLine font 1 (size * 0.5f) "aaaa"
    assertFloat size height

[<TestCaseSource(nameof alignmentCases)>]
let ``calculateCursorPosition aligns text`` alignment =
    let font = createTestFont ()
    let bounds = {X = 2f; Y = 3f; Width = 100f; Height = 50f}
    let scrollX = 4f
    let scrollY = 5f
    let struct(textSizeX, _) = NoobishFont.measureSingleLineSegment font 1 0 1 "ab"
    let textSizeY = NoobishFont.scaleFromFontSize 1 * font.Metrics.LineHeight
    let leftX = bounds.X
    let rightX = bounds.X + bounds.Width - textSizeX
    let topY = bounds.Y
    let bottomY = bounds.Y + bounds.Height - textSizeY
    let centerX = bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f
    let centerY = bounds.Y  + bounds.Height / 2.0f - textSizeY / 2.0f
    let struct(expectedX, expectedY) =
        match alignment with
        | NoobishAlignment.TopLeft -> struct(leftX, topY)
        | NoobishAlignment.TopCenter -> struct(centerX, topY)
        | NoobishAlignment.TopRight -> struct(rightX, topY)
        | NoobishAlignment.Left -> struct(leftX, centerY)
        | NoobishAlignment.Center -> struct(centerX, centerY)
        | NoobishAlignment.Right -> struct(rightX, centerY)
        | NoobishAlignment.BottomLeft -> struct(leftX, bottomY)
        | NoobishAlignment.BottomCenter -> struct(centerX, bottomY)
        | NoobishAlignment.BottomRight -> struct(rightX, bottomY)
        | NoobishAlignment.None -> failwith "Unexpected."
    let boundsResult =
        NoobishFont.calculateCursorPosition font 1 false bounds scrollX scrollY alignment 1 "ab"
    assertFloat (expectedX + scrollX) boundsResult.X
    assertFloat (expectedY + scrollY) boundsResult.Y

[<Test>]
let ``calculateCursorPosition throws on wrap`` () =
    let font = createTestFont ()
    let bounds = {X = 0f; Y = 0f; Width = 100f; Height = 10f}
    Assert.Throws<System.Exception>(fun () ->
        NoobishFont.calculateCursorPosition font 1 true bounds 0f 0f NoobishAlignment.TopLeft 0 "a"
        |> ignore) |> ignore

[<Test>]
let ``calculateCursorPosition throws on none alignment`` () =
    let font = createTestFont ()
    let bounds = {X = 0f; Y = 0f; Width = 100f; Height = 10f}
    Assert.Throws<System.Exception>(fun () ->
        NoobishFont.calculateCursorPosition font 1 false bounds 0f 0f NoobishAlignment.None 0 "a"
        |> ignore) |> ignore

[<TestCaseSource(nameof alignmentCases)>]
let ``calculateCursorIndex aligns text`` alignment =
    let font = createTestFont ()
    let bounds = {X = 2f; Y = 3f; Width = 100f; Height = 50f}
    let scrollX = 6f
    let scrollY = 0f
    let struct(textSizeX, _) = NoobishFont.measureSingleLine font 1 "ab"
    let textSizeY = NoobishFont.scaleFromFontSize 1 * font.Metrics.LineHeight
    let leftX = bounds.X
    let rightX = bounds.X + bounds.Width - textSizeX
    let topY = bounds.Y
    let bottomY = bounds.Y + bounds.Height - textSizeY
    let centerX = bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f
    let centerY = bounds.Y  + bounds.Height / 2.0f - textSizeY / 2.0f
    let struct(textStartX, _) =
        match alignment with
        | NoobishAlignment.TopLeft -> struct(leftX, topY)
        | NoobishAlignment.TopCenter -> struct(centerX, topY)
        | NoobishAlignment.TopRight -> struct(rightX, topY)
        | NoobishAlignment.Left -> struct(leftX, centerY)
        | NoobishAlignment.Center -> struct(centerX, centerY)
        | NoobishAlignment.Right -> struct(rightX, centerY)
        | NoobishAlignment.BottomLeft -> struct(leftX, bottomY)
        | NoobishAlignment.BottomCenter -> struct(centerX, bottomY)
        | NoobishAlignment.BottomRight -> struct(rightX, bottomY)
        | NoobishAlignment.None -> failwith "Unexpected."
    let size = NoobishFont.scaleFromFontSize 1
    let relativeX = textStartX + scrollX + (size * 0.6f)
    let index =
        NoobishFont.calculateCursorIndex
            font
            1
            false
            bounds
            scrollX
            scrollY
            alignment
            relativeX
            0f
            "ab"
    Assert.AreEqual(1, index)

[<Test>]
let ``calculateCursorIndex throws on wrap`` () =
    let font = createTestFont ()
    let bounds = {X = 0f; Y = 0f; Width = 100f; Height = 10f}
    Assert.Throws<System.Exception>(fun () ->
        NoobishFont.calculateCursorIndex font 1 true bounds 0f 0f NoobishAlignment.TopLeft 0f 0f "a"
        |> ignore) |> ignore

[<Test>]
let ``calculateCursorIndex throws on none alignment`` () =
    let font = createTestFont ()
    let bounds = {X = 0f; Y = 0f; Width = 100f; Height = 10f}
    Assert.Throws<System.Exception>(fun () ->
        NoobishFont.calculateCursorIndex font 1 false bounds 0f 0f NoobishAlignment.None 0f 0f "a"
        |> ignore)|> ignore

[<TestCaseSource(nameof alignmentCases)>]
let ``calculateBounds aligns text`` alignment =
    let font = createTestFont ()
    let bounds = {X = 2f; Y = 3f; Width = 100f; Height = 50f}
    let scrollX = 1f
    let scrollY = 2f
    let struct(textSizeX, textSizeY) = NoobishFont.measureSingleLine font 1 "ab"
    let leftX = bounds.X
    let rightX = bounds.X + bounds.Width - textSizeX
    let topY = bounds.Y
    let bottomY = bounds.Y + bounds.Height - textSizeY
    let centerX = bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f
    let centerY = bounds.Y  + bounds.Height / 2.0f - textSizeY / 2.0f
    let struct(expectedX, expectedY) =
        match alignment with
        | NoobishAlignment.TopLeft -> struct(leftX, topY)
        | NoobishAlignment.TopCenter -> struct(centerX, topY)
        | NoobishAlignment.TopRight -> struct(rightX, topY)
        | NoobishAlignment.Left -> struct(leftX, centerY)
        | NoobishAlignment.Center -> struct(centerX, centerY)
        | NoobishAlignment.Right -> struct(rightX, centerY)
        | NoobishAlignment.BottomLeft -> struct(leftX, bottomY)
        | NoobishAlignment.BottomCenter -> struct(centerX, bottomY)
        | NoobishAlignment.BottomRight -> struct(rightX, bottomY)
        | NoobishAlignment.None -> failwith "Unexpected."
    let result =
        NoobishFont.calculateBounds font 1 false bounds scrollX scrollY alignment "ab"
    assertFloat (expectedX + scrollX) result.X
    assertFloat (expectedY + scrollY) result.Y
    assertFloat textSizeX result.Width
    assertFloat textSizeY result.Height

[<Test>]
let ``calculateBounds uses wrap measurement`` () =
    let font = createTestFont ()
    let bounds = {X = 0f; Y = 0f; Width = 3f; Height = 50f}
    let size = NoobishFont.scaleFromFontSize 1
    let result = NoobishFont.calculateBounds font 1 true bounds 0f 0f NoobishAlignment.TopLeft "a a"
    assertFloat bounds.Width result.Width
    assertFloat (2.0f * size) result.Height

[<Test>]
let ``calculateBounds throws on none alignment`` () =
    let font = createTestFont ()
    let bounds = {X = 0f; Y = 0f; Width = 100f; Height = 10f}
    Assert.Throws<System.Exception>(fun () ->
        NoobishFont.calculateBounds font 1 false bounds 0f 0f NoobishAlignment.None "a"
        |> ignore) |> ignore

[<Test>]
let ``calculateCursorIndex respects scrollX`` () =
    let font = createTestFont ()
    let bounds = {X = 0f; Y = 0f; Width = 100f; Height = 10f}
    let index =
        NoobishFont.calculateCursorIndex
            font
            1
            false
            bounds
            10f
            0f
            NoobishAlignment.TopLeft
            10.1f
            0f
            "ab"
    Assert.AreEqual(1, index)
