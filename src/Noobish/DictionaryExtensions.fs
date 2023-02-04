namespace System.Collections.Generic

[<AutoOpen>]
module DictionaryExtensions =
    type System.Collections.Generic.Dictionary<'TKey, 'TValue> with

        member d.GetOrAdd (key: 'TKey) (init: unit -> 'TValue) =

            let (success, value) = d.TryGetValue(key)

            if success then
                value
            else
                let value = init()
                d.[key] <- value
                value
