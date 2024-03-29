namespace Noobish.PipelineExtension


open Microsoft.Xna.Framework;
open Microsoft.Xna.Framework.Content;
open Microsoft.Xna.Framework.Graphics;

open Noobish.TextureAtlas

type TextureAtlasReader () =
    inherit ContentTypeReader<NoobishTextureAtlas>()

    override s.Read(reader: ContentReader, input: NoobishTextureAtlas) =

        let name = reader.ReadString()
        let atlasTexture = reader.ReadExternalReference<Texture2D>()
        let count = reader.ReadInt32()

        let textures = System.Collections.Generic.Dictionary<string, NoobishTexture>()

        for i = 0 to count - 1 do
            let textureName = reader.ReadString()
            let textureType =
                let t = reader.ReadString()
                if t = "NinePatch" then
                    let top = reader.ReadInt32()
                    let right = reader.ReadInt32()
                    let bottom = reader.ReadInt32()
                    let left = reader.ReadInt32()
                    NoobishTextureType.NinePatch(top, right, bottom, left)
                else
                    NoobishTextureType.Texture

            let x = reader.ReadInt32()
            let y = reader.ReadInt32()
            let w = reader.ReadInt32()
            let h = reader.ReadInt32()
            let region = Rectangle(x, y, w, h)
            textures.[textureName] <- {Name = textureName; TextureType = textureType; SourceRectangle = region; Atlas = atlasTexture}


        {Name = name; Textures = textures}
