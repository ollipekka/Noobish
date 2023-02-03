namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline

[<ContentImporter( fileExtension=".json", DefaultProcessor = "TextureAtlasProcessor", DisplayName = "Texture Atlas Importer" )>]
type TextureAtlasImporter () =
    inherit ContentImporter<string*string*string[]>()

    override s.Import(filePath: string, context: ContentImporterContext) =

        if not (File.Exists filePath) then failwith $"Missing file %s{filePath}."

        let input = TextureAtlasJson.fromJsonFile filePath

        let inputFilePath = Path.GetDirectoryName filePath


        let textureFileNames = TextureAtlasJson.getFiles inputFilePath input

        inputFilePath, input.Name, textureFileNames



