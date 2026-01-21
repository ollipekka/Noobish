module Noobish.Test.NoobishRenderV2Tests

open NUnit.Framework
open Noobish
open Noobish.Internal
open System.Collections.Generic
open System

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
let ``computeScissorBounds floors and ceils bounds`` () =
    let bounds: NoobishRectangle = { X = 1.2f; Y = 2.8f; Width = 3.3f; Height = 4.1f }
    let result = NoobishRenderV2.computeScissorBounds bounds
    Assert.AreEqual(1f, result.X)
    Assert.AreEqual(2f, result.Y)
    Assert.AreEqual(4f, result.Width)
    Assert.AreEqual(5f, result.Height)

[<Test>]
let ``computeScissorBounds clamps negative sizes`` () =
    let bounds: NoobishRectangle = { X = 5f; Y = 6f; Width = -2f; Height = -3f }
    let result = NoobishRenderV2.computeScissorBounds bounds
    Assert.AreEqual(5f, result.X)
    Assert.AreEqual(6f, result.Y)
    Assert.AreEqual(0f, result.Width)
    Assert.AreEqual(0f, result.Height)

[<Test>]
let ``resolveCaretBlinkStart persists local id start across frames`` () =
    let localMap = Dictionary<uint32, TimeSpan>()
    let indexMap = Dictionary<int, TimeSpan>()
    let componentId = UIComponentIdV2.create 1us 0us 0us 7us
    let start = NoobishRenderV2.resolveCaretBlinkStart localMap indexMap componentId 0 (TimeSpan.FromSeconds 1.0) true
    let next = NoobishRenderV2.resolveCaretBlinkStart localMap indexMap componentId 0 (TimeSpan.FromSeconds 3.0) false
    Assert.AreEqual(TimeSpan.FromSeconds 1.0, start)
    Assert.AreEqual(TimeSpan.FromSeconds 1.0, next)

[<Test>]
let ``resolveCaretBlinkStart stores local id when missing`` () =
    let localMap = Dictionary<uint32, TimeSpan>()
    let indexMap = Dictionary<int, TimeSpan>()
    let componentId = UIComponentIdV2.create 2us 0us 0us 7us
    let now = TimeSpan.FromSeconds 10.0

    let start = NoobishRenderV2.resolveCaretBlinkStart localMap indexMap componentId 0 now false

    let key = (uint32 componentId.Namespace <<< 16) ||| uint32 componentId.LocalId
    Assert.AreEqual(now, start)
    Assert.AreEqual(now, localMap.[key])

[<Test>]
let ``resolveCaretBlinkStart falls back to index when local id is zero`` () =
    let localMap = Dictionary<uint32, TimeSpan>()
    let indexMap = Dictionary<int, TimeSpan>()
    let componentId = UIComponentIdV2.create 1us 0us 0us 0us
    let start = NoobishRenderV2.resolveCaretBlinkStart localMap indexMap componentId 4 (TimeSpan.FromSeconds 2.0) true
    let next = NoobishRenderV2.resolveCaretBlinkStart localMap indexMap componentId 4 (TimeSpan.FromSeconds 5.0) false
    Assert.AreEqual(TimeSpan.FromSeconds 2.0, start)
    Assert.AreEqual(TimeSpan.FromSeconds 2.0, next)

[<Test>]
let ``computeScrollTrackBounds reserves space for other axis`` () =
    let content: NoobishRectangle = { X = 0f; Y = 0f; Width = 100f; Height = 200f }
    let vertical = NoobishRenderV2.computeScrollTrackBounds content 8f false true
    let horizontal = NoobishRenderV2.computeScrollTrackBounds content 8f true true
    Assert.AreEqual(92f, vertical.X)
    Assert.AreEqual(0f, vertical.Y)
    Assert.AreEqual(8f, vertical.Width)
    Assert.AreEqual(192f, vertical.Height)
    Assert.AreEqual(0f, horizontal.X)
    Assert.AreEqual(192f, horizontal.Y)
    Assert.AreEqual(92f, horizontal.Width)
    Assert.AreEqual(8f, horizontal.Height)

[<Test>]
let ``computeScrollTrackBounds returns empty when thickness is zero`` () =
    let content: NoobishRectangle = { X = 10f; Y = 20f; Width = 100f; Height = 50f }
    let horizontal = NoobishRenderV2.computeScrollTrackBounds content 0f true false
    let vertical = NoobishRenderV2.computeScrollTrackBounds content 0f false false
    Assert.AreEqual(0f, horizontal.Width)
    Assert.AreEqual(0f, horizontal.Height)
    Assert.AreEqual(0f, vertical.Width)
    Assert.AreEqual(0f, vertical.Height)

[<Test>]
let ``computeScrollPinBounds positions pin based on scroll`` () =
    let track: NoobishRectangle = { X = 0f; Y = 0f; Width = 8f; Height = 100f }
    let pin = NoobishRenderV2.computeScrollPinBounds track 50f 100f -25f 10f false
    Assert.AreEqual(0f, pin.X)
    Assert.AreEqual(25f, pin.Y)
    Assert.AreEqual(8f, pin.Width)
    Assert.AreEqual(50f, pin.Height)

[<Test>]
let ``computeScrollPinBounds enforces minimum length`` () =
    let track: NoobishRectangle = { X = 0f; Y = 0f; Width = 8f; Height = 100f }
    let pin = NoobishRenderV2.computeScrollPinBounds track 10f 500f -245f 20f false
    Assert.AreEqual(20f, pin.Height)
    Assert.AreEqual(40f, pin.Y)

[<Test>]
let ``computeScrollPinBounds handles horizontal scroll`` () =
    let track: NoobishRectangle = { X = 5f; Y = 7f; Width = 120f; Height = 6f }
    let pin = NoobishRenderV2.computeScrollPinBounds track 60f 180f -30f 10f true
    Assert.AreEqual(7f, pin.Y)
    Assert.AreEqual(6f, pin.Height)
    Assert.AreEqual(40f, pin.Width)
    Assert.AreEqual(25f, pin.X)

[<Test>]
let ``computeScrollPinBounds returns empty when no overflow`` () =
    let track: NoobishRectangle = { X = 0f; Y = 0f; Width = 8f; Height = 100f }
    let pin = NoobishRenderV2.computeScrollPinBounds track 100f 100f 0f 10f false
    Assert.AreEqual(0f, pin.Width)
    Assert.AreEqual(0f, pin.Height)
