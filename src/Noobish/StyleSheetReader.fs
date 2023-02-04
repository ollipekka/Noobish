namespace Noobish.PipelineExtension

open System

open Microsoft.Xna.Framework;
open Microsoft.Xna.Framework.Content;
open Microsoft.Xna.Framework.Graphics;

open Noobish.TextureAtlas
open Noobish.Styles
open System.Collections.Generic


module private DictionaryExtensions =
    type Dictionary<'TKey, 'TValue> with

        member d.GetOrAdd (key: 'TKey) (init: unit -> 'TValue) =

            let (success, value) = d.TryGetValue(key)

            if success then
                value
            else
                let value = init()
                d.[key] <- value
                value

open DictionaryExtensions

type StyleSheetReader () =
    inherit ContentTypeReader<NoobishStyleSheet>()

    let toColor (v: int) =
        let r = (v >>> 24) &&& 255;
        let g = (v >>> 16) &&& 255;
        let b = (v >>> 8) &&& 255;
        let a = v &&& 255;
        Color(r, g, b, a)


    let toReadOnlyDictionary (dictionary: Dictionary<string, Dictionary<string, 'T>>) =
        dictionary
            |> Seq.map(fun kvp -> KeyValuePair(kvp.Key, kvp.Value :> IReadOnlyDictionary<string, 'T>))
            |> Dictionary
            :> IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>>

    let readFloat32Arrays (reader: ContentReader)  =

        let dict = Dictionary<string, Dictionary<string, float32>>()
        let count = reader.ReadInt32()

        for i = 0 to count - 1 do
            let name = reader.ReadString()
            let count2 = reader.ReadInt32()

            let dict2 = dict.GetOrAdd name (fun _ -> Dictionary())

            for j = 0 to count2 - 1 do
                let state = reader.ReadString()
                let v = reader.ReadSingle()
                dict2.[state] <- v

        toReadOnlyDictionary dict

    let readStringArrays (reader: ContentReader)  =

        let dict = Dictionary<string, Dictionary<string, string>>()
        let count = reader.ReadInt32()

        for i = 0 to count - 1 do
            let name = reader.ReadString()
            let count2 = reader.ReadInt32()

            let dict2 = dict.GetOrAdd name (fun _ -> Dictionary())

            for j = 0 to count2 - 1 do
                let state = reader.ReadString()
                let v = reader.ReadString()
                dict2.[state] <- v

        toReadOnlyDictionary dict

    let readColorArrays (reader: ContentReader)  =

        let dict = Dictionary<string, Dictionary<string, Color>>()
        let count = reader.ReadInt32()

        for i = 0 to count - 1 do
            let name = reader.ReadString()
            let count2 = reader.ReadInt32()

            let dict2 = dict.GetOrAdd name (fun _ -> Dictionary())

            for j = 0 to count2 - 1 do
                let state = reader.ReadString()
                let v = reader.ReadInt32()

                dict2.[state] <- toColor v

        toReadOnlyDictionary dict

    let readDrawables (reader: ContentReader)  =

        let dict = Dictionary<string, Dictionary<string, NoobishDrawable[]>>()
        let count = reader.ReadInt32()

        for i = 0 to count - 1 do
            let name = reader.ReadString()
            let count2 = reader.ReadInt32()

            let dict2 = dict.GetOrAdd name (fun _ -> Dictionary())

            for j = 0 to count2 - 1 do
                let state = reader.ReadString()

                let count3 = reader.ReadInt32()

                let v = Array.init count3 (
                    fun k ->

                        let kind = reader.ReadInt32()

                        if kind = 1 then
                            NoobishDrawable.NinePatch (reader.ReadString())
                        elif kind = 2 then
                            NoobishDrawable.NinePatchWithColor (reader.ReadString(), reader.ReadInt32() |> toColor)
                        else
                            failwith "Mangled drawable."

                )

                dict2.[state] <- v

        toReadOnlyDictionary dict


    let readIntTuple4Array (reader: ContentReader)  =

        let dict = Dictionary<string, Dictionary<string, (int*int*int*int)>>()
        let count = reader.ReadInt32()

        for i = 0 to count - 1 do
            let name = reader.ReadString()
            let count2 = reader.ReadInt32()

            let dict2 = dict.GetOrAdd name (fun _ -> Dictionary())

            for j = 0 to count2 - 1 do
                let state = reader.ReadString()
                let t = reader.ReadInt32()
                let r = reader.ReadInt32()
                let b = reader.ReadInt32()
                let l = reader.ReadInt32()
                dict2.[state] <- (t, r, b, l)

        toReadOnlyDictionary dict

    override s.Read(reader: ContentReader, input: NoobishStyleSheet) =

        {
            Name = reader.ReadString()
            TextureAtlasId = reader.ReadString()
            Font = reader.ReadString()
            Widths = readFloat32Arrays reader
            Heights = readFloat32Arrays reader
            Fonts = readStringArrays reader
            FontColors = readColorArrays reader
            Colors = readColorArrays reader
            Drawables = readDrawables reader
            Paddings = readIntTuple4Array reader
            Margins = readIntTuple4Array reader
        }