namespace Noobish.PipelineExtension


module TexturePacker =

    open System.IO
    open SixLabors.ImageSharp
    open SixLabors.ImageSharp.PixelFormats
    open SixLabors.ImageSharp.Processing
    open System.Collections.Generic

    let textureExtension = ".png"

    let isNinePatch (fileName: string) =
        fileName.EndsWith NinePatch.fileExtension

    let createTextures (fileNames: string[]) =
        fileNames
            |> Array.map (fun fileName ->
                let name, textureType, image =
                    if isNinePatch fileName then
                        NinePatch.createData fileName
                    else
                        Path.GetFileNameWithoutExtension fileName, Texture, (Image.Load<Rgba32> fileName)

                let name =
                    if name.StartsWith "./" then
                        name.Substring(2)
                    else
                        name

                name, textureType, image

            )

    let findPreviousIntersection (textures: (string*TextureAtlasItem*Image<Rgba32>)[]) (placement: IReadOnlyDictionary<string, Rectangle>) (x:int) (y: int) (index: int) =

        let mutable i = 0
        let mutable intersectionIndex = -1


        let (_,_,image) = textures.[index]
        let w = image.Width
        let h = image.Height

        let getPlacement (index) =
            let (name, _, _) = textures.[index]
            placement.[name]

        while i < index && intersectionIndex = -1 do
            let previousPlacement = getPlacement i
            if ( previousPlacement.X >= x + w
                || previousPlacement.X + previousPlacement.Width <= x
                || previousPlacement.Y >= y + h
                || previousPlacement.Y + previousPlacement.Height <= y ) then

                i <- i + 1
            else
                intersectionIndex <- i

        intersectionIndex



    let findTexturePosition (textures: (string*TextureAtlasItem*Image<Rgba32>)[]) (placement: IReadOnlyDictionary<string, Rectangle>) (padding: int) (textureWidth: int) (index: int) =
            let mutable x = 0
            let mutable y = 0
            let mutable success = false

            let (_,_, image) = textures.[index]
            while not success do

                let intersectionIndex = findPreviousIntersection textures placement x y index

                if intersectionIndex <> -1 then

                    let (name, _, _) = textures.[intersectionIndex]
                    let (success, previousRegion) = placement.TryGetValue name
                    if not success then failwith "No placement for previous sprite."

                    x <- previousRegion.X + previousRegion.Width

                    if ( x + image.Width + padding * 2 > textureWidth ) then
                        x <- 0
                        y <- y + 1

                else
                    success <- true
            (x, y)

    let guessWidth (textures: (string*TextureAtlasItem*Image<Rgba32>)[]) (padding: int ) (isPowerOfTwo: bool) =

            let spriteWidths =
                textures
                |> Array.map( fun (_,_,i) -> i.Width + padding * 2 )
                |> Array.sort

            let maxWidth = spriteWidths.[spriteWidths.Length - 1]
            let medianWidth = spriteWidths.[spriteWidths.Length / 2]

            let width = medianWidth * int(round(System.Math.Sqrt( float textures.Length )) )

            let proposedWidth = max width maxWidth

            if isPowerOfTwo then
                int (System.Numerics.BitOperations.RoundUpToPowerOf2 (uint width))
            else
                proposedWidth

    let createRegions (maxWidth: int) (maxHeight: int) (padding: int) (isPowerOfTwo: bool) (textures: (string*TextureAtlasItem*Image<Rgba32>)[])  =
        let result = Dictionary<string, Rectangle>()

        let sortedTextures = textures |> Array.sortByDescending(fun (_,_,image) -> (image.Height + 2 * padding) * 1000 + (image.Width + 2 * padding))
        let (firstName, _, firstImage) = sortedTextures.[0]
        result.[firstName] <- Rectangle(0, 0, firstImage.Width + 2 * padding, firstImage.Height + 2 * padding)

        let mutable atlasWidth = guessWidth textures padding isPowerOfTwo
        let mutable atlasHeight = 0

        for i = 0 to sortedTextures.Length - 1 do
            let (name,_ ,image) = sortedTextures.[i]
            let (x, y) = findTexturePosition sortedTextures result padding atlasWidth i
            let x = x
            let y = y
            let width = image.Width + padding * 2
            let height = image.Height + padding * 2
            result.[name] <- Rectangle(x, y, width, height)

            atlasHeight <- max atlasHeight (y + height )


        if isPowerOfTwo then
            atlasHeight <- int (System.Numerics.BitOperations.RoundUpToPowerOf2 (uint atlasHeight))

        if atlasWidth > maxWidth then
            failwith $"Texture atlas width ({atlasWidth}) is too big."

        if atlasHeight > maxHeight then
            failwith $"Texture atlas height ({atlasHeight}) is too big."

        (result, atlasWidth, atlasHeight)

    let createImage (textures: (string*TextureAtlasItem*Image<Rgba32>)[]) (regions: IReadOnlyDictionary<string, Rectangle>) (padding: int) (atlasWidth: int) (atlasHeight: int ) =
        let atlasImage = new Image<Rgba32>(atlasWidth, atlasHeight)
        // Add debug border
        //atlasImage.Mutate(fun i -> (i.BackgroundColor(Rgba32(1f, 0f, 0.498f, 1f)) |> ignore ))
        for (name,_,image) in textures do
            let destinationRectangle = regions.[name]
            for x = 0 to image.Width - 1 do
                for y = 0 to image.Height - 1 do
                    atlasImage.[destinationRectangle.X + x + padding, destinationRectangle.Y + y + padding] <- image.[x, y]


        atlasImage






