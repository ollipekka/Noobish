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
    | ShowSliders
    | SliderValueChanged of float32
    | ChangePadding of int
    | ToggleDebug

type ViewState = | Containers | Buttons | Text | Slider

type DemoModel =
    {
        UI: NoobishUI
        State: ViewState
        Padding: int
        SliderAValue: float32
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


module Slider =
    let view model dispatch =

        [
            grid 2 2
                [
                panel
                    [
                        label [text (sprintf "Slider A Value: %f" model.SliderAValue); fillHorizontal]
                        slider [sliderRange 0.0f 100.0f; sliderValue model.SliderAValue; sliderOnValueChanged (fun v -> dispatch (SliderValueChanged v)); padding model.Padding; fillHorizontal]
                        slider [sliderRange 0.0f 100.0f; sliderValue 50.0f; padding model.Padding; fillHorizontal]
                        slider [sliderRange 0.0f 100.0f; sliderValue 90.0f; padding model.Padding; fillHorizontal]
                    ]
                    []
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
            FontSettings = {Small = "AnomyousPro16"; Normal = "AnonymousPro16"; Large = "AnonymousPro16"};
            Pixel = "Pixel"
            FontPrefix = ""
            GraphicsPrefix = ""
        }

        nui <- NoobishMonoGame.create game.Content width height settings
            |> NoobishMonoGame.overrideDebug false

        let init () =
            { UI = nui; State = Buttons; Padding = 5; SliderAValue = 25.0f;}, Cmd.ofMsg (ShowButtons)

        let update (message: DemoMessage) (model: DemoModel) =
            match message with
            | ShowButtons ->
                {model with State = Buttons}, Cmd.none
            | ShowContainers ->
                {model with State = Containers}, Cmd.none
            | ShowText ->
                {model with State = Text}, Cmd.none
            | ShowSliders ->
                {model with State = Slider}, Cmd.none
            | SliderValueChanged v ->
                {model with SliderAValue = v}, Cmd.none
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
                    button [text "Slider"; onClick (fun () -> dispatch ShowSliders); fillHorizontal; toggled (model.State = Slider)]
                ]

            let title, content  =
                match model.State with
                | Buttons -> "Buttons", Buttons.view model dispatch
                | Containers -> "Containers", []
                | Text -> "Labels", Text.view dispatch
                | Slider -> "Slider", Slider.view model dispatch

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

        previousKeyboardState <- keyboardState
        keyboardState <- Keyboard.GetState()

        previousMouseState <- mouseState
        mouseState <- Mouse.GetState()

        previousTouchState <- touchState
        touchState <- TouchPanel.GetState()

        #if __MOBILE__
        NoobishMonoGame.updateMobile nui previousTouchState touchState gameTime
        #else
        NoobishMonoGame.updateMouse nui previousMouseState mouseState gameTime
        NoobishMonoGame.updateKeyboard nui previousKeyboardState keyboardState gameTime
        #endif




    override this.Draw (gameTime) =
        base.Draw(gameTime)
        this.GraphicsDevice.Clear(Color.Black)
        NoobishMonoGame.draw game.Content game.GraphicsDevice spriteBatch nui gameTime.TotalGameTime




[<EntryPoint>]
let main argv =
    use game = new DemoGame ()

    game.Run()
    0 // return an integer exit code