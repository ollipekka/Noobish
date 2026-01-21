module Noobish.MonoGame.Test.InputStateTests

open Microsoft.Xna.Framework.Input
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
