module Noobish.Test.NoobishInputV2Tests

open NUnit.Framework
open Noobish

[<Test>]
let ``InputBufferV2 Reset builds localId map`` () =
    let components = NoobishComponentsV2(3)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 7us ctx
    let _ = NoobishV2.beginLabel "Ignored" ctx
    let buffer = InputBufferV2(3)
    buffer.Reset components
    Assert.IsTrue(buffer.LocalIdToIndex.ContainsKey 7us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 marks and queries input`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 1us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(2)
    buffer.Reset components

    buffer.MarkClicked index
    buffer.MarkPressed index
    buffer.MarkTextChanged(index, "Hello")

    Assert.IsTrue(buffer.WasClicked 1us)
    Assert.IsTrue(buffer.WasPressed 1us)
    Assert.AreEqual(ValueSome "Hello", buffer.TryGetTextChanged 1us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 tracks down and release`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 2us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.SetDown index
    Assert.IsTrue(buffer.IsDown 2us)
    buffer.ClearDown()
    Assert.IsFalse(buffer.IsDown 2us)
    Assert.IsTrue(buffer.WasReleased 2us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 marks hovered`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 1us ctx
    let index = int buttonCtx.ComponentId.Index
    components.Bounds.[index] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}
    let buffer = InputBufferV2(1)

    let input =
        { new INoobishInputState with
            member _.PointerX = 5f
            member _.PointerY = 5f
            member _.IsPrimaryClick() = false
            member _.IsPrimaryDown() = false
            member _.IsSecondaryClick() = false
            member _.IsKeyPressed _ = false }

    NoobishInputV2.process input components buffer

    Assert.IsTrue(components.Hovered.[index])

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx
