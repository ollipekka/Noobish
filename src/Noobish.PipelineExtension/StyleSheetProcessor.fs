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
    inherit ContentProcessor<string*Dictionary<string, Dictionary<string, StyleJson>>, StyleSheetContent>()


    override s.Process((textureAtlasId:string, styles: Dictionary<string, Dictionary<string, StyleJson>>), context: ContentProcessorContext) =



        let widths = Dictionary<string, Dictionary<string, float32>>()
        let heights = Dictionary<string, Dictionary<string, float32>>()

        let fonts = Dictionary<string, Dictionary<string, string>>()
        let fontSizes = Dictionary<string, Dictionary<string, int32>>()
        let fontColors = Dictionary<string, Dictionary<string, string>>()
        let colors = Dictionary<string, Dictionary<string, string>>()
        let drawables = Dictionary<string, Dictionary<string, string[][]>>()
        let textAligns = Dictionary<string, Dictionary<string, string>>()
        let paddings = Dictionary<string, Dictionary<string, (int*int*int*int)>>()
        let margins = Dictionary<string, Dictionary<string, (int*int*int*int)>>()


        let styles = styles |> Seq.map(fun kvp ->  (kvp.Key, kvp.Value |> Seq.map (fun kvp -> (kvp.Key, kvp.Value))|> Seq.toArray)) |> Seq.toArray

        for (name, componentStyles) in styles do

            for (stateId, style) in componentStyles do
                match style.color with
                | null -> ()
                | color ->
                    let colorsByComponent = colors.GetOrAdd name (fun () -> Dictionary())
                    colorsByComponent.[stateId] <- color

                match style.font with
                | null -> ()
                | font ->
                    let fontByComponent = fonts.GetOrAdd name (fun () -> Dictionary())
                    fontByComponent.[stateId] <- font

                if style.fontSize > 0 then
                    let fontSizesByComponent = fontSizes.GetOrAdd name (fun () -> Dictionary())
                    fontSizesByComponent.[stateId] <- style.fontSize

                match style.fontColor with
                | null -> ()
                | fontColor ->
                    let fontColorsByComponent = fontColors.GetOrAdd name (fun () -> Dictionary())
                    fontColorsByComponent.[stateId] <- fontColor

                match style.padding with
                | null -> ()
                | p ->
                    let paddingsByComponent = paddings.GetOrAdd name (fun () -> Dictionary())
                    paddingsByComponent.[stateId] <- (p.[0], p.[1], p.[2], p.[3])

                match style.margin with
                | null -> ()
                | m ->
                    let marginsByComponent = margins.GetOrAdd name (fun () -> Dictionary())
                    marginsByComponent.[stateId] <- (m.[0], m.[1], m.[2], m.[3])

                match style.drawables with
                | null -> ()
                | d ->
                    let drawablesByComponent = drawables.GetOrAdd name (fun () -> Dictionary())
                    drawablesByComponent.[stateId] <- d

                match style.textAlign with
                | null -> ()
                | t ->
                    let textAlignsByComponent = textAligns.GetOrAdd name (fun () -> Dictionary())
                    textAlignsByComponent.[stateId] <- t

                if style.width > 0 then
                    let widthsByComponent = widths.GetOrAdd name (fun () -> Dictionary())
                    widthsByComponent.[stateId] <- float32 style.width


                if style.height > 0 then
                    let heightsByComponent = heights.GetOrAdd name (fun () -> Dictionary())
                    heightsByComponent.[stateId] <- float32 style.height

        let toArray (d: Dictionary<string, Dictionary<string, 'T>>) =
            d |> Seq.map(fun kvp -> kvp.Key, kvp.Value |> Seq.map(fun kvp' -> kvp'.Key, kvp'.Value) |> Seq.toArray) |> Seq.toArray

        {
            Name = Path.GetFileNameWithoutExtension context.OutputFilename
            TextureAtlas = textureAtlasId.Replace(".json", "")
            Widths = toArray widths
            Heights = toArray heights
            Fonts = toArray fonts
            FontSizes = toArray fontSizes
            FontColors = toArray fontColors
            Colors = toArray colors
            Drawables = toArray drawables
            TextAlignments = toArray textAligns
            Paddings = toArray paddings
            Margins = toArray margins
        }