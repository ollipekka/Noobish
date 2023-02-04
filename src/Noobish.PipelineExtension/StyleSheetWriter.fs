namespace Noobish.PipelineExtension

open System.Collections.Generic

open Microsoft.Xna.Framework.Content.Pipeline;
open Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler

[<ContentTypeWriter>]
type StyleSheetWriter () =
    inherit ContentTypeWriter<StyleSheetContent>()

    let writeFloat32Arrays (writer: ContentWriter) (s: (string*(string*float32)[])[]) =
        writer.Write s.Length
        for (name, byState) in s do
            writer.Write name
            writer.Write byState.Length

            for (state, v) in byState do
                writer.Write state
                writer.Write v

    let writeColors (writer: ContentWriter) (s: (string*(string*string)[])[]) =
        writer.Write s.Length
        for (name, byState) in s do
            writer.Write name
            writer.Write byState.Length

            for (state, v) in byState do
                writer.Write state
                writer.Write (System.Convert.ToInt32 (v, 16))

    let writeStringArrays (writer: ContentWriter) (s: (string*(string*string)[])[]) =
        writer.Write s.Length
        for (name, valueByState) in s do
            writer.Write name
            writer.Write valueByState.Length

            for (state, v) in valueByState do
                writer.Write state
                writer.Write v

    let writeDrawables (writer: ContentWriter) (s: (string*(string*string[][])[])[]) =
        writer.Write s.Length
        for (name, valueByState) in s do
            writer.Write name
            writer.Write valueByState.Length

            for (state, values) in valueByState do
                writer.Write state
                writer.Write values.Length
                for v in values do
                    if v.Length = 1 then
                        writer.Write 1
                        writer.Write v.[0]
                    elif v.Length = 2 then
                        writer.Write 2
                        writer.Write v.[0]
                        writer.Write (System.Convert.ToInt32 (v.[1], 16))
                    else
                        failwith $"Unrecoginzed drawable %A{v}"

    let writeIntTuple4Array (writer: ContentWriter) (s: (string*(string*(int*int*int*int))[])[]) =
        writer.Write s.Length
        for (name, valueByState) in s do
            writer.Write name
            writer.Write valueByState.Length

            for (state, (t, r, b, l)) in valueByState do
                writer.Write state
                writer.Write t
                writer.Write r
                writer.Write b
                writer.Write l

    override s.Write(writer: ContentWriter, input: StyleSheetContent) =
        writer.Write input.Name
        writer.Write input.TextureAtlas
        writer.Write input.Font
        writeFloat32Arrays writer input.Widths
        writeFloat32Arrays writer input.Heights

        writeStringArrays writer input.Fonts
        writeColors writer input.FontColors

        writeColors writer input.Colors

        writeDrawables writer input.Drawables

        writeIntTuple4Array writer input.Margins
        writeIntTuple4Array writer input.Paddings

    override s.GetRuntimeReader(targetPlatform: TargetPlatform) = "Noobish.PipelineExtension.StyleSheetReader, Noobish.Types"