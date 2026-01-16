module Noobish.Test.NoobishComponentsV2Tests

open NUnit.Framework
open Noobish

[<Test>]
let ``NoobishComponentsV2 initializes arrays`` () =
    let components = NoobishComponentsV2(4)
    Assert.AreEqual(4, components.Id.Length)
    Assert.AreEqual(4, components.ParentId.Length)
    Assert.AreEqual(4, components.Children.Length)
    Assert.AreEqual(4, components.Layout.Length)
    Assert.AreEqual(4, components.Text.Length)
    Assert.AreEqual(4, components.WantsText.Length)
    Assert.AreEqual(4, components.WantsSlider.Length)
    Assert.AreEqual(4, components.SliderMin.Length)
    Assert.AreEqual(4, components.SliderMax.Length)
    Assert.AreEqual(4, components.SliderStep.Length)
    Assert.AreEqual(4, components.SliderValue.Length)

[<Test>]
let ``NoobishComponentsV2 defaults ParentId to empty`` () =
    let components = NoobishComponentsV2(2)
    Assert.AreEqual(UIComponentIdV2.empty, components.ParentId.[0])
    Assert.AreEqual(UIComponentIdV2.empty, components.ParentId.[1])

[<Test>]
let ``NoobishComponentsV2 defaults toggled to false`` () =
    let components = NoobishComponentsV2(1)
    Assert.IsFalse(components.Toggled.[0])

[<Test>]
let ``NoobishComponentsV2 defaults hovered to false`` () =
    let components = NoobishComponentsV2(1)
    Assert.IsFalse(components.Hovered.[0])

[<Test>]
let ``NoobishComponentsV2 defaults wants toggle to false`` () =
    let components = NoobishComponentsV2(1)
    Assert.IsFalse(components.WantsToggle.[0])

[<Test>]
let ``NoobishComponentsV2 defaults wants slider to false`` () =
    let components = NoobishComponentsV2(1)
    Assert.IsFalse(components.WantsSlider.[0])

[<Test>]
let ``NoobishComponentsV2 isClickable and isPressable respect flags`` () =
    let components = NoobishComponentsV2(1)
    let index = 0
    Assert.IsFalse(NoobishComponentsV2.isClickable components index)
    Assert.IsFalse(NoobishComponentsV2.isPressable components index)

    components.Visible.[index] <- true
    components.Enabled.[index] <- true
    components.WantsOnClick.[index] <- true
    components.WantsOnPress.[index] <- true

    Assert.IsTrue(NoobishComponentsV2.isClickable components index)
    Assert.IsTrue(NoobishComponentsV2.isPressable components index)

    components.Visible.[index] <- false
    Assert.IsFalse(NoobishComponentsV2.isClickable components index)
    Assert.IsFalse(NoobishComponentsV2.isPressable components index)

    components.Visible.[index] <- true
    components.Enabled.[index] <- false
    Assert.IsFalse(NoobishComponentsV2.isClickable components index)
    Assert.IsFalse(NoobishComponentsV2.isPressable components index)

    components.Enabled.[index] <- true
    components.WantsOnClick.[index] <- false
    components.WantsOnPress.[index] <- false
    Assert.IsFalse(NoobishComponentsV2.isClickable components index)
    Assert.IsFalse(NoobishComponentsV2.isPressable components index)

[<Test>]
let ``ComponentContextV2 reset updates frame data`` () =
    let components = NoobishComponentsV2(1)
    let ctx = ComponentContextV2(components)
    ctx.ParentId <- UIComponentIdV2.create 1us 2us 3us 4us
    ctx.Reset(7, "Settings/Audio", 9us)
    Assert.AreEqual(7, ctx.FrameId)
    Assert.AreEqual("Settings/Audio", ctx.Page)
    Assert.AreEqual(9us, ctx.NamespaceId)
    Assert.AreEqual(UIComponentIdV2.empty, ctx.ParentId)

[<Test>]
let ``NoobishComponentsV2 implements interface`` () =
    let components = NoobishComponentsV2(1)
    let iface = components :> INoobishComponents2
    iface.Count <- 1
    iface.RunningId <- 2
    Assert.AreEqual(1, iface.Count)
    Assert.AreEqual(2, iface.RunningId)
    Assert.AreSame(components.Id, iface.Id)
