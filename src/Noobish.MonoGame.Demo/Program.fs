﻿// Learn more about F# at http://fsharp.org

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

open Elmish
open Noobish
open Noobish.Components
open Noobish.NoobishMonoGame

let loremIpsum1 =
    "Scroll me!\n\n Lorem\nipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."

let loremIpsum2 =
    "Part 2\n Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?"


type DemoMessage =
    | ShowContainers
    | ShowButtons
    | ShowLabels

type ViewState = | Containers | Buttons | Labels

type DemoModel =
    {
        State: ViewState
    }

module Labels =

    let view _dispatch =

        [
            grid 2 2
                [
                panelWithGrid 3 3
                    [
                        label [text "Top Left"; textAlign TopLeft; fill]
                        label [text "Top"; textAlign TopCenter; fill]
                        label [text "Top Right"; textAlign TopRight; fill]
                        label [text "Left"; textAlign Left; fill]
                        label [text "Center"; textAlign Center; fill]
                        label [text "Right"; textAlign Right; fill]
                        label [text "Bottom Left"; textAlign BottomLeft; fill]
                        label [text "Bottom Center"; textAlign BottomCenter; fill]
                        label [text "Bottom Right"; textAlign BottomRight; fill]
                    ]
                    [

                    ]
                panel
                    [
                        scroll
                            [
                                paragraph [text (loremIpsum1); block]
                                paragraph [text (loremIpsum2); block]
                            ]
                            [
                                scrollVertical
                            ]
                    ]
                    [

                    ]
                ]
                [

                ]

        ]

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

            int (ceil (size.X)), int (ceil (size.Y))

        nui <- NoobishMonoGame.create measureText "AnonymousPro" this.GraphicsDevice.Viewport.Width this.GraphicsDevice.Viewport.Height 1.0f
            |> NoobishMonoGame.withDebug true

        let init () =
            { State = Buttons}, Cmd.ofMsg (ShowButtons)

        let update (message: DemoMessage) (model: DemoModel) =
            match message with
            | ShowButtons ->
                {model with State = Buttons}, Cmd.none
            | ShowContainers ->
                {model with State = Containers}, Cmd.none
            | ShowLabels ->
                {model with State = Labels}, Cmd.none

        let view (model: DemoModel) dispatch =


            let scrollItems =
                [
                    button [text "Buttons"; onClick (fun () -> dispatch ShowButtons); fillHorizontal; toggled (model.State = Buttons); block]
                    button [text "Labels"; onClick (fun () -> dispatch ShowLabels); fillHorizontal; toggled (model.State = Labels);block]
                    button [text "Containers"; onClick (fun () -> dispatch ShowContainers); fillHorizontal; toggled (model.State = Containers); block]
                ]
            let title, content  =
                match model.State with
                | Buttons -> "Buttons", []
                | Containers -> "Containers", []
                | Labels -> "Labels", Labels.view dispatch



            [
                grid 12 8
                    [
                        panel [label [text "Noobish"]] [colspan 3; rowspan 1]
                        panel [label [text title]] [colspan 9; rowspan 1]
                        panel [scroll scrollItems []] [colspan 3; rowspan 7]
                        panel content [colspan 9; rowspan 7]
                    ]
                    [
                        padding 10
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