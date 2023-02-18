namespace Noobish.PipelineExtension

open System.IO
open System.Collections.Generic

open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Png

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open Microsoft.Xna.Framework.Content.Pipeline.Processors

[<ContentProcessor(DisplayName = "Texture Atlas Procesor")>]
type TextureAtlasProcessor () =
    inherit ContentProcessor<string*string*string[], TextureAtlasContent>()

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


    override s.Process((_path: string, name: string, files: string[]), context: ContentProcessorContext) =
        let padding = s.Padding
        for f in files do
            context.AddDependency f

        let textures = TexturePacker.createTextures files

        if textures.Length = 0 then failwith "No textures in the atlas."

        let (regions, atlasWidth, atlasHeight) = TexturePacker.createRegions s.MaxAtlasWidth s.MaxAtlasHeight padding s.ResizeToPowerOfTwo textures

        let image = TexturePacker.createImage textures regions padding atlasWidth atlasHeight

        let atlasTextureFileName = Path.Combine(context.OutputDirectory, (sprintf "%sTexture.png" name))
        let stream = File.OpenWrite(atlasTextureFileName)
        image.Save(stream, new PngEncoder())
        stream.Close()

        context.AddOutputFile(atlasTextureFileName)

        {Name = name; Padding = padding; Textures = textures; Regions = regions; Texture = s.BuildTexture (sprintf "%sTexture" name) atlasTextureFileName context}