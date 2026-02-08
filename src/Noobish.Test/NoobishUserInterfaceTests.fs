module Noobish.Test.NoobishUserInterfaceTests

open NUnit.Framework
open Noobish

let private createInput () =
    { new INoobishInputState with
        member _.PointerX = 0f
        member _.PointerY = 0f
        member _.ScrollWheelDelta = 0f
        member _.IsPrimaryClick() = false
        member _.IsPrimaryDown() = false
        member _.IsSecondaryClick() = false
        member _.IsKeyPressed _ = false
        member _.ConsumeTextInput() = struct([||], 0)
        }

[<Test>]
let ``NoobishUserInterface BeginFrame clears components`` () =
    let ui = NoobishUserInterface(4)
    let firstCtx = ui.BeginFrame "Page"
    let buttonCtx = NoobishV2.beginButton "Ok" 1us firstCtx
    Assert.AreEqual(1, ui.Components.Count)

    ui.ReleaseContext buttonCtx
    ui.ReleaseContext firstCtx

    let secondCtx = ui.BeginFrame "Page"
    Assert.AreEqual(0, ui.Components.Count)
    ui.ReleaseContext secondCtx

[<Test>]
let ``NoobishUserInterface queries bounds by local id`` () =
    let ui = NoobishUserInterface(2)
    let frameCtx = ui.BeginFrame "Page"
    let buttonCtx = NoobishV2.beginButton "Ok" 2us frameCtx
    let index = int buttonCtx.ComponentId.Index

    ui.EndFrame(100f, 100f, frameCtx)
    ui.Components.Bounds.[index] <- { X = 10f; Y = 20f; Width = 30f; Height = 40f }
    ui.ProcessInput(createInput())

    match ui.TryGetBounds 2us with
    | ValueSome bounds ->
        Assert.AreEqual(10f, bounds.Left)
        Assert.AreEqual(40f, bounds.Right)
        Assert.AreEqual(20f, bounds.Top)
        Assert.AreEqual(60f, bounds.Bottom)
    | ValueNone ->
        Assert.Fail("Expected bounds for local id.")

    Assert.AreEqual(ValueSome { Width = 30f; Height = 40f }, ui.TryGetSize 2us)
    Assert.IsTrue(ValueOption.isNone (ui.TryGetBounds 99us))

    ui.ReleaseContext buttonCtx
    ui.ReleaseContext frameCtx
