module Noobish.Test.TypesV2Tests

open System

open NUnit.Framework

open Noobish

[<Test>]
let ``UIComponentIdV2 empty is empty`` () =
    Assert.IsTrue(UIComponentIdV2.isEmpty UIComponentIdV2.empty)

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
let ``NamespaceHash fnv1a32 normalizes input`` () =
    let hash1 = NamespaceHash.fnv1a32 " Settings/Audio "
    let hash2 = NamespaceHash.fnv1a32 "settings/audio"
    Assert.AreEqual(hash1, hash2)

[<Test>]
let ``NamespaceHash fromPage folds fnv1a32`` () =
    let hash = NamespaceHash.fnv1a32 "Settings/Audio"
    let ns = NamespaceHash.fromPage "Settings/Audio"
    Assert.AreEqual(uint16 (hash &&& 0xFFFFu), ns)
    Assert.LessOrEqual(ns, UInt16.MaxValue)
