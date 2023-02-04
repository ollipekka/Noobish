module Noobish.TextureAtlas

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

[<RequireQualifiedAccess>]
type TextureType =
| Texture
| NinePatch of top: int * right: int * bottom: int * left: int

type Texture = {
    Name: string
    TextureType: TextureType
    Atlas: Texture2D
    SourceRectangle: Rectangle
} with
    member s.Width with get () = s.SourceRectangle.Width
    member s.Height with get () = s.SourceRectangle.Height
    member s.Origin with get() = Vector2(float32 s.SourceRectangle.Width / 2f, float32 s.SourceRectangle.Height / 2f)

type TextureAtlas = {
    Name: string
    Textures: System.Collections.Generic.IReadOnlyDictionary<string, Texture>
} with
    member this.Item
        with get (tid: string) = this.Textures.[tid]