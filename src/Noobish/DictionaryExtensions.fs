namespace System.Collections.Generic

[<AutoOpen>]
module DictionaryExtensions =
    type System.Collections.Generic.Dictionary<'TKey, 'TValue> with

        member d.GetOrAdd (key: 'TKey, init: unit -> 'TValue) =

            let mutable value = Unchecked.defaultof<'TValue>
            let success = d.TryGetValue(key, &value)

            if success then
                value
            else
                let value = init()
                d.[key] <- value
                value

    type System.Collections.Generic.IReadOnlyDictionary<'TKey, 'TValue> with

        member d.GetValueOrDefault (key: 'TKey) =
            let mutable value = Unchecked.defaultof<'TValue>
            let success = d.TryGetValue(key, &value)
            if success then value else Unchecked.defaultof<'TValue>

        member d.GetValueOrDefault (key: 'TKey, fallback: 'TValue) =
            let mutable value = fallback
            let success = d.TryGetValue(key, &value)
            if success then value else fallback

    let toReadOnlyDictionary (dictionary: Dictionary<'K1, Dictionary<'K2, 'T>>) =
        dictionary
            |> Seq.map(fun kvp -> KeyValuePair(kvp.Key, kvp.Value :> IReadOnlyDictionary<'K2, 'T>))
            |> Dictionary<'K1, IReadOnlyDictionary<'K2, 'T>>
            :> IReadOnlyDictionary<'K1, IReadOnlyDictionary<'K2, 'T>>
