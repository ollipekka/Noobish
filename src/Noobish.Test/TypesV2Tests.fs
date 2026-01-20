module Noobish.Test.TypesV2Tests

open System

open NUnit.Framework

open Noobish

[<Test>]
let ``UIComponentIdV2 empty is empty`` () =
    Assert.IsTrue(UIComponentIdV2.isEmpty UIComponentIdV2.empty)

[<Test>]
let ``UIComponentIdV2 isEmpty returns false when any field differs`` () =
    let baseline = UIComponentIdV2.empty
    Assert.IsFalse(UIComponentIdV2.isEmpty { baseline with Namespace = 0us })
    Assert.IsFalse(UIComponentIdV2.isEmpty { baseline with Version = 0us })
    Assert.IsFalse(UIComponentIdV2.isEmpty { baseline with Index = 0us })
    Assert.IsFalse(UIComponentIdV2.isEmpty { baseline with LocalId = 0us })

[<Test>]
let ``UIComponentIdV2 create sets fields`` () =
    let id = UIComponentIdV2.create 1us 2us 3us 4us
    Assert.AreEqual(1us, id.Namespace)
    Assert.AreEqual(2us, id.Version)
    Assert.AreEqual(3us, id.Index)
    Assert.AreEqual(4us, id.LocalId)

[<Test>]
let ``NamespaceHash fnv1a32 is deterministic`` () =
    let hash1 = NamespaceHash.fnv1a32 "Settings/Audio"
    let hash2 = NamespaceHash.fnv1a32 "Settings/Audio"
    Assert.AreEqual(hash1, hash2)

[<Test>]
let ``NamespaceHash fromPage folds fnv1a32`` () =
    let hash = NamespaceHash.fnv1a32 "Settings/Audio"
    let ns = NamespaceHash.fromPage "Settings/Audio"
    Assert.AreEqual(uint16 (hash &&& 0xFFFFu), ns)
    Assert.LessOrEqual(ns, UInt16.MaxValue)

[<Test>]
let ``Internal max0 clamps negatives`` () =
    Assert.AreEqual(0f, Internal.max0 -1f)
    Assert.AreEqual(0f, Internal.max0 0f)
    Assert.AreEqual(2.5f, Internal.max0 2.5f)

[<Test>]
let ``NoobishRectangle Contains checks bounds`` () =
    let bounds: NoobishRectangle = { X = 1f; Y = 2f; Width = 3f; Height = 4f }
    Assert.IsTrue(bounds.Contains 1f 2f)
    Assert.IsTrue(bounds.Contains 4f 6f)
    Assert.IsFalse(bounds.Contains 0.9f 2f)
    Assert.IsFalse(bounds.Contains 4.1f 6f)
