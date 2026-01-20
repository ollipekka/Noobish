module Noobish.Test.DictionaryExtensionsTests

open System.Collections.Generic
open NUnit.Framework

[<Test>]
let ``Dictionary GetOrAdd returns existing value without calling init`` () =
    let dictionary = Dictionary<string, int>()
    dictionary.["a"] <- 10
    let mutable invoked = false
    let init () =
        invoked <- true
        20

    let value = dictionary.GetOrAdd"a" init

    Assert.AreEqual(10, value)
    Assert.IsFalse(invoked)

[<Test>]
let ``Dictionary GetOrAdd adds and returns value when missing`` () =
    let dictionary = Dictionary<string, int>()
    let mutable invoked = false
    let init () =
        invoked <- true
        30

    let value = dictionary.GetOrAdd "b" init

    Assert.AreEqual(30, value)
    Assert.IsTrue(invoked)
    Assert.AreEqual(30, dictionary.["b"])

[<Test>]
let ``ReadOnlyDictionary GetValueOrDefault returns value when present`` () =
    let dictionary = Dictionary<string, int>()
    dictionary.["a"] <- 5
    let readOnly = dictionary :> IReadOnlyDictionary<_, _>

    let value = readOnly.GetValueOrDefault "a"

    Assert.AreEqual(5, value)

[<Test>]
let ``ReadOnlyDictionary GetValueOrDefault returns default when missing`` () =
    let dictionary = Dictionary<string, int>()
    let readOnly = dictionary :> IReadOnlyDictionary<_, _>

    let value = readOnly.GetValueOrDefault "missing"

    Assert.AreEqual(0, value)

[<Test>]
let ``ReadOnlyDictionary GetValueOrDefault uses fallback when missing`` () =
    let dictionary = Dictionary<string, string>()
    let readOnly = dictionary :> IReadOnlyDictionary<_, _>

    let value = readOnly.GetValueOrDefault("missing", "fallback")

    Assert.AreEqual("fallback", value)

[<Test>]
let ``ReadOnlyDictionary GetValueOrDefault ignores fallback when present`` () =
    let dictionary = Dictionary<string, string>()
    dictionary.["key"] <- "value"
    let readOnly = dictionary :> IReadOnlyDictionary<_, _>

    let value = readOnly.GetValueOrDefault("key", "fallback")

    Assert.AreEqual("value", value)

[<Test>]
let ``toReadOnlyDictionary maps nested dictionaries`` () =
    let outer = Dictionary<string, Dictionary<int, string>>()
    let inner = Dictionary<int, string>()
    inner.[1] <- "one"
    outer.["numbers"] <- inner

    let readOnly = DictionaryExtensions.toReadOnlyDictionary outer

    Assert.IsTrue(readOnly.ContainsKey "numbers")
    let mappedInner = readOnly.["numbers"]
    Assert.AreEqual("one", mappedInner.[1])
