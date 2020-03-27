// Learn more about F# at http://fsharp.org

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

open Elmish
open Noobish
open Noobish.Components

let loremIpsum1 =
    "Scroll me!\n\n Lorem\nipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."

let loremIpsum2 =
    "Part 2\n Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?"

type DemoMessage =
    | ShowContainers
    | ShowButtons
    | ShowText
    | ChangePadding of int
    | ToggleDebug

type ViewState = | Containers | Buttons | Text

type DemoModel =
    {
        UI: NoobishUI
        State: ViewState
        Padding: int
    }

module Text =

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
                                paragraph [text loremIpsum1; block]
                                paragraph [text loremIpsum2; block]
                            ]
                            [
                                scrollVertical
                            ]
                    ]
                    [

                    ]
                panelWithGrid 1 3
                    [

                        paragraph [text loremIpsum2; textAlign TopCenter; rowspan 1; name "Paragraph"]
                        scroll
                            [
                                paragraph [text loremIpsum2; textAlign TopCenter; name "Paragraph"]
                            ]
                            [
                            ]
                        scroll
                            [
                                paragraph [text "Could scroll, but won't."]

                            ]
                            [
                                name "DebugScroll"
                                scrollVertical
                                rowspan 1
                            ]
                    ]
                    [
                        name "ParentPanel"
                    ]

                panelWithGrid 2 1
                    [
                        div
                            [
                                label [text "Font size 22"; textFont "AnonymousPro22"; block]
                                label [text "Regular"; textFont "AnonymousPro22"; textColor 0xac3232aa; block]
                                label [text "Bold"; textFont "AnonymousProBold22"; textColor 0x4b692faa; block]
                                label [text "Italic"; textFont "AnonymousProItalic22"; textColor 0x3f3f74aa; block]
                            ]
                            [

                            ]
                        div
                            [
                                label [text "Font size 16"; block]
                                label [text "Regular"; textColor 0xac3232aa; block]
                                label [text "Bold"; textFont "AnonymousProBold16"; textColor 0x4b692faa; block]
                                label [text "Italic"; textFont "AnonymousProItalic16"; textColor 0x3f3f74aa; block]
                            ]
                            [

                            ]

                    ]
                    [

                    ]
                ]
                [

                ]

        ]

module Buttons =
    let view model dispatch =

        [
            grid 2 2
                [
                panel
                    [
                        button [text "Padding 5"; onClick (fun () -> dispatch (ChangePadding 5)); padding model.Padding; fillHorizontal]
                        button [text "Padding 10"; onClick (fun () -> dispatch (ChangePadding 10));  padding model.Padding; fillHorizontal]
                        button [text "Padding 15"; onClick (fun () -> dispatch (ChangePadding 15)); padding model.Padding; fillHorizontal]
                    ]
                    [
                        padding model.Padding;

                    ]
                panel
                    [
                    ]
                    [

                    ]
                panel
                    [
                    ]
                    [

                    ]
                panelWithGrid 2 1
                    [

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

    override this.Initialize() =

        let measureText (font: string) (text: string) =
            let font = game.Content.Load<SpriteFont>(font)
            let size = font.MeasureString text

            int (ceil (size.X)), int (ceil (size.Y))

        let width = this.GraphicsDevice.Viewport.Width
        let height = this.GraphicsDevice.Viewport.Height

        let settings = {
            Scale = 1.0f
            DefaultFont = "AnonymousPro16"
            Pixel = "Pixel"
            FontPrefix = ""
            GraphicsPrefix = ""
        }

        nui <- NoobishMonoGame.create game.Content width height settings
            |> NoobishMonoGame.overrideDebug false

        let init () =
            { UI = nui; State = Buttons; Padding = 5}, Cmd.ofMsg (ShowButtons)

        let update (message: DemoMessage) (model: DemoModel) =
            match message with
            | ShowButtons ->
                {model with State = Buttons}, Cmd.none
            | ShowContainers ->
                {model with State = Containers}, Cmd.none
            | ShowText ->
                {model with State = Text}, Cmd.none
            | ChangePadding padding ->
                {model with Padding = padding}, Cmd.none
            | ToggleDebug ->
                model.UI.Debug <- (not model.UI.Debug)
                model, Cmd.none

        let view (model: DemoModel) dispatch =

            let scrollItems =
                [
                    button [text "Buttons"; onClick (fun () -> dispatch ShowButtons); fillHorizontal; toggled (model.State = Buttons)]
                    button [text "Text"; onClick (fun () -> dispatch ShowText); fillHorizontal; toggled (model.State = Text)]
                    button [text "Containers"; onClick (fun () -> dispatch ShowContainers); fillHorizontal; toggled (model.State = Containers)]
                ]

            let title, content  =
                match model.State with
                | Buttons -> "Buttons", Buttons.view model dispatch
                | Containers -> "Containers", []
                | Text -> "Labels", Text.view dispatch

            [
                [
                    grid 12 8
                        [
                            panel [label [text "Noobish"; textFont "AnonymousProBold22"]] [colspan 3; rowspan 1]
                            panelWithGrid 12 1
                                [
                                    label [text title; textFont "AnonymousProBold22"; fill; colspan 10];
                                    button
                                        [
                                            text "Debug";
                                            toggled model.UI.Debug;
                                            textFont "AnonymousProBold22";
                                            fill;
                                            onClick (fun () -> dispatch ToggleDebug)
                                            colspan 2
                                        ]
                                ]
                                [
                                    colspan 9;
                                    rowspan 1
                                ]
                            panel [scroll scrollItems []] [colspan 3; rowspan 7]
                            panel content [colspan 9; rowspan 7]
                        ]
                        [
                            padding 10
                        ]
                ]
            ]

        base.Initialize()
        this.GraphicsDevice.PresentationParameters.RenderTargetUsage <- RenderTargetUsage.PreserveContents
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)


        Program.mkProgram init update view
            |> Program.withNoobishRenderer nui
            |> Program.run


    override this.LoadContent() =

        ()

    override _this.UnloadContent() = ()

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
        this.GraphicsDevice.Clear(Color.Black)
        NoobishMonoGame.draw game.Content game.GraphicsDevice spriteBatch nui gameTime.TotalGameTime




[<EntryPoint>]
let main argv =
    use game = new DemoGame ()

    game.Run()
    0 // return an integer exit code