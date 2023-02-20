namespace Noobish.PipelineExtension

open System.IO
open Newtonsoft.Json
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open System.Collections.Generic
open Microsoft.Xna.Framework.Graphics

type TextureAtlasItem =
| Texture
| NinePatch of top:int*right:int*bottom:int*left:int

module Texture =
    let padEdges (image: Image<Rgba32>) =
            // Copy top edge.
        for x = 0 to image.Width - 1 do
            image.[x, 0] <- image.[x, 1]

        // Copy bottom edge.
        for x = 0 to image.Width - 1 do
            image.[x, image.Height - 1] <- image.[x, image.Height - 2]

        // Copy left edge.
        for y = 0 to image.Height - 1 do
            image.[0, y] <- image.[1, y]

        // Copy left edge.
        for y = 0 to image.Height - 1 do
            image.[image.Width - 1, y] <- image.[image.Width - 2, y]

    let createData (fileName: string) =
        let image = Image.Load<Rgba32> fileName
        image.Mutate(
            fun i ->
                let resizeOptions = ResizeOptions(Mode = ResizeMode.BoxPad, Position = AnchorPositionMode.Center, Size = Size(image.Width + 2, image.Height + 2))
                i.Resize(resizeOptions) |> ignore
            )
        padEdges image
        Path.GetFileNameWithoutExtension fileName, Texture, image

module NinePatch =

    let fileExtension = ".9.png"

    let findTopSlice (image: Image<Rgba32>) =
        let mutable top = 0
        let mutable finished = false
        while top < image.Height && not finished do
            let color = image.[0, top]
            if color.A = 0uy then
                top <- top + 1
            else
                finished <- true
        top

    let findRightSlice (image: Image<Rgba32>) =
        let mutable x = image.Width - 1
        let mutable right = 0
        let mutable finished = false
        while x > 0 && not finished do
            let color = image.[x, 0]
            if color.A = 0uy then
                x <- x - 1
                right <- right + 1
            else
                finished <- true
        right

    let findBottomSlice (image: Image<Rgba32>) =
        let mutable y = image.Height - 1
        let mutable bottom = 0
        let mutable finished = false
        while y > 0 && not finished do
            let color = image.[0, y]
            if color.A = 0uy then
                y <- y - 1
                bottom <- bottom + 1
            else
                finished <- true
        bottom

    let findLeftSlice (image: Image<Rgba32>) =
        let mutable left = 0
        let mutable finished = false
        while left < image.Width && not finished do
            let color = image.[left, 0]
            if color.A = 0uy then
                left <- left + 1
            else
                finished <- true
        left

    let createData(fileName: string) =

        if not (fileName.EndsWith fileExtension) then
            failwith "Given file doesn't follow the naming conventions."

        let image = Image<Rgba32>.Load<Rgba32>(fileName)
        let name = Path.GetFileNameWithoutExtension fileName
        let top = findTopSlice image
        let right = findRightSlice image
        let bottom = findBottomSlice image
        let left = findLeftSlice image

        // Remove the metadata border.
        //image.Mutate(fun img -> img.Crop(Rectangle(1, 1, image.Width - 2, image.Height - 2)) |> ignore)

        Texture.padEdges image

        (name, NinePatch (top - 1, right - 1, bottom - 1, left - 1), image)


type TextureAtlasContent = {
    Name: string
    Padding: int
    Textures: (string*TextureAtlasItem*Image<Rgba32>)[]
    Regions: System.Collections.Generic.IReadOnlyDictionary<string, Rectangle>
    Texture: ExternalReference<TextureContent>
}

type TextureAtlasJson = {
    Name: string
    Include: string[]
    Exclude: string[]
}

module TextureAtlasJson =
    open Newtonsoft.Json
    open System.IO
    open Microsoft.Extensions.FileSystemGlobbing
    open Microsoft.Extensions.FileSystemGlobbing.Abstractions

    let fromJsonFile (fileName: string) =
        use fileStream = File.OpenText(fileName)
        use jsonReader = new JsonTextReader(fileStream)
        let serializer = new JsonSerializer()
        serializer.Deserialize<TextureAtlasJson>(jsonReader)

    let getFiles (path: string) (config: TextureAtlasJson) =
        let matcher = Matcher()

        match config.Include with
        | null -> failwith "Include element is mandatory."
        | includePaths ->
            for path in includePaths do
                matcher.AddInclude path |> ignore

        match config.Exclude with
        | null -> ()
        | excludePaths ->
            for path in excludePaths do
                matcher.AddExclude path |> ignore

        matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(path))).Files |> Seq.map (fun f -> sprintf "%s/%s" path f.Path) |> Seq.toArray

type TextureAtlasImporterResult = {
    Files: string[]
}

type StyleJson = {
    width: int
    height: int
    font: string
    textAlign: string
    fontSize: int
    fontColor: string
    color: string
    padding: int[]
    margin: int[]
    drawables: string[][]
}


type StyleSheetContent = {
    Name: string
    TextureAtlas: string

    Widths: (string*(string*float32)[])[]
    Heights: (string*(string*float32)[])[]
    Fonts: (string*(string*string)[])[]
    FontSizes: (string*(string*int)[])[]
    FontColors: (string*(string*string)[])[]
    Colors: (string*(string*string)[])[]
    Drawables: (string*(string*string[][])[])[]
    TextAlignments: (string*(string*string)[])[]
    Paddings: (string*(string*(int*int*int*int))[])[]
    Margins: (string*(string*(int*int*int*int))[])[]
}


type MSDFAtlas = {
    ``type``: string
    distanceRange: float32
    size: float32
    width: int
    height: int
    yOrigin: string

}

type MSDFMetrics = {
    emSize: int
    lineHeight: float32
    ascender: float32
    descender: float32
    underlineY: float32
    underlineThickness: float32
}

type MSDFBounds = {
    top:float32
    right:float32
    bottom: float32
    left: float32
}

type MSDFGlyph = {
    unicode: int64
    advance: float32
    planeBounds: MSDFBounds
    atlasBounds: MSDFBounds
}

type MSDFKerning = {
    unicode1: int64
    unicode2: int64
    advance: float32
}

type MSDFFont = {
    atlas: MSDFAtlas
    metrics: MSDFMetrics
    glyphs: MSDFGlyph[]
    kerning: MSDFKerning[]
    texture: ExternalReference<TextureContent>
}