namespace Noobish.PipelineExtension

open System.IO
open System.Collections.Generic

open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Png

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open Microsoft.Xna.Framework.Content.Pipeline.Processors

[<ContentProcessor(DisplayName = "MSDFFont Processor")>]
type MSDFFontProcessor () =
    inherit ContentProcessor<MSDFFont, string*MSDFFont>()

    override s.Process(input, context) =

        let name = Path.GetFileNameWithoutExtension context.OutputFilename
        name, input