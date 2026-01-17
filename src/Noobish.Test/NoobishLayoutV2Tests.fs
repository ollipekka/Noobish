module Noobish.Test.NoobishLayoutV2Tests

open NUnit.Framework
open Noobish

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

    NoobishLayoutV2.layoutFrame components 100f 60f

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

    NoobishLayoutV2.layoutFrame components 100f 20f

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

    NoobishLayoutV2.layoutFrame components 100f 80f

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

    NoobishLayoutV2.layoutFrame components 100f 60f

    let childBounds = components.Bounds.[int childId.Index]
    Assert.AreEqual(42f, childBounds.Width)
    Assert.AreEqual(12f, childBounds.Height)

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

    NoobishLayoutV2.layoutFrame components 100f 40f

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

    NoobishLayoutV2.layoutFrame components 100f 40f

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

    NoobishLayoutV2.layoutFrame components 100f 80f

    let childBounds = components.Bounds.[int childId.Index]
    Assert.AreEqual(100f, childBounds.Width)
    Assert.AreEqual(40f, childBounds.Height)

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

    NoobishLayoutV2.layoutFrame components 120f 40f

    let leftBounds = components.Bounds.[int leftId.Index]
    let rightBounds = components.Bounds.[int rightId.Index]
    let buttonBounds = components.Bounds.[int buttonId.Index]

    Assert.AreEqual(40f, leftBounds.Width)
    Assert.AreEqual(40f, rightBounds.X)
    Assert.AreEqual(10f, buttonBounds.X - leftBounds.X)
