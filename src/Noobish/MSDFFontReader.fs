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

    let readGlyph (reader:ContentReader) (kerning: Dictionary<char, Dictionary<char, float32>>)  =
        let unicode = char (reader.ReadInt64())
        let advance = reader.ReadSingle()
        let atlasBounds = readBounds (reader)
        let planeBounds = readBounds (reader)
        let kerning =
            let success, result =
                kerning.TryGetValue unicode

            if success then
                result
            else
                Dictionary()

        {Unicode = unicode; Advance = advance; AtlasBounds = atlasBounds; PlaneBounds = planeBounds; Kerning = kerning }

    let readKerning (reader:ContentReader) =
        let unicode1 = char (reader.ReadInt64())
        let unicode2 = char (reader.ReadInt64())
        let advance = reader.ReadSingle()
        struct(unicode1, unicode2, advance)

    override s.Read(reader: ContentReader, input: NoobishFont) =

        let texture = reader.ReadExternalReference<Texture2D>()
        let atlas = readAtlas reader
        let metrics = readMetrics reader

        let kerning = Dictionary<char, Dictionary<char, float32>>()
        let kerningCount = reader.ReadInt32()
        for _i = 0 to kerningCount - 1 do
            let struct(u1, u2, advance) = readKerning reader

            let glyphKerning = kerning.GetOrAdd u1 (fun _ -> Dictionary())
            glyphKerning.[u2] <- advance

        let glyphCount = reader.ReadInt32()
        let glyphs = Dictionary<char, NoobishGlyph>()
        for _i = 0 to glyphCount - 1 do
            let g = readGlyph reader kerning
            glyphs.[g.Unicode] <- g

        {Atlas = atlas; Metrics = metrics; Glyphs = glyphs; Kerning = toReadOnlyDictionary kerning; Texture = texture}