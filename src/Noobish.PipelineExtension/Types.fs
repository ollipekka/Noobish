namespace Noobish.PipelineExtension

open System.IO
open Newtonsoft.Json
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Graphics
open System.Collections.Generic

type TextureAtlasItem =
| Texture
| NinePatch of top:int*right:int*bottom:int*left:int

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
        image.Mutate(fun img -> img.Crop(Rectangle(1, 1, image.Width - 2, image.Height - 2)) |> ignore)


        // Subtract 1 from all sides because of the metadata row / col.
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
        for path in config.Include do
            matcher.AddInclude path |> ignore

        for path in config.Exclude do
            matcher.AddExclude path |> ignore

        matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(path))).Files |> Seq.map (fun f -> sprintf "%s/%s" path f.Path) |> Seq.toArray

type TextureAtlasImporterResult = {
    Files: string[]
}

type StyleJson = {
    width: int
    height: int
    font: string
    fontColor: string
    color: string
    padding: int[]
    margin: int[]
    drawables: string[][]
}


type StyleSheetContent = {
    Name: string
    Font: string
    TextureAtlas: string

    Widths: (string*(string*float32)[])[]
    Heights: (string*(string*float32)[])[]
    Fonts: (string*(string*string)[])[]
    FontColors: (string*(string*string)[])[]
    Colors: (string*(string*string)[])[]
    Drawables: (string*(string*string[][])[])[]
    Paddings: (string*(string*(int*int*int*int))[])[]
    Margins: (string*(string*(int*int*int*int))[])[]
}