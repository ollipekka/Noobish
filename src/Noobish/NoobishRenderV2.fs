namespace Noobish

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Noobish.Internal
open Noobish.Styles
open Noobish.TextureAtlas

module NoobishRenderV2 =
    let private max0 value =
        if value < 0f then 0f else value

    let resolveState (enabled: bool) =
        if enabled then "default" else "disabled"

    let computeTextBounds (bounds: NoobishRectangle) (padding: NoobishPadding) =
        {
            X = bounds.X + padding.Left
            Y = bounds.Y + padding.Top
            Width = max0 (bounds.Width - padding.Left - padding.Right)
            Height = max0 (bounds.Height - padding.Top - padding.Bottom)
        }

type NoobishMonoGameRendererV2() =
    let rasterizerState =
        let state = new RasterizerState()
        state.ScissorTestEnable <- true
        state

    let drawQueue = PriorityQueue<int, int>()

    member val Debug = false with get, set

    member private this.EnsureTextAlignment (components: NoobishComponentsV2) (styleSheet: NoobishStyleSheet) (index: int) =
        if components.TextAlign.[index] = NoobishAlignment.None then
            let themeId = components.ThemeId.[index]
            components.TextAlign.[index] <- styleSheet.GetTextAlignment themeId "default"

    member private this.DrawBackground
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (spriteBatch: SpriteBatch)
        (index: int) =
        let themeId = components.ThemeId.[index]
        let state = NoobishRenderV2.resolveState components.Enabled.[index]
        let bounds = components.Bounds.[index]
        if bounds.Width > 0f && bounds.Height > 0f then
            let layer = 1f - float32 components.Layer.[index] / 255f
            let color = styleSheet.GetColor themeId state
            let drawables = styleSheet.GetDrawables themeId state
            let position = Vector2(bounds.X, bounds.Y)
            let size = Vector2(bounds.Width, bounds.Height)
            DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables

    member private this.DrawText
        (components: NoobishComponentsV2)
        (content: ContentManager)
        (styleSheet: NoobishStyleSheet)
        (textBatch: TextBatch)
        (index: int) =
        let text = components.Text.[index]
        if not (String.IsNullOrWhiteSpace text) then
            let themeId = components.ThemeId.[index]
            let state = NoobishRenderV2.resolveState components.Enabled.[index]
            this.EnsureTextAlignment components styleSheet index

            let fontId = styleSheet.GetFont themeId state
            let font = content.Load<NoobishFont> fontId
            let fontSize = styleSheet.GetFontSize themeId state
            let textColor = styleSheet.GetFontColor themeId state

            let bounds = components.Bounds.[index]
            let padding = components.Padding.[index]
            let textBounds = NoobishRenderV2.computeTextBounds bounds padding
            let textAlign = components.TextAlign.[index]
            let textWrap = components.Textwrap.[index]
            let textBounds = NoobishFont.calculateBounds font fontSize textWrap textBounds 0f 0f textAlign text
            let layer = 1f - float32 (components.Layer.[index] + 1) / 32768.0f

            if textWrap then
                textBatch.DrawMultiLine font fontSize textBounds.Width (Vector2(textBounds.X, textBounds.Y)) layer textColor text
            else
                textBatch.DrawSingleLine font fontSize (Vector2(textBounds.X, textBounds.Y)) layer textColor text

    member private this.DrawComponent
        (components: NoobishComponentsV2)
        (graphics: GraphicsDevice)
        (content: ContentManager)
        (spriteBatch: SpriteBatch)
        (textBatch: TextBatch)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (parentBounds: NoobishRectangle)
        (index: int) =
        if components.Visible.[index] then
            let bounds = components.Bounds.[index]
            let clippedBounds = bounds.Clamp parentBounds
            if clippedBounds.Width > 0f && clippedBounds.Height > 0f then
                let oldScissorRect = graphics.ScissorRectangle
                graphics.ScissorRectangle <- DrawUI.toRectangle clippedBounds

                spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)
                this.DrawBackground components styleSheet textureAtlas spriteBatch index
                spriteBatch.End()

                let textClip = NoobishRenderV2.computeTextBounds bounds components.Padding.[index]
                let textClip = textClip.Clamp clippedBounds
                if textClip.Width > 0f && textClip.Height > 0f then
                    graphics.ScissorRectangle <- DrawUI.toRectangle textClip
                    this.DrawText components content styleSheet textBatch index

                graphics.ScissorRectangle <- oldScissorRect

                if this.Debug then
                    let pixel = content.Load<Texture2D> "Pixel"
                    let color = Color.Multiply(Color.Yellow, 0.1f)
                    spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)
                    DrawUI.drawRectangle spriteBatch pixel color bounds.X bounds.Y bounds.Width bounds.Height
                    spriteBatch.End()

                let children = components.Children.[index]
                if children.Count > 0 then
                    for i = 0 to children.Count - 1 do
                        let childIndex = int children.[i].Index
                        this.DrawComponent components graphics content spriteBatch textBatch styleSheet textureAtlas clippedBounds childIndex

    member this.Draw
        (components: NoobishComponentsV2)
        (graphics: GraphicsDevice)
        (content: ContentManager)
        (spriteBatch: SpriteBatch)
        (textBatch: TextBatch)
        (styleSheetId: string)
        (_gameTime: GameTime) =

        let screenWidth = float32 graphics.Viewport.Width
        let screenHeight = float32 graphics.Viewport.Height

        let styleSheet = content.Load<NoobishStyleSheet> styleSheetId
        let textureAtlas = content.Load<NoobishTextureAtlas> styleSheet.TextureAtlasId

        drawQueue.Clear()
        for i = 0 to components.Count - 1 do
            if components.ParentId.[i] = UIComponentIdV2.empty then
                drawQueue.Enqueue(i, components.Layer.[i])

        let oldRasterizerState = graphics.RasterizerState
        graphics.RasterizerState <- rasterizerState

        while drawQueue.Count > 0 do
            let i = drawQueue.Dequeue()
            this.DrawComponent components graphics content spriteBatch textBatch styleSheet textureAtlas {X = 0f; Y = 0f; Width = screenWidth; Height = screenHeight} i

        graphics.RasterizerState <- oldRasterizerState
