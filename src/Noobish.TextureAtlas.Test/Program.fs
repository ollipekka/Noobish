module Noobish.TextureAtlas.TestApp

open Noobish.TextureAtlas.PipelineExtension

open System.IO
open Newtonsoft.Json
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open SixLabors.ImageSharp.Processing
open SixLabors.ImageSharp.Formats.Png

let textureFileNames = [|
    "bin/net6.0/button-toggled.9.png";
    "bin/net6.0/panel-default.9.png";
    "bin/net6.0/textbox-focused.9.png"
    "bin/net6.0/frame-default.9.png";
    "bin/net6.0/cursor.9.png";
    "bin/net6.0/pixel.png";
    "bin/net6.0/textbox-default.9.png"
    "bin/net6.0/button-default.9.png";
    "bin/net6.0/button-disabled.9.png";
    "bin/net6.0/scrollbar-default.9.png";
    "bin/net6.0/scrollbar-pin-default.9.png"
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

for texture2 in textures2 do
    let stream = File.OpenWrite(sprintf "testOut/%s.png" texture2.Name)
    texture2.Image.Save(stream, new PngEncoder())
    stream.Close()