namespace Noobish

open System
open System.Collections.Generic

open Elmish
open Noobish

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

open Microsoft.Xna.Framework.Content

type NoobishUI = {
    Debug: bool
    MeasureText: string -> string -> int*int
    Width: int
    Height: int
    Scale: float32
    Theme: Theme

    mutable Tree: LayoutComponent[]

    State: Dictionary<string, LayoutComponentState>
}

module NoobishMonoGame =

    let create measureText defaultFont width height scale = {
        Debug = false
        MeasureText = measureText
        Width = width
        Height = height
        Scale = scale
        Tree = [||]
        Theme = Theme.createDefaultTheme defaultFont
        State = Dictionary<string, LayoutComponentState>()
    }

    let withDebug d ui = {
        ui with Debug = d
    }


    let private createRectangle (x: float32, y:float32, width: float32, height: float32) =
        Rectangle (int (floor x), int (floor y), int (ceil width), int (ceil height))

    let private toColor (v: int) =

        let r = (v >>> 24) &&& 255;
        let g = (v >>> 16) &&& 255; // 255
        let b = (v >>> 8) &&& 255; // 122
        let a = v &&& 255; // 15

        Color(r, g, b, a)

    let private drawBackground (state: IReadOnlyDictionary<string, LayoutComponentState>) (content: ContentManager) (spriteBatch: SpriteBatch) (c: LayoutComponent) (time: TimeSpan) scrollX scrollY =
        let cs = state.[c.Id]
        let pixel = content.Load<Texture2D>("Pixel")

        let bounds = c.RectangleWithMargin
        let dest = Rectangle(int (bounds.X + scrollX), int (bounds.Y + scrollY), int bounds.Width, int bounds.Height)

        let color =
            if not c.Enabled then
                toColor c.ColorDisabled
            else
                match cs.State with
                | ComponentState.Normal ->
                    let progress = 1.0 - min ((time - cs.PressedTime).TotalSeconds / 0.2) 1.0
                    let color = toColor c.Color
                    let pressedColor = toColor c.PressedColor
                    Color.Lerp(color, pressedColor, float32 progress)
                | ComponentState.Toggled ->
                    toColor c.PressedColor

        spriteBatch.Draw(pixel, dest, Nullable(), color)

    let private drawBorders  (content: ContentManager) (spriteBatch: SpriteBatch) (c: LayoutComponent) scrollX scrollY =
        if c.BorderSize > 0.0f then
            let pixel = content.Load<Texture2D>("Pixel")
            let bounds = c.RectangleWithMargin

            let scrolledStartY = bounds.Y + scrollY

            let widthWithoutBorders = bounds.Width - c.BorderSize * 2.0f

            let borderColor = toColor (if c.Enabled then c.BorderColor else c.BorderColorDisabled)
            let left = createRectangle(bounds.X + scrollX, scrolledStartY, c.BorderSize, bounds.Height)
            spriteBatch.Draw(pixel, left, Nullable(), borderColor)

            let right = createRectangle(bounds.X + bounds.Width - c.BorderSize, scrolledStartY, c.BorderSize, bounds.Height)
            spriteBatch.Draw(pixel, right, Nullable(), borderColor)

            let bottom = createRectangle(bounds.X + c.BorderSize, scrolledStartY, widthWithoutBorders, c.BorderSize)
            spriteBatch.Draw(pixel, bottom, Nullable(), borderColor)

            let top = createRectangle(bounds.X + c.BorderSize, scrolledStartY + bounds.Height - c.BorderSize, widthWithoutBorders, c.BorderSize)
            spriteBatch.Draw(pixel, top, Nullable(), borderColor)


    let private drawText (content: ContentManager) (spriteBatch: SpriteBatch) (c: LayoutComponent) scrollX scrollY =
        if not (c.Text |> Array.isEmpty) then

            let bounds = c.RectangleWithPadding

            let font = content.Load<SpriteFont>(c.TextFont)

            let mutable startY = 0.0f

            for line in c.Text do
                let size = font.MeasureString (line)
                let textSizeX, textSizeY = ceil (size.X), ceil (size.Y)

                let textX =
                    match c.TextHorizontalAlignment with
                    | NoobishHorizontalTextAlign.Left -> bounds.X + scrollX
                    | NoobishHorizontalTextAlign.Right -> bounds.X + bounds.Width - textSizeX
                    | NoobishHorizontalTextAlign.Center ->  bounds.X + bounds.Width / 2.0f  - textSizeX / 2.0f

                let textY =
                    match c.TextVerticalAlignment with
                    | NoobishVerticalTextAlign.Top ->
                        bounds.Y + scrollY
                    | NoobishVerticalTextAlign.Center ->
                        bounds.Y + scrollY + bounds.Height / 2.0f - textSizeY / 2.0f
                    | NoobishVerticalTextAlign.Bottom ->
                        raise (NotImplementedException("Bottom not implemented! Needs to reverse the draw order?"))

                let textColor = toColor (if c.Enabled then c.TextColor else c.TextColorDisabled)
                spriteBatch.DrawString(font, line, Vector2(floor textX, floor (startY + textY)), textColor, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f)
                startY <- startY + float32 font.LineSpacing
    let private drawScrollBars
        (state: IReadOnlyDictionary<string, LayoutComponentState>)
        (content: ContentManager)
        (spriteBatch: SpriteBatch)
        (c: LayoutComponent)
        time
        _scrollX
        _scrollY =

        let cs = state.[c.Id]

        let delta = float32 (min (time - cs.ScrolledTime).TotalSeconds 0.3)
        let progress = 1.0f - delta / 0.3f

        if c.ScrollVertical && progress > 0.0f then

            let pixel = content.Load<Texture2D>("Pixel")

            let scrollBarWidth = 2.0f
            let bounds = c.RectangleWithMargin

            let right = createRectangle(bounds.X + bounds.Width - c.BorderSize - scrollBarWidth, bounds.Y, scrollBarWidth, bounds.Height)

            let color = Color.Multiply(Color.DarkRed, progress)
            spriteBatch.Draw(pixel, right, Nullable(), color)

            let pinPosition =  - ( cs.ScrollY / c.OverflowHeight) * bounds.Height
            let pinHeight = ( c.Height / c.OverflowHeight) * bounds.Height
            let color = Color.Multiply(Color.Red, progress)

            let right = createRectangle(bounds.X + bounds.Width - c.BorderSize - scrollBarWidth, bounds.Y + pinPosition, scrollBarWidth, pinHeight)
            spriteBatch.Draw(pixel, right, Nullable(), color)


    let private drawImage (content: ContentManager) (spriteBatch: SpriteBatch) (c: LayoutComponent) scrollX scrollY =
        if not (c.Texture |> String.IsNullOrWhiteSpace) then

            let texture = content.Load<Texture2D> c.Texture

            let rect =
                match c.TextureSize with
                | NoobishTextureSize.Stretch ->
                    let bounds = c.RectangleWithPadding
                    createRectangle(bounds.X + scrollX, bounds.Y + scrollY, bounds.Width, bounds.Height)
                | NoobishTextureSize.BestFitMax ->
                    let bounds = c.RectangleWithPadding
                    let ratio = max (float32 bounds.Width / float32 texture.Width) (float32 bounds.Height / float32 texture.Height)
                    let width = ratio * float32 texture.Width
                    let height = ratio * float32 texture.Height
                    let padLeft = (bounds.Width - width) / 2.0f
                    let padTop = (bounds.Height - height) / 2.0f
                    createRectangle(bounds.X + scrollX + padLeft, bounds.Y + scrollY + padTop, width, height)
                | NoobishTextureSize.BestFitMin ->
                    let bounds = c.RectangleWithPadding
                    let ratio = min (float32 bounds.Width / float32 texture.Width) (float32 bounds.Height / float32 texture.Height)
                    let width = ratio * float32 texture.Width
                    let height = ratio * float32 texture.Height
                    let padLeft = (bounds.Width - width) / 2.0f
                    let padTop = (bounds.Height - height) / 2.0f
                    createRectangle(bounds.X + scrollX + padLeft, bounds.Y + scrollY + padTop, width, height)
                | NoobishTextureSize.Original ->
                    let bounds = c.RectangleWithPadding
                    createRectangle(bounds.X + scrollX, bounds.Y + scrollY, float32 texture.Width, float32 texture.Height)
                | NoobishTextureSize.Custom (w, h) ->
                    let bounds = c.RectangleWithPadding
                    createRectangle(bounds.X + scrollX, bounds.Y + scrollY, float32 w, float32 h)


            let textureColor = toColor (if c.Enabled then c.TextureColor else c.TextureColorDisabled)
            spriteBatch.Draw(texture, rect, Nullable(), textureColor)

    let rec private drawComponent
        (state: IReadOnlyDictionary<string, LayoutComponentState>)
        (content: ContentManager)
        (graphics: GraphicsDevice)
        (spriteBatch: SpriteBatch)
        (mainRenderTarget: RenderTarget2D)
        (secondaryRenderTarget: RenderTarget2D)
        (debug: bool)
        (time: TimeSpan)
        (c: LayoutComponent)
        (parentScrollX: float32)
        (parentScrollY: float32)
        (parentRectangle: Rectangle)  =

        let cs = state.[c.Id]

        let totalScrollX = cs.ScrollX + parentScrollX
        let totalScrollY = cs.ScrollY + parentScrollY

        let bounds = c.RectangleWithMargin

        let startX = bounds.X + totalScrollX
        let startY = bounds.Y + totalScrollY



        graphics.SetRenderTarget(secondaryRenderTarget)
        graphics.Clear (Color.TransparentBlack)
        spriteBatch.Begin()

        drawBackground state content spriteBatch c time totalScrollX totalScrollY
        drawBorders content spriteBatch c totalScrollX totalScrollY
        drawImage content spriteBatch c totalScrollX totalScrollY
        drawText content spriteBatch c totalScrollX totalScrollY
        drawScrollBars state content spriteBatch c time totalScrollX totalScrollY

        let sourceStartX = max startX (float32 parentRectangle.X)
        let sourceStartY = max startY (float32 parentRectangle.Y)
        let sourceEndX = min (bounds.Width) (float32 parentRectangle.Right - startX)
        let sourceEndY = min (bounds.Height) (float32 parentRectangle.Bottom - startY)

        let outerRectangle =
            createRectangle(
                sourceStartX,
                sourceStartY,
                sourceEndX,
                sourceEndY )

        if debug then

            let debugColor =
                if c.ThemeId = "Scroll" then Color.Multiply(Color.Red, 0.1f)
                elif c.ThemeId = "Button" then Color.Multiply(Color.Green, 0.1f)
                else Color.Multiply(Color.Yellow, 0.1f)




            let pixel = content.Load<Texture2D>("Pixel")
            spriteBatch.Draw(pixel, outerRectangle, Nullable(outerRectangle), debugColor)

        spriteBatch.End()

        graphics.SetRenderTarget mainRenderTarget
        spriteBatch.Begin()
        spriteBatch.Draw(secondaryRenderTarget, outerRectangle, Nullable(outerRectangle), Color.White)
        spriteBatch.End()

        let innerRectangle =
            createRectangle (
                float32 outerRectangle.X + c.PaddingLeft,
                float32 outerRectangle.Y + c.PaddingTop,
                c.PaddedWidth,
                c.PaddedHeight )

        c.Children |> Array.iter(fun c ->
            drawComponent state content graphics spriteBatch mainRenderTarget secondaryRenderTarget debug time c totalScrollX totalScrollY innerRectangle
        )

    let draw (content: ContentManager) (graphics: GraphicsDevice) (spriteBatch: SpriteBatch) (mainRenderTarget: RenderTarget2D) (secondaryRenderTarget: RenderTarget2D) (ui: NoobishUI)  (time: TimeSpan) =

        graphics.SetRenderTarget(mainRenderTarget)
        graphics.BlendState <- BlendState.AlphaBlend
        graphics.Clear(Color.TransparentBlack)

        graphics.SetRenderTarget(secondaryRenderTarget)
        graphics.BlendState <- BlendState.AlphaBlend
        graphics.Clear(Color.TransparentBlack)

        ui.Tree |> Array.iter(fun c ->
            let source = Rectangle(0, 0, mainRenderTarget.Width, mainRenderTarget.Height)
            drawComponent ui.State content graphics spriteBatch mainRenderTarget secondaryRenderTarget ui.Debug time c 0.0f 0.0f source
        )

        graphics.SetRenderTarget(null)
        graphics.BlendState <- BlendState.AlphaBlend
        spriteBatch.Begin(blendState = BlendState.AlphaBlend)
        let dest = Rectangle(0, 0, mainRenderTarget.Width, mainRenderTarget.Height)
        spriteBatch.Draw(mainRenderTarget, dest, Nullable(), Color.White)
        spriteBatch.End()


    let updateDesktop (ui: NoobishUI) (prevState: MouseState) (curState: MouseState) (gameTime: GameTime) =
        let mousePosition = curState.Position
        if curState.LeftButton = ButtonState.Pressed then
            Input.press ui.State ui.Tree gameTime.TotalGameTime (float32 mousePosition.X) (float32 mousePosition.Y) 0.0f 0.0f
        elif prevState.LeftButton = ButtonState.Pressed && curState.LeftButton = ButtonState.Released then
            Input.click ui.State ui.Tree gameTime.TotalGameTime (float32 mousePosition.X) (float32 mousePosition.Y) 0.0f 0.0f

        let scrollWheelValue = curState.ScrollWheelValue - prevState.ScrollWheelValue
        if scrollWheelValue <> 0 then

            let scroll = float32 scrollWheelValue / 10.0f

            let absScroll = abs scroll

            let sign = sign scroll |> float32

            let absScrollAmount = min absScroll (absScroll * float32 gameTime.ElapsedGameTime.TotalSeconds * 10.0f)

            Input.scroll ui.State ui.Tree (float32 mousePosition.X) (float32 mousePosition.Y) ui.Scale gameTime.TotalGameTime 0.0f (- absScrollAmount * sign) |> ignore



    let updateMobile (ui: NoobishUI) (_prevState: TouchCollection) (curState: TouchCollection) (gameTime: GameTime) =
        for touch in curState  do
            match touch.State with
            | TouchLocationState.Pressed ->
                let mousePosition = touch.Position
                Input.press ui.State ui.Tree gameTime.TotalGameTime mousePosition.X mousePosition.Y 0.0f 0.0f
            | TouchLocationState.Released ->
                let mousePosition = touch.Position
                Input.click ui.State ui.Tree gameTime.TotalGameTime mousePosition.X mousePosition.Y 0.0f 0.0f
            | _ -> ()


module Program =
    open MonoGame
    let rec private getComponentIds (c: LayoutComponent) =
        let childIds = c.Children |> Array.collect getComponentIds
        Array.append [|c.Id|] childIds

    let withNoobishRenderer (ui: NoobishUI) (program: Program<_,_,_,_>) =
        let setState model dispatch =
            let oldComponents =
                ui.Tree
                |> Array.collect getComponentIds
                |> Set.ofArray

            let tree = Program.view program model dispatch

            ui.Tree <- Logic.layout ui.MeasureText ui.Theme ui.Scale (float32 ui.Width) (float32 ui.Height) tree

            let newComponents =
                ui.Tree
                |> Array.collect getComponentIds
                |> Set.ofArray

            let sameComponents = Set.intersect newComponents oldComponents
            let removedComponents = oldComponents - sameComponents
            let newComponents = newComponents - sameComponents

            for cid in newComponents do
                ui.State.[cid] <- Logic.createLayoutComponentState()

            for cid in removedComponents do
                ui.State.Remove cid |> ignore

        program
            |> Program.withSetState setState