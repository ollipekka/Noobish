module Noobish.MonoGame.Test.InputStateTests

open System
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open Microsoft.FSharp.Reflection
open NUnit.Framework
open Noobish
open NoobishInputStateHelpers

[<Test>]
let ``mapKeyId returns keys for mapped ids`` () =
    let mapped =
        [ (NoobishKeyId.A, Keys.A)
          (NoobishKeyId.Escape, Keys.Escape)
          (NoobishKeyId.Left, Keys.Left)
          (NoobishKeyId.Right, Keys.Right) ]

    for (keyId, expected) in mapped do
        Assert.AreEqual(ValueSome expected, mapKeyId keyId)

[<Test>]
let ``mapKeyId returns none for unmapped ids`` () =
    Assert.IsTrue(ValueOption.isNone (mapKeyId NoobishKeyId.None))

[<Test>]
let ``mapMouseButtonId returns flags for mapped ids`` () =
    let mapped =
        [ (NoobishMouseButtonId.Left, 1uy)
          (NoobishMouseButtonId.Right, 2uy)
          (NoobishMouseButtonId.Middle, 4uy)
          (NoobishMouseButtonId.XButton1, 8uy)
          (NoobishMouseButtonId.XButton2, 16uy) ]

    for (buttonId, expected) in mapped do
        Assert.AreEqual(ValueSome expected, mapMouseButtonId buttonId)

[<Test>]
let ``mapMouseButtonId returns none for unmapped ids`` () =
    Assert.IsTrue(ValueOption.isNone (mapMouseButtonId NoobishMouseButtonId.None))

[<Test>]
let ``mapKeyId is defined for every key except None`` () =
    let cases = FSharpType.GetUnionCases typeof<NoobishKeyId>
    for case in cases do
        let keyId = FSharpValue.MakeUnion(case, [||]) :?> NoobishKeyId
        if keyId = NoobishKeyId.None then
            Assert.IsTrue(ValueOption.isNone (mapKeyId keyId))
        else
            Assert.IsTrue(ValueOption.isSome (mapKeyId keyId))

[<Test>]
let ``mapMouseButtonId is defined for every button except None`` () =
    let cases = FSharpType.GetUnionCases typeof<NoobishMouseButtonId>
    for case in cases do
        let buttonId = FSharpValue.MakeUnion(case, [||]) :?> NoobishMouseButtonId
        if buttonId = NoobishMouseButtonId.None then
            Assert.IsTrue(ValueOption.isNone (mapMouseButtonId buttonId))
        else
            Assert.IsTrue(ValueOption.isSome (mapMouseButtonId buttonId))

[<Test>]
let ``NoobishInputState enqueues and consumes text input`` () =
    let state =
        NoobishInputState(
            (fun () -> Unchecked.defaultof<KeyboardState>),
            (fun () -> Unchecked.defaultof<MouseState>),
            (fun () -> Unchecked.defaultof<TouchCollection>))
    state.EnqueueTextInput 'a'
    state.EnqueueTextInput 'b'

    let struct(buffer, count) = state.ConsumeTextInput()
    Assert.AreEqual(2, count)
    Assert.AreEqual('a', buffer.[0])
    Assert.AreEqual('b', buffer.[1])

    let struct(buffer2, count2) = state.ConsumeTextInput()
    Assert.AreSame(buffer, buffer2)
    Assert.AreEqual(0, count2)

[<Test>]
let ``NoobishInputState text input does not allocate after warmup`` () =
    let state =
        NoobishInputState(
            (fun () -> Unchecked.defaultof<KeyboardState>),
            (fun () -> Unchecked.defaultof<MouseState>),
            (fun () -> Unchecked.defaultof<TouchCollection>))

    for _ = 1 to 32 do
        state.EnqueueTextInput 'x'
    state.ConsumeTextInput() |> ignore

    let inline measureLoop (iterations: int) (action: unit -> unit) =
        // Warm to avoid JIT allocations in the measurement window.
        action()
        let before = GC.GetAllocatedBytesForCurrentThread()
        for _ = 1 to iterations do
            action()
        GC.GetAllocatedBytesForCurrentThread() - before

    let iterations = 50
    let allocated =
        measureLoop iterations (fun () ->
            state.EnqueueTextInput 'y'
            state.ConsumeTextInput() |> ignore)

    Assert.AreEqual(0L, allocated, $"Expected 0 allocations but got {allocated}.")

[<Test>]
let ``NoobishInputState reports mouse button 1 click`` () =
    let pressed =
        MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed,
            ButtonState.Released)
    let released =
        MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released)

    let states = [| pressed; released |]
    let mutable index = 0
    let getMouseState () =
        let state = states.[index]
        if index < states.Length - 1 then
            index <- index + 1
        state

    let input =
        NoobishInputState(
            (fun () -> Unchecked.defaultof<KeyboardState>),
            getMouseState,
            (fun () -> Unchecked.defaultof<TouchCollection>))

    input.Update()

    Assert.IsTrue(input.IsMouseClick NoobishMouseButtonId.XButton1)
    Assert.IsFalse(input.IsMouseClick NoobishMouseButtonId.XButton2)

[<Test>]
let ``NoobishInputState reports mouse button 2 down`` () =
    let pressed =
        MouseState(
            0,
            0,
            0,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Released,
            ButtonState.Pressed)

    let input =
        NoobishInputState(
            (fun () -> Unchecked.defaultof<KeyboardState>),
            (fun () -> pressed),
            (fun () -> Unchecked.defaultof<TouchCollection>))

    Assert.IsTrue(input.IsMouseDown NoobishMouseButtonId.XButton2)
