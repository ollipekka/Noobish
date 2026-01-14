module Noobish.Test.NoobishMeasureV2Tests

open System.Collections.Generic
open NUnit.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish

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
