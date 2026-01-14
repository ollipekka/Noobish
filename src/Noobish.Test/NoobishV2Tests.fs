module Noobish.Test.NoobishV2Tests

open NUnit.Framework

open Noobish

[<Test>]
let ``beginFrame resets context`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Settings/Audio" components
    Assert.AreEqual(0, ctx.FrameId)
    Assert.AreEqual("Settings/Audio", ctx.Page)
    Assert.AreEqual(NamespaceHash.fromPage "Settings/Audio", ctx.NamespaceId)
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
let ``endPanel returns parent context`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginLabel "Nested" rootCtx
    let childId = childCtx.ComponentId
    let rootId = rootCtx.ComponentId
    Assert.AreEqual(rootId, components.ParentId.[int childId.Index])
    Assert.AreEqual(1, components.Children.[int rootId.Index].Count)
    let parentCtx = NoobishV2.endPanel childCtx
    Assert.AreEqual(rootCtx.ComponentId, parentCtx.ComponentId)
    Assert.AreEqual(UIComponentIdV2.empty, parentCtx.ParentId)

[<Test>]
let ``header writes text and blocks`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginHeader "Title"
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Header", components.ThemeId.[index])
    Assert.AreEqual("Title", components.Text.[index])
    Assert.IsTrue(components.Block.[index])

[<Test>]
let ``label writes text without blocking`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginLabel "Tag"
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Label", components.ThemeId.[index])
    Assert.AreEqual("Tag", components.Text.[index])
    Assert.IsFalse(components.Block.[index])

[<Test>]
let ``paragraph enables wrap and fill`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginParagraph "Copy"
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
        |> NoobishV2.beginTextbox "Seed" localId
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
        |> NoobishV2.beginButton "Press" localId
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Button", components.ThemeId.[index])
    Assert.IsTrue(components.WantsText.[index])
    Assert.AreEqual("Press", components.Text.[index])
    Assert.AreEqual(localId, cid.LocalId)
    Assert.IsTrue(components.WantsOnClick.[index])
    Assert.IsTrue(components.WantsOnPress.[index])

[<Test>]
let ``endButton returns parent context`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginButton "Ok" 1us rootCtx
    let parentCtx = NoobishV2.endButton childCtx
    Assert.AreEqual(rootCtx.ComponentId, parentCtx.ComponentId)
    Assert.AreEqual(UIComponentIdV2.empty, parentCtx.ParentId)

[<Test>]
let ``space fills in both directions`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Space", components.ThemeId.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsTrue(components.Fill.[index].Vertical)

[<Test>]
let ``checkbox opts into toggle`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginCheckbox "Option" 5us
    let index = int ctx.ComponentId.Index
    Assert.AreEqual("Checkbox", components.ThemeId.[index])
    Assert.IsTrue(components.WantsOnClick.[index])
    Assert.IsTrue(components.WantsToggle.[index])


[<Test>]
let ``canvas uses relative layout`` () =
    let components = NoobishComponentsV2(1)
    let ctx = 
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginCanvas
    let cid = ctx.ComponentId
    let index = int cid.Index
    Assert.AreEqual("Division", components.ThemeId.[index])
    Assert.AreEqual(LayoutV2.Relative cid, components.Layout.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsTrue(components.Fill.[index].Vertical)

[<Test>]
let ``setFill updates fill flags`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
        |> NoobishV2.setFill {Horizontal = true; Vertical = false}
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsFalse(components.Fill.[index].Vertical)

[<Test>]
let ``setPadding writes padding values`` () =
    let components = NoobishComponentsV2(1)
    let padding = {NoobishPadding.Top = 1f; Right = 2f; Bottom = 3f; Left = 4f}
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
        |> NoobishV2.setPadding padding
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(1f, components.Padding.[index].Top)
    Assert.AreEqual(2f, components.Padding.[index].Right)
    Assert.AreEqual(3f, components.Padding.[index].Bottom)
    Assert.AreEqual(4f, components.Padding.[index].Left)

[<Test>]
let ``setToggled updates flag`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginButton "Ok" 1us
        |> NoobishV2.setToggled true
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.Toggled.[index])

[<Test>]
let ``setMinSize writes size values`` () =
    let components = NoobishComponentsV2(1)
    let size = {Width = 12f; Height = 34f}
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
        |> NoobishV2.setMinSize size
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(12f, components.MinSize.[index].Width)
    Assert.AreEqual(34f, components.MinSize.[index].Height)

[<Test>]
let ``setMinWidth updates only width`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
        |> NoobishV2.setMinSize {Width = 10f; Height = 20f}
        |> NoobishV2.setMinWidth 42f
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(42f, components.MinSize.[index].Width)
    Assert.AreEqual(20f, components.MinSize.[index].Height)

[<Test>]
let ``setMinHeight updates only height`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
        |> NoobishV2.setMinSize {Width = 10f; Height = 20f}
        |> NoobishV2.setMinHeight 55f
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(10f, components.MinSize.[index].Width)
    Assert.AreEqual(55f, components.MinSize.[index].Height)

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
