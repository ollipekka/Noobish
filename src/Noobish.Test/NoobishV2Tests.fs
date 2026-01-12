module Noobish.Test.NoobishV2Tests

open NUnit.Framework
open Microsoft.Xna.Framework
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
    let ctx = 
        NoobishV2.beginFrame "Settings/Audio" components
        |> NoobishV2.createComponent "Panel" 0us
    let cid = ctx.ComponentId
    Assert.AreEqual(1, components.Count)
    Assert.AreEqual(cid, components.Id.[0])
    Assert.AreEqual("Panel", components.ThemeId.[0])
    Assert.AreEqual(UIComponentIdV2.empty, components.ParentId.[0])

[<Test>]
let ``header writes text and blocks`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.header "Title"
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Header", components.ThemeId.[index])
    Assert.IsTrue(components.WantsText.[index])
    Assert.AreEqual("Title", components.Text.[index])
    Assert.IsTrue(components.Block.[index])

[<Test>]
let ``label writes text without blocking`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.label "Tag"
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Label", components.ThemeId.[index])
    Assert.IsTrue(components.WantsText.[index])
    Assert.AreEqual("Tag", components.Text.[index])
    Assert.IsFalse(components.Block.[index])

[<Test>]
let ``paragraph enables wrap and fill`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.paragraph "Copy"
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Paragraph", components.ThemeId.[index])
    Assert.IsTrue(components.Textwrap.[index])
    Assert.AreEqual(NoobishAlignment.TopLeft, components.TextAlign.[index])
    Assert.IsTrue(components.Block.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsFalse(components.Fill.[index].Vertical)

[<Test>]
let ``textbox stores text and local id`` () =
    let components = NoobishComponentsV2(1)
    let localId = 11us
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.textbox "Seed" localId
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("TextBox", components.ThemeId.[index])
    Assert.AreEqual("Seed", components.Text.[index])
    Assert.AreEqual(localId, cid.LocalId)
    Assert.IsTrue(components.WantsTextChanged.[index])

[<Test>]
let ``button stores local id`` () =
    let components = NoobishComponentsV2(1)
    let localId = 7us
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.button "Press" localId
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Button", components.ThemeId.[index])
    Assert.IsTrue(components.WantsText.[index])
    Assert.AreEqual("Press", components.Text.[index])
    Assert.AreEqual(localId, cid.LocalId)
    Assert.IsTrue(components.WantsOnClick.[index])
    Assert.IsTrue(components.WantsOnPress.[index])

[<Test>]
let ``space fills in both directions`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.space
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Space", components.ThemeId.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsTrue(components.Fill.[index].Vertical)


[<Test>]
let ``canvas uses relative layout`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.canvas
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Division", components.ThemeId.[index])
    Assert.AreEqual(LayoutV2.Relative cid, components.Layout.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsTrue(components.Fill.[index].Vertical)

[<Test>]
let ``beginStackVertical sets vertical layout`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Division", components.ThemeId.[index])
    Assert.AreEqual(LayoutV2.LinearVertical, components.Layout.[index])
    Assert.IsTrue(components.Block.[index])

[<Test>]
let ``beginStackHorizontal sets horizontal layout`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackHorizontal
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Division", components.ThemeId.[index])
    Assert.AreEqual(LayoutV2.LinearHorizontal, components.Layout.[index])
    Assert.IsTrue(components.Block.[index])

[<Test>]
let ``beginGrid sets grid layout and fill`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginGrid (2, 3)
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Division", components.ThemeId.[index])
    Assert.AreEqual(LayoutV2.Grid(2, 3), components.Layout.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsTrue(components.Fill.[index].Vertical)
