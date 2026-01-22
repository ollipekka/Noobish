module Noobish.Test.NoobishInputV2Tests

open NUnit.Framework
open Noobish

type InputConfig =
    { PointerX: float32
      PointerY: float32
      ScrollDelta: float32
      PrimaryClick: bool
      PrimaryDown: bool
      SecondaryClick: bool
      IsKeyPressed: NoobishKeyId -> bool
      ConsumeTextInput: unit -> struct(char[] * int) }

let defaultInputConfig =
    { PointerX = 0f
      PointerY = 0f
      ScrollDelta = 0f
      PrimaryClick = false
      PrimaryDown = false
      SecondaryClick = false
      IsKeyPressed = fun _ -> false
      ConsumeTextInput = fun () -> struct([||], 0) }

let createInput (config: InputConfig) =
    { new INoobishInputState with
        member _.PointerX = config.PointerX
        member _.PointerY = config.PointerY
        member _.ScrollWheelDelta = config.ScrollDelta
        member _.IsPrimaryClick() = config.PrimaryClick
        member _.IsPrimaryDown() = config.PrimaryDown
        member _.IsSecondaryClick() = config.SecondaryClick
        member _.IsKeyPressed keyId = config.IsKeyPressed keyId
        member _.ConsumeTextInput() = config.ConsumeTextInput()
        }

[<Test>]
let ``InputBufferV2 Reset builds localId map`` () =
    let components = NoobishComponentsV2(3)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 7us ctx
    let _ = NoobishV2.beginLabel "Ignored" ctx
    let buffer = InputBufferV2(3)
    buffer.Reset components
    Assert.IsTrue(buffer.LocalIdToIndex.ContainsKey 7us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 marks and queries input`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 1us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(2)
    buffer.Reset components

    buffer.MarkClicked index
    buffer.MarkPressed index
    buffer.MarkTextChanged(index, "Hello")

    Assert.IsTrue(buffer.WasClicked 1us)
    Assert.IsTrue(buffer.WasPressed 1us)
    Assert.AreEqual(ValueSome "Hello", buffer.TryGetTextChanged 1us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 GetClicked returns last clicked local id`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 7us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(2)
    buffer.Reset components

    buffer.MarkClicked index
    buffer.LastClickedLocalId <- 7us

    Assert.AreEqual(7us, buffer.GetClicked())
    Assert.AreEqual(ValueSome 7us, buffer.TryGetClicked())

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 TryGetClicked returns none when unset`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _buttonCtx = NoobishV2.beginButton "Ok" 7us ctx
    let buffer = InputBufferV2(2)
    buffer.Reset components

    Assert.IsTrue(ValueOption.isNone (buffer.TryGetClicked()))

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 GetPressed returns last pressed local id`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 9us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(2)
    buffer.Reset components

    buffer.MarkPressed index
    buffer.LastPressedLocalId <- 9us

    Assert.AreEqual(9us, buffer.GetPressed())
    Assert.AreEqual(ValueSome 9us, buffer.TryGetPressed())

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 TryGetPressed returns none when unset`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _buttonCtx = NoobishV2.beginButton "Ok" 9us ctx
    let buffer = InputBufferV2(2)
    buffer.Reset components

    Assert.IsTrue(ValueOption.isNone (buffer.TryGetPressed()))

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 EnsureCapacity allows equal or smaller counts`` () =
    let components = NoobishComponentsV2(1)
    let buffer = InputBufferV2(1)
    buffer.Reset components
    Assert.DoesNotThrow(fun () -> buffer.EnsureCapacity 1)
    Assert.DoesNotThrow(fun () -> buffer.EnsureCapacity 0)

[<Test>]
let ``InputBufferV2 EnsureCapacity throws when too small`` () =
    let buffer = InputBufferV2(1)
    let ex = Assert.Throws<System.ArgumentException>(fun () -> buffer.EnsureCapacity 2 |> ignore)
    Assert.IsTrue(ex.Message.Contains("capacity too small"))

[<Test>]
let ``InputBufferV2 Reset clears active flags`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 1us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.MarkClicked index
    Assert.IsTrue(buffer.WasClicked 1us)
    Assert.Greater(buffer.ActiveIndices.Count, 0)

    buffer.Reset components
    Assert.AreEqual(0, buffer.ActiveIndices.Count)
    Assert.IsFalse(buffer.WasClicked 1us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 clippedBounds clamps to parent`` () =
    let components = NoobishComponentsV2(2)
    let parentCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginLabel "Child" parentCtx
    let parentIndex = int parentCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index

    components.Bounds.[parentIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[childIndex] <- {X = -5f; Y = -5f; Width = 20f; Height = 20f}

    let clipped = NoobishInputV2.clippedBounds components childIndex
    Assert.AreEqual(0f, clipped.X)
    Assert.AreEqual(0f, clipped.Y)
    Assert.AreEqual(10f, clipped.Width)
    Assert.AreEqual(10f, clipped.Height)

    components.ReleaseContext childCtx
    components.ReleaseContext parentCtx

[<Test>]
let ``NoobishInputV2 clippedBounds offsets for scroll`` () =
    let components = NoobishComponentsV2(2)
    let parentCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginLabel "Child" parentCtx
    let parentIndex = int parentCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index

    components.Bounds.[parentIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[childIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Scroll.[parentIndex] <- {Horizontal = false; Vertical = true}
    components.ScrollY.[parentIndex] <- -5f

    let clipped = NoobishInputV2.clippedBounds components childIndex
    Assert.AreEqual(0f, clipped.X)
    Assert.AreEqual(0f, clipped.Y)
    Assert.AreEqual(10f, clipped.Width)
    Assert.AreEqual(5f, clipped.Height)

    components.ReleaseContext childCtx
    components.ReleaseContext parentCtx

[<Test>]
let ``NoobishInputV2 clippedBounds clamps to parent content bounds`` () =
    let components = NoobishComponentsV2(2)
    let parentCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginLabel "Child" parentCtx
    let parentIndex = int parentCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index

    components.Bounds.[parentIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Padding.[parentIndex] <- {NoobishPadding.Top = 2f; Right = 0f; Bottom = 1f; Left = 1f}
    components.Bounds.[childIndex] <- {X = -5f; Y = -5f; Width = 20f; Height = 20f}

    let clipped = NoobishInputV2.clippedBounds components childIndex
    Assert.AreEqual(1f, clipped.X)
    Assert.AreEqual(2f, clipped.Y)
    Assert.AreEqual(9f, clipped.Width)
    Assert.AreEqual(7f, clipped.Height)

    components.ReleaseContext childCtx
    components.ReleaseContext parentCtx

[<Test>]
let ``NoobishInputV2 hitTest returns topmost match`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let firstCtx = NoobishV2.beginLabel "First" rootCtx
    let secondCtx = NoobishV2.beginLabel "Second" rootCtx
    let firstIndex = int firstCtx.ComponentId.Index
    let secondIndex = int secondCtx.ComponentId.Index
    let rootIndex = int rootCtx.ComponentId.Index

    components.Bounds.[rootIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[firstIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[secondIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}

    let hit = NoobishInputV2.hitTest components 5f 5f (fun _ -> true)
    Assert.AreEqual(secondIndex, hit)

    components.ReleaseContext secondCtx
    components.ReleaseContext firstCtx
    components.ReleaseContext rootCtx

[<Test>]
let ``NoobishInputV2 hitTest prefers higher layer`` () =
    let components = NoobishComponentsV2(3)
    let rootCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let firstCtx = NoobishV2.beginLabel "First" rootCtx
    let secondCtx = NoobishV2.beginLabel "Second" rootCtx
    let firstIndex = int firstCtx.ComponentId.Index
    let secondIndex = int secondCtx.ComponentId.Index
    let rootIndex = int rootCtx.ComponentId.Index

    components.Bounds.[rootIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[firstIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[secondIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Layer.[firstIndex] <- 10
    components.Layer.[secondIndex] <- 1

    let hit = NoobishInputV2.hitTest components 5f 5f (fun _ -> true)
    Assert.AreEqual(firstIndex, hit)

    components.ReleaseContext secondCtx
    components.ReleaseContext firstCtx
    components.ReleaseContext rootCtx

[<Test>]
let ``NoobishInputV2 hitTest returns -1 when no hit`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let labelCtx = NoobishV2.beginLabel "Label" ctx
    let index = int labelCtx.ComponentId.Index

    components.Bounds.[index] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}

    let hit = NoobishInputV2.hitTest components 50f 50f (fun _ -> true)
    Assert.AreEqual(-1, hit)

    components.ReleaseContext labelCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 hitTestWith returns topmost match`` () =
    let bounds: Noobish.NoobishRectangle[] =
        [| {X = 0f; Y = 0f; Width = 10f; Height = 10f }
           { X = 0f; Y = 0f; Width = 10f; Height = 10f }
           { X = 0f; Y = 0f; Width = 10f; Height = 10f } |]
    let layers = [| 0; 0; 0 |]
    let hit = NoobishInputV2.hitTestWith bounds.Length (fun i -> bounds.[i]) (fun i -> layers.[i]) 5f 5f (fun _ -> true)
    Assert.AreEqual(2, hit)

[<Test>]
let ``NoobishInputV2 hitTestWith respects predicate`` () =
    let bounds:  Noobish.NoobishRectangle[] =
        [| { X = 0f; Y = 0f; Width = 10f; Height = 10f }
           { X = 0f; Y = 0f; Width = 10f; Height = 10f } |]
    let layers = [| 0; 1 |]
    let hit = NoobishInputV2.hitTestWith bounds.Length (fun i -> bounds.[i]) (fun i -> layers.[i]) 5f 5f (fun i -> i = 0)
    Assert.AreEqual(0, hit)

[<Test>]
let ``NoobishInputV2 ProcessInput clears down when not primary down`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let buttonCtx = NoobishV2.beginButton "Ok" 21us ctx
    let rootIndex = int ctx.ComponentId.Index
    let buttonIndex = int buttonCtx.ComponentId.Index

    components.Bounds.[rootIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[buttonIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.WantsOnClick.[buttonIndex] <- false

    let buffer = InputBufferV2(2)
    buffer.Reset components
    buffer.SetDown buttonIndex

    let input = createInput { defaultInputConfig with PointerX = 50f; PointerY = 50f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.IsFalse(buffer.IsDown 21us)
    Assert.IsTrue(buffer.WasReleased 21us)
    Assert.IsFalse(buffer.WasClicked 21us)
    Assert.AreEqual(0us, buffer.GetClicked())

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 ProcessInput marks pressed when primary down`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let buttonCtx = NoobishV2.beginButton "Ok" 22us ctx
    let rootIndex = int ctx.ComponentId.Index
    let buttonIndex = int buttonCtx.ComponentId.Index

    components.Bounds.[rootIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[buttonIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}

    let buffer = InputBufferV2(2)
    let input =
        createInput
            { defaultInputConfig with
                PointerX = 5f
                PointerY = 5f
                PrimaryDown = true }

    NoobishInputV2.ProcessInput input components buffer

    Assert.IsTrue(buffer.IsDown 22us)
    Assert.IsTrue(buffer.WasPressed 22us)
    Assert.AreEqual(22us, buffer.GetPressed())

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 ProcessInput clicks on release over same component`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let buttonCtx = NoobishV2.beginButton "Ok" 23us ctx
    let rootIndex = int ctx.ComponentId.Index
    let buttonIndex = int buttonCtx.ComponentId.Index

    components.Bounds.[rootIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[buttonIndex] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.WantsOnClick.[buttonIndex] <- true

    let buffer = InputBufferV2(2)
    buffer.SetDown buttonIndex

    let input = createInput { defaultInputConfig with PointerX = 5f; PointerY = 5f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.IsFalse(buffer.IsDown 23us)
    Assert.AreEqual(23us, buffer.GetClicked())
    Assert.IsTrue(buffer.WasReleased 23us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 ProcessInput clears down when index out of range`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let buttonCtx = NoobishV2.beginButton "Ok" 30us ctx
    let index = int buttonCtx.ComponentId.Index

    let buffer = InputBufferV2(2)
    buffer.SetDown index

    components.Count <- 1
    let input = createInput { defaultInputConfig with PrimaryDown = true }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(-1, buffer.DownIndex)
    Assert.IsTrue(buffer.Released.[index])

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 UpdateHover swaps hovered component`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components
    let first = NoobishV2.beginLabel "One" ctx
    let second = NoobishV2.beginLabel "Two" ctx
    let firstIndex = int first.ComponentId.Index
    let secondIndex = int second.ComponentId.Index
    let buffer = InputBufferV2(2)

    buffer.UpdateHover(components, firstIndex)
    Assert.IsTrue(components.Hovered.[firstIndex])
    Assert.IsFalse(components.Hovered.[secondIndex])

    buffer.UpdateHover(components, secondIndex)
    Assert.IsFalse(components.Hovered.[firstIndex])
    Assert.IsTrue(components.Hovered.[secondIndex])

    buffer.UpdateHover(components, -1)
    Assert.IsFalse(components.Hovered.[firstIndex])
    Assert.IsFalse(components.Hovered.[secondIndex])

    components.ReleaseContext first
    components.ReleaseContext second
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 UpdateDown and Release toggles when enabled`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 3us ctx
    let index = int buttonCtx.ComponentId.Index
    components.WantsOnClick.[index] <- true
    components.WantsOnPress.[index] <- true
    components.WantsToggle.[index] <- true
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.UpdateDown(components, index)
    Assert.IsTrue(buffer.IsDown 3us)

    buffer.Release(components, index)
    Assert.IsFalse(buffer.IsDown 3us)
    Assert.IsTrue(components.Toggled.[index])
    Assert.AreEqual(3us, buffer.GetClicked())
    Assert.AreEqual(3us, buffer.GetPressed())

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 UpdateDown ignores invalid hit`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _buttonCtx = NoobishV2.beginButton "Ok" 4us ctx
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.UpdateDown(components, -1)
    Assert.AreEqual(0us, buffer.GetPressed())

    buffer.UpdateDown(components, 2)
    Assert.AreEqual(0us, buffer.GetPressed())

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 UpdateDown ignores when already down`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components
    let first = NoobishV2.beginButton "First" 4us ctx
    let second = NoobishV2.beginButton "Second" 5us ctx
    let firstIndex = int first.ComponentId.Index
    let secondIndex = int second.ComponentId.Index
    let buffer = InputBufferV2(2)
    buffer.Reset components

    buffer.UpdateDown(components, firstIndex)
    buffer.UpdateDown(components, secondIndex)

    Assert.IsTrue(buffer.IsDown 4us)
    Assert.IsFalse(buffer.IsDown 5us)
    Assert.AreEqual(4us, buffer.GetPressed())

    components.ReleaseContext first
    components.ReleaseContext second
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 Release ignores non-matching hit`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components
    let first = NoobishV2.beginButton "First" 5us ctx
    let second = NoobishV2.beginButton "Second" 6us ctx
    let firstIndex = int first.ComponentId.Index
    let secondIndex = int second.ComponentId.Index
    components.WantsOnClick.[firstIndex] <- true
    components.WantsOnPress.[firstIndex] <- true
    components.WantsToggle.[firstIndex] <- true

    let buffer = InputBufferV2(2)
    buffer.Reset components
    buffer.UpdateDown(components, firstIndex)
    buffer.Release(components, secondIndex)

    Assert.IsFalse(components.Toggled.[firstIndex])
    Assert.AreEqual(0us, buffer.GetClicked())

    components.ReleaseContext first
    components.ReleaseContext second
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 Release ignores when no down`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 6us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.Release(components, index)
    Assert.AreEqual(0us, buffer.GetClicked())
    Assert.IsFalse(components.Toggled.[index])

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 UpdateHover ignores invalid index`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _label = NoobishV2.beginLabel "Label" ctx
    let buffer = InputBufferV2(1)

    buffer.UpdateHover(components, -1)
    Assert.AreEqual(-1, buffer.HoveredIndex)

    buffer.UpdateHover(components, 2)
    Assert.AreEqual(-1, buffer.HoveredIndex)

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 Reset restores hovered localId`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Hover" 7us ctx
    let buffer = InputBufferV2(2)

    buffer.UpdateHover(components, int buttonCtx.ComponentId.Index)
    Assert.AreEqual(7us, buffer.LastHoveredLocalId)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx
    components.Clear()
    let ctx2 = NoobishV2.beginFrame "Page" components
    let buttonCtx2 = NoobishV2.beginButton "Hover" 7us ctx2
    buffer.Reset components

    let index = int buttonCtx2.ComponentId.Index
    Assert.IsTrue(components.Hovered.[index])
    Assert.AreEqual(index, buffer.HoveredIndex)

    components.ReleaseContext buttonCtx2
    components.ReleaseContext ctx2

[<Test>]
let ``InputBufferV2 Reset clears hover when localId is missing`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "PageA" components
    let buttonCtx = NoobishV2.beginButton "Hover" 7us ctx
    let buffer = InputBufferV2(2)

    buffer.UpdateHover(components, int buttonCtx.ComponentId.Index)
    Assert.AreEqual(7us, buffer.LastHoveredLocalId)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx
    components.Clear()
    let ctx2 = NoobishV2.beginFrame "PageB" components
    let _buttonCtx2 = NoobishV2.beginButton "Other" 9us ctx2
    buffer.Reset components

    Assert.AreEqual(0us, buffer.LastHoveredLocalId)
    Assert.AreEqual(-1, buffer.HoveredIndex)
    Assert.IsFalse(components.Hovered.[0])

    components.ReleaseContext ctx2

[<Test>]
let ``InputBufferV2 ClearDown releases and marks released`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 8us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.SetDown index
    buffer.ClearDown()

    Assert.IsFalse(buffer.IsDown 8us)
    Assert.IsTrue(buffer.WasReleased 8us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 ClearDown no-ops when no down`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _buttonCtx = NoobishV2.beginButton "Ok" 8us ctx
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.ClearDown()
    Assert.AreEqual(0us, buffer.GetPressed())
    Assert.AreEqual(0us, buffer.GetClicked())

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 UpdateDown ignores zero localId`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 0us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.UpdateDown(components, index)

    Assert.IsTrue(buffer.Pressed.[index])
    Assert.AreEqual(0us, buffer.GetPressed())

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 Release ignores click when not clickable`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 9us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    components.Enabled.[index] <- false
    buffer.SetDown index
    buffer.Release(components, index)

    Assert.AreEqual(0us, buffer.GetClicked())
    Assert.IsFalse(components.Toggled.[index])

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 Release ignores zero localId`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 0us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components
    components.WantsToggle.[index] <- true

    buffer.SetDown index
    buffer.Release(components, index)

    Assert.AreEqual(0us, buffer.GetClicked())
    Assert.IsTrue(components.Toggled.[index])

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx
[<Test>]
let ``InputBufferV2 wasclicked/waspressed/wasreleased reflect flags`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 9us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.MarkClicked index
    buffer.MarkPressed index
    buffer.MarkReleased index

    Assert.IsTrue(buffer.WasClicked 9us)
    Assert.IsTrue(buffer.WasPressed 9us)
    Assert.IsTrue(buffer.WasReleased 9us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 wasclicked/waspressed/wasreleased default false`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _buttonCtx = NoobishV2.beginButton "Ok" 9us ctx
    let buffer = InputBufferV2(1)
    buffer.Reset components

    Assert.IsFalse(buffer.WasClicked 9us)
    Assert.IsFalse(buffer.WasPressed 9us)
    Assert.IsFalse(buffer.WasReleased 9us)
    Assert.IsFalse(buffer.WasClicked 99us)
    Assert.IsFalse(buffer.WasPressed 99us)
    Assert.IsFalse(buffer.WasReleased 99us)

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 IsDown reflects down state`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 10us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.SetDown index
    Assert.IsTrue(buffer.IsDown 10us)

    buffer.ClearDown()
    Assert.IsFalse(buffer.IsDown 10us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 SetDown updates existing down`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components
    let first = NoobishV2.beginButton "First" 12us ctx
    let second = NoobishV2.beginButton "Second" 13us ctx
    let firstIndex = int first.ComponentId.Index
    let secondIndex = int second.ComponentId.Index
    let buffer = InputBufferV2(2)
    buffer.Reset components

    buffer.SetDown firstIndex
    Assert.IsTrue(buffer.IsDown 12us)
    Assert.IsFalse(buffer.IsDown 13us)

    buffer.SetDown secondIndex
    Assert.IsFalse(buffer.IsDown 12us)
    Assert.IsTrue(buffer.IsDown 13us)

    buffer.SetDown secondIndex
    Assert.IsTrue(buffer.IsDown 13us)

    components.ReleaseContext first
    components.ReleaseContext second
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 IsDown default false`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _buttonCtx = NoobishV2.beginButton "Ok" 10us ctx
    let buffer = InputBufferV2(1)
    buffer.Reset components

    Assert.IsFalse(buffer.IsDown 10us)
    Assert.IsFalse(buffer.IsDown 99us)

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 TryGetTextChanged returns payload when set`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Text" 11us ctx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.MarkTextChanged(index, "Hello")
    Assert.AreEqual(ValueSome "Hello", buffer.TryGetTextChanged 11us)

    components.ReleaseContext textboxCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 TryGetTextChanged returns none when unset`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _textboxCtx = NoobishV2.beginTextbox "Text" 11us ctx
    let buffer = InputBufferV2(1)
    buffer.Reset components

    Assert.IsTrue(ValueOption.isNone (buffer.TryGetTextChanged 11us))

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 TryGetTextChanged returns none when localId missing`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Text" 11us ctx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.MarkTextChanged(index, "Hello")
    Assert.IsTrue(ValueOption.isNone (buffer.TryGetTextChanged 99us))

    components.ReleaseContext textboxCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 TryGetSliderChanged returns payload when set`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let sliderCtx = NoobishV2.beginSlider (0f, 10f) 1f 2f 15us ctx
    let index = int sliderCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.MarkSliderChanged(index, 4f)
    Assert.AreEqual(ValueSome 4f, buffer.TryGetSliderChanged 15us)

    components.ReleaseContext sliderCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 TryGetSliderChanged returns none when localId missing`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let sliderCtx = NoobishV2.beginSlider (0f, 10f) 1f 2f 15us ctx
    let index = int sliderCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.MarkSliderChanged(index, 4f)
    Assert.IsTrue(ValueOption.isNone (buffer.TryGetSliderChanged 99us))

    components.ReleaseContext sliderCtx
    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 TryGetSliderChanged returns none when unset`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let _sliderCtx = NoobishV2.beginSlider (0f, 10f) 1f 2f 16us ctx
    let buffer = InputBufferV2(1)
    buffer.Reset components

    Assert.IsTrue(ValueOption.isNone (buffer.TryGetSliderChanged 16us))

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 tracks down and release`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 2us ctx
    let index = int buttonCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.Reset components

    buffer.SetDown index
    Assert.IsTrue(buffer.IsDown 2us)
    buffer.ClearDown()
    Assert.IsFalse(buffer.IsDown 2us)
    Assert.IsTrue(buffer.WasReleased 2us)

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 ProcessInput marks hovered`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 1us ctx
    let index = int buttonCtx.ComponentId.Index
    components.Bounds.[index] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    let buffer = InputBufferV2(1)

    let input = createInput { defaultInputConfig with PointerX = 5f; PointerY = 5f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.IsTrue(components.Hovered.[index])

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 ProcessInput updates slider value on drag`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let sliderCtx = NoobishV2.beginSlider (0f, 10f) 1f 0f 17us ctx
    let index = int sliderCtx.ComponentId.Index
    components.Bounds.[index] <- {X = 0f; Y = 0f; Width = 10f; Height = 10f}
    let buffer = InputBufferV2(1)

    let input =
        createInput
            { defaultInputConfig with
                PointerX = 5f
                PointerY = 5f
                PrimaryDown = true }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(5f, components.SliderValue.[index])
    Assert.AreEqual(ValueSome 5f, buffer.TryGetSliderChanged 17us)

    components.ReleaseContext sliderCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 ProcessInput scrolls vertical containers`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let index = int ctx.ComponentId.Index
    components.Bounds.[index] <- {X = 0f; Y = 0f; Width = 100f; Height = 100f}
    components.ContentSize.[index] <- {Width = 100f; Height = 200f}
    components.Scroll.[index] <- {Horizontal = false; Vertical = true}
    let buffer = InputBufferV2(1)

    let input =
        createInput
            { defaultInputConfig with
                PointerX = 10f
                PointerY = 10f
                ScrollDelta = 10f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(-5f, components.ScrollY.[index])

    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 ProcessInput scrolls when hovering child`` () =
    let components = NoobishComponentsV2(2)
    let parentCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginLabel "Child" parentCtx
    let parentIndex = int parentCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index
    components.Bounds.[parentIndex] <- {X = 0f; Y = 0f; Width = 100f; Height = 100f}
    components.Bounds.[childIndex] <- {X = 0f; Y = 0f; Width = 100f; Height = 200f}
    components.ContentSize.[parentIndex] <- {Width = 100f; Height = 200f}
    components.Scroll.[parentIndex] <- {Horizontal = false; Vertical = true}
    let buffer = InputBufferV2(2)

    let input =
        createInput
            { defaultInputConfig with
                PointerX = 10f
                PointerY = 10f
                ScrollDelta = 10f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(-5f, components.ScrollY.[parentIndex])

    components.ReleaseContext childCtx
    components.ReleaseContext parentCtx

[<Test>]
let ``NoobishInputV2 scroll uses child bounds over content size`` () =
    let components = NoobishComponentsV2(2)
    let parentCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginLabel "Child" parentCtx
    let parentIndex = int parentCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index
    components.Bounds.[parentIndex] <- {X = 0f; Y = 0f; Width = 100f; Height = 100f}
    components.Bounds.[childIndex] <- {X = 0f; Y = 0f; Width = 100f; Height = 200f}
    components.ContentSize.[parentIndex] <- {Width = 10f; Height = 10f}
    components.Scroll.[parentIndex] <- {Horizontal = false; Vertical = true}
    let buffer = InputBufferV2(2)

    let input =
        createInput
            { defaultInputConfig with
                PointerX = 10f
                PointerY = 10f
                ScrollDelta = 10f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(-5f, components.ScrollY.[parentIndex])

    components.ReleaseContext childCtx
    components.ReleaseContext parentCtx

[<Test>]
let ``NoobishInputV2 scroll uses child bounds over content size horizontally`` () =
    let components = NoobishComponentsV2(2)
    let parentCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginLabel "Child" parentCtx
    let parentIndex = int parentCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index
    components.Bounds.[parentIndex] <- {X = 0f; Y = 0f; Width = 100f; Height = 100f}
    components.Bounds.[childIndex] <- {X = 0f; Y = 0f; Width = 200f; Height = 20f}
    components.ContentSize.[parentIndex] <- {Width = 10f; Height = 10f}
    components.Scroll.[parentIndex] <- {Horizontal = true; Vertical = false}
    let buffer = InputBufferV2(2)

    let input =
        createInput
            { defaultInputConfig with
                PointerX = 10f
                PointerY = 10f
                ScrollDelta = 10f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(-5f, components.ScrollX.[parentIndex])

    components.ReleaseContext childCtx
    components.ReleaseContext parentCtx

[<Test>]
let ``NoobishInputV2 tryFindScrollableAncestor returns -1 when no parent`` () =
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    let index = 0
    components.ParentId.[index] <- UIComponentIdV2.empty
    components.Scroll.[index] <- { Horizontal = false; Vertical = false }

    let result = NoobishInputV2.tryFindScrollableAncestor components index
    Assert.AreEqual(-1, result)

[<Test>]
let ``NoobishInputV2 scrolls horizontally when both axes enabled`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let index = int ctx.ComponentId.Index
    components.Bounds.[index] <- {X = 0f; Y = 0f; Width = 100f; Height = 100f}
    components.ContentSize.[index] <- {Width = 200f; Height = 200f}
    components.Scroll.[index] <- {Horizontal = true; Vertical = true}
    let buffer = InputBufferV2(1)

    let input =
        createInput
            { defaultInputConfig with
                PointerX = 10f
                PointerY = 10f
                ScrollDelta = 10f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(-5f, components.ScrollX.[index])
    Assert.AreEqual(-5f, components.ScrollY.[index])

    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 calculateSliderValue uses floor stepping`` () =
    let bounds:  Noobish.NoobishRectangle = { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    let value = NoobishInputV2.calculateSliderValue bounds 0f 10f 2f 5f
    Assert.AreEqual(4f, value)

[<Test>]
let ``NoobishInputV2 calculateSliderValue floors negative values`` () =
    let bounds:  Noobish.NoobishRectangle = { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    let value = NoobishInputV2.calculateSliderValue bounds -10f 10f 2f 2.5f
    Assert.AreEqual(-6f, value)

[<Test>]
let ``NoobishInputV2 calculateSliderValue anchors step to range start`` () =
    let bounds: Noobish.NoobishRectangle = { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    let value = NoobishInputV2.calculateSliderValue bounds 1f 9f 2f 1.25f
    Assert.AreEqual(1f, value)

[<Test>]
let ``InputBufferV2 Reset restores focus by local id`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Hi" 9us frameCtx
    let index = int textboxCtx.ComponentId.Index
    components.Text.[index] <- "Hi"
    let buffer = InputBufferV2(1)

    buffer.SetFocus(components, index, 5)
    buffer.Reset components

    Assert.IsTrue(components.Focused.[index])
    Assert.AreEqual(2, components.CaretIndex.[index])

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``InputBufferV2 SetFocus clears previous focus`` () =
    let components = NoobishComponentsV2(2)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx1 = NoobishV2.beginTextbox "One" 1us frameCtx
    let textboxCtx2 = NoobishV2.beginTextbox "Two" 2us frameCtx
    let index1 = int textboxCtx1.ComponentId.Index
    let index2 = int textboxCtx2.ComponentId.Index
    let buffer = InputBufferV2(2)

    buffer.SetFocus(components, index1, 0)
    Assert.IsTrue(components.Focused.[index1])

    buffer.SetFocus(components, index2, 1)

    Assert.IsFalse(components.Focused.[index1])
    Assert.IsTrue(components.Focused.[index2])
    Assert.AreEqual(index2, buffer.FocusedIndex)
    Assert.AreEqual(2us, buffer.LastFocusedLocalId)

    components.ReleaseContext textboxCtx2
    components.ReleaseContext textboxCtx1
    components.ReleaseContext frameCtx

[<Test>]
let ``NoobishInputV2 ProcessInput updates text from text input`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Hi" 10us frameCtx
    let index = int textboxCtx.ComponentId.Index
    components.Bounds.[index] <- { X = 0f; Y = 0f; Width = 100f; Height = 20f }
    let buffer = InputBufferV2(1)

    let mutable click = true
    let mutable textBuffer = [||]
    let mutable textCount = 0
    let input =
        createInput
            { defaultInputConfig with
                PointerX = 5f
                PointerY = 5f
                PrimaryClick = click
                ConsumeTextInput =
                    fun () ->
                        let count = textCount
                        textCount <- 0
                        struct(textBuffer, count) }

    NoobishInputV2.ProcessInput input components buffer
    components.CaretBlinkReset.[index] <- false
    click <- false
    textBuffer <- [| 'a' |]
    textCount <- 1
    let inputAfterClick =
        createInput
            { defaultInputConfig with
                PointerX = 5f
                PointerY = 5f
                PrimaryClick = click
                ConsumeTextInput =
                    fun () ->
                        let count = textCount
                        textCount <- 0
                        struct(textBuffer, count) }
    NoobishInputV2.ProcessInput inputAfterClick components buffer

    Assert.AreEqual("Hia", components.Text.[index])
    Assert.AreEqual(ValueSome "Hia", buffer.TryGetTextChanged 10us)
    Assert.IsTrue(components.CaretBlinkReset.[index])

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``NoobishInputV2 ProcessInput moves caret with arrow keys`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Hello" 11us frameCtx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.SetFocus(components, index, 5)
    components.CaretBlinkReset.[index] <- false

    let input =
        createInput { defaultInputConfig with IsKeyPressed = fun keyId -> keyId = NoobishKeyId.Left }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(4, components.CaretIndex.[index])
    Assert.IsTrue(components.CaretBlinkReset.[index])

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``NoobishInputV2 calculateSliderValue returns start when width is zero`` () =
    let bounds: Noobish.NoobishRectangle = { X = 0f; Y = 0f; Width = 0f; Height = 10f }
    let value = NoobishInputV2.calculateSliderValue bounds 2f 8f 0f 5f
    Assert.AreEqual(2f, value)

[<Test>]
let ``NoobishInputV2 calculateSliderValue respects unstepped values`` () =
    let bounds: Noobish.NoobishRectangle = { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    let value = NoobishInputV2.calculateSliderValue bounds 0f 10f 0f 5f
    Assert.AreEqual(5f, value)

[<Test>]
let ``NoobishInputV2 ProcessInput edits text with backspace and delete`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "abc" 12us frameCtx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.SetFocus(components, index, 2)

    let input =
        createInput
            { defaultInputConfig with
                ConsumeTextInput = fun () -> struct([| '\b'; '\u007f' |], 2) }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual("a", components.Text.[index])
    Assert.AreEqual(1, components.CaretIndex.[index])
    Assert.AreEqual(ValueSome "a", buffer.TryGetTextChanged 12us)

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``NoobishInputV2 ProcessInput clears focus on enter`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Hi" 13us frameCtx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.SetFocus(components, index, 2)

    let input =
        createInput { defaultInputConfig with ConsumeTextInput = fun () -> struct([| '\r' |], 1) }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(-1, buffer.FocusedIndex)
    Assert.IsFalse(components.Focused.[index])

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``NoobishInputV2 ProcessInput clears focus on escape`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Hi" 18us frameCtx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.SetFocus(components, index, 2)

    let input =
        createInput { defaultInputConfig with ConsumeTextInput = fun () -> struct([| '\u001b' |], 1) }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(-1, buffer.FocusedIndex)
    Assert.IsFalse(components.Focused.[index])

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``NoobishInputV2 ProcessInput ignores control characters`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Hi" 19us frameCtx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.SetFocus(components, index, 2)

    let input =
        createInput { defaultInputConfig with ConsumeTextInput = fun () -> struct([| '\u0001' |], 1) }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual("Hi", components.Text.[index])
    Assert.IsTrue(ValueOption.isNone (buffer.TryGetTextChanged 19us))
    Assert.AreEqual(2, components.CaretIndex.[index])

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``NoobishInputV2 ProcessInput moves caret right with arrow keys`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Hello" 14us frameCtx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.SetFocus(components, index, 0)
    components.CaretBlinkReset.[index] <- false

    let input =
        createInput { defaultInputConfig with IsKeyPressed = fun keyId -> keyId = NoobishKeyId.Right }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(1, components.CaretIndex.[index])
    Assert.IsTrue(components.CaretBlinkReset.[index])

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx

[<Test>]
let ``NoobishInputV2 ProcessInput does not scroll when content fits`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let index = int ctx.ComponentId.Index
    components.Bounds.[index] <- {X = 0f; Y = 0f; Width = 100f; Height = 100f}
    components.ContentSize.[index] <- {Width = 100f; Height = 50f}
    components.Scroll.[index] <- {Horizontal = false; Vertical = true}
    let buffer = InputBufferV2(1)

    let input =
        createInput
            { defaultInputConfig with
                PointerX = 10f
                PointerY = 10f
                ScrollDelta = 10f }

    NoobishInputV2.ProcessInput input components buffer

    Assert.AreEqual(0f, components.ScrollY.[index])

    components.ReleaseContext ctx

[<Test>]
let ``InputBufferV2 Reset clears focus when localId is missing`` () =
    let components = NoobishComponentsV2(1)
    let frameCtx = NoobishV2.beginFrame "Page" components
    let textboxCtx = NoobishV2.beginTextbox "Hi" 15us frameCtx
    let index = int textboxCtx.ComponentId.Index
    let buffer = InputBufferV2(1)
    buffer.SetFocus(components, index, 1)

    components.ReleaseContext textboxCtx
    components.ReleaseContext frameCtx
    components.Clear()
    let frameCtx2 = NoobishV2.beginFrame "Page2" components
    let _textboxCtx2 = NoobishV2.beginTextbox "Other" 16us frameCtx2
    buffer.Reset components

    Assert.AreEqual(-1, buffer.FocusedIndex)
    Assert.AreEqual(0us, buffer.LastFocusedLocalId)
    Assert.AreEqual(0, buffer.CaretIndex)

    components.ReleaseContext frameCtx2
