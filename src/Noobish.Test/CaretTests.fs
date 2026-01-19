module Noobish.Test.CursorTests

open System
open NUnit.Framework
open Noobish.Cursor

[<Test>]
let ``Cursor blink starts visible and stays visible for half interval`` () =
    let half = blinkInterval.TotalSeconds * 0.6
    let epsilon = min 0.0001 (half * 0.1)
    Assert.AreEqual(0f, blink TimeSpan.Zero)
    Assert.AreEqual(0f, blink (TimeSpan.FromSeconds (half - epsilon)))
    Assert.AreEqual(1f, blink (TimeSpan.FromSeconds (half + epsilon)))
