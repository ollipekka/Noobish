module Noobish.MonoGame.Test.StyleSheetReaderTests

open NUnit.Framework
open Noobish
open Noobish.Styles
open Noobish.PipelineExtension.StyleSheetReaderHelpers

[<Test>]
let ``parseTextAlignment maps supported tokens`` () =
    Assert.AreEqual(NoobishAlignment.TopLeft, parseTextAlignment "TopLeft")
    Assert.AreEqual(NoobishAlignment.TopCenter, parseTextAlignment "topCenter")
    Assert.AreEqual(NoobishAlignment.TopRight, parseTextAlignment "top_right")
    Assert.AreEqual(NoobishAlignment.Left, parseTextAlignment "Left")
    Assert.AreEqual(NoobishAlignment.Center, parseTextAlignment "center")
    Assert.AreEqual(NoobishAlignment.Right, parseTextAlignment "right")
    Assert.AreEqual(NoobishAlignment.BottomLeft, parseTextAlignment "bottomLeft")
    Assert.AreEqual(NoobishAlignment.BottomCenter, parseTextAlignment "bottom_center")
    Assert.AreEqual(NoobishAlignment.BottomRight, parseTextAlignment "BottomRight")

[<Test>]
let ``parseTextAlignment rejects unknown tokens`` () =
    let ex = Assert.Throws<System.Exception>(fun () -> parseTextAlignment "middle" |> ignore)
    Assert.IsTrue(ex.Message.Contains("Cannot parse text alignment"))

[<Test>]
let ``parseDrawable maps nine patch`` () =
    let drawable = parseDrawable 1 "button" 0u
    Assert.AreEqual(NoobishDrawable.NinePatch "button", drawable)

[<Test>]
let ``parseDrawable maps nine patch with color`` () =
    let drawable = parseDrawable 2 "button" 0xFF00FF00u
    match drawable with
    | NoobishDrawable.NinePatchWithColor (name, color) ->
        Assert.AreEqual("button", name)
        Assert.AreEqual(NoobishColor.fromRgba32 0xFF00FF00u, color)
    | _ ->
        Assert.Fail("Expected NinePatchWithColor.")

[<Test>]
let ``parseDrawable rejects unknown kind`` () =
    let ex = Assert.Throws<System.Exception>(fun () -> parseDrawable 3 "broken" 0u |> ignore)
    Assert.IsTrue(ex.Message.Contains("Mangled drawable"))
