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
