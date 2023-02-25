namespace Noobish.PipelineExtension

open System.IO
open System.Collections.Generic

open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Png

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open Microsoft.Xna.Framework.Content.Pipeline.Processors

[<ContentProcessor(DisplayName = "MSDFFont Processor")>]
type MSDFFontProcessor () =
    inherit ContentProcessor<string*MSDFFont, ExternalReference<TextureContent>*MSDFFont>()

    member val Padding = 1 with get, set

    member val MaxAtlasWidth = 1024 with get, set
    member val MaxAtlasHeight = 1024 with get, set
    member val ColorKeyColor = Microsoft.Xna.Framework.Color.Transparent with get, set

    member val IsColorKeyEnabled = false with get, set
    member val ResizeToPowerOfTwo = false with get, set
    member val GenerateMipmaps = false with get, set
    member val PremultiplyTextureAlpha = true with get, set
    member val TextureFormat = TextureProcessorOutputFormat.Color with get, set

    member s.BuildTexture (name: string) (sourcePath: string) (context: ContentProcessorContext) =

        let parameters = new OpaqueDataDictionary()

        parameters.Add( "ColorKeyColor", s.ColorKeyColor )
        parameters.Add( "ColorKeyEnabled", s.IsColorKeyEnabled )
        parameters.Add( "GenerateMipmaps", s.GenerateMipmaps )
        parameters.Add( "PremultiplyAlpha", s.PremultiplyTextureAlpha )
        parameters.Add( "ResizeToPowerOfTwo", false )
        parameters.Add( "TextureFormat", s.TextureFormat )

        let sourceTexture = ExternalReference<TextureContent>( sourcePath )
        sourceTexture.Name <- name;

        context.BuildAsset<TextureContent, TextureContent>(
            sourceAsset = sourceTexture,
            processorName ="TextureProcessor",
            processorParameters = parameters,
            importerName = "TextureImporter",
            assetName = name );

    override s.Process((fileName, input), context) =

        let directory = Path.GetDirectoryName fileName
        let name = Path.GetFileNameWithoutExtension fileName
        let atlasName = $"{name}Atlas.png"
        let atlasFileName = Path.Combine(directory, atlasName)

        context.AddOutputFile atlasFileName
        let ref = s.BuildTexture atlasName atlasFileName context
        ref, input