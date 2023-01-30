namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline

[<ContentImporter( fileExtension=".json", DefaultProcessor = "StyleSheetProcessor", DisplayName = "Style Sheet Importer" )>]
type StyleSheetImporter () =
    inherit ContentImporter<StyleSheetJson>()

    override s.Import(filePath: string, context: ContentImporterContext) =

        if not (File.Exists filePath) then failwith $"Missing file %s{filePath}."


        StyleSheetJson.fromJsonFile filePath



