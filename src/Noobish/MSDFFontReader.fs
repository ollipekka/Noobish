namespace Noobish.Fonts


namespace Noobish.PipelineExtension

open System

open Microsoft.Xna.Framework;
open Microsoft.Xna.Framework.Content;
open Microsoft.Xna.Framework.Graphics;

open Noobish.TextureAtlas
open Noobish.Styles
open System.Collections.Generic
open Noobish


type MSDFFontReader () =
    inherit ContentTypeReader<NoobishFont>()

    let readAtlas (reader: ContentReader) =
        {
            FontType = reader.ReadString()
            DistanceRange = reader.ReadSingle()
            Size = reader.ReadSingle()
            Width = reader.ReadInt32()
            Height = reader.ReadInt32()
            yOrigin = reader.ReadString()
        }


    let readMetrics (reader: ContentReader) =
        {
            EmSize = reader.ReadInt32()
            LineHeight = reader.ReadSingle()
            Ascender = reader.ReadSingle()
            Descender = reader.ReadSingle()
            UnderlineY = reader.ReadSingle()
            UnderlineThickness = reader.ReadSingle()
        }

    let readBounds (reader: ContentReader) =
        let top = reader.ReadSingle()
        let right = reader.ReadSingle()
        let bottom = reader.ReadSingle()
        let left = reader.ReadSingle()

        struct(top, right, bottom, left)

    let readGlyph (reader:ContentReader)  =
        let unicode = reader.ReadInt64()
        let advance = reader.ReadSingle()
        let atlasBounds = readBounds (reader)
        let planeBounds = readBounds (reader)

        {Unicode = unicode; Advance = advance; AtlasBounds = atlasBounds; PlaneBounds = planeBounds}



    let readKerning (reader:ContentReader) =
        let unicode1 = reader.ReadInt64()
        let unicode2 = reader.ReadInt64()
        let advance = reader.ReadSingle()
        struct(unicode1, unicode2, advance)

    override s.Read(reader: ContentReader, input: NoobishFont) =

        let texture = reader.ReadExternalReference<Texture2D>()
        let atlas = readAtlas reader
        let metrics = readMetrics reader

        let glyphCount = reader.ReadInt32()
        let glyphs = Dictionary<int64, NoobishGlyph>()
        for i = 0 to glyphCount - 1 do
            let g = readGlyph reader
            glyphs.[g.Unicode] <- g


        let kerning = Dictionary<int64, Dictionary<int64, float32>>()
        let kerningCount = reader.ReadInt32()
        for i = 0 to kerningCount - 1 do
            let struct(u1, u2, advance) = readKerning reader

            let glyphKerning = kerning.GetOrAdd u1 (fun _ -> Dictionary())
            glyphKerning.[u2] <- advance

        {Atlas = atlas; Metrics = metrics; Glyphs = glyphs; Kerning = toReadOnlyDictionary kerning; Texture = texture}