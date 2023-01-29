module Noobish.PipelineExtension.TestApp

open Noobish.PipelineExtension

open System.IO
open Newtonsoft.Json
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open SixLabors.ImageSharp.Formats.Png
open System.Collections.Generic

let textureFileNames = [|
    "bin/net6.0/Dark/button-toggled.9.png";
    "bin/net6.0/Dark/panel-default.9.png";
    "bin/net6.0/Dark/textbox-focused.9.png"
    "bin/net6.0/Dark/frame-default.9.png";
    "bin/net6.0/Dark/cursor.9.png";
    "bin/net6.0/Dark/pixel.png";
    "bin/net6.0/Dark/textbox-default.9.png"
    "bin/net6.0/Dark/button-default.9.png";
    "bin/net6.0/Dark/button-disabled.9.png";
    "bin/net6.0/Dark/scrollbar.9.png";
    "bin/net6.0/Dark/scrollbar-pin.9.png"
|]


if textureFileNames |> Array.contains "to_be_excluded_by_glob.png" then
    failwith "Should be excluded."

for fileName in textureFileNames do
    printfn "%s" fileName

let padding = 1

let textures = TexturePacker.createTextures textureFileNames

let (regions, atlasWidth, atlasHeight) = TexturePacker.createRegions 1024 1024 padding false textures

let image = TexturePacker.createImage textures regions padding atlasWidth atlasHeight

let stream = File.OpenWrite("atlas.png")
image.Save(stream, new PngEncoder())
stream.Close()


TexturePacker.writeIndex textures regions padding

let textures2, regions2 = TexturePacker.readIndex()

Directory.CreateDirectory("testOut") |> ignore


let reader = new JsonTextReader(new StreamReader(File.OpenRead("bin/net6.0/Dark/Dark.json")))

type StyleJson = {
    fontColor: string
    color: string
    padding: int[]
    margin: int[]
    drawables: string[][]


}

type StyleSheetJson = {
    TextureAtlas: string
    Styles: Dictionary<string, Dictionary<string, StyleJson>>
}

let file = new JsonTextReader(File.OpenText "bin/net6.0/Dark/Dark.json")

let serializer = JsonSerializer()

let jsonStyleSheet = serializer.Deserialize<StyleSheetJson>(file)



for kvp in jsonStyleSheet.Styles do
    printfn $"%s{kvp.Key}"
    for kvp' in kvp.Value do
       printfn $"%s{kvp'.Key} %A{kvp'.Value}"

       match kvp'.Value.fontColor with
       | null -> ()
       | fontColor -> printfn "%s" fontColor


//printfn "%A" jsonStyleSheet
(*
for texture2 in textures2 do
    let stream = File.OpenWrite(sprintf "testOut/%s.png" texture2.Name)
    texture2.Image.Save(stream, new PngEncoder())
    stream.Close() *)