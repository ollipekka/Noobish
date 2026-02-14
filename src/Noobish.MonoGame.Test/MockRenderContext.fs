module Noobish.MonoGame.Test.MockRenderContext

open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish
open Noobish.Styles
open Noobish.TextureAtlas

type RenderCommand =
    | Scissor of NoobishRectangle
    | Drawable of NoobishRectangle * float32 * NoobishColor * NoobishDrawable[]
    | Rectangle of NoobishRectangle * Color
    | Triangle of Vector2 * Vector2 * Vector2 * float32 * Color
    | TextSingle of string
    | TextMulti of string

type MockRenderContext
    (styleSheet: NoobishStyleSheet,
     textureAtlas: NoobishTextureAtlas,
     viewportWidth: int,
     viewportHeight: int,
     fonts: IReadOnlyDictionary<string, NoobishMonoGameFont>) =
    let commands = ResizeArray<RenderCommand>()

    member _.Commands = commands :> IReadOnlyList<RenderCommand>

    interface INoobishMonoGameRenderContext with
        member _.ViewportWidth = viewportWidth
        member _.ViewportHeight = viewportHeight
        member _.LoadStyleSheet _ = styleSheet
        member _.LoadTextureAtlas _ = textureAtlas
        member _.LoadFont id =
            let mutable font = Unchecked.defaultof<NoobishMonoGameFont>
            if fonts.TryGetValue(id, &font) then font else Unchecked.defaultof<NoobishMonoGameFont>
        member _.LoadPixel () = Unchecked.defaultof<Texture2D>
        member _.WithScissor bounds draw =
            commands.Add(Scissor bounds)
            draw()
        member _.WithSpriteBatch _ _ draw = draw()
        member _.DrawDrawable _ position size layer color drawables =
            let bounds = { X = position.X; Y = position.Y; Width = size.X; Height = size.Y }
            commands.Add(Drawable(bounds, layer, color, drawables))
        member _.DrawRectangle _ color x y width height =
            let bounds = { X = x; Y = y; Width = width; Height = height }
            commands.Add(Rectangle(bounds, color))
        member _.DrawTriangle p1 p2 p3 layer color =
            commands.Add(Triangle(p1, p2, p3, layer, color))
        member _.DrawTextSingleLine _ _ _ _ _ text =
            commands.Add(TextSingle text)
        member _.DrawTextMultiLine _ _ _ _ _ _ text =
            commands.Add(TextMulti text)
