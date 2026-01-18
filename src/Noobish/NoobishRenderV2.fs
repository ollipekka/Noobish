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
    let max0 value =
        if value < 0f then 0f else value

    let resolveState (enabled: bool) (toggled: bool) (hovered: bool) =
        if not enabled then "disabled"
        elif toggled && hovered then "toggledHovered"
        elif toggled then "toggled"
        elif hovered then "hovered"
        else "default"

    let computeTextBounds (bounds: NoobishRectangle) (padding: NoobishPadding) =
        {
            X = bounds.X + padding.Left
            Y = bounds.Y + padding.Top
            Width = max0 (bounds.Width - padding.Left - padding.Right)
            Height = max0 (bounds.Height - padding.Top - padding.Bottom)
        }

    let computeProgressSegmentWidth (contentWidth: float32) (segments: int) (gap: float32) =
        if segments <= 0 then
            0f
        else
            max0 ((contentWidth - gap * float32 (segments - 1)) / float32 segments)

    let computeProgressBounds (bounds: NoobishRectangle) (padding: NoobishPadding) (progress: float32) =
        let content = computeTextBounds bounds padding
        let clamped = Noobish.Internal.clamp progress 0f 1f
        {
            X = content.X
            Y = content.Y
            Width = content.Width * clamped
            Height = content.Height
        }

    let computeSliderPinBounds
        (bounds: NoobishRectangle)
        (padding: NoobishPadding)
        (rangeStart: float32)
        (rangeEnd: float32)
        (value: float32)
        (pinWidth: float32)
        (pinHeight: float32) =
        let availableWidth = max0 (bounds.Width - padding.Left - padding.Right)
        let availableHeight = max0 (bounds.Height - padding.Top - padding.Bottom)
        let clampedPinWidth = min pinWidth availableWidth
        let clampedPinHeight = min pinHeight availableHeight
        let span = rangeEnd - rangeStart
        let t =
            if span = 0f then
                0f
            else
                Noobish.Internal.clamp ((value - rangeStart) / span) 0f 1f
        let usableWidth = max0 (availableWidth - clampedPinWidth)
        {
            X = bounds.X + padding.Left + t * usableWidth
            Y = bounds.Y + padding.Top + (availableHeight - clampedPinHeight) * 0.5f
            Width = clampedPinWidth
            Height = clampedPinHeight
        }

    let computeSliderTrackBounds
        (bounds: NoobishRectangle)
        (padding: NoobishPadding)
        (trackHeight: float32) =
        let availableWidth = max0 (bounds.Width - padding.Left - padding.Right)
        let availableHeight = max0 (bounds.Height - padding.Top - padding.Bottom)
        let clampedHeight =
            if trackHeight > 0f then
                min trackHeight availableHeight
            else
                availableHeight
        {
            X = bounds.X + padding.Left
            Y = bounds.Y + padding.Top + (availableHeight - clampedHeight) * 0.5f
            Width = availableWidth
            Height = clampedHeight
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
        let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index]
        let bounds = components.Bounds.[index]
        if bounds.Width > 0f && bounds.Height > 0f then
            let layer = 1f - float32 components.Layer.[index] / 255f
            let color = styleSheet.GetColor themeId state
            let drawables = styleSheet.GetDrawables themeId state
            let position = Vector2(bounds.X, bounds.Y)
            let size = Vector2(bounds.Width, bounds.Height)
            DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables


    member private this.DrawSliderPin
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (spriteBatch: SpriteBatch)
        (index: int) =
        let bounds = components.Bounds.[index]
        if bounds.Width > 0f && bounds.Height > 0f then
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index]
            let pinThemeId = "SliderPin"
            let pinWidth =
                let width = styleSheet.GetWidth pinThemeId state
                if width > 0f then
                    width
                else
                    let height = styleSheet.GetHeight pinThemeId state
                    if height > 0f then height else bounds.Height
            let pinHeight =
                let height = styleSheet.GetHeight pinThemeId state
                if height > 0f then height else bounds.Height
            let rangeStart = components.SliderMin.[index]
            let rangeEnd = components.SliderMax.[index]
            let value = components.SliderValue.[index]
            let padding = components.Padding.[index]
            let pinBounds = NoobishRenderV2.computeSliderPinBounds bounds padding rangeStart rangeEnd value pinWidth pinHeight
            if pinBounds.Width > 0f && pinBounds.Height > 0f then
                let layer = 1f - float32 components.Layer.[index] / 255f
                let pinLayer = NoobishRenderV2.max0 (layer - 0.0001f)
                let color = styleSheet.GetColor pinThemeId state
                let drawables = styleSheet.GetDrawables pinThemeId state
                let position = Vector2(pinBounds.X, pinBounds.Y)
                let size = Vector2(pinBounds.Width, pinBounds.Height)
                DrawUI.drawDrawable textureAtlas spriteBatch position size pinLayer color drawables

    member private this.DrawSliderTrack
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (spriteBatch: SpriteBatch)
        (index: int) =
        let bounds = components.Bounds.[index]
        if bounds.Width > 0f && bounds.Height > 0f then
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index]
            let themeId = components.ThemeId.[index]
            let trackHeight = styleSheet.GetHeight themeId state
            let padding = components.Padding.[index]
            let trackBounds = NoobishRenderV2.computeSliderTrackBounds bounds padding trackHeight
            if trackBounds.Width > 0f && trackBounds.Height > 0f then
                let layer = 1f - float32 components.Layer.[index] / 255f
                let color = styleSheet.GetColor themeId state
                let drawables = styleSheet.GetDrawables themeId state
                let position = Vector2(trackBounds.X, trackBounds.Y)
                let size = Vector2(trackBounds.Width, trackBounds.Height)
                DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables

    member private this.DrawProgressFill
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (spriteBatch: SpriteBatch)
        (index: int) =
        let bounds = components.Bounds.[index]
        if bounds.Width > 0f && bounds.Height > 0f then
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index]
            let padding = components.Padding.[index]
            let progress = components.ProgressValue.[index]
            let fillBounds = NoobishRenderV2.computeProgressBounds bounds padding progress
            if fillBounds.Width > 0f && fillBounds.Height > 0f then
                let themeId = "ProgressBar-Progress"
                let layer = 1f - float32 components.Layer.[index] / 255f
                let color = styleSheet.GetColor themeId state
                let drawables = styleSheet.GetDrawables themeId state
                let position = Vector2(fillBounds.X, fillBounds.Y)
                let size = Vector2(fillBounds.Width, fillBounds.Height)
                DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables

    member private this.DrawProgressSegments
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (spriteBatch: SpriteBatch)
        (index: int) =
        let bounds = components.Bounds.[index]
        if bounds.Width > 0f && bounds.Height > 0f then
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index]
            let padding = components.Padding.[index]
            let content = NoobishRenderV2.computeTextBounds bounds padding
            let segments = max 1 components.ProgressSegments.[index]
            let dashMargin = styleSheet.GetMargin "ProgressBar-Dash" "default"
            let gap = max 0f (dashMargin.Left + dashMargin.Right)
            let segmentWidth = NoobishRenderV2.computeProgressSegmentWidth content.Width segments gap
            let dashThemeId = "ProgressBar-Dash"
            let dashColor = styleSheet.GetColor dashThemeId state
            let dashDrawables = styleSheet.GetDrawables dashThemeId state
            let dashPadding = styleSheet.GetPadding dashThemeId "default"
            let progress = components.ProgressValue.[index]

            for s = 0 to segments - 1 do
                let segmentX = content.X + float32 s * (segmentWidth + gap)
                let segmentBounds: NoobishRectangle = {
                    X = segmentX
                    Y = content.Y
                    Width = segmentWidth
                    Height = content.Height
                }
                if segmentBounds.Width > 0f && segmentBounds.Height > 0f then
                    let layer = 1f - float32 components.Layer.[index] / 255f
                    let position = Vector2(segmentBounds.X, segmentBounds.Y)
                    let size = Vector2(segmentBounds.Width, segmentBounds.Height)
                    DrawUI.drawDrawable textureAtlas spriteBatch position size layer dashColor dashDrawables

                    let segmentProgress = Noobish.Internal.clamp (progress * float32 segments - float32 s) 0f 1f
                    if segmentProgress > 0f then
                        let fillBounds = NoobishRenderV2.computeTextBounds segmentBounds dashPadding
                        let fillWidth = fillBounds.Width * segmentProgress
                        if fillWidth > 0f then
                            let themeId = "ProgressBar-Progress"
                            let color = styleSheet.GetColor themeId state
                            let drawables = styleSheet.GetDrawables themeId state
                            let fillPosition = Vector2(fillBounds.X, fillBounds.Y)
                            let fillSize = Vector2(fillWidth, fillBounds.Height)
                            DrawUI.drawDrawable textureAtlas spriteBatch fillPosition fillSize layer color drawables

    member private this.DrawText
        (components: NoobishComponentsV2)
        (content: ContentManager)
        (styleSheet: NoobishStyleSheet)
        (textBatch: TextBatch)
        (index: int) =
        let text = components.Text.[index]
        if not (String.IsNullOrWhiteSpace text) then
            let themeId = components.ThemeId.[index]
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index]
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
                if components.WantsSlider.[index] then
                    this.DrawSliderTrack components styleSheet textureAtlas spriteBatch index
                    this.DrawSliderPin components styleSheet textureAtlas spriteBatch index
                elif components.WantsProgress.[index] then
                    this.DrawBackground components styleSheet textureAtlas spriteBatch index
                    if components.ProgressSegments.[index] > 1 then
                        this.DrawProgressSegments components styleSheet textureAtlas spriteBatch index
                    else
                        this.DrawProgressFill components styleSheet textureAtlas spriteBatch index
                else
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
