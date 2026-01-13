module Noobish.Test.NoobishAllocationTests

open System
open NUnit.Framework
open Noobish

let private resetComponents (components: NoobishComponentsV2) =
    for i = 0 to components.Children.Length - 1 do
        components.Children.[i].Clear()
    components.Count <- 0
    components.RunningId <- 0

let private buildUi (components: NoobishComponentsV2) (width: float32) (height: float32) =
    let frameCtx = NoobishV2.beginFrame "Demo/Simple" components
    let panelCtx =
        NoobishV2.beginPanel frameCtx
        |> NoobishV2.setFill {Horizontal = true; Vertical = true}
        |> NoobishV2.setPadding {NoobishPadding.Top = 24f; Right = 24f; Bottom = 24f; Left = 24f}

    let headerCtx =
        NoobishV2.beginHeader "Noobish V2" panelCtx
        |> NoobishV2.setMinHeight 48f
    NoobishV2.endHeader headerCtx |> ignore

    let paragraphCtx =
        NoobishV2.beginParagraph "A tiny demo screen to grow from." panelCtx
        |> NoobishV2.setMinHeight 96f
    NoobishV2.endParagraph paragraphCtx |> ignore

    let buttonCtx =
        NoobishV2.beginButton "Get Started" 1us panelCtx
        |> NoobishV2.setMinHeight 40f
        |> NoobishV2.setFill {Horizontal = true; Vertical = false}
    NoobishV2.endButton buttonCtx |> ignore

    NoobishV2.endPanel panelCtx |> ignore
    NoobishV2.endFrame width height frameCtx

    components.ReleaseContext headerCtx
    components.ReleaseContext paragraphCtx
    components.ReleaseContext buttonCtx
    components.ReleaseContext panelCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``buildUi and layout allocate no managed memory`` () =
    let components = NoobishComponentsV2(64)
    let width = 800f
    let height = 600f
    buildUi components width height
    resetComponents components

    let before = GC.GetAllocatedBytesForCurrentThread()
    buildUi components width height
    resetComponents components
    let allocated = GC.GetAllocatedBytesForCurrentThread() - before

    Assert.AreEqual(0L, allocated, $"Expected 0 allocations but got {allocated}.")
