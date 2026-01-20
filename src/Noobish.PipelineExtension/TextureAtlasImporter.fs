namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline

open System.IO

open System.Text.Json

[<ContentImporter( fileExtension=".json", DefaultProcessor = "TextureAtlasProcessor", DisplayName = "Texture Atlas Importer" )>]
type TextureAtlasImporter () =
    inherit ContentImporter<string*string*string[]>()

    override s.Import(fileName: string, context: ContentImporterContext) =

        if not (File.Exists fileName) then failwith $"Missing file %s{fileName}."

        let json = File.ReadAllText fileName
        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        let input = JsonSerializer.Deserialize<TextureAtlasJson>(json, options)

        let inputFilePath = Path.GetDirectoryName fileName


        let textureFileNames = Glob.getFiles inputFilePath input.Include input.Exclude

        inputFilePath, input.Name, textureFileNames


