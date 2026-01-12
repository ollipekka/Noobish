module Noobish.Test.NoobishRenderV2Tests

open NUnit.Framework
open Noobish
open Noobish.Styles
open Noobish.Internal

[<Test>]
let ``resolveState maps enabled flag`` () =
    Assert.AreEqual("default", NoobishRenderV2.resolveState true)
    Assert.AreEqual("disabled", NoobishRenderV2.resolveState false)

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
