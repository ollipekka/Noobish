namespace Noobish.PipelineExtension

open System.IO
open System.Collections.Generic

open Newtonsoft.Json

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open Microsoft.Xna.Framework.Content.Pipeline.Processors




[<ContentProcessor(DisplayName = "Localization Bundle Procesor")>]
type LocalizationBundleProcessor () =
    inherit ContentProcessor<string*string*string[], string*Dictionary<string, string>>()


    override s.Process((inputFilePath, name, files), context: ContentProcessorContext) =

        let result = Dictionary<string, string>()

        let serializer = JsonSerializer()

        for file in files do
            printfn "processing %s" file
            context.AddDependency file
            use fileStream = new JsonTextReader(File.OpenText file)
            let input = serializer.Deserialize<Dictionary<string, string>>(fileStream)

            for kvp in input do
                result.Add(kvp.Key,kvp.Value)


        name, result