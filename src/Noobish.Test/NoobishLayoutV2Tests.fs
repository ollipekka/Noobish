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
