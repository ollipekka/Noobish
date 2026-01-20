namespace Noobish

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Noobish.Styles
open Noobish.TextureAtlas
open NoobishColorMonoGame

type NoobishMonoGameRenderContext
    (graphics: GraphicsDevice,
     content: ContentManager,
     spriteBatch: SpriteBatch,
     textBatch: TextBatch) =

    member _.Graphics = graphics
    member _.Content = content
    member _.SpriteBatch = spriteBatch
    member _.TextBatch = textBatch

    member _.ToScissorRectangle (bounds: NoobishRectangle) =
        let scissor = NoobishRenderV2.computeScissorBounds bounds
        Rectangle(int scissor.X, int scissor.Y, int scissor.Width, int scissor.Height)

    member _.DrawDrawable
        (textureAtlas: NoobishTextureAtlas)
        (position: Vector2)
        (size: Vector2)
        (layer: float32)
        (color: NoobishColor)
        (drawables: NoobishDrawable[]) =
        let baseColor = toColor color
        for drawable in drawables do
            match drawable with
            | NoobishDrawable.Texture _ -> failwith "Texture not supported for cursor."
            | NoobishDrawable.NinePatch(tid) ->
                let texture = textureAtlas.[tid]
                spriteBatch.DrawAtlasNinePatch2(
                    texture,
                    Rectangle(int position.X, int position.Y, int size.X, int size.Y),
                    baseColor,
                    layer)
            | NoobishDrawable.NinePatchWithColor(tid, tint) ->
                let texture = textureAtlas.[tid]
                let tintColor = toColor tint
                spriteBatch.DrawAtlasNinePatch2(
                    texture,
                    Rectangle(int position.X, int position.Y, int size.X, int size.Y),
                    tintColor,
                    layer)

    member _.DrawRectangle
        (pixel: Texture2D)
        (color: Color)
        (x: float32)
        (y: float32)
        (width: float32)
        (height: float32) =
        let origin = Vector2(0.0f, 0.0f)
        let startPos = Vector2(x, y)
        let scale = Vector2(width / float32 pixel.Width, height / float32 pixel.Height)
        spriteBatch.Draw(
            pixel,
            startPos,
            Nullable(Rectangle(0, 0, pixel.Width, pixel.Height)),
            color,
            0.0f,
            origin,
            scale,
            SpriteEffects.None,
            1.0f)
