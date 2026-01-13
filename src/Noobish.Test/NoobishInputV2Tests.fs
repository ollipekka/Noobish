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
