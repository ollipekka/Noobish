module Noobish.MonoGame.Test.TextBatchHelpersTests
open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open NUnit.Framework
open Noobish
open Noobish.TextBatchHelpers

let private createFont () =
    let glyph =
        { Unicode = 'x'
          Advance = 2f
          AtlasBounds = struct(1f, 1f, 0f, 0f)
          PlaneBounds = struct(1f, 1f, 0f, 0f)
          Kerning = Dictionary<char, float32>() :> IReadOnlyDictionary<_, _> }
    let glyphs = Dictionary<char, NoobishGlyph>()
    glyphs.['x'] <- glyph
    { Atlas =
        { FontType = "Test"
          DistanceRange = 2f
          Size = 1f
          Width = 1
          Height = 1
          yOrigin = "top" }
      Metrics =
        { EmSize = 1
          LineHeight = 1f
          Ascender = 0f
          Descender = -0.5f
          UnderlineY = 0f
          UnderlineThickness = 0f }
      Glyphs = glyphs :> IReadOnlyDictionary<_, _>
      Kerning = Dictionary<char, IReadOnlyDictionary<char, float32>>() :> IReadOnlyDictionary<_, _> }

[<Test>]
let ``resolveTextRenderInfo selects technique and applies descender`` () =
    let font = createFont ()
    let wvp = Matrix.Identity
    let infoSmall = resolveTextRenderInfo font 6 (Vector2(1f, 2f)) Color.White 10 10 wvp
    let infoLarge = resolveTextRenderInfo font 9 (Vector2(1f, 2f)) Color.White 10 10 wvp

    Assert.AreEqual("SmallText", infoSmall.Technique)
    Assert.AreEqual("LargeText", infoLarge.Technique)
    Assert.Less(infoSmall.Position.Y, 2f)

[<Test>]
let ``iterateGlyphPlacements emits single glyph`` () =
    let font = createFont ()
    let placements = ResizeArray<Vector2>()
    iterateGlyphPlacements font 1f (Vector2(0f, 0f)) ("x".AsSpan()) (fun center _ _ ->
        placements.Add center)

    Assert.AreEqual(1, placements.Count)
    Assert.AreEqual(0.5f, placements.[0].X)

[<Test>]
let ``iterateGlyphPlacements applies kerning to advance`` () =
    let font = createFont ()
    let glyph = font.Glyphs.['x']
    let kerning = Dictionary<char, float32>()
    kerning.['x'] <- 1f
    let glyphs = Dictionary<char, NoobishGlyph>()
    glyphs.['x'] <- { glyph with Kerning = kerning :> IReadOnlyDictionary<_, _> }
    let fontWithKern = { font with Glyphs = glyphs :> IReadOnlyDictionary<_, _> }
    let positions = ResizeArray<float32>()

    iterateGlyphPlacements fontWithKern 1f (Vector2(0f, 0f)) ("xx".AsSpan()) (fun center _ _ ->
        positions.Add center.X)

    Assert.AreEqual(2, positions.Count)
    Assert.AreEqual(0.5f, positions.[0])
    Assert.AreEqual(3.5f, positions.[1])

[<Test>]
let ``iterateMultiLineSegments handles single character`` () =
    let font = createFont ()
    let segments = ResizeArray<struct(int * int * Vector2)>()
    iterateMultiLineSegments font 1f 100f "x" (fun start length offset ->
        segments.Add(struct(start, length, offset)))

    Assert.AreEqual(1, segments.Count)
    let struct(start, length, offset) = segments.[0]
    Assert.AreEqual(0, start)
    Assert.AreEqual(1, length)
    Assert.AreEqual(0f, offset.X)

[<Test>]
let ``iterateMultiLineSegments skips leading whitespace when line is empty`` () =
    let font = createFont ()
    let segments = ResizeArray<struct(int * int * Vector2)>()
    iterateMultiLineSegments font 1f 100f " x" (fun start length offset ->
        segments.Add(struct(start, length, offset)))

    Assert.AreEqual(1, segments.Count)
    let struct(start, length, offset) = segments.[0]
    Assert.AreEqual(1, start)
    Assert.AreEqual(1, length)
    Assert.AreEqual(0f, offset.X)

[<Test>]
let ``iterateMultiLineSegments wraps when line is full`` () =
    let font = createFont ()
    let segments = ResizeArray<struct(int * int * Vector2)>()
    iterateMultiLineSegments font 1f 4f "x x" (fun start length offset ->
        segments.Add(struct(start, length, offset)))

    Assert.AreEqual(2, segments.Count)
    let struct(_start1, _length1, offset1) = segments.[0]
    let struct(_start2, _length2, offset2) = segments.[1]
    Assert.AreEqual(0f, offset1.X)
    Assert.AreEqual(0f, offset2.X)
    Assert.Greater(offset2.Y, offset1.Y)
