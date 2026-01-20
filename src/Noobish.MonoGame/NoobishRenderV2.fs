namespace Noobish

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish.Internal
open Noobish.Styles
open Noobish.TextureAtlas
open NoobishColorMonoGame

type NoobishMonoGameRendererV2() =
    let rasterizerState =
        let state = new RasterizerState()
        state.ScissorTestEnable <- true
        state

    let drawQueue = PriorityQueue<int, int>()
    let caretBlinkByLocalId = Dictionary<uint32, TimeSpan>()
    let caretBlinkByIndex = Dictionary<int, TimeSpan>()

    member val Debug = false with get, set

    member private _.ResolveState (components: NoobishComponentsV2) (index: int) =
        NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index] components.Focused.[index]

    member private _.ResolveLayer (components: NoobishComponentsV2) (index: int) =
        1f - float32 components.Layer.[index] / 255f

    member private _.ResolveTextLayer (components: NoobishComponentsV2) (index: int) (offset: int) =
        1f - float32 (components.Layer.[index] + offset) / 32768.0f

    member private _.WithScissor (ctx: NoobishMonoGameRenderContext) (bounds: NoobishRectangle) (draw: unit -> unit) =
        let graphics = ctx.Graphics
        let oldScissorRect = graphics.ScissorRectangle
        graphics.ScissorRectangle <- ctx.ToScissorRectangle bounds
        try
            draw()
        finally
            graphics.ScissorRectangle <- oldScissorRect

    member private _.WithSpriteBatch (ctx: NoobishMonoGameRenderContext) (draw: unit -> unit) =
        let spriteBatch = ctx.SpriteBatch
        spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)
        draw()
        spriteBatch.End()

    member private this.EnsureTextAlignment (components: NoobishComponentsV2) (styleSheet: NoobishStyleSheet) (index: int) =
        if components.TextAlign.[index] = NoobishAlignment.None then
            let themeId = components.ThemeId.[index]
            components.TextAlign.[index] <- styleSheet.GetTextAlignment themeId "default"

    member private this.DrawBackground
        (ctx: NoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int) =
        if NoobishRectangle.hasArea bounds then
            let themeId = components.ThemeId.[index]
            let state = this.ResolveState components index
            let layer = this.ResolveLayer components index
            let color = styleSheet.GetColor themeId state
            let drawables = styleSheet.GetDrawables themeId state
            let position = Vector2(bounds.X, bounds.Y)
            let size = Vector2(bounds.Width, bounds.Height)
            ctx.DrawDrawable textureAtlas position size layer color drawables

    member private this.DrawSliderPin
        (ctx: NoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int) =
        if NoobishRectangle.hasArea bounds then
            let state = this.ResolveState components index
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
            if NoobishRectangle.hasArea pinBounds then
                let layer = this.ResolveLayer components index
                let pinLayer = Internal.max0 (layer - 0.0001f)
                let color = styleSheet.GetColor pinThemeId state
                let drawables = styleSheet.GetDrawables pinThemeId state
                let position = Vector2(pinBounds.X, pinBounds.Y)
                let size = Vector2(pinBounds.Width, pinBounds.Height)
                ctx.DrawDrawable textureAtlas position size pinLayer color drawables

    member private this.DrawSliderTrack
        (ctx: NoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int) =
        if NoobishRectangle.hasArea bounds then
            let state = this.ResolveState components index
            let themeId = components.ThemeId.[index]
            let trackHeight = styleSheet.GetHeight themeId state
            let padding = components.Padding.[index]
            let trackBounds = NoobishRenderV2.computeSliderTrackBounds bounds padding trackHeight
            if NoobishRectangle.hasArea trackBounds then
                let layer = this.ResolveLayer components index
                let color = styleSheet.GetColor themeId state
                let drawables = styleSheet.GetDrawables themeId state
                let position = Vector2(trackBounds.X, trackBounds.Y)
                let size = Vector2(trackBounds.Width, trackBounds.Height)
                ctx.DrawDrawable textureAtlas position size layer color drawables

    member private this.DrawProgressFill
        (ctx: NoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int) =
        if NoobishRectangle.hasArea bounds then
            let state = this.ResolveState components index
            let padding = components.Padding.[index]
            let progress = components.ProgressValue.[index]
            let fillBounds = NoobishRenderV2.computeProgressBounds bounds padding progress
            if NoobishRectangle.hasArea fillBounds then
                let themeId = "ProgressBar-Progress"
                let layer = this.ResolveLayer components index
                let color = styleSheet.GetColor themeId state
                let drawables = styleSheet.GetDrawables themeId state
                let position = Vector2(fillBounds.X, fillBounds.Y)
                let size = Vector2(fillBounds.Width, fillBounds.Height)
                ctx.DrawDrawable textureAtlas position size layer color drawables

    member private this.DrawProgressSegments
        (ctx: NoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int) =
        if NoobishRectangle.hasArea bounds then
            let state = this.ResolveState components index
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
            let layer = this.ResolveLayer components index

            for s = 0 to segments - 1 do
                let segmentX = content.X + float32 s * (segmentWidth + gap)
                let segmentBounds: NoobishRectangle = {
                    X = segmentX
                    Y = content.Y
                    Width = segmentWidth
                    Height = content.Height
                }
                if NoobishRectangle.hasArea segmentBounds then
                    let position = Vector2(segmentBounds.X, segmentBounds.Y)
                    let size = Vector2(segmentBounds.Width, segmentBounds.Height)
                    ctx.DrawDrawable textureAtlas position size layer dashColor dashDrawables

                    let segmentProgress = Math.Clamp (progress * float32 segments - float32 s,  0f, 1f)
                    if segmentProgress > 0f then
                        let fillBounds = NoobishRenderV2.computeTextBounds segmentBounds dashPadding
                        let fillWidth = fillBounds.Width * segmentProgress
                        if fillWidth > 0f then
                            let themeId = "ProgressBar-Progress"
                            let color = styleSheet.GetColor themeId state
                            let drawables = styleSheet.GetDrawables themeId state
                            let fillPosition = Vector2(fillBounds.X, fillBounds.Y)
                            let fillSize = Vector2(fillWidth, fillBounds.Height)
                            ctx.DrawDrawable textureAtlas fillPosition fillSize layer color drawables

    member private this.DrawText
        (ctx: NoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (bounds: NoobishRectangle)
        (index: int) =
        let text = components.Text.[index]
        if not (String.IsNullOrWhiteSpace text) then
            let themeId = components.ThemeId.[index]
            let state = this.ResolveState components index
            this.EnsureTextAlignment components styleSheet index

            let fontId = styleSheet.GetFont themeId state
            let font = ctx.Content.Load<NoobishMonoGameFont> fontId
            let fontSize = styleSheet.GetFontSize themeId state
            let textColor = styleSheet.GetFontColor themeId state |> toColor

            let padding = components.Padding.[index]
            let textBounds = NoobishRenderV2.computeTextBounds bounds padding
            let textAlign = components.TextAlign.[index]
            let textWrap = components.Textwrap.[index]
            let textBounds = NoobishFont.calculateBounds font.Font fontSize textWrap textBounds 0f 0f textAlign text
            let layer = this.ResolveTextLayer components index 1

            if textWrap then
                ctx.TextBatch.DrawMultiLine font fontSize textBounds.Width (Vector2(textBounds.X, textBounds.Y)) layer textColor text
            else
                ctx.TextBatch.DrawSingleLine font fontSize (Vector2(textBounds.X, textBounds.Y)) layer textColor text

    member private this.DrawCaret
        (ctx: NoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int)
        (gameTime: GameTime) =
        if components.Focused.[index] && components.WantsTextChanged.[index] && components.Enabled.[index] then
            let themeId = components.ThemeId.[index]
            this.EnsureTextAlignment components styleSheet index
            let fontId = styleSheet.GetFont themeId "default"
            let font = ctx.Content.Load<NoobishMonoGameFont> fontId
            let fontSize = styleSheet.GetFontSize themeId "default"
            let padding = components.Padding.[index]
            let textBounds = NoobishRenderV2.computeTextBounds bounds padding
            let textAlign = components.TextAlign.[index]
            let textWrap = components.Textwrap.[index]
            let caretIndex = components.CaretIndex.[index]
            let text = components.Text.[index]
            let caretBounds = NoobishFont.calculateCursorPosition font.Font fontSize textWrap textBounds 0f 0f textAlign caretIndex text
            let cursorWidth = styleSheet.GetWidth "Cursor" "default"
            if cursorWidth > 0f && caretBounds.Height > 0f then
                let reset = components.CaretBlinkReset.[index]
                let startTime =
                    NoobishRenderV2.resolveCaretBlinkStart
                        caretBlinkByLocalId
                        caretBlinkByIndex
                        components.Id.[index]
                        index
                        gameTime.TotalGameTime
                        reset
                if reset then
                    components.CaretBlinkReset.[index] <- false
                let elapsed = gameTime.TotalGameTime - startTime
                let blinkProgress = Cursor.blink elapsed
                let baseColor = styleSheet.GetColor "Cursor" "default" |> toColor
                let color = Color.Lerp(baseColor, Color.Transparent, blinkProgress)
                let drawables = styleSheet.GetDrawables "Cursor" "default"
                let layer = this.ResolveTextLayer components index 2
                let position = Vector2(caretBounds.X + caretBounds.Width, caretBounds.Y)
                let size = Vector2(cursorWidth, caretBounds.Height)
                ctx.DrawDrawable textureAtlas position size layer (color |> NoobishColorMonoGame.ofColor) drawables

    member private this.DrawComponent
        (ctx: NoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (parentBounds: NoobishRectangle)
        (scrollX: float32)
        (scrollY: float32)
        (index: int)
        (gameTime: GameTime) =
        if components.Visible.[index] then
            let bounds = NoobishRenderV2.offsetBounds components.Bounds.[index] scrollX scrollY
            let clippedBounds = bounds.Clamp parentBounds
            if NoobishRectangle.hasArea clippedBounds then
                this.WithScissor ctx clippedBounds (fun () ->
                    this.WithSpriteBatch ctx (fun () ->
                        if components.WantsSlider.[index] then
                            this.DrawSliderTrack ctx components styleSheet textureAtlas bounds index
                            this.DrawSliderPin ctx components styleSheet textureAtlas bounds index
                        elif components.WantsProgress.[index] then
                            this.DrawBackground ctx components styleSheet textureAtlas bounds index
                            if components.ProgressSegments.[index] > 1 then
                                this.DrawProgressSegments ctx components styleSheet textureAtlas bounds index
                            else
                                this.DrawProgressFill ctx components styleSheet textureAtlas bounds index
                        else
                            this.DrawBackground ctx components styleSheet textureAtlas bounds index))

                let textClip = NoobishRenderV2.computeTextBounds bounds components.Padding.[index]
                let textClip = textClip.Clamp clippedBounds
                if NoobishRectangle.hasArea textClip then
                    this.WithScissor ctx textClip (fun () ->
                        this.DrawText ctx components styleSheet bounds index
                        if components.Focused.[index] && components.WantsTextChanged.[index] then
                            this.WithSpriteBatch ctx (fun () ->
                                this.DrawCaret ctx components styleSheet textureAtlas bounds index gameTime))

                if this.Debug then
                    let pixel = ctx.Content.Load<Texture2D> "Pixel"
                    let color = Color.Multiply(Color.Yellow, 0.1f)
                    this.WithSpriteBatch ctx (fun () ->
                        ctx.DrawRectangle pixel color bounds.X bounds.Y bounds.Width bounds.Height)

                let children = components.Children.[index]
                if children.Count > 0 then
                    let contentBounds = NoobishRenderV2.computeTextBounds bounds components.Padding.[index]
                    let childClip = contentBounds.Clamp clippedBounds
                    if NoobishRectangle.hasArea childClip then
                        let scroll = components.Scroll.[index]
                        let childScrollX = scrollX + if scroll.Horizontal then components.ScrollX.[index] else 0f
                        let childScrollY = scrollY + if scroll.Vertical then components.ScrollY.[index] else 0f
                        for i = 0 to children.Count - 1 do
                            let childIndex = int children.[i].Index
                            this.DrawComponent ctx components styleSheet textureAtlas childClip childScrollX childScrollY childIndex gameTime

    member this.Draw
        (components: NoobishComponentsV2)
        (ctx: NoobishMonoGameRenderContext)
        (styleSheetId: string)
        (gameTime: GameTime) =

        let graphics = ctx.Graphics
        let screenWidth = float32 graphics.Viewport.Width
        let screenHeight = float32 graphics.Viewport.Height

        let styleSheet = ctx.Content.Load<NoobishStyleSheet> styleSheetId
        let textureAtlas = ctx.Content.Load<NoobishTextureAtlas> styleSheet.TextureAtlasId

        drawQueue.Clear()
        for i = 0 to components.Count - 1 do
            if components.ParentId.[i] = UIComponentIdV2.empty then
                drawQueue.Enqueue(i, components.Layer.[i])

        let oldRasterizerState = graphics.RasterizerState
        graphics.RasterizerState <- rasterizerState

        while drawQueue.Count > 0 do
            let i = drawQueue.Dequeue()
            this.DrawComponent ctx components styleSheet textureAtlas {X = 0f; Y = 0f; Width = screenWidth; Height = screenHeight} 0f 0f i gameTime

        graphics.RasterizerState <- oldRasterizerState
