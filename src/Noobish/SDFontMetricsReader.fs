namespace Noobish.Fonts


namespace Noobish.PipelineExtension

open System

open Microsoft.Xna.Framework;
open Microsoft.Xna.Framework.Content;
open Microsoft.Xna.Framework.Graphics;

open Noobish.TextureAtlas
open Noobish.Styles
open System.Collections.Generic


type SignedDistanceFieldFontReader () =
    inherit ContentTypeReader<NoobishFont>()

    override s.Read(reader: ContentReader, input: NoobishFont) =

        let textureName = reader.ReadString()


        {TextureId = textureName}

