module Noobish.Test.NoobishLayoutV2Tests

open NUnit.Framework
open Noobish

[<Test>]
let ``computeAvailableContent clamps to zero`` () =
    let margin = {NoobishMargin.Top = 1f; Right = 4f; Bottom = 1f; Left = 3f}
    let padding = {NoobishPadding.Top = 2f; Right = 2f; Bottom = 3f; Left = 2f}
    let struct(width, height) = NoobishLayoutV2.computeAvailableContent 5f 10f margin padding
    Assert.AreEqual(0f, width)
    Assert.AreEqual(3f, height)

[<Test>]
let ``resolveContentSize respects fill and scroll overflow`` () =
    let minSize = {Width = 2f; Height = 2f}
    let contentSize = {Width = 8f; Height = 8f}
    let fill: Fill = {Horizontal = false; Vertical = true}
    let scroll = {Horizontal = true; Vertical = false}
    let struct(width, height) = NoobishLayoutV2.resolveContentSize minSize contentSize fill scroll 5f 6f
    Assert.AreEqual(5f, width)
    Assert.AreEqual(6f, height)

[<Test>]
let ``computeOuterSize helpers include padding and margin`` () =
    let minSize = {Width = 4f; Height = 4f}
    let contentSize = {Width = 10f; Height = 12f}
    let padding = {NoobishPadding.Top = 2f; Right = 3f; Bottom = 1f; Left = 1f}
    let margin = {NoobishMargin.Top = 1f; Right = 2f; Bottom = 2f; Left = 4f}
    let struct(vOuterContent, vOuterMin) = NoobishLayoutV2.computeOuterSizeVertical minSize contentSize padding margin
    let struct(hOuterContent, hOuterMin) = NoobishLayoutV2.computeOuterSizeHorizontal minSize contentSize padding margin
    Assert.AreEqual(18f, vOuterContent)
    Assert.AreEqual(10f, vOuterMin)
    Assert.AreEqual(20f, hOuterContent)
    Assert.AreEqual(14f, hOuterMin)

[<Test>]
let ``shouldFill respects fill and scroll overflow`` () =
    Assert.IsTrue(NoobishLayoutV2.shouldFill true false 5f 10f)
    Assert.IsTrue(NoobishLayoutV2.shouldFill false true 12f 10f)
    Assert.IsFalse(NoobishLayoutV2.shouldFill false true 5f 10f)

[<Test>]
let ``computeFillShare handles zero count`` () =
    Assert.AreEqual(0f, NoobishLayoutV2.computeFillShare 10f 0)
    Assert.AreEqual(5f, NoobishLayoutV2.computeFillShare 10f 2)

[<Test>]
let ``layoutFrame stacks children to same bounds`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
        |> NoobishV2.setPadding {NoobishPadding.Top = 2f; Right = 3f; Bottom = 4f; Left = 5f}
    let rootId = rootCtx.ComponentId
    components.Layout.[int rootId.Index] <- LayoutV2.Stack
    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}

    let child1Ctx = NoobishV2.beginLabel "One" rootCtx |> NoobishV2.setFill {Horizontal = true; Vertical = true}
    let child2Ctx = NoobishV2.beginLabel "Two" rootCtx |> NoobishV2.setFill {Horizontal = true; Vertical = true}
    let child1 = child1Ctx.ComponentId
    let child2 = child2Ctx.ComponentId

    NoobishLayoutV2.layoutStack components 0f 0f 100f 60f (int rootId.Index)

    let child1Bounds = components.Bounds.[int child1.Index]
    let child2Bounds = components.Bounds.[int child2.Index]
    Assert.AreEqual(5f, child1Bounds.X)
    Assert.AreEqual(2f, child1Bounds.Y)
    Assert.AreEqual(child1Bounds.X, child2Bounds.X)
    Assert.AreEqual(child1Bounds.Y, child2Bounds.Y)
    Assert.AreEqual(92f, child1Bounds.Width)
    Assert.AreEqual(54f, child1Bounds.Height)
    Assert.AreEqual(child1Bounds.Width, child2Bounds.Width)
    Assert.AreEqual(child1Bounds.Height, child2Bounds.Height)

[<Test>]
let ``layoutFrame throws on non-positive grid size`` () =
    let components = NoobishComponentsV2(1)
    let ctx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
    let index = int ctx.ComponentId.Index
    components.Layout.[index] <- LayoutV2.Grid(0, 2)

    let ex = Assert.Throws<System.ArgumentException>(fun () -> NoobishLayoutV2.layoutGrid components 0f 0f 10f 10f index |> ignore)
    Assert.IsTrue(ex.Message.Contains("Grid layout requires positive columns and rows"))

[<Test>]
let ``layoutFrame stacks vertical children and respects fill`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
    let rootId = rootCtx.ComponentId
    let child1Ctx = NoobishV2.beginLabel "One" rootCtx
    let child2Ctx = NoobishV2.beginSpace rootCtx
    let child1 = child1Ctx.ComponentId
    let child2 = child2Ctx.ComponentId

    components.MinSize.[int child1.Index] <- {Width = 30f; Height = 10f}
    components.MinSize.[int child2.Index] <- {Width = 10f; Height = 5f}
    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.Fill.[int child2.Index] <- {Horizontal = true; Vertical = true}

    NoobishLayoutV2.layoutLinearVertical components 0f 0f 100f 60f (int rootId.Index)

    let child1Bounds = components.Bounds.[int child1.Index]
    let child2Bounds = components.Bounds.[int child2.Index]
    Assert.AreEqual(10f, child1Bounds.Height)
    Assert.AreEqual(50f, child2Bounds.Height)
    Assert.AreEqual(0f, child1Bounds.Y)
    Assert.AreEqual(10f, child2Bounds.Y)
    Assert.AreEqual(100f, child2Bounds.Width)

[<Test>]
let ``layoutFrame stacks horizontal children and respects fill`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackHorizontal
    let rootId = rootCtx.ComponentId
    let child1Ctx = NoobishV2.beginLabel "Left" rootCtx
    let child2Ctx = NoobishV2.beginSpace rootCtx
    let child1 = child1Ctx.ComponentId
    let child2 = child2Ctx.ComponentId

    components.MinSize.[int child1.Index] <- {Width = 30f; Height = 10f}
    components.MinSize.[int child2.Index] <- {Width = 5f; Height = 10f}
    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.Fill.[int child2.Index] <- {Horizontal = true; Vertical = false}

    NoobishLayoutV2.layoutLinearHorizontal components 0f 0f 100f 20f (int rootId.Index)

    let child1Bounds = components.Bounds.[int child1.Index]
    let child2Bounds = components.Bounds.[int child2.Index]
    Assert.AreEqual(30f, child1Bounds.Width)
    Assert.AreEqual(70f, child2Bounds.Width)
    Assert.AreEqual(0f, child1Bounds.X)
    Assert.AreEqual(30f, child2Bounds.X)

[<Test>]
let ``layoutFrame places grid children by order`` () =
    let components = NoobishComponentsV2(4)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginGrid (2, 2)
    let rootId = rootCtx.ComponentId
    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}

    let child1Ctx = NoobishV2.beginLabel "A" rootCtx
    let child2Ctx = NoobishV2.beginLabel "B" rootCtx
    let child3Ctx = NoobishV2.beginLabel "C" rootCtx
    let child1 = child1Ctx.ComponentId
    let child2 = child2Ctx.ComponentId
    let child3 = child3Ctx.ComponentId
    components.Fill.[int child1.Index] <- {Horizontal = true; Vertical = true}
    components.Fill.[int child2.Index] <- {Horizontal = true; Vertical = true}
    components.Fill.[int child3.Index] <- {Horizontal = true; Vertical = true}

    NoobishLayoutV2.layoutGrid components 0f 0f 100f 80f (int rootId.Index)

    let bounds1 = components.Bounds.[int child1.Index]
    let bounds2 = components.Bounds.[int child2.Index]
    let bounds3 = components.Bounds.[int child3.Index]
    Assert.AreEqual(0f, bounds1.X)
    Assert.AreEqual(0f, bounds1.Y)
    Assert.AreEqual(50f, bounds1.Width)
    Assert.AreEqual(40f, bounds1.Height)
    Assert.AreEqual(50f, bounds2.X)
    Assert.AreEqual(0f, bounds2.Y)
    Assert.AreEqual(0f, bounds3.X)
    Assert.AreEqual(40f, bounds3.Y)

[<Test>]
let ``layoutFrame uses content size when not filling`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
    let rootId = rootCtx.ComponentId
    let childCtx = NoobishV2.beginLabel "Sized" rootCtx
    let childId = childCtx.ComponentId

    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.MinSize.[int childId.Index] <- {Width = 0f; Height = 0f}
    components.ContentSize.[int childId.Index] <- {Width = 42f; Height = 12f}

    NoobishLayoutV2.layoutLinearVertical components 0f 0f 100f 60f (int rootId.Index)

    let childBounds = components.Bounds.[int childId.Index]
    Assert.AreEqual(42f, childBounds.Width)
    Assert.AreEqual(12f, childBounds.Height)

[<Test>]
let ``layoutFrame clamps scroll containers to available space`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
    let rootId = rootCtx.ComponentId
    let childCtx =
        NoobishV2.beginPanel rootCtx
        |> NoobishV2.setScrollVertical
    let childId = childCtx.ComponentId

    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.MinSize.[int childId.Index] <- {Width = 0f; Height = 0f}
    components.ContentSize.[int childId.Index] <- {Width = 50f; Height = 200f}

    NoobishLayoutV2.layoutLinearVertical components 0f 0f 100f 60f (int rootId.Index)

    let childBounds = components.Bounds.[int childId.Index]
    Assert.AreEqual(60f, childBounds.Height)

[<Test>]
let ``layoutFrame scroll container height respects parent padding and child margin`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
        |> NoobishV2.setFill {Horizontal = true; Vertical = true}
        |> NoobishV2.setPadding {NoobishPadding.Top = 6f; Right = 4f; Bottom = 10f; Left = 4f}
    let childCtx =
        NoobishV2.beginPanel rootCtx
        |> NoobishV2.setScrollVertical
        |> NoobishV2.setMargin {NoobishMargin.Top = 3f; Right = 0f; Bottom = 5f; Left = 0f}
    let rootId = rootCtx.ComponentId
    let childId = childCtx.ComponentId

    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.MinSize.[int childId.Index] <- {Width = 0f; Height = 0f}
    components.ContentSize.[int childId.Index] <- {Width = 50f; Height = 200f}

    NoobishLayoutV2.layoutLinearVertical components 0f 0f 100f 100f (int rootId.Index)

    let childBounds = components.Bounds.[int childId.Index]
    let expectedHeight = 100f - 6f - 10f - 3f - 5f
    Assert.AreEqual(expectedHeight, childBounds.Height)

[<Test>]
let ``layoutFrame scroll container height respects its own padding`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
        |> NoobishV2.setFill {Horizontal = true; Vertical = true}
    let childCtx =
        NoobishV2.beginPanel rootCtx
        |> NoobishV2.setScrollVertical
        |> NoobishV2.setPadding {NoobishPadding.Top = 4f; Right = 0f; Bottom = 6f; Left = 0f}
    let rootId = rootCtx.ComponentId
    let childId = childCtx.ComponentId

    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.MinSize.[int childId.Index] <- {Width = 0f; Height = 0f}
    components.ContentSize.[int childId.Index] <- {Width = 50f; Height = 200f}

    NoobishLayoutV2.layoutLinearVertical components 0f 0f 100f 80f (int rootId.Index)

    let childBounds = components.Bounds.[int childId.Index]
    Assert.AreEqual(80f, childBounds.Height)

[<Test>]
let ``layoutFrame nested scroll containers respect parent padding`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
        |> NoobishV2.setFill {Horizontal = true; Vertical = true}
        |> NoobishV2.setPadding {NoobishPadding.Top = 5f; Right = 0f; Bottom = 5f; Left = 0f}
    let outerCtx =
        NoobishV2.beginPanel rootCtx
        |> NoobishV2.setScrollVertical
        |> NoobishV2.setPadding {NoobishPadding.Top = 3f; Right = 0f; Bottom = 3f; Left = 0f}
    let innerCtx =
        NoobishV2.beginPanel outerCtx
        |> NoobishV2.setScrollVertical
    let rootId = rootCtx.ComponentId
    let outerId = outerCtx.ComponentId
    let innerId = innerCtx.ComponentId

    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.Fill.[int outerId.Index] <- {Horizontal = true; Vertical = true}
    components.MinSize.[int outerId.Index] <- {Width = 0f; Height = 0f}
    components.ContentSize.[int outerId.Index] <- {Width = 50f; Height = 200f}
    components.MinSize.[int innerId.Index] <- {Width = 0f; Height = 0f}
    components.ContentSize.[int innerId.Index] <- {Width = 50f; Height = 200f}

    NoobishLayoutV2.layoutLinearVertical components 0f 0f 100f 90f (int rootId.Index)

    let outerBounds = components.Bounds.[int outerId.Index]
    let innerBounds = components.Bounds.[int innerId.Index]
    let expectedOuterHeight = 90f - 5f - 5f
    let expectedInnerHeight = expectedOuterHeight - 3f - 3f
    Assert.AreEqual(expectedOuterHeight, outerBounds.Height)
    Assert.AreEqual(expectedInnerHeight, innerBounds.Height)

[<Test>]
let ``layoutFrame stacks using content size`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
    let rootId = rootCtx.ComponentId
    let child1Ctx = NoobishV2.beginLabel "First" rootCtx
    let child2Ctx = NoobishV2.beginLabel "Second" rootCtx
    let child1 = child1Ctx.ComponentId
    let child2 = child2Ctx.ComponentId

    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.ContentSize.[int child1.Index] <- {Width = 10f; Height = 12f}
    components.ContentSize.[int child2.Index] <- {Width = 10f; Height = 8f}

    NoobishLayoutV2.layoutLinearVertical components 0f 0f 100f 40f (int rootId.Index)

    let bounds1 = components.Bounds.[int child1.Index]
    let bounds2 = components.Bounds.[int child2.Index]
    Assert.AreEqual(0f, bounds1.Y)
    Assert.AreEqual(12f, bounds2.Y)

[<Test>]
let ``layoutFrame applies margins once`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackVertical
    let rootId = rootCtx.ComponentId
    let childCtx =
        NoobishV2.beginLabel "Margin" rootCtx
        |> NoobishV2.setMinHeight 10f
        |> NoobishV2.setMargin {NoobishMargin.Top = 5f; Right = 0f; Bottom = 5f; Left = 0f}
    let childId = childCtx.ComponentId

    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}

    NoobishLayoutV2.layoutLinearVertical components 0f 0f 100f 40f (int rootId.Index)

    let childBounds = components.Bounds.[int childId.Index]
    Assert.AreEqual(5f, childBounds.Y)
    Assert.AreEqual(10f, childBounds.Height)

[<Test>]
let ``layoutFrame applies grid spans`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginGrid (2, 2)
    let rootId = rootCtx.ComponentId
    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}

    let childCtx =
        NoobishV2.beginLabel "Span" rootCtx
        |> NoobishV2.setColspan 2
        |> NoobishV2.setFill {Horizontal = true; Vertical = true}
    let childId = childCtx.ComponentId

    NoobishLayoutV2.layoutGrid components 0f 0f 100f 80f (int rootId.Index)

    let childBounds = components.Bounds.[int childId.Index]
    Assert.AreEqual(100f, childBounds.Width)
    Assert.AreEqual(40f, childBounds.Height)

[<Test>]
let ``layoutFrame places grid children after spans`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginGrid (2, 2)
    let rootId = rootCtx.ComponentId
    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}

    let firstCtx = NoobishV2.beginLabel "Span" rootCtx |> NoobishV2.setColspan 2
    let secondCtx = NoobishV2.beginLabel "Next" rootCtx
    let firstId = firstCtx.ComponentId
    let secondId = secondCtx.ComponentId
    components.Fill.[int firstId.Index] <- {Horizontal = true; Vertical = true}
    components.Fill.[int secondId.Index] <- {Horizontal = true; Vertical = true}

    NoobishLayoutV2.layoutGrid components 0f 0f 100f 80f (int rootId.Index)

    let firstBounds = components.Bounds.[int firstId.Index]
    let secondBounds = components.Bounds.[int secondId.Index]
    Assert.AreEqual(0f, firstBounds.Y)
    Assert.AreEqual(40f, secondBounds.Y)

[<Test>]
let ``layoutFrame reserves padding in horizontal layout`` () =
    let components = NoobishComponentsV2(4)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginStackHorizontal
    let rootId = rootCtx.ComponentId
    let leftCtx =
        NoobishV2.beginPanel rootCtx
        |> NoobishV2.setMinWidth 20f
        |> NoobishV2.setPadding {NoobishPadding.Top = 0f; Right = 10f; Bottom = 0f; Left = 10f}
    let leftId = leftCtx.ComponentId
    let buttonCtx =
        NoobishV2.beginButton "Left" 1us leftCtx
        |> NoobishV2.setFillHorizontal
    let buttonId = buttonCtx.ComponentId
    let rightCtx = NoobishV2.beginPanel rootCtx
    let rightId = rightCtx.ComponentId

    components.Fill.[int rootId.Index] <- {Horizontal = true; Vertical = true}
    components.Fill.[int rightId.Index] <- {Horizontal = true; Vertical = true}

    NoobishLayoutV2.layoutLinearHorizontal components 0f 0f 120f 40f (int rootId.Index)

    let leftBounds = components.Bounds.[int leftId.Index]
    let rightBounds = components.Bounds.[int rightId.Index]
    let buttonBounds = components.Bounds.[int buttonId.Index]

    Assert.AreEqual(40f, leftBounds.Width)
    Assert.AreEqual(40f, rightBounds.X)
    Assert.AreEqual(10f, buttonBounds.X - leftBounds.X)

[<Test>]
let ``layoutComponent handles all layout types`` () =
    let layouts =
        [ LayoutV2.LinearVertical
          LayoutV2.LinearHorizontal
          LayoutV2.Grid(1, 1)
          LayoutV2.Stack
          LayoutV2.Relative UIComponentIdV2.empty
          LayoutV2.None ]

    for layout in layouts do
        let components = NoobishComponentsV2(2)
        let rootCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
        let childCtx = NoobishV2.beginLabel "Child" rootCtx
        let rootIndex = int rootCtx.ComponentId.Index
        let childIndex = int childCtx.ComponentId.Index

        components.Layout.[rootIndex] <- layout
        components.Fill.[rootIndex] <- { Horizontal = true; Vertical = true }
        components.Fill.[childIndex] <- { Horizontal = true; Vertical = true }

        NoobishLayoutV2.layoutComponent components 0f 0f 50f 40f rootIndex

        let rootBounds = components.Bounds.[rootIndex]
        Assert.AreEqual(50f, rootBounds.Width)
        Assert.AreEqual(40f, rootBounds.Height)

        let childBounds = components.Bounds.[childIndex]
        if layout = LayoutV2.None then
            Assert.AreEqual(0f, childBounds.Width)
            Assert.AreEqual(0f, childBounds.Height)
        else
            Assert.Greater(childBounds.Width, 0f)
            Assert.Greater(childBounds.Height, 0f)

        components.ReleaseContext childCtx
        components.ReleaseContext rootCtx

[<Test>]
let ``layoutRelative lays out children within content bounds`` () =
    let components = NoobishComponentsV2(2)
    let rootCtx =
        NoobishV2.beginFrame "Page" components
        |> NoobishV2.beginPanel
        |> NoobishV2.setPadding { NoobishPadding.Top = 2f; Right = 3f; Bottom = 4f; Left = 5f }
    let childCtx = NoobishV2.beginLabel "Child" rootCtx
    let rootIndex = int rootCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index

    components.Layout.[rootIndex] <- LayoutV2.Relative UIComponentIdV2.empty
    components.Fill.[rootIndex] <- { Horizontal = true; Vertical = true }
    components.Fill.[childIndex] <- { Horizontal = true; Vertical = true }

    NoobishLayoutV2.layoutRelative components 0f 0f 50f 40f rootIndex

    let childBounds = components.Bounds.[childIndex]
    Assert.AreEqual(5f, childBounds.X)
    Assert.AreEqual(2f, childBounds.Y)
    Assert.AreEqual(42f, childBounds.Width)
    Assert.AreEqual(34f, childBounds.Height)

    components.ReleaseContext childCtx
    components.ReleaseContext rootCtx
