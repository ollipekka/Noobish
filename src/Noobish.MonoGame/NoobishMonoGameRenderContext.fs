namespace Noobish

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Noobish.Styles
open Noobish.TextureAtlas
open NoobishColorMonoGame

type internal INoobishMonoGameRenderContext =
    abstract member ViewportWidth: int
    abstract member ViewportHeight: int
    abstract member LoadStyleSheet: string -> NoobishStyleSheet
    abstract member LoadTextureAtlas: string -> NoobishTextureAtlas
    abstract member LoadFont: string -> NoobishMonoGameFont
    abstract member LoadPixel: unit -> Texture2D
    abstract member WithScissor: NoobishRectangle -> (unit -> unit) -> unit
    abstract member WithSpriteBatch: RasterizerState -> SamplerState -> (unit -> unit) -> unit
    abstract member DrawDrawable: NoobishTextureAtlas -> Vector2 -> Vector2 -> float32 -> NoobishColor -> NoobishDrawable[] -> unit
    abstract member DrawRectangle: Texture2D -> Color -> float32 -> float32 -> float32 -> float32 -> unit
    abstract member DrawTriangle: Vector2 -> Vector2 -> Vector2 -> float32 -> Color -> unit
    abstract member DrawTextSingleLine: NoobishMonoGameFont -> int -> Vector2 -> float32 -> Color -> string -> unit
    abstract member DrawTextMultiLine: NoobishMonoGameFont -> int -> float32 -> Vector2 -> float32 -> Color -> string -> unit

type NoobishMonoGameRenderContext
    (graphics: GraphicsDevice,
     content: ContentManager,
     spriteBatch: SpriteBatch,
     textBatch: TextBatch) =
    let triangleEffect = new BasicEffect(graphics)
    let triangleRasterizer =
        let state = new RasterizerState()
        state.CullMode <- CullMode.CullCounterClockwiseFace
        state.ScissorTestEnable <- true
        state
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
            | NoobishDrawable.Texture _ -> failwith "Texture not supported for drawable."
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

    member _.DrawTriangle
        (p1: Vector2)
        (p2: Vector2)
        (p3: Vector2)
        (layer: float32)
        (color: Color) =
        let oldBlend = graphics.BlendState
        let oldDepth = graphics.DepthStencilState
        let oldRasterizer = graphics.RasterizerState

        let viewport = graphics.Viewport
        triangleEffect.World <- Matrix.Identity
        triangleEffect.View <- Matrix.Identity
        triangleEffect.Projection <- Matrix.CreateOrthographicOffCenter(0.0f, float32 viewport.Width, float32 viewport.Height, 0.0f, 0.0f, -1.0f)
        triangleEffect.VertexColorEnabled <- true
        triangleEffect.TextureEnabled <- false

        let z = 0.0f
        let vertices = [|
            VertexPositionColor(Vector3(p1.X, p1.Y, z), color)
            VertexPositionColor(Vector3(p2.X, p2.Y, z), color)
            VertexPositionColor(Vector3(p3.X, p3.Y, z), color)
        |]

        graphics.BlendState <- BlendState.AlphaBlend
        graphics.DepthStencilState <- DepthStencilState.None
        graphics.RasterizerState <- triangleRasterizer

        try
            for pass in triangleEffect.CurrentTechnique.Passes do
                pass.Apply()
                graphics.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 1)
        finally
            graphics.BlendState <- oldBlend
            graphics.DepthStencilState <- oldDepth
            graphics.RasterizerState <- oldRasterizer

    interface INoobishMonoGameRenderContext with
        member _.ViewportWidth = graphics.Viewport.Width
        member _.ViewportHeight = graphics.Viewport.Height
        member _.LoadStyleSheet id = content.Load<NoobishStyleSheet> id
        member _.LoadTextureAtlas id = content.Load<NoobishTextureAtlas> id
        member _.LoadFont id = content.Load<NoobishMonoGameFont> id
        member _.LoadPixel () = content.Load<Texture2D> "Pixel"
        member this.WithScissor bounds draw =
            let oldScissorRect = graphics.ScissorRectangle
            graphics.ScissorRectangle <- this.ToScissorRectangle bounds
            try
                draw()
            finally
                graphics.ScissorRectangle <- oldScissorRect
        member _.WithSpriteBatch rasterizerState samplerState draw =
            spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = samplerState)
            draw()
            spriteBatch.End()
        member this.DrawDrawable atlas position size layer color drawables =
            this.DrawDrawable atlas position size layer color drawables
        member this.DrawRectangle pixel color x y width height =
            this.DrawRectangle pixel color x y width height
        member this.DrawTriangle p1 p2 p3 layer color =
            this.DrawTriangle p1 p2 p3 layer color
        member _.DrawTextSingleLine font size position layer color text =
            textBatch.DrawSingleLine font size position layer color text
        member _.DrawTextMultiLine font size width position layer color text =
            textBatch.DrawMultiLine font size width position layer color text
