namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline
open System.Collections.Generic
open System.Text.Json

type StyleSheetJson = {
    TextureAtlas: string
    Styles: Dictionary<string, Dictionary<string, StyleJson>>
}

[<ContentImporter( fileExtension=".json", DefaultProcessor = "StyleSheetProcessor", DisplayName = "Style Sheet Importer" )>]
type StyleSheetImporter () =
    inherit ContentImporter<string*Dictionary<string, Dictionary<string, StyleJson>>>()

    override s.Import(filePath: string, context: ContentImporterContext) =

        if not (File.Exists filePath) then failwith $"Missing file %s{filePath}."

        let json = File.ReadAllText filePath
        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        let styleSheetJson = JsonSerializer.Deserialize<StyleSheetJson>(json, options)

        (styleSheetJson.TextureAtlas,styleSheetJson.Styles)



