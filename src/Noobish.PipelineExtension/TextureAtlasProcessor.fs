namespace Noobish.PipelineExtension

open System.IO
open System.Collections.Generic

open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Png

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open Microsoft.Xna.Framework.Content.Pipeline.Processors

[<ContentProcessor(DisplayName = "TextureAtlasContent procesor")>]
type TextureAtlasProcessor () =
    inherit ContentProcessor<TextureAtlasJson, TextureAtlasContent>()

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


    override s.Process(input: TextureAtlasJson, context: ContentProcessorContext) =


        let outputFilePath = Path.GetDirectoryName(context.OutputFilename)

        let relativePath = Path.GetRelativePath (context.OutputDirectory, outputFilePath)

        let textureFileNames = TextureAtlasJson.getFiles relativePath input

        let textures = TexturePacker.createTextures textureFileNames
        let (regions, atlasWidth, atlasHeight) = TexturePacker.createRegions s.MaxAtlasWidth s.MaxAtlasHeight s.Padding s.ResizeToPowerOfTwo textures

        let image = TexturePacker.createImage textures regions s.Padding atlasWidth atlasHeight

        let atlasTextureFileName = Path.Combine(context.OutputDirectory, (sprintf "%sTexture.png" input.Name))
        let stream = File.OpenWrite(atlasTextureFileName)
        image.Save(stream, new PngEncoder())
        stream.Close()

        context.AddOutputFile(atlasTextureFileName)

        {Name = input.Name; Padding = s.Padding; Textures = textures; Regions = regions; Texture = s.BuildTexture (sprintf "%sTexture" input.Name) atlasTextureFileName context}