module Noobish.Test.NoobishV2Tests

open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open NUnit.Framework

open Noobish
open Noobish.Styles

let private emptyNested<'T> () =
    Dictionary<string, IReadOnlyDictionary<string, 'T>>() :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>

let private createEmptyStyleSheet () =
    {
        Name = "Test"
        TextureAtlasId = ""
        Widths = emptyNested<float32>()
        Heights = emptyNested<float32>()
        Paddings = emptyNested<NoobishPadding>()
        Margins = emptyNested<NoobishMargin>()
        Colors = emptyNested<NoobishColor>()
        Fonts = emptyNested<string>()
        FontSizes = emptyNested<int>()
        FontColors = emptyNested<NoobishColor>()
        TextAlignments = emptyNested<NoobishAlignment>()
        Drawables = emptyNested<NoobishDrawable[]>()
    }

let private createMeasureProvider (styleSheet: NoobishStyleSheet) =
    { new INoobishMeasureProvider with
        member _.GetFontSize _ _ = 0
        member _.MeasureSingleLine _ _ _ _ = struct(0f, 0f)
        member _.MeasureMultiLine _ _ _ _ _ = struct(0f, 0f)
        member _.GetMargin themeId state = styleSheet.GetMargin themeId state
        member _.GetPadding themeId state = styleSheet.GetPadding themeId state
        member _.GetHeight themeId state = styleSheet.GetHeight themeId state }

[<Test>]
let ``beginFrame resets context`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Settings/Audio" components
    Assert.AreEqual(0, ctx.FrameId)
    Assert.AreEqual("Settings/Audio", ctx.Page)
    Assert.AreEqual(NamespaceHash.fromPage "Settings/Audio", ctx.NamespaceId)
    Assert.AreEqual(UIComponentIdV2.empty, ctx.ParentId)

[<Test>]
let ``beginFrame throws when components are not cleared`` () =
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    Assert.Throws<System.InvalidOperationException>(fun () ->
        NoobishV2.beginFrame "Settings/Audio" components |> ignore)
    |> ignore

[<Test>]
let ``processFrame runs layout and input`` () =
    let components = NoobishComponentsV2(1)
    let inputBuffer = InputBufferV2(4)
    let styleSheet = createEmptyStyleSheet ()
    let inputState =
        { new INoobishInputState with
            member _.PointerX = 0f
            member _.PointerY = 0f
            member _.ScrollWheelDelta = 0f
            member _.IsMouseClick _ = false
            member _.IsMouseDown _ = false
            member _.IsKeyPressed _ = false
            member _.ConsumeTextInput() = struct([||], 0) }

    let sliderId = 3us
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSlider (0f, 1f) 0.1f 0.5f sliderId
        |> NoobishV2.setMinHeight 10f
    NoobishV2.endSlider ctx |> ignore

    let provider = createMeasureProvider styleSheet
    NoobishV2MonoGame.processFrameWith provider components 100f 40f inputState inputBuffer

    Assert.IsTrue(inputBuffer.LocalIdToIndex.ContainsKey sliderId)
    let bounds = components.Bounds.[0]
    Assert.AreEqual(100f, bounds.Width)
    Assert.AreEqual(10f, bounds.Height)

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
let ``beginOverlayRoot configures full screen container`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginOverlayRoot 10us
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(LayoutV2.Relative ctx.ComponentId, components.Layout.[index])
    Assert.AreEqual({Fill.Horizontal = true; Vertical = true}, components.Fill.[index])
    Assert.IsTrue(components.Block.[index])
    Assert.AreEqual(200, components.Layer.[index])

[<Test>]
let ``beginOverlayScrim captures clicks and fills`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginOverlayRoot 10us
    let scrimCtx = NoobishV2.beginOverlayScrim 11us rootCtx
    let index = int scrimCtx.ComponentId.Index
    Assert.AreEqual({Fill.Horizontal = true; Vertical = true}, components.Fill.[index])
    Assert.IsTrue(components.WantsOnClick.[index])
    Assert.IsTrue(components.WantsOnPress.[index])
    Assert.IsTrue(components.PaddingOverride.[index])
    Assert.AreEqual(NoobishPadding.empty, components.Padding.[index])
    Assert.AreEqual(201, components.Layer.[index])

[<Test>]
let ``beginOverlayPanel creates stacked panel`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginOverlayRoot 10us
    let panelCtx = NoobishV2.beginOverlayPanel 12us rootCtx
    let index = int panelCtx.ComponentId.Index
    Assert.AreEqual(LayoutV2.LinearVertical, components.Layout.[index])
    Assert.IsTrue(components.Block.[index])
    Assert.AreEqual(202, components.Layer.[index])

[<Test>]
let ``setLayer updates component layer`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let index = int ctx.ComponentId.Index
    let _ = NoobishV2.setLayer 42 ctx
    Assert.AreEqual(42, components.Layer.[index])

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
    let components = NoobishComponentsV2(3)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginCheckbox "Option" 5us
    let boxIndex = int ctx.ComponentId.Index
    let containerIndex = int components.ParentId.[boxIndex].Index
    let labelIndex = int components.Children.[containerIndex].[1].Index
    Assert.AreEqual("Division", components.ThemeId.[containerIndex])
    Assert.AreEqual(LayoutV2.LinearHorizontal, components.Layout.[containerIndex])
    Assert.IsTrue(components.Block.[containerIndex])
    Assert.AreEqual("Checkbox", components.ThemeId.[boxIndex])
    Assert.IsTrue(components.WantsOnClick.[boxIndex])
    Assert.IsTrue(components.WantsOnPress.[boxIndex])
    Assert.IsTrue(components.WantsToggle.[boxIndex])
    Assert.AreEqual("Label", components.ThemeId.[labelIndex])
    Assert.IsTrue(components.WantsText.[labelIndex])
    Assert.AreEqual("Option", components.Text.[labelIndex])

[<Test>]
let ``slider stores range and value`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSlider (0f, 10f) 0.5f 3.5f 12us
    let index = int ctx.ComponentId.Index
    Assert.AreEqual("Slider", components.ThemeId.[index])
    Assert.AreEqual(12us, ctx.ComponentId.LocalId)
    Assert.IsTrue(components.WantsOnPress.[index])
    Assert.IsTrue(components.WantsSlider.[index])
    Assert.AreEqual(0f, components.SliderMin.[index])
    Assert.AreEqual(10f, components.SliderMax.[index])
    Assert.AreEqual(0.5f, components.SliderStep.[index])
    Assert.AreEqual(3.5f, components.SliderValue.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsFalse(components.Fill.[index].Vertical)


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
let ``setFillHorizontal preserves vertical`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
        |> NoobishV2.setFill {Horizontal = false; Vertical = true}
        |> NoobishV2.setFillHorizontal
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsTrue(components.Fill.[index].Vertical)

[<Test>]
let ``setFillVertical preserves horizontal`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
        |> NoobishV2.setFill {Horizontal = true; Vertical = false}
        |> NoobishV2.setFillVertical
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsTrue(components.Fill.[index].Vertical)

[<Test>]
let ``setScroll updates scroll flags`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
        |> NoobishV2.setScroll {Horizontal = true; Vertical = true}
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.Scroll.[index].Horizontal)
    Assert.IsTrue(components.Scroll.[index].Vertical)

[<Test>]
let ``setScrollVertical and setScrollHorizontal set axis`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
        |> NoobishV2.setScrollVertical
        |> NoobishV2.setScrollHorizontal
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.Scroll.[index].Horizontal)
    Assert.IsTrue(components.Scroll.[index].Vertical)

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
    Assert.IsTrue(components.PaddingOverride.[index])

[<Test>]
let ``setMargin writes margin values`` () =
    let components = NoobishComponentsV2(1)
    let margin = {NoobishMargin.Top = 5f; Right = 6f; Bottom = 7f; Left = 8f}
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginSpace
        |> NoobishV2.setMargin margin
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(5f, components.Margin.[index].Top)
    Assert.AreEqual(6f, components.Margin.[index].Right)
    Assert.AreEqual(7f, components.Margin.[index].Bottom)
    Assert.AreEqual(8f, components.Margin.[index].Left)
    Assert.IsTrue(components.MarginOverride.[index])

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
let ``setRowspan updates grid span`` () =
    let components = NoobishComponentsV2(2)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginGrid (2, 2)
        |> NoobishV2.beginLabel "Cell"
        |> NoobishV2.setRowspan 2
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(2, components.GridSpan.[index].Rowspan)
    Assert.AreEqual(1, components.GridSpan.[index].Colspan)

[<Test>]
let ``setColspan updates grid span`` () =
    let components = NoobishComponentsV2(2)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginGrid (2, 2)
        |> NoobishV2.beginLabel "Cell"
        |> NoobishV2.setColspan 2
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(1, components.GridSpan.[index].Rowspan)
    Assert.AreEqual(2, components.GridSpan.[index].Colspan)

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

[<Test>]
let ``progressBar stores value`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginProgressBar 0.25f
    let index = int ctx.ComponentId.Index
    Assert.AreEqual("ProgressBar", components.ThemeId.[index])
    Assert.IsTrue(components.WantsProgress.[index])
    Assert.AreEqual(0.25f, components.ProgressValue.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsFalse(components.Fill.[index].Vertical)

[<Test>]
let ``setProgressSegments clamps at one`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginProgressBar 0.4f
        |> NoobishV2.setProgressSegments 0
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(1, components.ProgressSegments.[index])

[<Test>]
let ``setProgressSegments clamps negative to one`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginProgressBar 0.4f
        |> NoobishV2.setProgressSegments -2
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(1, components.ProgressSegments.[index])

[<Test>]
let ``setProgressSegments keeps positive values`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginProgressBar 0.4f
        |> NoobishV2.setProgressSegments 3
    let index = int ctx.ComponentId.Index
    Assert.AreEqual(3, components.ProgressSegments.[index])

[<Test>]
let ``setProgress enables progress rendering on buttons`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginButton "Build Unit" 1us
        |> NoobishV2.setProgress 0.6f
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.WantsOnClick.[index])
    Assert.IsTrue(components.WantsOnPress.[index])
    Assert.IsTrue(components.WantsProgress.[index])
    Assert.AreEqual(0.6f, components.ProgressValue.[index])

[<Test>]
let ``setProgressStyle stores radial style`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginButton "Build Unit" 1us
        |> NoobishV2.setProgressStyle NoobishProgressStyle.Radial
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.WantsProgress.[index])
    Assert.AreEqual(NoobishProgressStyle.Radial, components.ProgressStyle.[index])

[<Test>]
let ``setProgressStyle stores square radial style`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginButton "Build Unit" 1us
        |> NoobishV2.setProgressStyle NoobishProgressStyle.RadialSquare
    let index = int ctx.ComponentId.Index
    Assert.IsTrue(components.WantsProgress.[index])
    Assert.AreEqual(NoobishProgressStyle.RadialSquare, components.ProgressStyle.[index])

[<Test>]
let ``horizontalRule fills horizontally and blocks`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginHorizontalRule
    let index = int ctx.ComponentId.Index
    Assert.AreEqual("HorizontalRule", components.ThemeId.[index])
    Assert.IsTrue(components.Block.[index])
    Assert.IsTrue(components.Fill.[index].Horizontal)
    Assert.IsFalse(components.Fill.[index].Vertical)
