module Noobish.Test.NoobishV2Tests

open NUnit.Framework
open Noobish

[<Test>]
let ``beginFrame resets context`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Settings/Audio" components
    Assert.AreEqual(42, ctx.FrameId)
    Assert.AreEqual("Settings/Audio", ctx.Page)
    Assert.AreEqual(UIComponentIdV2.empty, ctx.ParentId)

[<Test>]
let ``createComponent stores id and theme`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Settings/Audio" components
    let struct (_, cid) = NoobishV2.createComponent "Panel" ctx
    Assert.AreEqual(1, components.Count)
    Assert.AreEqual(cid, components.Id.[0])
    Assert.AreEqual("Panel", components.ThemeId.[0])
    Assert.AreEqual(UIComponentIdV2.empty, components.ParentId.[0])
