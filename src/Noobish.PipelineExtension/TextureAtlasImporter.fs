namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline

[<ContentImporter( fileExtension=".json", DefaultProcessor = "TextureAtlasProcessor", DisplayName = "Texture Atlas Importer" )>]
type TextureAtlasImporter () =
    inherit ContentImporter<TextureAtlasJson>()

    override s.Import(filePath: string, context: ContentImporterContext) =

        if not (File.Exists filePath) then failwith $"Missing file %s{filePath}."

        TextureAtlasJson.fromJson filePath



