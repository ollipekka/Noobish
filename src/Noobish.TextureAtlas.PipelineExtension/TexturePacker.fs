namespace Noobish.TextureAtlas.PipelineExtension

module TexturePacker =

    open System.IO
    open SixLabors.ImageSharp
    open SixLabors.ImageSharp.PixelFormats
    open System.Collections.Generic

    let textureExtension = ".png"

    let isNinePatch (fileName: string) =
        fileName.EndsWith NinePatch.fileExtension

    let createTextures (fileNames: string[]) =
        fileNames
            |> Array.map (fun fileName ->
                printfn "%s" fileName
                let name, textureType, image =
                    if isNinePatch fileName then
                        NinePatch.createData fileName
                    else
                        Path.GetFileNameWithoutExtension fileName, TextureType.Texture, (Image.Load<Rgba32> fileName)

                let name =
                    if name.StartsWith "./" then
                        name.Substring(2)
                    else
                        name
                {
                    Name = name
                    TextureType = textureType
                    Image = image
                }
            )

    let findPreviousIntersection (textures: Texture[]) (placement: IReadOnlyDictionary<string, Rectangle>) (x:int) (y: int) (index: int) =

        let mutable i = 0
        let mutable intersectionIndex = -1


        let texture = textures.[index]
        let w = texture.Image.Width
        let h = texture.Image.Height

        let getPlacement (index) =
            placement.[textures.[index].Name]

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



    let findTexturePosition (textures: Texture[]) (placement: IReadOnlyDictionary<string, Rectangle>) (textureWidth: int) (index: int) =
            let mutable x = 0
            let mutable y = 0
            let mutable success = false

            let texture = textures.[index]
            while not success do

                let intersectionIndex = findPreviousIntersection textures placement x y index

                if intersectionIndex <> -1 then

                    let previousTexture = textures.[intersectionIndex]
                    let (success, previousRegion) = placement.TryGetValue previousTexture.Name
                    if not success then failwith "No placement for previous sprite."

                    x <- previousRegion.X + previousRegion.Width

                    if ( x + texture.Image.Width > textureWidth ) then
                        x <- 0
                        y <- y + 1

                else
                    success <- true
            (x, y)

    let guessWidth (textures: Texture[]) (padding: int ) (isPowerOfTwo: bool) =

            let spriteWidths =
                textures
                |> Array.map( fun t -> t.Image.Width + padding * 2 )
                |> Array.sort

            let maxWidth = spriteWidths.[spriteWidths.Length - 1]
            let medianWidth = spriteWidths.[spriteWidths.Length / 2]

            let width = medianWidth * int(round(System.Math.Sqrt( float textures.Length )) )

            let proposedWidth = max width maxWidth

            if isPowerOfTwo then
                int (System.Numerics.BitOperations.RoundUpToPowerOf2 (uint width))
            else
                proposedWidth

    let createRegions (maxWidth: int) (maxHeight: int) (padding: int) (isPowerOfTwo: bool) (textures: Texture[])  =
        let result = Dictionary<string, Rectangle>()

        let sortedTextures = textures |> Array.sortByDescending(fun t -> (t.Image.Height + 2*padding) * 1000 + (t.Image.Width + 2*padding))
        result.[sortedTextures.[0].Name] <- Rectangle(padding, padding, sortedTextures.[0].Image.Width + 2 * padding, sortedTextures.[0].Image.Height + 2 * padding)

        let mutable atlasWidth = guessWidth textures padding isPowerOfTwo
        let mutable atlasHeight = 0

        for i = 0 to sortedTextures.Length - 1 do
            let texture = sortedTextures.[i]
            let (x, y) = findTexturePosition sortedTextures result atlasWidth i
            let x = x + padding
            let y = y + padding
            let width = texture.Image.Width + padding
            let height = texture.Image.Height + padding
            result.[texture.Name] <- Rectangle(x, y, width, height)

            atlasHeight <- max atlasHeight (y + height )


        if isPowerOfTwo then
            atlasHeight <- int (System.Numerics.BitOperations.RoundUpToPowerOf2 (uint atlasHeight))

        if atlasWidth > maxWidth then
            failwith $"Texture atlas width ({atlasWidth}) is too big."

        if atlasHeight > maxHeight then
            failwith $"Texture atlas height ({atlasHeight}) is too big."

        (result, atlasWidth, atlasHeight)

    let createImage (textures: Texture[]) (regions: IReadOnlyDictionary<string, Rectangle>) (atlasWidth: int) (atlasHeight: int ) =
        let atlasImage = new Image<Rgba32>(atlasWidth, atlasHeight)

        for texture in textures do
            let destinationRectangle = regions.[texture.Name]

            for x = 0 to texture.Image.Width - 1 do
                for y = 0 to texture.Image.Height - 1 do
                    atlasImage.[destinationRectangle.X + x, destinationRectangle.Y + y] <- texture.Image.[x, y]


        atlasImage

    let writeIndex (textures: Texture[]) (regions: IReadOnlyDictionary<string, Rectangle>) (padding: int) =

        use binaryWriter = new BinaryWriter(File.OpenWrite("atlas.index"))

        binaryWriter.Write(textures.Length)
        for texture in textures do
            binaryWriter.Write texture.Name
            match texture.TextureType with
            | TextureType.NinePatch(top, right, bottom, left) ->
                binaryWriter.Write("NinePatch")
                binaryWriter.Write top
                binaryWriter.Write right
                binaryWriter.Write bottom
                binaryWriter.Write left
            | TextureType.Texture ->
                binaryWriter.Write("Texture")

            let region = regions.[texture.Name]
            binaryWriter.Write (region.X - padding)
            binaryWriter.Write (region.Y - padding)
            binaryWriter.Write (region.Width - padding)
            binaryWriter.Write (region.Height - padding)


    let readIndex () =

        let reader = new BinaryReader(File.OpenRead("atlas.index"))
        let atlasImage = Image.Load<Rgba32>("atlas.png")

        let count = reader .ReadInt32()

        let textures = ResizeArray<Texture>()
        let regions = Dictionary<string, Rectangle>()

        for i = 0 to count - 1 do
            let name = reader.ReadString()
            let textureType =
                let t = reader.ReadString()

                if t = "NinePatch" then
                    let top = reader.ReadInt32()
                    let right = reader.ReadInt32()
                    let bottom = reader.ReadInt32()
                    let left = reader.ReadInt32()
                    NinePatch(top, right, bottom, left)
                else
                    Texture

            let region =
                let x = reader.ReadInt32()
                let y = reader.ReadInt32()
                let width = reader.ReadInt32()
                let height = reader.ReadInt32()
                Rectangle(x, y, width, height)

            let image =
                let img = new Image<Rgba32>(region.Width, region.Height)
                for x' = 0 to region.Width - 1 do
                    for y' = 0 to region.Height - 1 do
                        img.[x', y'] <- atlasImage[region.X + x', region.Y + y']
                img

            textures.Add({Name = name; TextureType = textureType; Image = image})
            regions.[name] <- region
        textures, regions






