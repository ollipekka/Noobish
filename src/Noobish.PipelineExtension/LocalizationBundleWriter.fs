namespace Noobish.PipelineExtension

open System.Collections.Generic

open Microsoft.Xna.Framework.Content.Pipeline;
open Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler

[<ContentTypeWriter>]
type LocalizationBundleWriter () =
    inherit ContentTypeWriter<string*Dictionary<string, string>>()

    override s.Write(writer: ContentWriter, (name: string, input: Dictionary<string, string>)) =
        writer.Write name
        writer.Write input.Count

        for kvp in input do
            writer.Write kvp.Key
            writer.Write kvp.Value


    override s.GetRuntimeReader(targetPlatform: TargetPlatform) = "Noobish.PipelineExtension.LocalizationBundleReader, Noobish"