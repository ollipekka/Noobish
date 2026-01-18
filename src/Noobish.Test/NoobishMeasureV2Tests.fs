module Noobish.Test.NoobishMeasureV2Tests

open System.Collections.Generic
open NUnit.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish
open Noobish.Styles
open Microsoft.Xna.Framework

let private createGlyph c =
    {
        NoobishGlyph.Unicode = c
        Advance = 1.0f
        AtlasBounds = struct(0f, 0f, 0f, 0f)
        PlaneBounds = struct(1f, 1f, 0f, 0f)
        Kerning = Dictionary<char, float32>() :> IReadOnlyDictionary<char, float32>
    }

let private createFont () =
    let glyphs = Dictionary<char, NoobishGlyph>()
    for c in [|'x'; 'L'; 'a'; 'b'; 'e'; 'l'; 's'; ' '; 'S'; 'm'; 'p'|] do
        glyphs.[c] <- createGlyph c
    {
        NoobishFont.Atlas = {FontType = "Test"; DistanceRange = 0f; Size = 0f; Width = 0; Height = 0; yOrigin = "bottom"}
        Metrics = {EmSize = 1; LineHeight = 1.0f; Ascender = 0f; Descender = 0f; UnderlineY = 0f; UnderlineThickness = 0f}
        Glyphs = glyphs
        Kerning = Dictionary<char, IReadOnlyDictionary<char, float32>>() :> IReadOnlyDictionary<char, IReadOnlyDictionary<char, float32>>
        Texture = Unchecked.defaultof<Texture2D>
    }

let private emptyNested<'T> () =
    Dictionary<string, IReadOnlyDictionary<string, 'T>>() :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>

let private singleState<'T> (value: 'T) =
    let states = Dictionary<string, 'T>()
    states.["default"] <- value
    states :> IReadOnlyDictionary<string, 'T>

let private createStyleSheet (sliderHeight: float32) (pinHeight: float32) =
    let heights = Dictionary<string, IReadOnlyDictionary<string, float32>>()
    heights.["Slider"] <- singleState sliderHeight
    heights.["SliderPin"] <- singleState pinHeight
    {
        Name = "Test"
        TextureAtlasId = ""
        Widths = emptyNested<float32>()
        Heights = heights :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
        Paddings = emptyNested<NoobishPadding>()
        Margins = emptyNested<NoobishMargin>()
        Colors = emptyNested<Color>()
        Fonts = emptyNested<string>()
        FontSizes = emptyNested<int>()
        FontColors = emptyNested<Color>()
        TextAlignments = emptyNested<NoobishAlignment>()
        Drawables = emptyNested<NoobishDrawable[]>()
    }

let private createProgressStyleSheet (height: float32) =
    let heights = Dictionary<string, IReadOnlyDictionary<string, float32>>()
    heights.["ProgressBar"] <- singleState height
    {
        Name = "Test"
        TextureAtlasId = ""
        Widths = emptyNested<float32>()
        Heights = heights :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>
        Paddings = emptyNested<NoobishPadding>()
        Margins = emptyNested<NoobishMargin>()
        Colors = emptyNested<Color>()
        Fonts = emptyNested<string>()
        FontSizes = emptyNested<int>()
        FontColors = emptyNested<Color>()
        TextAlignments = emptyNested<NoobishAlignment>()
        Drawables = emptyNested<NoobishDrawable[]>()
    }

let private createSpacingStyleSheet (padding: NoobishPadding) (margin: NoobishMargin) =
    let paddings = Dictionary<string, IReadOnlyDictionary<string, NoobishPadding>>()
    let margins = Dictionary<string, IReadOnlyDictionary<string, NoobishMargin>>()
    paddings.["Panel"] <- singleState padding
    margins.["Panel"] <- singleState margin
    {
        Name = "Test"
        TextureAtlasId = ""
        Widths = emptyNested<float32>()
        Heights = emptyNested<float32>()
        Paddings = paddings :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishPadding>>
        Margins = margins :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishMargin>>
        Colors = emptyNested<Color>()
        Fonts = emptyNested<string>()
        FontSizes = emptyNested<int>()
        FontColors = emptyNested<Color>()
        TextAlignments = emptyNested<NoobishAlignment>()
        Drawables = emptyNested<NoobishDrawable[]>()
    }

[<Test>]
let ``measureFrameWith uses text width when wants text`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginLabel "Label"
    let index = int ctx.ComponentId.Index
    components.MinSize.[index] <- {Width = 0f; Height = 0f}

    let font = createFont ()
    NoobishMeasureV2.measureFrameWith (fun _ -> font) (fun _ -> 10) components

    Assert.Greater(components.ContentSize.[index].Width, 0f)
    Assert.Greater(components.ContentSize.[index].Height, 0f)

[<Test>]
let ``measureFrameWith wraps text using bounds width when min width is zero`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginParagraph "x x x x x x x x"
    let index = int ctx.ComponentId.Index
    components.MinSize.[index] <- {Width = 0f; Height = 0f}
    components.Bounds.[index] <- {X = 0f; Y = 0f; Width = 40f; Height = 0f}
    components.Padding.[index] <- {NoobishPadding.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

    let font = createFont ()
    NoobishMeasureV2.measureFrameWith (fun _ -> font) (fun _ -> 10) components

    Assert.AreEqual(40f, components.ContentSize.[index].Width)
    Assert.Greater(components.ContentSize.[index].Height, 10f)

[<Test>]
let ``measureFrameWith wraps text using parent bounds when available`` () =
    let components = NoobishComponentsV2(2)
    let parentCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginParagraph "x x x x x x x x" parentCtx
    let parentIndex = int parentCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index
    components.MinSize.[childIndex] <- {Width = 0f; Height = 0f}
    components.Bounds.[parentIndex] <- {X = 0f; Y = 0f; Width = 30f; Height = 0f}
    components.Padding.[parentIndex] <- {NoobishPadding.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    components.Margin.[childIndex] <- {NoobishMargin.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

    let font = createFont ()
    NoobishMeasureV2.measureFrameWith (fun _ -> font) (fun _ -> 10) components

    Assert.AreEqual(30f, components.ContentSize.[childIndex].Width)
    Assert.Greater(components.ContentSize.[childIndex].Height, 10f)

[<Test>]
let ``measureFrameWith sizes horizontal containers from children`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackHorizontal
    let child1Ctx = NoobishV2.beginLabel "A" rootCtx
    let child1Id = child1Ctx.ComponentId
    let rootCtx = NoobishV2.endLabel child1Ctx
    let child2Ctx = NoobishV2.beginLabel "B" rootCtx
    let child2Id = child2Ctx.ComponentId
    let _ = NoobishV2.endLabel child2Ctx
    let rootId = rootCtx.ComponentId

    let font = createFont ()
    NoobishMeasureV2.measureFrameWith (fun _ -> font) (fun _ -> 10) components

    let rootSize = components.ContentSize.[int rootId.Index]
    let child1Size = components.ContentSize.[int child1Id.Index]
    let child2Size = components.ContentSize.[int child2Id.Index]
    Assert.AreEqual(child1Size.Width + child2Size.Width, rootSize.Width)

[<Test>]
let ``measureFrameWith sizes checkbox box from padding`` () =
    let components = NoobishComponentsV2(3)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginCheckbox "Option" 1us
    let boxIndex = int ctx.ComponentId.Index
    components.Padding.[boxIndex] <- {NoobishPadding.Top = 2f; Right = 2f; Bottom = 2f; Left = 2f}

    let font = createFont ()
    NoobishMeasureV2.measureFrameWith (fun _ -> font) (fun _ -> 10) components

    let boxSize = components.ContentSize.[boxIndex]
    Assert.AreEqual(4f, boxSize.Width)
    Assert.AreEqual(4f, boxSize.Height)

[<Test>]
let ``measureFrameWith keeps checkbox box square for min height`` () =
    let components = NoobishComponentsV2(3)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginCheckbox "Option" 1us
        |> NoobishV2.setMinHeight 20f
    let boxIndex = int ctx.ComponentId.Index

    let font = createFont ()
    NoobishMeasureV2.measureFrameWith (fun _ -> font) (fun _ -> 10) components

    let boxSize = components.ContentSize.[boxIndex]
    Assert.AreEqual(20f, boxSize.Width)
    Assert.AreEqual(20f, boxSize.Height)

[<Test>]
let ``measureFrame applies slider height from style`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSlider (0f, 10f) 1f 0f 1us
    let index = int ctx.ComponentId.Index
    components.MinSize.[index] <- {Width = 0f; Height = 0f}
    let styleSheet = createStyleSheet 4f 16f

    NoobishMeasureV2.measureFrame Unchecked.defaultof<_> styleSheet components

    Assert.AreEqual(16f, components.ContentSize.[index].Height)

[<Test>]
let ``measureFrame applies progress bar height from style`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginProgressBar 0.5f
    let index = int ctx.ComponentId.Index
    let styleSheet = createProgressStyleSheet 12f

    NoobishMeasureV2.measureFrame Unchecked.defaultof<_> styleSheet components

    Assert.AreEqual(12f, components.ContentSize.[index].Height)

[<Test>]
let ``measureFrame applies style padding and margin defaults`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
    let index = int ctx.ComponentId.Index
    let padding = {NoobishPadding.Top = 2f; Right = 3f; Bottom = 4f; Left = 5f}
    let margin = {NoobishMargin.Top = 6f; Right = 7f; Bottom = 8f; Left = 9f}
    let styleSheet = createSpacingStyleSheet padding margin

    NoobishMeasureV2.measureFrame Unchecked.defaultof<_> styleSheet components

    Assert.AreEqual(2f, components.Padding.[index].Top)
    Assert.AreEqual(3f, components.Padding.[index].Right)
    Assert.AreEqual(4f, components.Padding.[index].Bottom)
    Assert.AreEqual(5f, components.Padding.[index].Left)
    Assert.AreEqual(6f, components.Margin.[index].Top)
    Assert.AreEqual(7f, components.Margin.[index].Right)
    Assert.AreEqual(8f, components.Margin.[index].Bottom)
    Assert.AreEqual(9f, components.Margin.[index].Left)

[<Test>]
let ``measureFrame keeps overridden padding and margin`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
        |> NoobishV2.setPadding {NoobishPadding.Top = 1f; Right = 1f; Bottom = 1f; Left = 1f}
        |> NoobishV2.setMargin {NoobishMargin.Top = 2f; Right = 2f; Bottom = 2f; Left = 2f}
    let index = int ctx.ComponentId.Index
    let padding = {NoobishPadding.Top = 9f; Right = 9f; Bottom = 9f; Left = 9f}
    let margin = {NoobishMargin.Top = 8f; Right = 8f; Bottom = 8f; Left = 8f}
    let styleSheet = createSpacingStyleSheet padding margin

    NoobishMeasureV2.measureFrame Unchecked.defaultof<_> styleSheet components

    Assert.AreEqual(1f, components.Padding.[index].Top)
    Assert.AreEqual(1f, components.Padding.[index].Right)
    Assert.AreEqual(1f, components.Padding.[index].Bottom)
    Assert.AreEqual(1f, components.Padding.[index].Left)
    Assert.AreEqual(2f, components.Margin.[index].Top)
    Assert.AreEqual(2f, components.Margin.[index].Right)
    Assert.AreEqual(2f, components.Margin.[index].Bottom)
    Assert.AreEqual(2f, components.Margin.[index].Left)
