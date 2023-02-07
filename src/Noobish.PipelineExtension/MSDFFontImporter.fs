namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline
open System.Collections.Generic

open Newtonsoft.Json


[<ContentImporter( fileExtension=".txt", DefaultProcessor = "MSDFFontProcessor", DisplayName = "SDFont Importer" )>]
type MSDFFontImporter () =
    inherit ContentImporter<string*MSDFFont>()


    override s.Import(fileName: string, context: ContentImporterContext) =

        if not (File.Exists fileName) then failwith $"Missing file %s{fileName}."

        use fileStream = File.OpenText(fileName)
        use jsonReader = new JsonTextReader(fileStream)
        let serializer = new JsonSerializer()
        let font = serializer.Deserialize<MSDFFont>(jsonReader)

        fileName, font

