namespace Noobish.PipelineExtension

open System.IO
open System.Collections.Generic

open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Png

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open Microsoft.Xna.Framework.Content.Pipeline.Processors

[<ContentProcessor(DisplayName = "Style Sheet Procesor")>]
type StyleSheetProcessor () =
    inherit ContentProcessor<StyleSheetJson, StyleSheetContent>()

    member val Padding = 1 with get, set

    member val MaxAtlasWidth = 1024 with get, set
    member val MaxAtlasHeight = 1024 with get, set
    member val ColorKeyColor = Microsoft.Xna.Framework.Color.Transparent with get, set

    member val IsColorKeyEnabled = false with get, set
    member val ResizeToPowerOfTwo = false with get, set
    member val GenerateMipmaps = false with get, set
    member val PremultiplyTextureAlpha = true with get, set
    member val TextureFormat = TextureProcessorOutputFormat.Color with get, set

    member s.BuildTextureAtlas (name: string) (sourcePath: string) (context: ContentProcessorContext) =
        let parameters = new OpaqueDataDictionary()

        parameters.Add( "ColorKeyColor", s.ColorKeyColor )
        parameters.Add( "ColorKeyEnabled", s.IsColorKeyEnabled )
        parameters.Add( "GenerateMipmaps", s.GenerateMipmaps )
        parameters.Add( "PremultiplyAlpha", s.PremultiplyTextureAlpha )
        parameters.Add( "ResizeToPowerOfTwo", false )
        parameters.Add( "TextureFormat", s.TextureFormat )

        let sourceAtlas = ExternalReference<TextureAtlasContent>( sourcePath )
        sourceAtlas.Name <- name;

        context.BuildAsset<TextureAtlasContent, TextureAtlasContent>(
            sourceAsset = sourceAtlas,
            processorName ="TextureAtlasProcessor",
            processorParameters = parameters,
            importerName = "TextureAtlasImporter",
            assetName = name );


    override s.Process(input: StyleSheetJson, context: ContentProcessorContext) =

        let atlas = s.BuildTextureAtlas (Path.GetFileNameWithoutExtension input.TextureAtlas) input.TextureAtlas context

        {
            Name = Path.GetFileNameWithoutExtension context.OutputFilename
            Font = input.Font
            TextureAtlas = atlas
            Styles = input.Styles
        }