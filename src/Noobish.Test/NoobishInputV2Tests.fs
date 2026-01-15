module Noobish.Test.NoobishInputV2Tests

open NUnit.Framework
open Noobish

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
let ``NoobishInputV2 contains checks bounds`` () =
    let bounds:Noobish.Internal.NoobishRectangle = {X = 1f; Y = 2f; Width = 3f; Height = 4f}
    Assert.IsTrue(NoobishInputV2.contains bounds 1f 2f)
    Assert.IsTrue(NoobishInputV2.contains bounds 4f 6f)
    Assert.IsFalse(NoobishInputV2.contains bounds 0.9f 2f)
    Assert.IsFalse(NoobishInputV2.contains bounds 4.1f 6f)

[<Test>]
let ``NoobishInputV2 clippedBounds clamps to parent`` () =
    let components = NoobishComponentsV2(2)
    let parentCtx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let childCtx = NoobishV2.beginLabel "Child" parentCtx
    let parentIndex = int parentCtx.ComponentId.Index
    let childIndex = int childCtx.ComponentId.Index

    components.Bounds.[parentIndex] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[childIndex] <- {Noobish.Internal.NoobishRectangle.X = -5f; Y = -5f; Width = 20f; Height = 20f}

    let clipped = NoobishInputV2.clippedBounds components childIndex
    Assert.AreEqual(0f, clipped.X)
    Assert.AreEqual(0f, clipped.Y)
    Assert.AreEqual(10f, clipped.Width)
    Assert.AreEqual(10f, clipped.Height)

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

    components.Bounds.[rootIndex] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[firstIndex] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[secondIndex] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}

    let hit = NoobishInputV2.hitTest components 5f 5f (fun _ -> true)
    Assert.AreEqual(secondIndex, hit)

    components.ReleaseContext secondCtx
    components.ReleaseContext firstCtx
    components.ReleaseContext rootCtx

[<Test>]
let ``NoobishInputV2 hitTest returns -1 when no hit`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let labelCtx = NoobishV2.beginLabel "Label" ctx
    let index = int labelCtx.ComponentId.Index

    components.Bounds.[index] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}

    let hit = NoobishInputV2.hitTest components 50f 50f (fun _ -> true)
    Assert.AreEqual(-1, hit)

    components.ReleaseContext labelCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 process clears down when not primary down`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let buttonCtx = NoobishV2.beginButton "Ok" 21us ctx
    let rootIndex = int ctx.ComponentId.Index
    let buttonIndex = int buttonCtx.ComponentId.Index

    components.Bounds.[rootIndex] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[buttonIndex] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.WantsOnClick.[buttonIndex] <- false

    let buffer = InputBufferV2(2)
    buffer.Reset components
    buffer.SetDown buttonIndex

    let input =
        { new INoobishInputState with
            member _.PointerX = 50f
            member _.PointerY = 50f
            member _.IsPrimaryClick() = false
            member _.IsPrimaryDown() = false
            member _.IsSecondaryClick() = false
            member _.IsKeyPressed _ = false }

    NoobishInputV2.process input components buffer

    Assert.IsFalse(buffer.IsDown 21us)
    Assert.IsTrue(buffer.WasReleased 21us)
    Assert.IsFalse(buffer.WasClicked 21us)
    Assert.AreEqual(0us, buffer.GetClicked())

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 process marks pressed when primary down`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let buttonCtx = NoobishV2.beginButton "Ok" 22us ctx
    let rootIndex = int ctx.ComponentId.Index
    let buttonIndex = int buttonCtx.ComponentId.Index

    components.Bounds.[rootIndex] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}
    components.Bounds.[buttonIndex] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}

    let buffer = InputBufferV2(2)
    let input =
        { new INoobishInputState with
            member _.PointerX = 5f
            member _.PointerY = 5f
            member _.IsPrimaryClick() = false
            member _.IsPrimaryDown() = true
            member _.IsSecondaryClick() = false
            member _.IsKeyPressed _ = false }

    NoobishInputV2.process input components buffer

    Assert.IsTrue(buffer.IsDown 22us)
    Assert.IsTrue(buffer.WasPressed 22us)
    Assert.AreEqual(22us, buffer.GetPressed())

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx

[<Test>]
let ``NoobishInputV2 process clears down when index out of range`` () =
    let components = NoobishComponentsV2(2)
    let ctx = NoobishV2.beginFrame "Page" components |> NoobishV2.beginPanel
    let buttonCtx = NoobishV2.beginButton "Ok" 30us ctx
    let index = int buttonCtx.ComponentId.Index

    let buffer = InputBufferV2(2)
    buffer.SetDown index

    components.Count <- 1
    let input =
        { new INoobishInputState with
            member _.PointerX = 0f
            member _.PointerY = 0f
            member _.IsPrimaryClick() = false
            member _.IsPrimaryDown() = true
            member _.IsSecondaryClick() = false
            member _.IsKeyPressed _ = false }

    NoobishInputV2.process input components buffer

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
let ``NoobishInputV2 marks hovered`` () =
    let components = NoobishComponentsV2(1)
    let ctx = NoobishV2.beginFrame "Page" components
    let buttonCtx = NoobishV2.beginButton "Ok" 1us ctx
    let index = int buttonCtx.ComponentId.Index
    components.Bounds.[index] <- {Noobish.Internal.NoobishRectangle.X = 0f; Y = 0f; Width = 10f; Height = 10f}
    let buffer = InputBufferV2(1)

    let input =
        { new INoobishInputState with
            member _.PointerX = 5f
            member _.PointerY = 5f
            member _.IsPrimaryClick() = false
            member _.IsPrimaryDown() = false
            member _.IsSecondaryClick() = false
            member _.IsKeyPressed _ = false }

    NoobishInputV2.process input components buffer

    Assert.IsTrue(components.Hovered.[index])

    components.ReleaseContext buttonCtx
    components.ReleaseContext ctx
