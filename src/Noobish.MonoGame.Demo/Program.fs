// Learn more about F# at http://fsharp.org

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

open Elmish
open Noobish
open Noobish.Components
open Noobish.NoobishMonoGame


type DemoMessage =
    | ShowContainers
    | ShowButtons
    | ShowLabels

type DemoModel =
    {
        Text: string
    }

let createGraphicsDevice (game: Game) =
    let graphics = new GraphicsDeviceManager(game)
    graphics.GraphicsProfile <- GraphicsProfile.HiDef
    #if !__MOBILE__
    graphics.PreferredBackBufferWidth <- 1280
    graphics.PreferredBackBufferHeight <- 720
    #endif
    graphics.PreferMultiSampling <- true

    graphics.SupportedOrientations <-
        DisplayOrientation.LandscapeLeft ||| DisplayOrientation.LandscapeRight;
    graphics.ApplyChanges()

type DemoGame () as game =
    inherit Game()

    do game.IsMouseVisible <- true

    let _graphics = createGraphicsDevice game

    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable nui = Unchecked.defaultof<NoobishUI>

    let mutable previousKeyboardState = Unchecked.defaultof<KeyboardState>
    let mutable keyboardState = Unchecked.defaultof<KeyboardState>

    let mutable previousMouseState= Unchecked.defaultof<MouseState>
    let mutable mouseState = Unchecked.defaultof<MouseState>

    let mutable previousTouchState = Unchecked.defaultof<TouchCollection>
    let mutable touchState = Unchecked.defaultof<TouchCollection>


    let mutable uiRenderTarget1 = Unchecked.defaultof<RenderTarget2D>
    let mutable uiRenderTarget2 = Unchecked.defaultof<RenderTarget2D>

    override this.Initialize() =

        let measureText (font: string) (text: string) =
            let font = game.Content.Load<SpriteFont>(font)
            let size = font.MeasureString text

            int (ceil (size.X)), int (ceil (size.Y * 1.5f))

        nui <- NoobishMonoGame.create measureText "AnonymousPro" this.GraphicsDevice.Viewport.Width this.GraphicsDevice.Viewport.Height 1.0f
            |> NoobishMonoGame.withDebug false

        let init () =
            { Text = ""}, Cmd.ofMsg (ShowButtons)

        let update (message: DemoMessage) (model: DemoModel) =
            match message with
            | ShowButtons ->
                model, Cmd.none
            | ShowContainers ->
                model, Cmd.none
            | ShowLabels ->
                model, Cmd.none

        let view (model: DemoModel) dispatch =

            let scrollItems =
                [
                    button [text "Containers"; onClick (fun () -> dispatch ShowContainers); fillHorizontal; block]
                    button [text "Buttons"; onClick (fun () -> dispatch ShowButtons); fillHorizontal; block]
                    button [text "Labels"; onClick (fun () -> dispatch ShowLabels); fillHorizontal; block]

                ]

            [
                grid 12 8
                    [
                        panel [label [text "Noobish"]] [colspan 12; rowspan 1]
                        panel [scroll scrollItems []] [colspan 3; rowspan 7]
                        panel [] [colspan 9; rowspan 7]
                    ]
                    [

                    ]
            ]

        base.Initialize()
        this.GraphicsDevice.PresentationParameters.RenderTargetUsage <- RenderTargetUsage.PreserveContents
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)


        uiRenderTarget1 <-
            new RenderTarget2D(
                this.GraphicsDevice,
                this.GraphicsDevice.Viewport.Width,
                this.GraphicsDevice.Viewport.Height,
                false, SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.PreserveContents)

        uiRenderTarget2 <-
            new RenderTarget2D(
                this.GraphicsDevice,
                this.GraphicsDevice.Viewport.Width,
                this.GraphicsDevice.Viewport.Height,
                false, SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents)




        Program.mkProgram init update view
            |> Program.withNoobishRenderer nui
            |> Program.run


    override this.LoadContent() =

        ()

    override _this.UnloadContent() =
        uiRenderTarget1.Dispose()
        uiRenderTarget1 <- null
        uiRenderTarget2.Dispose()
        uiRenderTarget2 <- null

    override this.Update gameTime =
        base.Update(gameTime)

        #if __MOBILE__
        NoobishMonoGame.updateMobile nui previousTouchState touchState gameTime
        #else
        NoobishMonoGame.updateDesktop nui previousMouseState mouseState gameTime
        #endif

        previousKeyboardState <- keyboardState
        keyboardState <- Keyboard.GetState()

        previousMouseState <- mouseState
        mouseState <- Mouse.GetState()

        previousTouchState <- touchState
        touchState <- TouchPanel.GetState()


    override this.Draw (gameTime) =
        base.Draw(gameTime)

        NoobishMonoGame.draw game.Content game.GraphicsDevice spriteBatch uiRenderTarget1 uiRenderTarget2 nui gameTime.TotalGameTime




[<EntryPoint>]
let main argv =
    use game = new DemoGame ()

    game.Run()
    0 // return an integer exit code