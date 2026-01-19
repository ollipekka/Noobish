namespace Noobish

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Noobish.Internal
open Noobish.Styles
open Noobish.TextureAtlas


module DrawUI = 
    open Internal


    let createRectangle (x: float32, y:float32, width: float32, height: float32) =
        Rectangle (int (x), int (y), int (width), int (height))


    let toRectangle (r: NoobishRectangle) =
        Rectangle (int (r.X), int (r.Y), int (r.Width), int (r.Height))


    let toRotatedRectangle (r: NoobishRectangle) (phi: float32) = 
        if abs (phi) >= Single.Epsilon then 
            let rotation = Matrix.CreateRotationZ phi
            let halfWidth = r.Width / 2f
            let halfHeight = r.Height / 2f
            let center = Vector2(r.X + halfWidth, r.Y + halfHeight)
            let p1 = Vector2.Transform(Vector2(-halfWidth, -halfHeight), rotation)
            let p2 = Vector2.Transform(Vector2( halfWidth, -halfHeight), rotation)
            let p3 = Vector2.Transform(Vector2( halfWidth,  halfHeight), rotation)
            let p4 = Vector2.Transform(Vector2(-halfWidth,  halfHeight), rotation)

            let minX = min p1.X (min p2.X (min p3.X p4.X))
            let minY = min p1.Y (min p2.Y (min p3.Y p4.Y))
            let maxX = max p1.X (max p2.X (max p3.X p4.X))
            let maxY = max p1.Y (max p2.Y (max p3.Y p4.Y))
            Rectangle (int (round (center.X + minX)), int (round(center.Y + minY)), int (round(maxX - minX)), int (round(maxY - minY)))
        else 
            toRectangle r

    let calculateImageBounds (imageSize: NoobishImageSize) (imageAlign: NoobishAlignment) (bounds: NoobishRectangle) (textureWidth: int) (textureHeight: int) (scrollX: float32) (scrollY: float32) =
        match imageSize with
        | NoobishImageSize.Stretch ->
            createRectangle
                ((bounds.X + scrollX),
                (bounds.Y + scrollY),
                bounds.Width,
                bounds.Height)

        | NoobishImageSize.BestFitMax ->
            let ratio = max (bounds.Width / float32 textureWidth) (bounds.Height / float32 textureHeight)
            let width = ratio * float32 textureWidth
            let height = ratio * float32 textureHeight
            let padLeft = (bounds.Width - width) / 2.0f
            let padTop = (bounds.Height - height) / 2.0f
            createRectangle
                ((bounds.X + scrollX + padLeft),
                (bounds.Y + scrollY + padTop),
                width,
                height)

        | NoobishImageSize.BestFitMin ->
            let ratio = min (bounds.Width / float32 textureWidth) (bounds.Height / float32 textureHeight)
            let width = ratio * float32 textureWidth
            let height = ratio * float32 textureHeight
            let padLeft = (bounds.Width - width) / 2.0f
            let padTop = (bounds.Height - height) / 2.0f
            createRectangle
                ((bounds.X + scrollX + padLeft),
                (bounds.Y + scrollY + padTop),
                width,
                height)

        | NoobishImageSize.Original ->
            createRectangle
                ((bounds.X + scrollX),
                (bounds.Y + scrollY),
                bounds.Width,
                bounds.Height)


    let drawDrawable (textureAtlas: NoobishTextureAtlas) (spriteBatch: SpriteBatch)  (position: Vector2) (size: Vector2) (layer: float32) (color: Color) (drawables: NoobishDrawable[]) =
        for drawable in drawables do
            match drawable with
            | NoobishDrawable.Texture _ -> failwith "Texture not supported for cursor."
            | NoobishDrawable.NinePatch(tid) ->
                let texture = textureAtlas.[tid]

                spriteBatch.DrawAtlasNinePatch2(
                    texture,
                    Rectangle(int position.X, int position.Y, int size.X, int size.Y),
                    color,
                    layer)
            | NoobishDrawable.NinePatchWithColor(tid, color) ->
                let texture = textureAtlas.[tid]

                spriteBatch.DrawAtlasNinePatch2(
                    texture,
                    Rectangle(int position.X, int position.Y, int size.X, int size.Y),
                    color,
                    layer)
    let drawRectangle (spriteBatch: SpriteBatch) (pixel: Texture2D) (color: Color) (x: float32) (y:float32) (width: float32) (height: float32) =
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

    let getTextureEfffect (t: NoobishTextureEffect) =
        if t = NoobishTextureEffect.FlipHorizontally then
            SpriteEffects.FlipHorizontally
        else if t = NoobishTextureEffect.FlipVertically then
            SpriteEffects.FlipVertically
        else
            SpriteEffects.None

    let debugDrawBorders (spriteBatch: SpriteBatch) pixel (borderColor: Color) (bounds: NoobishRectangle) =
        let borderSize = 2f
        let widthWithoutBorders = float32 bounds.Width - borderSize

        //Left
        drawRectangle spriteBatch pixel borderColor (bounds.X) bounds.Y borderSize bounds.Height
        // Right
        drawRectangle spriteBatch pixel borderColor (bounds.X + bounds.Width - borderSize) bounds.Y  borderSize bounds.Height
        // Top
        drawRectangle spriteBatch pixel borderColor (bounds.X + borderSize) bounds.Y widthWithoutBorders borderSize
        // Bottom
        drawRectangle spriteBatch pixel borderColor (bounds.X + borderSize) ( bounds.Y + bounds.Height - borderSize) widthWithoutBorders borderSize




module NoobishRenderV2 =
    let resolveState (enabled: bool) (toggled: bool) (hovered: bool) (focused: bool) =
        if not enabled then "disabled"
        elif focused then "focused"
        elif toggled && hovered then "toggledHovered"
        elif toggled then "toggled"
        elif hovered then "hovered"
        else "default"

    let computeTextBounds (bounds: NoobishRectangle) (padding: NoobishPadding) =
        {
            X = bounds.X + padding.Left
            Y = bounds.Y + padding.Top
            Width = Internal.max0 (bounds.Width - padding.Left - padding.Right)
            Height = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        }

    let offsetBounds (bounds: NoobishRectangle) (offsetX: float32) (offsetY: float32) =
        { bounds with X = bounds.X + offsetX; Y = bounds.Y + offsetY }

    let toScissorRectangle (bounds: NoobishRectangle) =
        let left = MathF.Floor bounds.X
        let top = MathF.Floor bounds.Y
        let right = MathF.Ceiling (bounds.X + bounds.Width)
        let bottom = MathF.Ceiling (bounds.Y + bounds.Height)
        let width = Internal.max0 (right - left)
        let height = Internal.max0 (bottom - top)
        Rectangle(int left, int top, int width, int height)

    let internal resolveCaretBlinkStart
        (byLocalId: Dictionary<uint32, TimeSpan>)
        (byIndex: Dictionary<int, TimeSpan>)
        (componentId: UIComponentIdV2)
        (index: int)
        (now: TimeSpan)
        (reset: bool) =
        let localId = componentId.LocalId
        if localId <> 0us then
            let key = (uint32 componentId.Namespace <<< 16) ||| uint32 localId
            if reset then
                byLocalId.[key] <- now
            match byLocalId.TryGetValue key with
            | true, value -> value
            | false, _ ->
                byLocalId.[key] <- now
                now
        else
            if reset then
                byIndex.[index] <- now
            match byIndex.TryGetValue index with
            | true, value -> value
            | false, _ ->
                byIndex.[index] <- now
                now

    let computeProgressSegmentWidth (contentWidth: float32) (segments: int) (gap: float32) =
        if segments <= 0 then
            0f
        else
            Internal.max0 ((contentWidth - gap * float32 (segments - 1)) / float32 segments)

    let computeProgressBounds (bounds: NoobishRectangle) (padding: NoobishPadding) (progress: float32) =
        let content = computeTextBounds bounds padding
        let clamped = Math.Clamp(progress, 0f, 1f)
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
        let availableWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let availableHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        let clampedPinWidth = min pinWidth availableWidth
        let clampedPinHeight = min pinHeight availableHeight
        let span = rangeEnd - rangeStart
        let t =
            if span = 0f then
                0f
            else
                Math.Clamp ((value - rangeStart) / span, 0f, 1f)
        let usableWidth = Internal.max0 (availableWidth - clampedPinWidth)
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
        let availableWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let availableHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
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
    let caretBlinkByLocalId = Dictionary<uint32, TimeSpan>()
    let caretBlinkByIndex = Dictionary<int, TimeSpan>()

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
        (bounds: NoobishRectangle)
        (index: int) =
        let themeId = components.ThemeId.[index]
        let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index] components.Focused.[index]
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
        (bounds: NoobishRectangle)
        (index: int) =
        if bounds.Width > 0f && bounds.Height > 0f then
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index] components.Focused.[index]
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
                let pinLayer = Internal.max0 (layer - 0.0001f)
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
        (bounds: NoobishRectangle)
        (index: int) =
        if bounds.Width > 0f && bounds.Height > 0f then
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index] components.Focused.[index]
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
        (bounds: NoobishRectangle)
        (index: int) =
        if bounds.Width > 0f && bounds.Height > 0f then
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index] components.Focused.[index]
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
        (bounds: NoobishRectangle)
        (index: int) =
        if bounds.Width > 0f && bounds.Height > 0f then
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index] components.Focused.[index]
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
                            DrawUI.drawDrawable textureAtlas spriteBatch fillPosition fillSize layer color drawables

    member private this.DrawText
        (components: NoobishComponentsV2)
        (content: ContentManager)
        (styleSheet: NoobishStyleSheet)
        (textBatch: TextBatch)
        (bounds: NoobishRectangle)
        (index: int) =
        let text = components.Text.[index]
        if not (String.IsNullOrWhiteSpace text) then
            let themeId = components.ThemeId.[index]
            let state = NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index] components.Focused.[index]
            this.EnsureTextAlignment components styleSheet index

            let fontId = styleSheet.GetFont themeId state
            let font = content.Load<NoobishFont> fontId
            let fontSize = styleSheet.GetFontSize themeId state
            let textColor = styleSheet.GetFontColor themeId state

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

    member private this.DrawCaret
        (components: NoobishComponentsV2)
        (content: ContentManager)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (spriteBatch: SpriteBatch)
        (bounds: NoobishRectangle)
        (index: int)
        (gameTime: GameTime) =
        if components.Focused.[index] && components.WantsTextChanged.[index] && components.Enabled.[index] then
            let themeId = components.ThemeId.[index]
            this.EnsureTextAlignment components styleSheet index
            let fontId = styleSheet.GetFont themeId "default"
            let font = content.Load<NoobishFont> fontId
            let fontSize = styleSheet.GetFontSize themeId "default"
            let padding = components.Padding.[index]
            let textBounds = NoobishRenderV2.computeTextBounds bounds padding
            let textAlign = components.TextAlign.[index]
            let textWrap = components.Textwrap.[index]
            let caretIndex = components.CaretIndex.[index]
            let text = components.Text.[index]
            let caretBounds = NoobishFont.calculateCursorPosition font fontSize textWrap textBounds 0f 0f textAlign caretIndex text
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
                let baseColor = styleSheet.GetColor "Cursor" "default"
                let color = Color.Lerp(baseColor, Color.Transparent, blinkProgress)
                let drawables = styleSheet.GetDrawables "Cursor" "default"
                let layer = 1f - float32 (components.Layer.[index] + 2) / 32768.0f
                let position = Vector2(caretBounds.X + caretBounds.Width, caretBounds.Y)
                let size = Vector2(cursorWidth, caretBounds.Height)
                DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables

    member private this.DrawComponent
        (components: NoobishComponentsV2)
        (graphics: GraphicsDevice)
        (content: ContentManager)
        (spriteBatch: SpriteBatch)
        (textBatch: TextBatch)
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
            if clippedBounds.Width > 0f && clippedBounds.Height > 0f then
                let oldScissorRect = graphics.ScissorRectangle
                graphics.ScissorRectangle <- NoobishRenderV2.toScissorRectangle clippedBounds

                spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)
                if components.WantsSlider.[index] then
                    this.DrawSliderTrack components styleSheet textureAtlas spriteBatch bounds index
                    this.DrawSliderPin components styleSheet textureAtlas spriteBatch bounds index
                elif components.WantsProgress.[index] then
                    this.DrawBackground components styleSheet textureAtlas spriteBatch bounds index
                    if components.ProgressSegments.[index] > 1 then
                        this.DrawProgressSegments components styleSheet textureAtlas spriteBatch bounds index
                    else
                        this.DrawProgressFill components styleSheet textureAtlas spriteBatch bounds index
                else
                    this.DrawBackground components styleSheet textureAtlas spriteBatch bounds index
                spriteBatch.End()

                let textClip = NoobishRenderV2.computeTextBounds bounds components.Padding.[index]
                let textClip = textClip.Clamp clippedBounds
                if textClip.Width > 0f && textClip.Height > 0f then
                    graphics.ScissorRectangle <- NoobishRenderV2.toScissorRectangle textClip
                    this.DrawText components content styleSheet textBatch bounds index
                    if components.Focused.[index] && components.WantsTextChanged.[index] then
                        spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)
                        this.DrawCaret components content styleSheet textureAtlas spriteBatch bounds index gameTime
                        spriteBatch.End()

                graphics.ScissorRectangle <- oldScissorRect

                if this.Debug then
                    let pixel = content.Load<Texture2D> "Pixel"
                    let color = Color.Multiply(Color.Yellow, 0.1f)
                    spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)
                    DrawUI.drawRectangle spriteBatch pixel color bounds.X bounds.Y bounds.Width bounds.Height
                    spriteBatch.End()

                let children = components.Children.[index]
                if children.Count > 0 then
                    let contentBounds = NoobishRenderV2.computeTextBounds bounds components.Padding.[index]
                    let childClip = contentBounds.Clamp clippedBounds
                    if childClip.Width > 0f && childClip.Height > 0f then
                        let scroll = components.Scroll.[index]
                        let childScrollX = scrollX + if scroll.Horizontal then components.ScrollX.[index] else 0f
                        let childScrollY = scrollY + if scroll.Vertical then components.ScrollY.[index] else 0f
                        for i = 0 to children.Count - 1 do
                            let childIndex = int children.[i].Index
                            this.DrawComponent components graphics content spriteBatch textBatch styleSheet textureAtlas childClip childScrollX childScrollY childIndex gameTime

    member this.Draw
        (components: NoobishComponentsV2)
        (graphics: GraphicsDevice)
        (content: ContentManager)
        (spriteBatch: SpriteBatch)
        (textBatch: TextBatch)
        (styleSheetId: string)
        (gameTime: GameTime) =

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
            this.DrawComponent components graphics content spriteBatch textBatch styleSheet textureAtlas {X = 0f; Y = 0f; Width = screenWidth; Height = screenHeight} 0f 0f i gameTime

        graphics.RasterizerState <- oldRasterizerState
