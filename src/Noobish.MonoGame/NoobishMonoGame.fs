namespace Noobish

open System
open System.Collections.Generic

open Elmish
open Noobish
open Noobish.Internal
open Noobish.Theme
open Noobish.TextureAtlas
open Noobish.Styles

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

open Microsoft.Xna.Framework.Content

open Noobish


type NoobishUI = {
    MeasureText: string -> string -> int*int
    Width: int
    Height: int
    Settings: NoobishSettings
    Components: Dictionary<string, NoobishLayoutElement>
    State: NoobishState
    mutable StyleSheet: NoobishStyleSheet
    mutable Debug: bool
    mutable Version: Guid
    mutable FPSEnabled: bool
    mutable FPS: int
    mutable FPSCounter: int
    mutable FPSTime: TimeSpan
    // Contains layers of layout components. Bottom is 0, one above bototm is 1.
    mutable Layers: NoobishLayoutElement[][]

} with
    member private this.DoElementsConsumeMouse (elements: NoobishLayoutElement[]) (positionX: float32) (positionY: float32) (button: NoobishMouseButtonId)=
        let mutable handled = false
        let mutable i = 0
        while not handled && i < elements.Length do
            let e = elements.[i]
            let handledByChild =
                if e.Children.Length > 0 then
                    this.DoElementsConsumeMouse e.Children positionX positionY button
                else
                    false

            if handledByChild then
                handled <- true

            elif not handledByChild && (e.ConsumedMouseButtons |> Array.contains button) && e.Contains positionX positionY 0f 0f then
                handled <- true

            i <- i + 1
        handled

    member this.ConsumeMouse (positionX: float32) (positionY: float32) (button: NoobishMouseButtonId) =
        let mutable handled = false
        let mutable l = 0
        while not handled && l < this.Layers.Length do
            let elements = this.Layers.[l]
            handled <- this.DoElementsConsumeMouse elements positionX positionY button
            l <- l + 1
        handled

[<RequireQualifiedAccess>]
module NoobishMonoGame =



    open Noobish
    open Noobish.Internal
    open Noobish.Theme
    let private createRectangle (x: float32) (y:float32) (width: float32) (height: float32) =
        Rectangle (int (x), int (y), int (width), int (height))


    let private calculateBounds (textureSize: NoobishTextureSize) (bounds: NoobishRectangle) (textureWidth: int) (textureHeight: int) (scrollX: float32) (scrollY: float32) =
        match textureSize with
        | NoobishTextureSize.Stretch ->
            createRectangle
                (bounds.X + scrollX)
                (bounds.Y + scrollY)
                bounds.Width
                bounds.Height

        | NoobishTextureSize.BestFitMax ->
            let ratio = max (bounds.Width / float32 textureWidth) (bounds.Height / float32 textureHeight)
            let width = ratio * float32 textureWidth
            let height = ratio * float32 textureHeight
            let padLeft = (bounds.Width - width) / 2.0f
            let padTop = (bounds.Height - height) / 2.0f
            createRectangle
                (bounds.X + scrollX + padLeft)
                (bounds.Y + scrollY + padTop)
                width
                height

        | NoobishTextureSize.BestFitMin ->
            let ratio = min (bounds.Width / float32 textureWidth) (bounds.Height / float32 textureHeight)
            let width = ratio * float32 textureWidth
            let height = ratio * float32 textureHeight
            let padLeft = (bounds.Width - width) / 2.0f
            let padTop = (bounds.Height - height) / 2.0f
            createRectangle
                (bounds.X + scrollX + padLeft)
                (bounds.Y + scrollY + padTop)
                width
                height

        | NoobishTextureSize.Original ->
            createRectangle
                (bounds.X + scrollX)
                (bounds.Y + scrollY)
                bounds.Width
                bounds.Height

    let getTextureEfffect (t: NoobishTextureEffect) =
        if t = NoobishTextureEffect.FlipHorizontally then
            SpriteEffects.FlipHorizontally
        else if t = NoobishTextureEffect.FlipVertically then
            SpriteEffects.FlipVertically
        else
            SpriteEffects.None


    let create (content: ContentManager) (styleSheetId: string) width height (settings: NoobishSettings) =

        let styleSheet = content.Load<NoobishStyleSheet> styleSheetId
        let measureText (font: string) (text: string) =
            let font = content.Load<SpriteFont> font
            let size = font.MeasureString text
            int (ceil (size.X)), int (ceil (size.Y))

        {
            MeasureText = measureText
            Width = width
            Height = height
            StyleSheet = styleSheet
            Settings = settings
            Components = Dictionary()
            State = NoobishState()
            Debug = false
            Version = Guid.NewGuid()
            Layers = [||]
            FPSEnabled = false
            FPS = 0
            FPSCounter = 0
            FPSTime = TimeSpan.Zero
        }

    let overrideMeasureText measureText ui = {
        ui with MeasureText = measureText
    }

    let overrideDebug d ui = {
        ui with Debug = d
    }

    let enableFps ui = {
        ui with FPSEnabled = true
    }

    let private drawRectangle (spriteBatch: SpriteBatch) (pixel: Texture2D) (color: Color) (x: float32) (y:float32) (width: float32) (height: float32) =
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

    let private toColor (v: int) =
        let r = (v >>> 24) &&& 255;
        let g = (v >>> 16) &&& 255; // 255
        let b = (v >>> 8) &&& 255; // 122
        let a = v &&& 255; // 15

        Color(r, g, b, a)

    let private drawDrawable (textureAtlas: TextureAtlas) (spriteBatch: SpriteBatch)  (position: Vector2) (size: Vector2) (color: Color) (drawables: NoobishDrawable[]) =
        for drawable in drawables do
            match drawable with
            | NoobishDrawable.Texture _ -> failwith "Texture not supported for cursor."
            | NoobishDrawable.NinePatch(tid) ->
                let texture = textureAtlas.[tid]

                spriteBatch.DrawAtlasNinePatch(
                    texture,
                    position,
                    size.X,
                    size.Y,
                    color,
                    0f,
                    Vector2.One,
                    SpriteEffects.None,
                    0f )
            | NoobishDrawable.NinePatchWithColor(tid, color) ->
                let texture = textureAtlas.[tid]

                spriteBatch.DrawAtlasNinePatch(
                    texture,
                    position,
                    size.X,
                    size.Y,
                    toColor color,
                    0f,
                    Vector2.One,
                    SpriteEffects.None,
                    0f )

    let private drawBackground (styleSheet: NoobishStyleSheet) (state: NoobishState)  (textureAtlas: TextureAtlas) (spriteBatch: SpriteBatch) (c: NoobishLayoutElement) (time: TimeSpan) scrollX scrollY =
        let cs = state.ElementsById.[c.Id]

        let cstate =
            if cs.CanFocus && cs.Focused then
                "focused"
            elif not c.Enabled then
                "disabled"
            elif c.Toggled then
                "toggled"
            else
                "default"

        let color =
            if cs.CanFocus && cs.Focused then
                styleSheet.GetColor c.ThemeId "focused" |> toColor
            elif not c.Enabled then
                styleSheet.GetColor c.ThemeId "disabled" |> toColor
            elif c.Toggled then
                styleSheet.GetColor c.ThemeId "toggled" |> toColor
            else
                if cs.Visible then
                    let progress = 1.0 - min ((time - cs.PressedTime).TotalSeconds / 0.15) 1.0

                    let color = styleSheet.GetColor c.ThemeId "default" |> toColor
                    let pressedColor = styleSheet.GetColor c.ThemeId "toggled" |> toColor
                    let finalColor = Color.Lerp(color, pressedColor, float32 progress)

                    finalColor

                else if cs.Toggled then
                    styleSheet.GetColor c.ThemeId "toggled" |> toColor
                else
                    Color.Transparent


        let rect = c.ContentWithPadding
        let drawables = styleSheet.GetDrawables c.ThemeId cstate

        let position = Vector2(float32 rect.X, float32 rect.Y)
        let size = Vector2( float32 rect.Width, float32 rect.Height)

        drawDrawable textureAtlas spriteBatch position size color drawables

    let private debugDrawBorders (spriteBatch: SpriteBatch) pixel (borderColor: Color) (bounds: NoobishRectangle) =
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


    let private drawText (styleSheet: NoobishStyleSheet) (content: ContentManager) (spriteBatch: SpriteBatch) (c: NoobishLayoutElement) (cs: NoobishLayoutElementState) scrollX scrollY =
        let mutable startY = 0.0f

        let textLines =
            cs.Model
            |> Option.map(
                function
                | Textbox model' ->
                    if not (String.IsNullOrEmpty model'.Text) then
                        Some([|model'.Text|])
                    else
                        None
                | _ -> None)
            |> Option.flatten
            |> Option.defaultValue c.Text


        let state =
            if cs.CanFocus && cs.Focused then "focused"
            elif not c.Enabled then "disabled"
            elif cs.Toggled then "toggled"
            else "default"

        let fontId = styleSheet.GetFont c.ThemeId state
        let font = content.Load<SpriteFont> fontId

        let bounds = c.Content
        for line in textLines do


            let size = font.MeasureString (line)

            let textSizeX, textSizeY = size.X, size.Y

            let leftX () = bounds.X + scrollX
            let rightX () = bounds.X + bounds.Width - textSizeX

            let topY () =
                bounds.Y + scrollY

            let bottomY () =
                bounds.Y + scrollY + bounds.Height - textSizeY

            let centerX () =
                bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f

            let centerY () =
                bounds.Y + scrollY + bounds.Height / 2.0f - textSizeY / 2.0f

            let textX, textY =
                match c.TextAlignment with
                | NoobishTextAlign.TopLeft -> leftX(), topY()
                | NoobishTextAlign.TopCenter -> centerX(), topY()
                | NoobishTextAlign.TopRight -> rightX(), topY()
                | NoobishTextAlign.Left -> leftX(), centerY()
                | NoobishTextAlign.Center -> centerX(), centerY()
                | NoobishTextAlign.Right -> rightX(), centerY()
                | NoobishTextAlign.BottomLeft -> leftX(), bottomY()
                | NoobishTextAlign.BottomCenter -> centerX(), bottomY()
                | NoobishTextAlign.BottomRight -> rightX(), bottomY()


            let textColor = toColor(styleSheet.GetFontColor c.ThemeId state)
            spriteBatch.DrawString(font, line, Vector2(floor textX, floor (startY + textY)), textColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f)
            startY <- startY + float32 font.LineSpacing

    let private drawScrollBars
        (styleSheet: NoobishStyleSheet)
        (state: NoobishState)
        (textureAtlas: TextureAtlas)
        (spriteBatch: SpriteBatch)
        (c: NoobishLayoutElement)
        time
        _scrollX
        _scrollY =

        let cs = state.[c.Id]

        let delta = float32 (min (time - cs.ScrolledTime).TotalSeconds 0.3)
        let progress = 1.0f - delta / 0.3f

        let scrollBarWidth = styleSheet.GetWidth "ScrollBar" "default"
        if scrollBarWidth = 0f then failwith "Missing width for styleSheet 'ScrollBar' mode 'default'."
        if c.ScrollVertical && progress > 0.0f then

            let scrollBarColor = styleSheet.GetColor "ScrollBar" "default" |> toColor
            let scrollbarDrawable = styleSheet.GetDrawables "ScrollBar" "default"

            let scrollbarPinColor = styleSheet.GetColor "ScrollBarPin" "default" |> toColor
            let scrollbarPinDrawable = styleSheet.GetDrawables "ScrollBar" "default"

            let bounds = c.ContentWithPadding
            let x = bounds.X + bounds.Width - scrollBarWidth
            let color = Color.Multiply(scrollBarColor, progress)

            drawDrawable textureAtlas spriteBatch (Vector2(x, bounds.Y + 4f))(Vector2(scrollBarWidth, bounds.Height - 8f)) color scrollbarDrawable

            let pinPosition =  - ( cs.ScrollY / c.OverflowHeight) * bounds.Height
            let pinHeight = ( c.Height / c.OverflowHeight) * bounds.Height
            let color = Color.Multiply(scrollbarPinColor, progress)

            drawDrawable textureAtlas spriteBatch (Vector2(x, bounds.Y + pinPosition + 4f))(Vector2(scrollBarWidth, pinHeight - 8f)) color scrollbarPinDrawable

    let private drawSlider
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: TextureAtlas)
        (spriteBatch: SpriteBatch)
        (c: NoobishLayoutElement)
        (slider: SliderModel)
        (_time: TimeSpan)
        _scrollX
        _scrollY =

        let pinWidth = 25.0f
        let pinHeight = styleSheet.GetHeight "SliderPin" "default"
        if pinHeight < 1f then failwith "SliderPin:default height is 0"

        let barHeight = styleSheet.GetHeight "Slider" "default"

        if barHeight < 1f then failwith "Slider:default height is 0"

        // Bar
        let bounds = c.Content
        let barPosition = Vector2(
            bounds.X + pinWidth / 2.0f,
            bounds.Y)
        let barSize = Vector2(bounds.Width - pinWidth, barHeight)
        let color = styleSheet.GetColor "Slider" "default" |> toColor

        let barDrawables = styleSheet.GetDrawables "Slider" "default"
        drawDrawable textureAtlas spriteBatch barPosition barSize color barDrawables

        // Pin
        let relativePosition = (slider.Value - slider.Min) / (slider.Max - slider.Min)

        let pinPosition = Vector2(
            bounds.X + (barSize.X * relativePosition) - (pinWidth / 2.0f) + (pinWidth / 2.0f),
            bounds.Y + barHeight / 2f - pinHeight / 2f)
        let pinSize = Vector2(pinWidth, pinHeight)
        let color = styleSheet.GetColor "SliderPin" "default" |> toColor

        let pinDrawables = styleSheet.GetDrawables "SliderPin" "default"
        drawDrawable textureAtlas spriteBatch pinPosition pinSize color pinDrawables

    let blinkInterval = TimeSpan.FromSeconds 1.2

    let private drawCursor
        (styleSheet: NoobishStyleSheet)
        (content: ContentManager)
        (textureAtlas: TextureAtlas)
        (spriteBatch: SpriteBatch)
        (c: NoobishLayoutElement)
        (cs: NoobishLayoutElementState)
        (textbox: TextboxModel)
        (time: TimeSpan) =

        let bounds = c.Content
        let cursorIndex = textbox.Cursor

        let textUpToCursor =
            if textbox.Text.Length > 0 then
                textbox.Text.Substring(0, cursorIndex)
            else
                ""

        let fontId = styleSheet.GetFont c.ThemeId "default"
        let font = content.Load<SpriteFont>  fontId
        let size = font.MeasureString textUpToCursor

        let timeFocused = (time - cs.FocusedTime)
        let blinkProgress = MathF.Pow(float32 (timeFocused.TotalSeconds % blinkInterval.TotalSeconds), 5f)

        let cursorColor = styleSheet.GetColor "Cursor" "default" |> toColor
        let color = Color.Lerp(cursorColor, Color.Transparent, float32 blinkProgress)


        let drawables = styleSheet.GetDrawables "Cursor" "default"

        let position = Vector2(float32 bounds.X + size.X, float32 bounds.Y)

        let cursorWidth = styleSheet.GetWidth "Cursor" "default"
        if cursorWidth = 0f then failwith "Cursor:defaul width is 0"
        let size = Vector2(cursorWidth, float32 font.LineSpacing)
        drawDrawable textureAtlas spriteBatch position size color drawables

    let private drawImage (content: ContentManager) (_settings: NoobishSettings) (spriteBatch: SpriteBatch) (c: NoobishLayoutElement) (t:NoobishTexture) (scrollX: float32) (scrollY: float32) =
        match t.Texture with
        | NoobishTextureId.Basic(textureId) ->
            let texture = content.Load<Texture2D> textureId


            let textureEffect = getTextureEfffect t.TextureEffect
            let sourceRect = Rectangle(0, 0, texture.Width, texture.Height)
            let rect = calculateBounds t.TextureSize c.ContentWithPadding texture.Width texture.Height scrollX scrollY

            let origin = Vector2(float32 sourceRect.Width / 2.0f, float32 sourceRect.Height / 2.0f)
            let rotation = toRadians t.Rotation
            let textureColor = toColor (if c.Enabled then t.TextureColor else t.TextureColorDisabled)
            spriteBatch.Draw(texture, Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width, rect.Height), sourceRect, textureColor, rotation, origin, textureEffect, 0.0f)


        | NoobishTextureId.Atlas(aid, tid) ->

            let atlas = content.Load<TextureAtlas> aid
            let texture = atlas.[tid]

            let textureEffect = getTextureEfffect t.TextureEffect
            let sourceRect = Rectangle(0, 0, texture.Width, texture.Height)
            let rect = calculateBounds t.TextureSize c.ContentWithPadding texture.Width texture.Height scrollX scrollY

            let origin = Vector2(float32 sourceRect.Width / 2.0f, float32 sourceRect.Height / 2.0f)
            let rotation = toRadians t.Rotation
            let textureColor = toColor (if c.Enabled then t.TextureColor else t.TextureColorDisabled)

            spriteBatch.Draw(texture.Atlas, Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width, rect.Height), sourceRect, textureColor, rotation, origin, textureEffect, 0.0f)

        | NoobishTextureId.NinePatch (aid, tid) ->

            let atlas = content.Load<TextureAtlas> aid
            let texture = atlas.[tid]

            let textureEffect = getTextureEfffect t.TextureEffect
            let sourceRect = Rectangle(0, 0, texture.Width, texture.Height)
            let rect = c.ContentWithPadding
            let textureColor = toColor (if c.Enabled then t.TextureColor else t.TextureColorDisabled)
            spriteBatch.DrawAtlasNinePatch(
                texture,
                Vector2(float32 rect.X, float32 rect.Y),
                float32 rect.Width,
                float32 rect.Height,
                textureColor,
                0f,
                Vector2.One,
                textureEffect,
                0f )

        | NoobishTextureId.None -> failwith "Can't have empty texture at this point."

    let rec private drawComponent
        (styleSheet: NoobishStyleSheet)
        (state: NoobishState)
        (content: ContentManager)
        (settings: NoobishSettings)
        (graphics: GraphicsDevice)
        (spriteBatch: SpriteBatch)
        (debug: bool)
        (time: TimeSpan)
        (c: NoobishLayoutElement)
        (parentScrollX: float32)
        (parentScrollY: float32)
        (parentRectangle: Rectangle)  =


        let cs = state.[c.Id]

        let totalScrollX = cs.ScrollX + parentScrollX
        let totalScrollY = cs.ScrollY + parentScrollY

        let outerRectangle =
            let bounds = c.ContentWithPadding
            let startX = bounds.X + totalScrollX
            let startY = bounds.Y + totalScrollY
            let sourceStartX = max startX (float32 parentRectangle.X)
            let sourceStartY = max startY (float32 parentRectangle.Y)
            let sourceWidth = min (bounds.Width) (float32 parentRectangle.Right - startX)
            let sourceHeight = min (bounds.Height) (float32 parentRectangle.Bottom - startY)
            createRectangle
                sourceStartX
                sourceStartY
                (min sourceWidth (float32 parentRectangle.Width))
                (min sourceHeight (float32 parentRectangle.Height))


        let oldScissorRect = graphics.ScissorRectangle

        let rasterizerState = new RasterizerState()
        rasterizerState.ScissorTestEnable <- true
        graphics.ScissorRectangle <- outerRectangle
        spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)


        let textureAtlas = content.Load<TextureAtlas> styleSheet.TextureAtlasId
        match cs.Model with
        | Some(model) ->
            match model with
            | Slider (s) -> drawSlider styleSheet textureAtlas spriteBatch c s time totalScrollX totalScrollX
            | Combobox (_c) ->
                drawBackground styleSheet state textureAtlas spriteBatch c time totalScrollX totalScrollY
            | Textbox (t) ->
                drawBackground styleSheet state textureAtlas spriteBatch c time totalScrollX totalScrollY
                if cs.Focused then
                    drawCursor styleSheet content textureAtlas spriteBatch c cs t time
        | None ->
            drawBackground styleSheet state textureAtlas spriteBatch c time totalScrollX totalScrollY
        match c.Texture with
        | Some (texture) ->
            drawImage content settings spriteBatch c texture totalScrollX totalScrollY
        | None -> ()
        drawText styleSheet content spriteBatch c cs totalScrollX totalScrollY
        drawScrollBars styleSheet state textureAtlas spriteBatch c time totalScrollX totalScrollY

        if debug then
            let childRect = c.Content

            let debugColor =
                if c.ThemeId = "Scroll" then Color.Multiply(Color.Red, 0.1f)
                elif c.ThemeId = "Button" then Color.Multiply(Color.Green, 0.1f)
                elif c.ThemeId = "Paragraph" then Color.Multiply(Color.Purple, 0.1f)
                else Color.Multiply(Color.Yellow, 0.1f)

            let pixel = content.Load<Texture2D> settings.Pixel

            drawRectangle spriteBatch pixel debugColor (childRect.X + totalScrollX) (childRect.Y + totalScrollY) (childRect.Width) (childRect.Height)
            if c.ThemeId = "Scroll" || c.ThemeId = "Slider" then
                let r = {
                    X = float32 outerRectangle.X
                    Y = float32 outerRectangle.Y
                    Width =  float32 outerRectangle.Width
                    Height = float32 outerRectangle.Height}
                debugDrawBorders spriteBatch pixel (Color.Multiply(Color.Red, 0.5f)) r

        spriteBatch.End()

        (*
            Viewport is the visible area. Nothing is rendered outside.
        *)
        match c.Layout with
        | NoobishLayout.Default ->

            let viewport =
                let bounds = c.Content
                let startX = bounds.X + totalScrollX
                let startY = bounds.Y + totalScrollY
                let sourceStartX = max startX (float32 parentRectangle.X)
                let sourceStartY = max startY (float32 parentRectangle.Y)
                let sourceWidth = min (bounds.Width) (float32 parentRectangle.Right - startX)
                let sourceHeight = min (bounds.Height) (float32 parentRectangle.Bottom - startY)
                createRectangle
                    sourceStartX
                    sourceStartY
                    (min sourceWidth (float32 parentRectangle.Width))
                    (min sourceHeight (float32 parentRectangle.Height))
            for child in c.Children do
                let cs = state.[child.Id]
                if cs.Visible then
                    drawComponent styleSheet state content settings graphics spriteBatch debug time child totalScrollX totalScrollY viewport

        | NoobishLayout.Grid(_cols, _rows) ->
            for c in c.Children do
                let cs = state.[c.Id]
                if cs.Visible then
                    let viewport =
                        let bounds = c.ContentWithPadding
                        createRectangle
                            bounds.X
                            bounds.Y
                            bounds.Width
                            bounds.Height
                    drawComponent styleSheet state content settings graphics spriteBatch debug time c totalScrollX totalScrollY viewport
        | NoobishLayout.Absolute | NoobishLayout.OverlaySource ->
            let viewport = Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height)
            for child in c.Children do
                let cs = state.[child.Id]
                if cs.Visible then
                    drawComponent styleSheet state content settings graphics spriteBatch debug time child totalScrollX totalScrollY viewport
        | NoobishLayout.None -> ()



        graphics.ScissorRectangle <- oldScissorRect

    let private fpsTimer = TimeSpan.FromSeconds(0.1)

    let private drawFps (content: ContentManager) (spriteBatch: SpriteBatch) (ui: NoobishUI) (time:TimeSpan) =

        let pixel = content.Load<Texture2D> ui.Settings.Pixel
        ui.FPSCounter <- ui.FPSCounter + 1

        let font = content.Load<SpriteFont> ui.Settings.FontSettings.Normal
        spriteBatch.Begin(samplerState = SamplerState.PointClamp)

        let (areaWidth, areaHeight) = ui.MeasureText ui.Settings.FontSettings.Normal "255"

        let fpsText = (sprintf "%i" (ui.FPS * 10))
        let (fpsWidth, fpsHeight) = ui.MeasureText ui.Settings.FontSettings.Normal fpsText
        let textX = areaWidth - fpsWidth
        let textY = areaHeight - fpsHeight

        drawRectangle spriteBatch pixel (Color.Multiply(Color.DarkRed, 0.5f)) 0.0f 0.0f (float32 areaWidth + 10.0f) (float32 areaHeight + 10.0f)
        spriteBatch.DrawString (font, fpsText, Vector2(float32 textX + 5.0f, float32 textY + 5.0f), Color.White)
        spriteBatch.End()

        if time - ui.FPSTime >= fpsTimer then
            ui.FPS <- ui.FPSCounter
            ui.FPSCounter <- 0
            ui.FPSTime <- time

    let draw (content: ContentManager) (graphics: GraphicsDevice) (spriteBatch: SpriteBatch) (ui: NoobishUI)  (time: TimeSpan) =

        let source = Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height)

        for layer in ui.Layers do
            layer |> Array.iter(fun e ->
                let es = ui.State.[e.Id]
                if es.Visible then
                    drawComponent ui.StyleSheet ui.State content ui.Settings graphics spriteBatch ui.Debug time e 0.0f 0.0f source
            )

        if ui.Debug || ui.FPSEnabled then
            drawFps content spriteBatch ui time

    let updateMouse (ui: NoobishUI) (prevState: MouseState) (curState: MouseState) (gameTime: GameTime) =

        ui.State.TempElements.Clear()
        for kvp in ui.State.ElementsById do
            ui.State.TempElements.Add(kvp.Key, kvp.Value)

        let mousePosition = curState.Position

        if curState.LeftButton = ButtonState.Pressed then
            let mutable handled = false
            let mutable i = ui.Layers.Length - 1
            while not handled && i >= 0 do
                handled <- Noobish.Input.press ui.Version ui.State ui.Layers.[i] gameTime.TotalGameTime (float32 mousePosition.X) (float32 mousePosition.Y) 0.0f 0.0f
                i <- i - 1
        elif prevState.LeftButton = ButtonState.Pressed && curState.LeftButton = ButtonState.Released then
            let mutable handled = false
            let mutable i = ui.Layers.Length - 1
            while not handled && i >= 0 do
                ui.State.SetFocus "" TimeSpan.Zero
                handled <- Noobish.Input.click ui.Version ui.State ui.Layers.[i] gameTime.TotalGameTime (float32 mousePosition.X) (float32 mousePosition.Y) 0.0f 0.0f
                i <- i - 1

        let scrollWheelValue = curState.ScrollWheelValue - prevState.ScrollWheelValue
        if scrollWheelValue <> 0 then

            let scroll = float32 scrollWheelValue / 2.0f

            let absScroll = abs scroll

            let sign = sign scroll |> float32

            let absScrollAmount = min absScroll (absScroll * float32 gameTime.ElapsedGameTime.TotalSeconds * 10.0f)
            for layer in ui.Layers do
                Noobish.Input.scroll ui.Version ui.State.TempElements layer (float32 mousePosition.X) (float32 mousePosition.Y) ui.Settings.Scale gameTime.TotalGameTime 0.0f (- absScrollAmount * sign) |> ignore

    let updateKeyboard (ui: NoobishUI)  (previous: KeyboardState) (current: KeyboardState) (_gameTime: GameTime) =
        ui.State.TempElements.Clear()

        if ui.State.FocusedElementId.IsSome then
            let cursorDelta =
                if not (current.IsKeyDown Keys.Left) && previous.IsKeyDown Keys.Left then
                    -1
                elif not (current.IsKeyDown Keys.Right) && previous.IsKeyDown Keys.Right then
                    1
                else
                    0

            let cs = ui.State.ElementsById.[ui.State.FocusedElementId.Value]
            let model' =
                cs.Model
                |> Option.map(fun model' ->
                    match model' with
                    | Textbox (model'') ->
                        let cursorPos = clamp (model''.Cursor + cursorDelta) 0 model''.Text.Length
                        Textbox({model'' with Cursor = cursorPos})
                    | _ -> model'
                )
            ui.State.ElementsById.[ui.State.FocusedElementId.Value] <- {cs with Model = model'}


        for kvp in ui.State.ElementsById do
            ui.State.TempElements.Add(kvp.Key, kvp.Value)

        for kvp in ui.State.TempElements do
            let noobishKey = kvp.Value.KeyboardShortcut
            if noobishKey <> NoobishKeyId.None then
                let key =
                    match noobishKey with
                    | NoobishKeyId.Enter -> Keys.Enter
                    | NoobishKeyId.Escape -> Keys.Escape
                    | NoobishKeyId.Space -> Keys.Space
                    | NoobishKeyId.None -> failwith "None can't be here."

                let c = ui.Components.[kvp.Key]
                if kvp.Value.Version = ui.Version && c.Enabled && not (current.IsKeyDown key) && (previous.IsKeyDown key) then
                    c.OnClickInternal c

        ui.State.TempElements.Clear()

    let keyTyped (ui: NoobishUI) (char: char) =
        ui.State.TempElements.Clear()
        for kvp in ui.State.ElementsById do
            ui.State.TempElements.Add(kvp.Key, kvp.Value)
        let mutable i = 0
        let mutable handled = false

        while not handled && i >= 0 do
            handled <- Noobish.Input.keyTyped ui.Version ui.State ui.Layers.[i] char
            i <- i - 1

        ui.State.TempElements.Clear()

    let updateMobile (ui: NoobishUI) (_prevState: TouchCollection) (curState: TouchCollection) (gameTime: GameTime) =

        for kvp in ui.State.ElementsById do
            ui.State.TempElements.Add(kvp.Key, kvp.Value)

        for touch in curState  do
            match touch.State with
            | TouchLocationState.Pressed ->
                let mutable handled = false
                let mutable i = ui.Layers.Length - 1
                let mousePosition = touch.Position
                while not handled && i >= 0 do
                    handled <- Noobish.Input.press ui.Version ui.State ui.Layers.[i] gameTime.TotalGameTime mousePosition.X mousePosition.Y 0.0f 0.0f

                    i <- i - 1
            | TouchLocationState.Released ->
                let mutable handled = false
                let mutable i = ui.Layers.Length - 1
                let mousePosition = touch.Position
                while not handled && i >= 0 do
                    handled <- Noobish.Input.click ui.Version ui.State ui.Layers.[i] gameTime.TotalGameTime mousePosition.X mousePosition.Y 0.0f 0.0f
                    i <- i - 1
            | _ -> ()

        ui.State.TempElements.Clear()

module Program =
    let rec private getElements (elements: Dictionary<string, NoobishLayoutElement>) (overlays: ResizeArray<NoobishLayoutElement>) (e: NoobishLayoutElement) =
        elements.[e.Id] <- e

        if e.Overlay then
            overlays.Add e

        for e2 in e.Children do
            getElements elements overlays e2

    let withNoobishRenderer (ui: NoobishUI) (program: Program<_,_,_,_>) =
        let setState model dispatch =
            let layers: list<list<NoobishElement>> = Program.view program model dispatch
            let width = (float32 ui.Width)
            let height = (float32 ui.Height)

            ui.Version <- Guid.NewGuid()

            ui.Layers <- layers |> List.mapi (fun i components -> Logic.layout ui.MeasureText ui.StyleSheet ui.Settings ui.State.Update (i + 1) width height components) |> List.toArray


            ui.Components.Clear()

            let overlays = ResizeArray<NoobishLayoutElement>()

            for layer in ui.Layers do
                for e in layer do
                    getElements ui.Components overlays e

            ui.Layers <- Array.concat [ui.Layers; [| overlays.ToArray() |]]

            for kvp in ui.Components do
                let (success, value) = ui.State.ElementsById.TryGetValue kvp.Key

                if success then
                    ui.State.ElementsById.[kvp.Key] <- { value with Version = ui.Version; Model = kvp.Value.Model; Toggled = kvp.Value.Toggled }
                else
                    ui.State.ElementsById.[kvp.Key] <- Logic.createNoobishLayoutElementState ui.Version kvp.Value


        program
            |> Program.withSetState setState