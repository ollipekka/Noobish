namespace Noobish.PipelineExtension

open System.IO
open System.Collections.Generic

open SixLabors.ImageSharp
open SixLabors.ImageSharp.Formats.Png

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open Microsoft.Xna.Framework.Content.Pipeline.Processors

[<ContentProcessor(DisplayName = "Style Sheet Procesor")>]
type StyleSheetProcessor () =
    inherit ContentProcessor<StyleSheetJson, StyleSheetContent>()


    override s.Process(input: StyleSheetJson, context: ContentProcessorContext) =


        {
            Name = Path.GetFileNameWithoutExtension context.OutputFilename
            Font = input.Font
            TextureAtlas = input.TextureAtlas
            Styles = input.Styles
        }