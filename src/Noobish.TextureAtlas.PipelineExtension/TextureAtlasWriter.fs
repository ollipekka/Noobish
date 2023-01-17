namespace Noobish.TextureAtlas.PipelineExtension

open Microsoft.Xna.Framework.Content.Pipeline;
open Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

[<ContentTypeWriter>]
type TextureAtlasWriter () =
    inherit ContentTypeWriter<TextureAtlasContent>()


    override s.Write(writer: ContentWriter, input: TextureAtlasContent) =
        writer.Write input.Name
        writer.WriteExternalReference input.Texture
        writer.Write input.Textures.Length

        for texture in input.Textures do

            writer.Write texture.Name
            match texture.TextureType with
            | TextureType.NinePatch(top, right, bottom, left) ->
                writer.Write("NinePatch")
                writer.Write top
                writer.Write right
                writer.Write bottom
                writer.Write left
            | TextureType.Texture ->
                writer.Write("Texture")

            let region = input.Regions.[texture.Name]
            writer.Write region.X
            writer.Write region.Y
            writer.Write region.Width
            writer.Write region.Height


    override s.GetRuntimeReader(targetPlatform: TargetPlatform) = "Noobish.TextureAtlas.TextureAtlasReader, Noobish.TextureAtlas"