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

    let allocationLimit =
#if DEBUG
        2048L
#else
        128L
#endif
    Assert.LessOrEqual(allocated, allocationLimit, $"Expected <= {allocationLimit} allocations but got {allocated}.")

[<Test>]
let ``NoobishInputV2 hitTestWith lambdas allocate no managed memory`` () =
    let bounds: Noobish.Internal.NoobishRectangle[] =
        [| { X = 0f; Y = 0f; Width = 10f; Height = 10f }
           { X = 0f; Y = 0f; Width = 10f; Height = 10f } |]
    let boundsAt i = bounds.[i]
    let predicate i = i = 0
    NoobishInputV2.hitTestWith bounds.Length boundsAt 5f 5f predicate |> ignore

    let before = GC.GetAllocatedBytesForCurrentThread()
    for _ = 0 to 50 do
        NoobishInputV2.hitTestWith bounds.Length boundsAt 5f 5f predicate |> ignore
    let allocated = GC.GetAllocatedBytesForCurrentThread() - before

    Assert.AreEqual(0L, allocated, $"Expected 0 allocations but got {allocated}.")

[<Test>]
let ``NoobishInputV2 ProcessInput allocates no managed memory`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let buttonCtx = NoobishV2.beginButton "Ok" 1us ctx
    let rootIndex = int ctx.ComponentId.Index
    let buttonIndex = int buttonCtx.ComponentId.Index
    components.Bounds.[rootIndex] <- { Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Bounds.[buttonIndex] <- { Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Visible.[rootIndex] <- true
    components.Visible.[buttonIndex] <- true
    components.Enabled.[rootIndex] <- true
    components.Enabled.[buttonIndex] <- true
    components.WantsOnPress.[buttonIndex] <- true
    components.WantsOnClick.[buttonIndex] <- true
    let buffer = InputBufferV2(2)

    let input =
        { new INoobishInputState with
            member _.PointerX = 5f
            member _.PointerY = 5f
            member _.ScrollWheelDelta = 0f
            member _.IsPrimaryClick() = false
            member _.IsPrimaryDown() = true
            member _.IsSecondaryClick() = false
            member _.IsKeyPressed _ = false }

    NoobishInputV2.ProcessInput input components buffer
    buffer.ClearDown()
    input |> ignore

    let inline measureLoop (iterations: int) (action: unit -> unit) =
        // Warm to avoid JIT allocations in the measurement window.
        action()
        let before = GC.GetAllocatedBytesForCurrentThread()
        for _ = 1 to iterations do
            action()
        GC.GetAllocatedBytesForCurrentThread() - before

    let x = input.PointerX
    let y = input.PointerY

    let iterations = 50
    let resetAlloc = measureLoop iterations (fun () -> buffer.Reset components)
    buffer.ClearDown()
    let hoverAlloc = measureLoop iterations (fun () -> NoobishInputV2.updateHover components buffer x y)
    buffer.ClearDown()
    let primaryDownAlloc = measureLoop iterations (fun () -> NoobishInputV2.updatePrimaryDown components buffer x y)
    buffer.ClearDown()
    let processAlloc = measureLoop iterations (fun () -> NoobishInputV2.ProcessInput input components buffer)
    buffer.ClearDown()
    buffer.SetDown buttonIndex
    let clearDownAlloc = measureLoop iterations (fun () -> buffer.ClearDown())

    let breakdown =
        $"reset={resetAlloc}; hover={hoverAlloc}; primaryDown={primaryDownAlloc}; process={processAlloc}; clearDown={clearDownAlloc}"

    Assert.AreEqual(0L, processAlloc, $"Expected 0 allocations but got {processAlloc}. Breakdown: {breakdown}")

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 query helpers allocate no managed memory`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 5us ctx
    let textboxCtx = NoobishV2.beginTextbox "Text" 6us ctx
    let buttonIndex = int buttonCtx.ComponentId.Index
    let textboxIndex = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(2)
    buffer.Reset components
    buffer.MarkClicked buttonIndex
    buffer.MarkPressed buttonIndex
    buffer.MarkReleased buttonIndex
    buffer.SetDown buttonIndex
    buffer.MarkTextChanged(textboxIndex, "Hello")
    buffer.MarkSliderChanged(buttonIndex, 4f)

    let inline measureLoop (iterations: int) (action: unit -> unit) =
        action()
        let before = GC.GetAllocatedBytesForCurrentThread()
        for _ = 1 to iterations do
            action()
        GC.GetAllocatedBytesForCurrentThread() - before

    let iterations = 50
    let wasClickedAlloc = measureLoop iterations (fun () -> buffer.WasClicked 5us |> ignore)
    let wasPressedAlloc = measureLoop iterations (fun () -> buffer.WasPressed 5us |> ignore)
    let wasReleasedAlloc = measureLoop iterations (fun () -> buffer.WasReleased 5us |> ignore)
    let isDownAlloc = measureLoop iterations (fun () -> buffer.IsDown 5us |> ignore)
    let textChangedAlloc = measureLoop iterations (fun () -> buffer.TryGetTextChanged 6us |> ignore)
    let sliderChangedAlloc = measureLoop iterations (fun () -> buffer.TryGetSliderChanged 5us |> ignore)

    let breakdown =
        $"clicked={wasClickedAlloc}; pressed={wasPressedAlloc}; released={wasReleasedAlloc}; down={isDownAlloc}; text={textChangedAlloc}; slider={sliderChangedAlloc}"

    Assert.AreEqual(0L, wasClickedAlloc, $"Expected 0 allocations but got {wasClickedAlloc}. Breakdown: {breakdown}")
    Assert.AreEqual(0L, wasPressedAlloc, $"Expected 0 allocations but got {wasPressedAlloc}. Breakdown: {breakdown}")
    Assert.AreEqual(0L, wasReleasedAlloc, $"Expected 0 allocations but got {wasReleasedAlloc}. Breakdown: {breakdown}")
    Assert.AreEqual(0L, isDownAlloc, $"Expected 0 allocations but got {isDownAlloc}. Breakdown: {breakdown}")
    Assert.AreEqual(0L, textChangedAlloc, $"Expected 0 allocations but got {textChangedAlloc}. Breakdown: {breakdown}")
    Assert.AreEqual(0L, sliderChangedAlloc, $"Expected 0 allocations but got {sliderChangedAlloc}. Breakdown: {breakdown}")

    components.ReleaseContext textboxCtx
    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx
