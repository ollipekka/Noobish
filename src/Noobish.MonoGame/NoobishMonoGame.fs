namespace Noobish

open System
open System.Collections.Generic

open Elmish
open Noobish
open Noobish.Components

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

open Microsoft.Xna.Framework.Content



type NoobishUI = {
    MeasureText: string -> string -> int*int
    Width: int
    Height: int
    Theme: Theme
    Settings: NoobishSettings
    State: Dictionary<string, LayoutComponentState>

    mutable Debug: bool
    mutable FPSEnabled: bool
    mutable FPS: int
    mutable FPSCounter: int
    mutable FPSTime: TimeSpan
    // Contains layers of layout components. Bottom is 0, one above bototm is 1.
    mutable Layers: LayoutComponent[][]

}

[<RequireQualifiedAccess>]
module NoobishMonoGame =

    let create (content: ContentManager) width height (settings: NoobishSettings) =
        let measureText (font: string) (text: string) =
            let font = content.Load<SpriteFont> font
            let size = font.MeasureString text

            int (ceil (size.X)), int (ceil (size.Y))

        {
            Debug = false
            MeasureText = measureText
            Width = width
            Height = height
            Theme = Theme.createDefaultTheme settings.DefaultFont
            Settings = settings
            State = Dictionary<string, LayoutComponentState>()

            Layers = [||]
            FPSEnabled = false
            FPS = 0
            FPSCounter = 0
            FPSTime = TimeSpan.Zero
        }

    let withTheme (theme: Theme) (ui: NoobishUI): NoobishUI =
        {ui with Theme = theme}

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

    let private drawBackground (state: IReadOnlyDictionary<string, LayoutComponentState>) (content: ContentManager) (settings: NoobishSettings) (spriteBatch: SpriteBatch) (c: LayoutComponent) (time: TimeSpan) scrollX scrollY =
        let cs = state.[c.Id]
        let pixel = content.Load<Texture2D> settings.Pixel

        let bounds = c.RectangleWithMargin

        let color =
            if not c.Enabled then
                toColor c.ColorDisabled
            elif c.Toggled then
                toColor c.PressedColor
            else
                match cs.State with
                | ComponentState.Normal ->
                    let progress = 1.0 - min ((time - cs.PressedTime).TotalSeconds / 0.2) 1.0
                    let color = toColor c.Color
                    let pressedColor = toColor c.PressedColor
                    Color.Lerp(color, pressedColor, float32 progress)
                | ComponentState.Toggled ->
                    toColor c.PressedColor


        drawRectangle spriteBatch pixel color (bounds.X + scrollX) (bounds.Y + scrollY) bounds.Width bounds.Height

    let private drawBorders  (content: ContentManager) (settings: NoobishSettings) (spriteBatch: SpriteBatch) (c: LayoutComponent) scrollX scrollY =
        if c.BorderSize > 0.0f then
            let pixel = content.Load<Texture2D> settings.Pixel
            let bounds = c.RectangleWithMargin

            let scrolledStartY = bounds.Y + scrollY

            let widthWithoutBorders = bounds.Width - c.BorderSize * 2.0f

            let borderColor = toColor (if c.Enabled then c.BorderColor else c.BorderColorDisabled)
            let borderSize = c.BorderSize

            //Left
            drawRectangle spriteBatch pixel borderColor (bounds.X + scrollX) scrolledStartY borderSize bounds.Height
            // Right
            drawRectangle spriteBatch pixel borderColor (bounds.X + bounds.Width - borderSize) scrolledStartY borderSize bounds.Height
            // Top
            drawRectangle spriteBatch pixel borderColor (bounds.X + borderSize) scrolledStartY widthWithoutBorders borderSize
            // Bottom
            drawRectangle spriteBatch pixel borderColor (bounds.X + borderSize) ( scrolledStartY + bounds.Height - borderSize) widthWithoutBorders borderSize


    let private drawText (content: ContentManager) (spriteBatch: SpriteBatch) (c: LayoutComponent) scrollX scrollY =
        let mutable startY = 0.0f

        for line in c.Text do
            let bounds = c.RectangleWithPadding

            let font = content.Load<SpriteFont> c.TextFont

            let size = font.MeasureString (line)

            let textSizeX, textSizeY = ceil (size.X), ceil (size.Y)

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
                | TopLeft -> leftX(), topY()
                | TopCenter -> centerX(), topY()
                | TopRight -> rightX(), topY()
                | Left -> leftX(), centerY()
                | Center -> centerX(), centerY()
                | Right -> rightX(), centerY()
                | BottomLeft -> leftX(), bottomY()
                | BottomCenter -> centerX(), bottomY()
                | BottomRight -> rightX(), bottomY()

            let textColor = toColor (if c.Enabled then c.TextColor else c.TextColorDisabled)
            spriteBatch.DrawString(font, line, Vector2(floor textX, floor (startY + textY)), textColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f)
            startY <- startY + float32 font.LineSpacing

    let private drawScrollBars
        (state: IReadOnlyDictionary<string, LayoutComponentState>)
        (content: ContentManager)
        (settings: NoobishSettings)
        (spriteBatch: SpriteBatch)
        (c: LayoutComponent)
        time
        _scrollX
        _scrollY =

        let cs = state.[c.Id]

        let delta = float32 (min (time - cs.ScrolledTime).TotalSeconds 0.3)
        let progress = 1.0f - delta / 0.3f

        if c.ScrollVertical && progress > 0.0f then

            let pixel = content.Load<Texture2D> settings.Pixel

            let scrollBarWidth = c.ScrollBarThickness
            let bounds = c.RectangleWithMargin
            let x = bounds.X + bounds.Width - c.BorderSize - scrollBarWidth
            let color = Color.Multiply(c.ScrollBarColor |> toColor, progress)
            drawRectangle spriteBatch pixel color x bounds.Y scrollBarWidth bounds.Height

            let pinPosition =  - ( cs.ScrollY / c.OverflowHeight) * bounds.Height
            let pinHeight = ( c.Height / c.OverflowHeight) * bounds.Height
            let color = Color.Multiply(c.ScrollPinColor |> toColor, progress)

            drawRectangle spriteBatch pixel color x (bounds.Y + pinPosition) scrollBarWidth pinHeight

    let private drawSlider
        (content: ContentManager)
        (settings: NoobishSettings)
        (spriteBatch: SpriteBatch)
        (c: LayoutComponent)
        (slider: Slider)
        (_time: TimeSpan)
        _scrollX
        _scrollY =

        let pinWidth = 25.0f
        let pinHeight = c.ScrollPinThickness
        let barHeight = c.ScrollBarThickness

        let pixel = content.Load<Texture2D> settings.Pixel

        // Bar
        let bounds = c.RectangleWithPadding
        let barPositionX = bounds.X + pinWidth / 2.0f
        let barPositionY = bounds.Y + (bounds.Height / 2.0f) - (barHeight / 2.0f)
        let barWidth = bounds.Width - pinWidth
        let color = c.ScrollBarColor |> toColor

        drawRectangle spriteBatch pixel color barPositionX barPositionY barWidth barHeight

        // Pin
        let relativePosition = (slider.Value - slider.Min) / (slider.Max - slider.Min)

        let pinPositionX = bounds.X + (barWidth * relativePosition) - (pinWidth / 2.0f) + (pinWidth / 2.0f)
        let pinPositionY = bounds.Y + (bounds.Height / 2.0f) - (pinHeight / 2.0f)

        let color = c.ScrollPinColor |> toColor
        drawRectangle spriteBatch pixel color pinPositionX pinPositionY pinWidth pinHeight

    let private drawImage (content: ContentManager) (_settings: NoobishSettings) (spriteBatch: SpriteBatch) (c: LayoutComponent) (t:Noobish.Texture) scrollX scrollY =


        let createRectangle (x: float32, y:float32, width: float32, height: float32) =
            Rectangle (int (floor x), int (floor y), int (ceil width), int (ceil height))


        let texture, sourceRect =
            match t.Texture with
            | NoobishTexture.Basic(textureId) ->
                let texture = content.Load<Texture2D> textureId
                (texture, Rectangle(0, 0, texture.Width, texture.Height) )
            | NoobishTexture.Atlas(textureId, sx, sy, sw, sh) ->
                let texture = content.Load<Texture2D> textureId
                (texture, Rectangle(sx, sy, sw, sh) )
            | NoobishTexture.NinePatch _ -> failwith "Not implemented"
            | NoobishTexture.None -> failwith "Can't have empty texture at this point."

        let rect =
            match t.TextureSize with
            | NoobishTextureSize.Stretch ->
                let bounds = c.RectangleWithMargin
                createRectangle(bounds.X + scrollX, bounds.Y + scrollY, bounds.Width, bounds.Height)
            | NoobishTextureSize.BestFitMax ->
                let bounds = c.RectangleWithMargin
                let ratio = max (float32 bounds.Width / float32 sourceRect.Width) (float32 bounds.Height / float32 sourceRect.Height)
                let width = ratio * float32 sourceRect.Width
                let height = ratio * float32 sourceRect.Height
                let padLeft = (bounds.Width - width) / 2.0f
                let padTop = (bounds.Height - height) / 2.0f
                createRectangle(bounds.X + scrollX + padLeft, bounds.Y + scrollY + padTop, width, height)
            | NoobishTextureSize.BestFitMin ->
                let bounds = c.RectangleWithMargin
                let ratio = min (float32 bounds.Width / float32 sourceRect.Width) (float32 bounds.Height / float32 sourceRect.Height)
                let width = ratio * float32 sourceRect.Width
                let height = ratio * float32 sourceRect.Height
                let padLeft = (bounds.Width - width) / 2.0f
                let padTop = (bounds.Height - height) / 2.0f
                createRectangle(bounds.X + scrollX + padLeft, bounds.Y + scrollY + padTop, width, height)
            | NoobishTextureSize.Original ->
                let bounds = c.RectangleWithMargin
                createRectangle(bounds.X + scrollX, bounds.Y + scrollY, bounds.Width, bounds.Height)

        let textureEffect =
            if t.TextureEffect = NoobishTextureEffect.FlipHorizontally then
                SpriteEffects.FlipHorizontally
            else if t.TextureEffect = NoobishTextureEffect.FlipVertically then
                SpriteEffects.FlipVertically
            else
                SpriteEffects.None


        let origin = Vector2(float32 sourceRect.Width / 2.0f, float32 sourceRect.Height / 2.0f)
        let rotation = Utils.toRadians t.Rotation
        let textureColor = toColor (if c.Enabled then t.TextureColor else t.TextureColorDisabled)
        spriteBatch.Draw(texture, Rectangle(rect.X + rect.Width / 2, rect.Y + rect.Height / 2, rect.Width, rect.Height), sourceRect, textureColor, rotation, origin, textureEffect, 0.0f)


    let rec private drawComponent
        (state: IReadOnlyDictionary<string, LayoutComponentState>)
        (content: ContentManager)
        (settings: NoobishSettings)
        (graphics: GraphicsDevice)
        (spriteBatch: SpriteBatch)
        (debug: bool)
        (time: TimeSpan)
        (c: LayoutComponent)
        (parentScrollX: float32)
        (parentScrollY: float32)
        (parentRectangle: Rectangle)  =


        let createRectangle (x: float32, y:float32, width: float32, height: float32) =
            Rectangle (int (floor x), int (floor y), int (ceil width), int (ceil height))

        let cs = state.[c.Id]

        let totalScrollX = cs.ScrollX + parentScrollX
        let totalScrollY = cs.ScrollY + parentScrollY

        let bounds = c.RectangleWithMargin

        let startX = bounds.X + totalScrollX
        let startY = bounds.Y + totalScrollY

        let sourceStartX = max startX (float32 parentRectangle.X)
        let sourceStartY = max startY (float32 parentRectangle.Y)
        let sourceEndX = min (bounds.Width) (float32 parentRectangle.Right - startX)
        let sourceEndY = min (bounds.Height) (float32 parentRectangle.Bottom - startY)

        let outerRectangle =
            createRectangle(
                sourceStartX,
                sourceStartY,
                min (float32 parentRectangle.Width) sourceEndX,
                min (float32 parentRectangle.Height) sourceEndY )

        let oldScissorRect = graphics.ScissorRectangle

        let rasterizerState = new RasterizerState()
        rasterizerState.ScissorTestEnable <- true
        graphics.ScissorRectangle <- outerRectangle
        spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)

        drawBackground state content settings spriteBatch c time totalScrollX totalScrollY
        match c.Texture with
        | Some (texture) ->
            drawImage content settings spriteBatch c texture totalScrollX totalScrollY
        | None -> ()
        drawBorders content settings spriteBatch c totalScrollX totalScrollY
        drawText content spriteBatch c totalScrollX totalScrollY
        drawScrollBars state content settings spriteBatch c time totalScrollX totalScrollY

        c.Slider
            |> Option.iter(
                fun s -> drawSlider content settings spriteBatch c s time totalScrollX totalScrollX )

        if debug then
            let childRect = c.RectangleWithPadding

            let debugColor =
                if c.ThemeId = "Scroll" then Color.Multiply(Color.Transparent, 0.1f)
                elif c.ThemeId = "Button" then Color.Multiply(Color.Green, 0.1f)
                else Color.Multiply(Color.Yellow, 0.1f)

            let pixel = content.Load<Texture2D> settings.Pixel
            drawRectangle spriteBatch pixel debugColor (childRect.X + totalScrollX) (childRect.Y + totalScrollY) (childRect.Width) (childRect.Height)

        spriteBatch.End()

        let innerRectangle =
            createRectangle (
                float32 outerRectangle.X + c.PaddingLeft,
                float32 outerRectangle.Y + c.PaddingTop,
                c.PaddingLeft + c.PaddedWidth,
                c.PaddingTop + c.PaddedHeight)

        c.Children |> Array.iter(fun c ->
            drawComponent state content settings graphics spriteBatch debug time c totalScrollX totalScrollY innerRectangle
        )

        graphics.ScissorRectangle <- oldScissorRect

    let private fpsTimer = TimeSpan.FromSeconds(0.1)

    let private drawFps (content: ContentManager) (spriteBatch: SpriteBatch) (ui: NoobishUI) (time:TimeSpan) =

        let pixel = content.Load<Texture2D> ui.Settings.Pixel
        ui.FPSCounter <- ui.FPSCounter + 1

        let fontId = (sprintf "%s%s" ui.Settings.FontPrefix ui.Settings.DefaultFont)
        let font = content.Load<SpriteFont> fontId
        spriteBatch.Begin(samplerState = SamplerState.PointClamp)

        let (areaWidth, areaHeight) = ui.MeasureText fontId "255"

        let fpsText = (sprintf "%i" (ui.FPS * 10))
        let (fpsWidth, fpsHeight) = ui.MeasureText fontId fpsText
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

        for layer in ui.Layers do
            layer |> Array.iter(fun c ->
                let source = Rectangle(0, 0, graphics.Viewport.Width, graphics.Viewport.Height)
                drawComponent ui.State content ui.Settings graphics spriteBatch ui.Debug time c 0.0f 0.0f source
            )

        if ui.Debug || ui.FPSEnabled then
            drawFps content spriteBatch ui time

    let updateDesktop (ui: NoobishUI) (prevState: MouseState) (curState: MouseState) (gameTime: GameTime) =
        let mousePosition = curState.Position
        if curState.LeftButton = ButtonState.Pressed then
            let mutable handled = false
            let mutable i = ui.Layers.Length - 1
            while not handled && i >= 0 do
                handled <- Input.press ui.State ui.Layers.[i] gameTime.TotalGameTime (float32 mousePosition.X) (float32 mousePosition.Y) 0.0f 0.0f
                i <- i - 1
        elif prevState.LeftButton = ButtonState.Pressed && curState.LeftButton = ButtonState.Released then
            let mutable handled = false
            let mutable i = ui.Layers.Length - 1
            while not handled && i >= 0 do
                handled <- Input.click ui.State ui.Layers.[i] gameTime.TotalGameTime (float32 mousePosition.X) (float32 mousePosition.Y) 0.0f 0.0f
                i <- i - 1

        let scrollWheelValue = curState.ScrollWheelValue - prevState.ScrollWheelValue
        if scrollWheelValue <> 0 then

            let scroll = float32 scrollWheelValue / 2.0f

            let absScroll = abs scroll

            let sign = sign scroll |> float32

            let absScrollAmount = min absScroll (absScroll * float32 gameTime.ElapsedGameTime.TotalSeconds * 10.0f)
            for layer in ui.Layers do
                Input.scroll ui.State layer (float32 mousePosition.X) (float32 mousePosition.Y) ui.Settings.Scale gameTime.TotalGameTime 0.0f (- absScrollAmount * sign) |> ignore



    let updateMobile (ui: NoobishUI) (_prevState: TouchCollection) (curState: TouchCollection) (gameTime: GameTime) =
        for touch in curState  do
            match touch.State with
            | TouchLocationState.Pressed ->
                let mutable handled = false
                let mutable i = ui.Layers.Length - 1
                let mousePosition = touch.Position
                while not handled && i >= 0 do
                    handled <- Input.press ui.State ui.Layers.[i] gameTime.TotalGameTime mousePosition.X mousePosition.Y 0.0f 0.0f

                    i <- i - 1
            | TouchLocationState.Released ->
                let mutable handled = false
                let mutable i = ui.Layers.Length - 1
                let mousePosition = touch.Position
                while not handled && i >= 0 do
                    handled <- Input.click ui.State ui.Layers.[i] gameTime.TotalGameTime mousePosition.X mousePosition.Y 0.0f 0.0f
                    i <- i - 1
            | _ -> ()


module Program =
    let rec private getComponentIds (c: LayoutComponent) =
        let childIds = c.Children |> Array.collect getComponentIds
        Array.append [|c.Id|] childIds

    let withNoobishRenderer (ui: NoobishUI) (program: Program<_,_,_,_>) =
        let setState model dispatch =

            let layers: list<list<Component>> = Program.view program model dispatch
            let width = (float32 ui.Width)
            let height = (float32 ui.Height)

            ui.Layers <- layers |> List.map (Logic.layout ui.MeasureText ui.Theme ui.Settings width height) |> List.toArray

            let oldState = Dictionary(ui.State)
            ui.State.Clear()
            let newComponents =
                ui.Layers
                |> Array.collect (fun l -> l |> Array.collect getComponentIds)
                |> Set.ofArray

            for cid in newComponents do
                let (success, value) = oldState.TryGetValue cid
                if success then
                    ui.State.[cid] <- value
                else
                    ui.State.[cid] <- Logic.createLayoutComponentState()

        program
            |> Program.withSetState setState