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
let ``mapKeyId is defined for every key except None`` () =
    let cases = FSharpType.GetUnionCases typeof<NoobishKeyId>
    for case in cases do
        let keyId = FSharpValue.MakeUnion(case, [||]) :?> NoobishKeyId
        if keyId = NoobishKeyId.None then
            Assert.IsTrue(ValueOption.isNone (mapKeyId keyId))
        else
            Assert.IsTrue(ValueOption.isSome (mapKeyId keyId))

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
