namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline
open System.Collections.Generic
open Newtonsoft.Json

type StyleSheetJson = {
    TextureAtlas: string
    Font: string
    Styles: Dictionary<string, Dictionary<string, StyleJson>>
}

[<ContentImporter( fileExtension=".json", DefaultProcessor = "StyleSheetProcessor", DisplayName = "Style Sheet Importer" )>]
type StyleSheetImporter () =
    inherit ContentImporter<string*string*Dictionary<string, Dictionary<string, StyleJson>>>()

    override s.Import(filePath: string, context: ContentImporterContext) =

        if not (File.Exists filePath) then failwith $"Missing file %s{filePath}."


        use fileStream = new JsonTextReader(File.OpenText filePath)
        let serializer = JsonSerializer()
        let styleSheetJson = serializer.Deserialize<StyleSheetJson>(fileStream)

        (styleSheetJson.TextureAtlas, styleSheetJson.Font, styleSheetJson.Styles)




