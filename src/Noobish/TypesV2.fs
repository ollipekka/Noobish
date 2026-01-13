namespace Noobish

open System

[<Struct>]
type UIComponentIdV2 = {
    Namespace: uint16
    Version: uint16
    Index: uint16
    LocalId: uint16
}

module UIComponentIdV2 =
    let empty: UIComponentIdV2 = {
        Namespace = UInt16.MaxValue
        Version = UInt16.MaxValue
        Index = UInt16.MaxValue
        LocalId = UInt16.MaxValue
    }

    let create (ns: uint16) (version: uint16) (index: uint16) (localId: uint16) : UIComponentIdV2 = {
        Namespace = ns
        Version = version
        Index = index
        LocalId = localId
    }

    let isEmpty (id: UIComponentIdV2) =
        id.Namespace = UInt16.MaxValue
        && id.Version = UInt16.MaxValue
        && id.Index = UInt16.MaxValue
        && id.LocalId = UInt16.MaxValue

module NamespaceHash =
    let fnv1a32 (value: string) =
        let mutable hash = 2166136261u
        for i = 0 to value.Length - 1 do
            hash <- hash ^^^ uint32 value.[i]
            hash <- hash * 16777619u
        hash

    let toNamespace (hash: uint32) = uint16 (hash &&& 0xFFFFu)

    let fromPage (page: string) =
        page
        |> fnv1a32
        |> toNamespace
