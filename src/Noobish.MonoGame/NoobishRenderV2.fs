namespace Noobish

open System
open System.Collections.Generic
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish.Internal
open Noobish.Styles
open Noobish.TextureAtlas
open NoobishColorMonoGame

[<Struct>]
type ScrollActivity = {
    ScrollX: float32
    ScrollY: float32
    LastActive: TimeSpan
}

type NoobishMonoGameRendererV2() =
    let rasterizerState =
        let state = new RasterizerState()
        state.ScissorTestEnable <- true
        state

    let drawQueue = PriorityQueue<int, int>()
    let caretBlinkByLocalId = Dictionary<uint32, TimeSpan>()
    let caretBlinkByIndex = Dictionary<int, TimeSpan>()
    let scrollActivityByLocalId = Dictionary<uint32, ScrollActivity>()
    let scrollActivityByIndex = Dictionary<int, ScrollActivity>()
    let scrollBarTimeout = TimeSpan.FromSeconds 1.0

    member val Debug = false with get, set

    member private _.ResolveState (components: NoobishComponentsV2) (index: int) =
        NoobishRenderV2.resolveState components.Enabled.[index] components.Toggled.[index] components.Hovered.[index] components.Focused.[index]

    member private _.ResolveLayer (components: NoobishComponentsV2) (index: int) =
        1f - float32 components.Layer.[index] / 255f

    member private _.ResolveTextLayer (components: NoobishComponentsV2) (index: int) (offset: int) =
        1f - float32 (components.Layer.[index] + offset) / 32768.0f

    member private this.EnsureTextAlignment (components: NoobishComponentsV2) (styleSheet: NoobishStyleSheet) (index: int) =
        if components.TextAlign.[index] = NoobishAlignment.None then
            let themeId = components.ThemeId.[index]
            components.TextAlign.[index] <- styleSheet.GetTextAlignment themeId "default"

    member private this.DrawBackground
        (ctx: INoobishMonoGameRenderContext)
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

    member private _.ComputeContentExtent (components: NoobishComponentsV2) (index: int) =
        let bounds = components.Bounds.[index]
        let padding = components.Padding.[index]
        let contentX = bounds.X + padding.Left
        let contentY = bounds.Y + padding.Top
        let children = components.Children.[index]
        if children.Count > 0 then
            let mutable maxRight = contentX
            let mutable maxBottom = contentY
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let childBounds = components.Bounds.[childIndex]
                let right = childBounds.X + childBounds.Width
                let bottom = childBounds.Y + childBounds.Height
                if right > maxRight then
                    maxRight <- right
                if bottom > maxBottom then
                    maxBottom <- bottom
            struct(Internal.max0 (maxRight - contentX), Internal.max0 (maxBottom - contentY))
        else
            let contentSize = components.ContentSize.[index]
            struct(contentSize.Width, contentSize.Height)

    member private _.UpdateScrollActivity (components: NoobishComponentsV2) (index: int) (now: TimeSpan) =
        let scrollX = components.ScrollX.[index]
        let scrollY = components.ScrollY.[index]
        let componentId = components.Id.[index]
        let initialActive = if scrollX <> 0f || scrollY <> 0f then now else TimeSpan.MinValue
        if componentId.LocalId <> 0us then
            let key = (uint32 componentId.Namespace <<< 16) ||| uint32 componentId.LocalId
            let mutable activity = Unchecked.defaultof<ScrollActivity>
            if scrollActivityByLocalId.TryGetValue(key, &activity) then
                if activity.ScrollX <> scrollX || activity.ScrollY <> scrollY then
                    let updated = { ScrollX = scrollX; ScrollY = scrollY; LastActive = now }
                    scrollActivityByLocalId.[key] <- updated
                    updated.LastActive
                else
                    activity.LastActive
            else
                let updated = { ScrollX = scrollX; ScrollY = scrollY; LastActive = initialActive }
                scrollActivityByLocalId.[key] <- updated
                updated.LastActive
        else
            let mutable activity = Unchecked.defaultof<ScrollActivity>
            if scrollActivityByIndex.TryGetValue(index, &activity) then
                if activity.ScrollX <> scrollX || activity.ScrollY <> scrollY then
                    let updated = { ScrollX = scrollX; ScrollY = scrollY; LastActive = now }
                    scrollActivityByIndex.[index] <- updated
                    updated.LastActive
                else
                    activity.LastActive
            else
                let updated = { ScrollX = scrollX; ScrollY = scrollY; LastActive = initialActive }
                scrollActivityByIndex.[index] <- updated
                updated.LastActive

    member private _.ShouldShowScrollBars (lastActive: TimeSpan) (now: TimeSpan) =
        lastActive <> TimeSpan.MinValue && (now - lastActive) <= scrollBarTimeout

    member private this.DrawSliderPin
        (ctx: INoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int) =
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
        (ctx: INoobishMonoGameRenderContext)
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
        (ctx: INoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int) =
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

    member private this.DrawProgressRadial
        (ctx: INoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (bounds: NoobishRectangle)
        (index: int) =
        let padding = components.Padding.[index]
        let content = NoobishRenderV2.computeTextBounds bounds padding
        let progress = Math.Clamp(components.ProgressValue.[index], 0f, 1f)
        if NoobishRectangle.hasArea content && progress > 0f then
            let centerX = content.X + content.Width * 0.5f
            let centerY = content.Y + content.Height * 0.5f
            let radiusX = content.Width * 0.5f
            let radiusY = content.Height * 0.5f
            if radiusX > 0f && radiusY > 0f then
                let state = this.ResolveState components index
                let fillThemeId = "ProgressBar-Progress"
                let fillColor = styleSheet.GetColor fillThemeId state |> toColor
                let layer = this.ResolveLayer components index
                let startAngle = -MathF.PI * 0.5f
                let fullSweep = MathF.PI * 2f
                let sweep = progress * fullSweep
                let center = Vector2(centerX, centerY)
                let pointAt angle =
                    Vector2(centerX + MathF.Cos(angle) * radiusX, centerY + MathF.Sin(angle) * radiusY)
                let maxRadius = max radiusX radiusY
                let triangleCount =
                    let fullTriangles = Math.Clamp(int (MathF.Ceiling(maxRadius * 0.8f)), 16, 128)
                    max 1 (int (MathF.Ceiling(sweep / fullSweep * float32 fullTriangles)))
                let step = sweep / float32 triangleCount
                for i = 0 to triangleCount - 1 do
                    let angle1 = startAngle + float32 i * step
                    let angle2 = startAngle + float32 (i + 1) * step
                    let p1 = pointAt angle1
                    let p2 = pointAt angle2
                    ctx.DrawTriangle center p1 p2 layer fillColor

    member private this.DrawProgressRadialSquare
        (ctx: INoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (bounds: NoobishRectangle)
        (index: int) =
        let padding = components.Padding.[index]
        let content = NoobishRenderV2.computeTextBounds bounds padding
        let progress = Math.Clamp(components.ProgressValue.[index], 0f, 1f)
        if NoobishRectangle.hasArea content && progress > 0f then
            let centerX = content.X + content.Width * 0.5f
            let centerY = content.Y + content.Height * 0.5f
            let state = this.ResolveState components index
            let fillThemeId = "ProgressBar-Progress"
            let fillColor = styleSheet.GetColor fillThemeId state |> toColor
            let layer = this.ResolveLayer components index
            let startAngle = -MathF.PI * 0.5f
            let fullSweep = MathF.PI * 2f
            let sweep = progress * fullSweep
            let halfWidth = content.Width * 0.5f
            let halfHeight = content.Height * 0.5f
            if halfWidth > 0f && halfHeight > 0f then
                let center = Vector2(centerX, centerY)
                let pointAt angle =
                    let dx = MathF.Cos angle
                    let dy = MathF.Sin angle
                    let tx = if MathF.Abs(dx) < 0.0001f then Single.PositiveInfinity else halfWidth / MathF.Abs dx
                    let ty = if MathF.Abs(dy) < 0.0001f then Single.PositiveInfinity else halfHeight / MathF.Abs dy
                    let t = min tx ty
                    Vector2(centerX + dx * t, centerY + dy * t)
                let normalizeAngle (angle: float32) =
                    let mutable wrapped = angle % fullSweep
                    if wrapped < 0f then
                        wrapped <- wrapped + fullSweep
                    wrapped
                let corners =
                    [|
                        Vector2(centerX + halfWidth, centerY - halfHeight) // top-right
                        Vector2(centerX + halfWidth, centerY + halfHeight) // bottom-right
                        Vector2(centerX - halfWidth, centerY + halfHeight) // bottom-left
                        Vector2(centerX - halfWidth, centerY - halfHeight) // top-left
                    |]
                let cornerAngles = corners |> Array.map (fun c -> normalizeAngle (MathF.Atan2(c.Y - centerY, c.X - centerX)))
                let angleEpsilon = 0.0001f
                let startPoint = pointAt startAngle
                let endPoint = pointAt (startAngle + sweep)
                let vertices = ResizeArray<Vector2>(8)
                vertices.Add startPoint
                for i = 0 to cornerAngles.Length - 1 do
                    let cornerAngle = cornerAngles.[i]
                    let cornerRelative = normalizeAngle (cornerAngle - startAngle)
                    if cornerRelative >= -angleEpsilon && cornerRelative <= sweep + angleEpsilon then
                        vertices.Add corners.[i]
                vertices.Add endPoint
                for i = 0 to vertices.Count - 2 do
                    let p1 = vertices.[i]
                    let p2 = vertices.[i + 1]
                    ctx.DrawTriangle center p1 p2 layer fillColor

    member private this.DrawProgressSegments
        (ctx: INoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (index: int) =
        let state = this.ResolveState components index
        let padding = components.Padding.[index]
        let content = NoobishRenderV2.computeTextBounds bounds padding
        if NoobishRectangle.hasArea content then
            let segments = max 1 components.ProgressSegments.[index]
            let dashMargin = styleSheet.GetMargin "ProgressBar-Dash" "default"
            let gap = max 0f (dashMargin.Left + dashMargin.Right)
            let segmentWidth = NoobishRenderV2.computeProgressSegmentWidth content.Width segments gap
            if segmentWidth > 0f then
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
        (ctx: INoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (bounds: NoobishRectangle)
        (index: int) =
        let text = NoobishTextDisplay.resolve components.TextDisplayMode.[index] components.Text.[index]
        if not (String.IsNullOrWhiteSpace text) then
            let themeId = components.ThemeId.[index]
            let state = this.ResolveState components index
            this.EnsureTextAlignment components styleSheet index

            let fontId = styleSheet.GetFont themeId state
            let font = ctx.LoadFont fontId
            let fontSize = styleSheet.GetFontSize themeId state
            let textColor = styleSheet.GetFontColor themeId state |> toColor

            let padding = components.Padding.[index]
            let textBounds = NoobishRenderV2.computeTextBounds bounds padding
            let textAlign = components.TextAlign.[index]
            let textWrap = components.Textwrap.[index]
            let textBounds = NoobishFont.calculateBounds font.Font fontSize textWrap textBounds 0f 0f textAlign text
            let layer = this.ResolveTextLayer components index 1

            if textWrap then
                ctx.DrawTextMultiLine font fontSize textBounds.Width (Vector2(textBounds.X, textBounds.Y)) layer textColor text
            else
                ctx.DrawTextSingleLine font fontSize (Vector2(textBounds.X, textBounds.Y)) layer textColor text

    member private this.DrawCaret
        (ctx: INoobishMonoGameRenderContext)
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
            let font = ctx.LoadFont fontId
            let fontSize = styleSheet.GetFontSize themeId "default"
            let padding = components.Padding.[index]
            let textBounds = NoobishRenderV2.computeTextBounds bounds padding
            let textAlign = components.TextAlign.[index]
            let textWrap = components.Textwrap.[index]
            let caretIndex = components.CaretIndex.[index]
            let text = NoobishTextDisplay.resolve components.TextDisplayMode.[index] components.Text.[index]
            let caretBounds = NoobishFont.calculateCaretPosition font.Font fontSize textWrap textBounds 0f 0f textAlign caretIndex text
            let caretWidth = styleSheet.GetWidth "Caret" "default"
            if caretWidth > 0f && caretBounds.Height > 0f then
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
                let blinkProgress = Caret.blink elapsed
                let baseColor = styleSheet.GetColor "Caret" "default" |> toColor
                let color = Color.Lerp(baseColor, Color.Transparent, blinkProgress)
                let drawables = styleSheet.GetDrawables "Caret" "default"
                let layer = this.ResolveTextLayer components index 2
                let position = Vector2(caretBounds.X + caretBounds.Width, caretBounds.Y)
                let size = Vector2(caretWidth, caretBounds.Height)
                ctx.DrawDrawable textureAtlas position size layer (color |> NoobishColorMonoGame.ofColor) drawables

    member private this.DrawScrollBars
        (ctx: INoobishMonoGameRenderContext)
        (components: NoobishComponentsV2)
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (bounds: NoobishRectangle)
        (clippedBounds: NoobishRectangle)
        (index: int)
        (gameTime: GameTime) =
        let scroll = components.Scroll.[index]
        if scroll.Horizontal || scroll.Vertical then
            let contentBounds = NoobishRenderV2.computeTextBounds bounds components.Padding.[index]
            let contentBounds = contentBounds.Clamp clippedBounds
            if NoobishRectangle.hasArea contentBounds then
                let struct(contentWidth, contentHeight) = this.ComputeContentExtent components index
                let viewportWidth = contentBounds.Width
                let viewportHeight = contentBounds.Height
                let canScrollVertical = scroll.Vertical && contentHeight > viewportHeight
                let canScrollHorizontal = scroll.Horizontal && contentWidth > viewportWidth
                if canScrollVertical || canScrollHorizontal then
                    let lastActive = this.UpdateScrollActivity components index gameTime.TotalGameTime
                    if this.ShouldShowScrollBars lastActive gameTime.TotalGameTime then
                        let state = this.ResolveState components index
                        let trackThickness =
                            let width = styleSheet.GetWidth "ScrollBar" state
                            if width > 0f then
                                width
                            else
                                let height = styleSheet.GetHeight "ScrollBar" state
                                if height > 0f then height else 0f
                        let pinThickness =
                            let width = styleSheet.GetWidth "ScrollBarPin" state
                            if width > 0f then width else trackThickness
                        let minPinLength =
                            let height = styleSheet.GetHeight "ScrollBarPin" state
                            if height > 0f then height else pinThickness
                        if trackThickness > 0f && pinThickness > 0f then
                            let layer = this.ResolveLayer components index
                            let pinLayer = Internal.max0 (layer - 0.0001f)
                            let trackColor = styleSheet.GetColor "ScrollBar" state
                            let trackDrawables = styleSheet.GetDrawables "ScrollBar" state
                            let pinColor = styleSheet.GetColor "ScrollBarPin" state
                            let pinDrawables = styleSheet.GetDrawables "ScrollBarPin" state
                            ctx.WithScissor contentBounds (fun () ->
                                ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
                                    if canScrollVertical then
                                        let trackBounds = NoobishRenderV2.computeScrollTrackBounds contentBounds trackThickness false canScrollHorizontal
                                        if NoobishRectangle.hasArea trackBounds then
                                            let position = Vector2(trackBounds.X, trackBounds.Y)
                                            let size = Vector2(trackBounds.Width, trackBounds.Height)
                                            ctx.DrawDrawable textureAtlas position size layer trackColor trackDrawables
                                            let pinBounds =
                                                NoobishRenderV2.computeScrollPinBounds
                                                    trackBounds
                                                    viewportHeight
                                                    contentHeight
                                                    components.ScrollY.[index]
                                                    minPinLength
                                                    false
                                            if NoobishRectangle.hasArea pinBounds then
                                                let pinPosition = Vector2(pinBounds.X, pinBounds.Y)
                                                let pinSize = Vector2(pinBounds.Width, pinBounds.Height)
                                                ctx.DrawDrawable textureAtlas pinPosition pinSize pinLayer pinColor pinDrawables
                                    if canScrollHorizontal then
                                        let trackBounds = NoobishRenderV2.computeScrollTrackBounds contentBounds trackThickness true canScrollVertical
                                        if NoobishRectangle.hasArea trackBounds then
                                            let position = Vector2(trackBounds.X, trackBounds.Y)
                                            let size = Vector2(trackBounds.Width, trackBounds.Height)
                                            ctx.DrawDrawable textureAtlas position size layer trackColor trackDrawables
                                            let pinBounds =
                                                NoobishRenderV2.computeScrollPinBounds
                                                    trackBounds
                                                    viewportWidth
                                                    contentWidth
                                                    components.ScrollX.[index]
                                                    minPinLength
                                                    true
                                            if NoobishRectangle.hasArea pinBounds then
                                                let pinPosition = Vector2(pinBounds.X, pinBounds.Y)
                                                let pinSize = Vector2(pinBounds.Width, pinBounds.Height)
                                                ctx.DrawDrawable textureAtlas pinPosition pinSize pinLayer pinColor pinDrawables))

    member private this.DrawComponent
        (ctx: INoobishMonoGameRenderContext)
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
                ctx.WithScissor clippedBounds (fun () ->
                    if components.WantsSlider.[index] then
                        ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
                            this.DrawSliderTrack ctx components styleSheet textureAtlas bounds index
                            this.DrawSliderPin ctx components styleSheet textureAtlas bounds index)
                    elif components.WantsProgress.[index] then
                        match components.ProgressStyle.[index] with
                        | NoobishProgressStyle.None ->
                            ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
                                this.DrawBackground ctx components styleSheet textureAtlas bounds index)
                        | NoobishProgressStyle.Radial ->
                            ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
                                this.DrawBackground ctx components styleSheet textureAtlas bounds index)
                            this.DrawProgressRadial ctx components styleSheet bounds index
                        | NoobishProgressStyle.RadialSquare ->
                            ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
                                this.DrawBackground ctx components styleSheet textureAtlas bounds index)
                            this.DrawProgressRadialSquare ctx components styleSheet bounds index
                        | NoobishProgressStyle.Bar ->
                            ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
                                this.DrawBackground ctx components styleSheet textureAtlas bounds index
                                if components.ProgressSegments.[index] > 1 then
                                    this.DrawProgressSegments ctx components styleSheet textureAtlas bounds index
                                else
                                    this.DrawProgressFill ctx components styleSheet textureAtlas bounds index)
                    else
                        ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
                            this.DrawBackground ctx components styleSheet textureAtlas bounds index))

                let textClip = NoobishRenderV2.computeTextBounds bounds components.Padding.[index]
                let textClip = textClip.Clamp clippedBounds
                if NoobishRectangle.hasArea textClip then
                    ctx.WithScissor textClip (fun () ->
                        this.DrawText ctx components styleSheet bounds index
                        if components.Focused.[index] && components.WantsTextChanged.[index] then
                            ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
                                this.DrawCaret ctx components styleSheet textureAtlas bounds index gameTime))

                if this.Debug then
                    let pixel = ctx.LoadPixel()
                    let color = Color.Multiply(Color.Yellow, 0.1f)
                    ctx.WithSpriteBatch rasterizerState SamplerState.PointClamp (fun () ->
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
                this.DrawScrollBars ctx components styleSheet textureAtlas bounds clippedBounds index gameTime

    member internal this.DrawWithContext
        (components: NoobishComponentsV2)
        (ctx: INoobishMonoGameRenderContext)
        (styleSheetId: string)
        (gameTime: GameTime) =

        let screenWidth = float32 ctx.ViewportWidth
        let screenHeight = float32 ctx.ViewportHeight

        let styleSheet = ctx.LoadStyleSheet styleSheetId
        let textureAtlas = ctx.LoadTextureAtlas styleSheet.TextureAtlasId

        drawQueue.Clear()
        for i = 0 to components.Count - 1 do
            if components.ParentId.[i] = UIComponentIdV2.empty then
                drawQueue.Enqueue(i, components.Layer.[i])

        while drawQueue.Count > 0 do
            let i = drawQueue.Dequeue()
            this.DrawComponent ctx components styleSheet textureAtlas {X = 0f; Y = 0f; Width = screenWidth; Height = screenHeight} 0f 0f i gameTime

    member this.Draw
        (components: NoobishComponentsV2)
        (ctx: NoobishMonoGameRenderContext)
        (styleSheetId: string)
        (gameTime: GameTime) =
        this.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) styleSheetId gameTime
