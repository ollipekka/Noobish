namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline
open System.Collections.Generic
open Newtonsoft.Json

type LocalizationBundleJson = {
    Name: string
    Include: string[]
    Exclude: string[]
}


[<ContentImporter( fileExtension=".json", DefaultProcessor = "LocalizationBundleProcessor", DisplayName = "Localization Bundle Importer" )>]
type LocalizationBundleImporter () =
    inherit ContentImporter<string*string*string[]>()

    override s.Import(filePath: string, context: ContentImporterContext) =

        if not (File.Exists filePath) then failwith $"Missing file %s{filePath}."


        use fileStream = new JsonTextReader(File.OpenText filePath)
        let serializer = JsonSerializer()
        let input = serializer.Deserialize<LocalizationBundleJson>(fileStream)

        let inputFilePath = Path.GetDirectoryName filePath

        let files = Glob.getFiles inputFilePath input.Include input.Exclude

        (inputFilePath, input.Name, files)




