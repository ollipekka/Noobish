namespace Noobish.PipelineExtension

open System.IO

open Microsoft.Xna.Framework.Content.Pipeline
open System.Collections.Generic

open System.Text.Json


[<ContentImporter( fileExtension=".txt", DefaultProcessor = "MSDFFontProcessor", DisplayName = "SDFont Importer" )>]
type MSDFFontImporter () =
    inherit ContentImporter<string*MSDFFont>()


    override s.Import(fileName: string, context: ContentImporterContext) =

        if not (File.Exists fileName) then failwith $"Missing file %s{fileName}."

        let json = File.ReadAllText fileName
        let options = JsonSerializerOptions(PropertyNameCaseInsensitive = true)
        let font = JsonSerializer.Deserialize<MSDFFont>(json, options)

        fileName, font
