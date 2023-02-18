namespace Noobish.PipelineExtension

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler

[<ContentTypeWriter>]
type TextureAtlasWriter () =
    inherit ContentTypeWriter<TextureAtlasContent>()


    override s.Write(writer: ContentWriter, input: TextureAtlasContent) =
        writer.Write input.Name
        writer.WriteExternalReference input.Texture
        writer.Write input.Textures.Length

        for (name, textureType, image) in input.Textures do

            writer.Write name
            match textureType with
            | NinePatch(top, right, bottom, left) ->
                writer.Write("NinePatch")
                writer.Write top
                writer.Write right
                writer.Write bottom
                writer.Write left
            | Texture ->
                writer.Write("Texture")

            let padding = input.Padding
            let region = input.Regions.[name]
            writer.Write (region.X + padding + 1)
            writer.Write (region.Y + padding + 1)
            writer.Write (region.Width - 2 * padding - 2)
            writer.Write (region.Height - 2 * padding- 2)


    override s.GetRuntimeReader(targetPlatform: TargetPlatform) = "Noobish.PipelineExtension.TextureAtlasReader, Noobish"