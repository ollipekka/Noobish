module Noobish.Test.NoobishRenderV2Tests

open Microsoft.Xna.Framework
open NUnit.Framework
open Noobish
open Noobish.Internal

[<Test>]
let ``resolveState maps enabled flag`` () =
    Assert.AreEqual("default", NoobishRenderV2.resolveState true false false false)
    Assert.AreEqual("disabled", NoobishRenderV2.resolveState false false false false)

[<Test>]
let ``resolveState toggled wins over default`` () =
    Assert.AreEqual("toggled", NoobishRenderV2.resolveState true true false false)

[<Test>]
let ``resolveState hovered when enabled`` () =
    Assert.AreEqual("hovered", NoobishRenderV2.resolveState true false true false)

[<Test>]
let ``resolveState toggledHovered when both`` () =
    Assert.AreEqual("toggledHovered", NoobishRenderV2.resolveState true true true false)

[<Test>]
let ``resolveState focused overrides hover`` () =
    Assert.AreEqual("focused", NoobishRenderV2.resolveState true false true true)

[<Test>]
let ``computeTextBounds applies padding and clamps size`` () =
    let bounds: NoobishRectangle = {X = 10f; Y = 20f; Width = 100f; Height = 50f}
    let padding = {NoobishPadding.Top = 5f; Right = 10f; Bottom = 15f; Left = 7f}
    let result = NoobishRenderV2.computeTextBounds bounds padding
    Assert.AreEqual(17f, result.X)
    Assert.AreEqual(25f, result.Y)
    Assert.AreEqual(83f, result.Width)
    Assert.AreEqual(30f, result.Height)

[<Test>]
let ``computeTextBounds clamps negative dimensions`` () =
    let bounds: NoobishRectangle = {X = 0f; Y = 0f; Width = 5f; Height = 4f}
    let padding = {NoobishPadding.Top = 4f; Right = 5f; Bottom = 6f; Left = 3f}
    let result = NoobishRenderV2.computeTextBounds bounds padding
    Assert.AreEqual(3f, result.X)
    Assert.AreEqual(4f, result.Y)
    Assert.AreEqual(0f, result.Width)
    Assert.AreEqual(0f, result.Height)

[<Test>]
let ``computeSliderPinBounds centers pin and respects range`` () =
    let bounds: NoobishRectangle = {X = 0f; Y = 0f; Width = 100f; Height = 20f}
    let padding = {NoobishPadding.Top = 0f; Right = 0f; Bottom = 0f; Left = 10f}
    let result = NoobishRenderV2.computeSliderPinBounds bounds padding 0f 10f 5f 10f 10f
    Assert.AreEqual(50f, result.X)
    Assert.AreEqual(5f, result.Y)
    Assert.AreEqual(10f, result.Width)
    Assert.AreEqual(10f, result.Height)

[<Test>]
let ``computeSliderPinBounds clamps value to range`` () =
    let bounds: NoobishRectangle = {X = 0f; Y = 0f; Width = 100f; Height = 10f}
    let padding = {NoobishPadding.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    let result = NoobishRenderV2.computeSliderPinBounds bounds padding 0f 10f 50f 10f 10f
    Assert.AreEqual(90f, result.X)

[<Test>]
let ``computeSliderTrackBounds centers track`` () =
    let bounds: NoobishRectangle = {X = 0f; Y = 0f; Width = 100f; Height = 20f}
    let padding = {NoobishPadding.Top = 2f; Right = 3f; Bottom = 4f; Left = 5f}
    let result = NoobishRenderV2.computeSliderTrackBounds bounds padding 4f
    Assert.AreEqual(5f, result.X)
    Assert.AreEqual(7f, result.Y)
    Assert.AreEqual(92f, result.Width)
    Assert.AreEqual(4f, result.Height)

[<Test>]
let ``computeProgressBounds clamps width`` () =
    let bounds: NoobishRectangle = {X = 0f; Y = 0f; Width = 50f; Height = 10f}
    let padding = {NoobishPadding.Top = 1f; Right = 2f; Bottom = 1f; Left = 2f}
    let result = NoobishRenderV2.computeProgressBounds bounds padding 1.5f
    Assert.AreEqual(2f, result.X)
    Assert.AreEqual(46f, result.Width)

[<Test>]
let ``computeProgressSegmentWidth accounts for gaps`` () =
    let width = NoobishRenderV2.computeProgressSegmentWidth 100f 4 2f
    Assert.AreEqual(23.5f, width)

[<Test>]
let ``computeProgressSegmentWidth returns zero when segments non-positive`` () =
    let zeroSegments = NoobishRenderV2.computeProgressSegmentWidth 100f 0 2f
    let negativeSegments = NoobishRenderV2.computeProgressSegmentWidth 100f -2 2f
    Assert.AreEqual(0f, zeroSegments)
    Assert.AreEqual(0f, negativeSegments)

[<Test>]
let ``offsetBounds applies offsets`` () =
    let bounds: NoobishRectangle = { X = 1f; Y = 2f; Width = 3f; Height = 4f }
    let result = NoobishRenderV2.offsetBounds bounds 5f -2f
    Assert.AreEqual(6f, result.X)
    Assert.AreEqual(0f, result.Y)
    Assert.AreEqual(3f, result.Width)
    Assert.AreEqual(4f, result.Height)

[<Test>]
let ``computeProgressSegmentWidth handles non-positive gaps`` () =
    let zeroGap = NoobishRenderV2.computeProgressSegmentWidth 10f 2 0f
    let negativeGap = NoobishRenderV2.computeProgressSegmentWidth 10f 2 -1f
    Assert.AreEqual(5f, zeroGap)
    Assert.AreEqual(5.5f, negativeGap)

[<Test>]
let ``computeSliderPinBounds handles zero span`` () =
    let bounds: NoobishRectangle = { X = 0f; Y = 0f; Width = 100f; Height = 20f }
    let padding = { NoobishPadding.Top = 0f; Right = 0f; Bottom = 0f; Left = 10f }
    let result = NoobishRenderV2.computeSliderPinBounds bounds padding 5f 5f 10f 10f 10f
    Assert.AreEqual(10f, result.X)
    Assert.AreEqual(5f, result.Y)
    Assert.AreEqual(10f, result.Width)
    Assert.AreEqual(10f, result.Height)

[<Test>]
let ``computeSliderTrackBounds uses full height when trackHeight non-positive`` () =
    let bounds: NoobishRectangle = { X = 0f; Y = 0f; Width = 100f; Height = 20f }
    let padding = { NoobishPadding.Top = 2f; Right = 3f; Bottom = 4f; Left = 5f }
    let result = NoobishRenderV2.computeSliderTrackBounds bounds padding 0f
    Assert.AreEqual(5f, result.X)
    Assert.AreEqual(2f, result.Y)
    Assert.AreEqual(92f, result.Width)
    Assert.AreEqual(14f, result.Height)

[<Test>]
let ``toScissorRectangle floors and ceils bounds`` () =
    let bounds: NoobishRectangle = { X = 1.2f; Y = 2.8f; Width = 3.3f; Height = 4.1f }
    let result = NoobishRenderV2.toScissorRectangle bounds
    Assert.AreEqual(1, result.X)
    Assert.AreEqual(2, result.Y)
    Assert.AreEqual(4, result.Width)
    Assert.AreEqual(5, result.Height)

[<Test>]
let ``toScissorRectangle clamps negative sizes`` () =
    let bounds: NoobishRectangle = { X = 5f; Y = 6f; Width = -2f; Height = -3f }
    let result = NoobishRenderV2.toScissorRectangle bounds
    Assert.AreEqual(5, result.X)
    Assert.AreEqual(6, result.Y)
    Assert.AreEqual(0, result.Width)
    Assert.AreEqual(0, result.Height)
