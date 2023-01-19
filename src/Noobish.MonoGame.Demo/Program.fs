// Learn more about F# at http://fsharp.org

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

open Elmish

open Noobish

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
    | ChangeMargin of int
    | ComboboxValueChanged of string
    | ToggleDebug

type ViewState = | Containers | Buttons | Text | Slider

type DemoModel =
    {
        UI: NoobishUI
        State: ViewState
        Padding: int
        Margin: int
        ComboboxValue: string
        SliderAValue: float32
    }

module Text =

    let view _dispatch =

        [
            grid 2 2
                [
                panelWithGrid 3 3
                    [
                        label [text "Top Left"; textTopLeft; fill]
                        label [text "Top"; textTopCenter; fill]
                        label [text "Top Right"; textTopRight; fill]
                        label [text "Left"; textLeft; fill]
                        label [text "Center"; textCenter; fill]
                        label [text "Right"; textRight; fill]
                        label [text "Bottom Left"; textBottomLeft; fill]
                        label [text "Bottom Center"; textBottomCenter; fill]
                        label [text "Bottom Right"; textBottomRight; fill]
                    ]
                    [

                    ]
                panel
                    [
                        scroll
                            [
                                paragraph [text loremIpsum1; block; name "FailedLorem1"]
                                paragraph [text loremIpsum2; block; name "FailedLorem2"]
                            ]
                            [
                                name "FailedScroll"
                                scrollVertical
                            ]
                    ]
                    [

                    ]
                panelWithGrid 1 3
                    [
                        paragraph [text loremIpsum2; textTopCenter; rowspan 1; ]
                        scroll
                            [
                                paragraph [text loremIpsum2; textTopCenter;
                                name "FailedParagraph2";]
                            ]
                            [
                                rowspan 1
                                name "FailedScroll2";
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
                                label [text "Font size 22"; textFont "Content/AnonymousPro22"; block]
                                label [text "Regular"; textFont "Content/AnonymousPro22"; textColor 0xac3232aa; block]
                                label [text "Bold"; textFont "Content/AnonymousProBold22"; textColor 0x4b692faa; block]
                                label [text "Italic"; textFont "Content/AnonymousProItalic22"; textColor 0x3f3f74aa; block]
                            ]
                            [

                            ]
                        div
                            [
                                label [text "Font size 16"; block]
                                label [text "Regular"; textColor 0xac3232aa; block]
                                label [text "Bold"; textFont "Content/AnonymousProBold16"; textColor 0x4b692faa; block]
                                label [text "Italic"; textFont "Content/AnonymousProItalic16"; textColor 0x3f3f74aa; block]
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

module Containers =



    let view model dispatch =
        [
            grid 2 2
                [
                panel
                    [
                        div [
                                header [text "Hello"; ];
                                hr []
                            ] [fillHorizontal];
                        button [ text "Continue"; onClick ignore; padding model.Padding; margin model.Margin; fillHorizontal; enabled false];
                        button [ text "Start"; onClick ignore; padding model.Padding; margin model.Margin; fillHorizontal; ];
                        button [ text "Options"; onClick ignore; padding model.Padding; margin model.Margin; fillHorizontal; ];
                    ]
                    [
                        name "ButtonsPanel"
                        padding model.Padding; margin model.Margin;

                    ]
                panel
                    [
                        canvas
                            [
                                image
                                    [
                                        name "Pixel Origin"
                                        texture "Pixel"
                                        textureBestFitMin
                                        minSize 10 10
                                        textureColor 0xff0000ff
                                        padding 0
                                        margin 0
                                        relativePosition -5 -5

                                    ]
                                image
                                    [
                                        name "Pixel 1"
                                        texture "Pixel"
                                        textureBestFitMin
                                        minSize 10 10
                                        textureColor 0xff0000ff
                                        padding 0
                                        margin 0
                                        relativePosition -25 15

                                    ]
                                button [ text "y"; relativePosition -30 -30 ]
                                button [ text "o"; relativePosition 30 30 ]
                                image [
                                        name "Pixel 2"
                                        texture "Pixel"
                                        textureBestFitMin
                                        minSize 10 10
                                        textureColor 0xff00FFff
                                        padding 0
                                        margin 0
                                        relativePosition 15 -25

                                    ]
                            ]
                            [
                                fill
                                text model.ComboboxValue; onChange (fun v -> dispatch (ComboboxValueChanged v))
                            ]
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



module Buttons =
    let view model dispatch =

        [
            grid 2 2
                [
                panel
                    [
                        button [text "Padding 0"; onClick (fun () -> dispatch (ChangePadding 0)); padding model.Padding; margin model.Margin; fillHorizontal]
                        button [text "Padding 5"; onClick (fun () -> dispatch (ChangePadding 5)); padding model.Padding; margin model.Margin; fillHorizontal]
                        button [text "Padding 10"; onClick (fun () -> dispatch (ChangePadding 10));  padding model.Padding; margin model.Margin; fillHorizontal]
                        button [text "Padding 15"; onClick (fun () -> dispatch (ChangePadding 15)); padding model.Padding; margin model.Margin; fillHorizontal]
                    ]
                    [
                        name "ButtonsPanel"
                        padding model.Padding; margin model.Margin;
                    ]
                panel
                    [
                        combobox
                            [
                                option "Value 1"
                                option "Value 2"
                                option "Value 3"
                            ]
                            [
                                text model.ComboboxValue;
                                onChange (fun v -> dispatch (ComboboxValueChanged v))
                                block;
                            ]
                        textBox [
                            text "Please insert coin"
                            //onOpenKeyboard (fun setText -> setText "opened")
                        ]
                    ]
                    [

                    ]
                panel
                    [
                        button [text "Margin 0"; onClick (fun () -> dispatch (ChangeMargin 0)); padding model.Padding; margin model.Margin; fillHorizontal]
                        button [text "Margin 2"; onClick (fun () -> dispatch (ChangeMargin 2)); padding model.Padding; margin model.Margin; fillHorizontal]
                        button [text "Margin 4"; onClick (fun () -> dispatch (ChangeMargin 4)); padding model.Padding; margin model.Margin; fillHorizontal]
                        button [text "Margin 6"; onClick (fun () -> dispatch (ChangeMargin 6)); padding model.Padding; margin model.Margin; fillHorizontal]
                    ]
                    [
                        name "MarginPanel"

                    ]
                panel
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


        let width = this.GraphicsDevice.Viewport.Width
        let height = this.GraphicsDevice.Viewport.Height

        this.Window.TextInput.Add(fun e ->
            NoobishMonoGame.keyTyped nui e.Character
        )

        let settings: NoobishSettings = {
            Scale = 1f
            FontSettings = {Small = "Content/AnomyousPro16"; Normal = "Content/AnonymousPro16"; Large = "Content/AnonymousPro16"}
            Pixel = "Content/Pixel"
            TextureAtlasName = "TestAtlas"
        }

        nui <- NoobishMonoGame.create game.Content width height settings
            |> NoobishMonoGame.overrideDebug false

        let init () =
            { UI = nui; State = Buttons; ComboboxValue = "Option 1"; Padding = 5; Margin = 5; SliderAValue = 25.0f;}, Cmd.ofMsg (ShowButtons)

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
            | ComboboxValueChanged v ->
                {model with ComboboxValue = v}, Cmd.none
            | ChangePadding padding ->
                {model with Padding = padding}, Cmd.none
            | ChangeMargin margin ->
                {model with Margin = margin}, Cmd.none
            | ToggleDebug ->
                model.UI.Debug <- (not model.UI.Debug)
                model, Cmd.none

        let view (model: DemoModel) dispatch =

            let scrollItems =
                [
                    button [text "Buttons"; onClick (fun () -> dispatch ShowButtons); fillHorizontal; toggled (model.State = Buttons); padding model.Padding; margin model.Margin; ]
                    button [text "Text"; onClick (fun () -> dispatch ShowText); fillHorizontal; toggled (model.State = Text); padding model.Padding; margin model.Margin; ]
                    button [text "Containers"; onClick (fun () -> dispatch ShowContainers); fillHorizontal; toggled (model.State = Containers); padding model.Padding; margin model.Margin; ]
                    button [text "Slider"; onClick (fun () -> dispatch ShowSliders); fillHorizontal; toggled (model.State = Slider); padding model.Padding; margin model.Margin; ]
                ]

            let title, content  =
                match model.State with
                | Buttons -> "Buttons", Buttons.view model dispatch
                | Containers -> "Containers", Containers.view model dispatch
                | Text -> "Labels", Text.view dispatch
                | Slider -> "Slider", Slider.view model dispatch

            [
                [
                    grid 12 8
                        [
                            panel [label [text "Noobish"; textFont "Content/AnonymousProBold22"]] [colspan 3; rowspan 1]
                            panelWithGrid 12 1
                                [
                                    label [text title; textFont "Content/AnonymousProBold22"; fill; colspan 10];
                                    button
                                        [
                                            text "Debug";
                                            toggled model.UI.Debug;
                                            textFont "Content/AnonymousProBold22";
                                            fill;
                                            onClick (fun () -> dispatch ToggleDebug)
                                            colspan 2
                                        ]
                                ]
                                [
                                    colspan 9;
                                    rowspan 1
                                    ninePatch "Content/TestAtlas" "window_background.9"
                                ]
                            panel [scroll scrollItems [name "LeftMenu";]] [colspan 3; rowspan 7;]
                            panel content [colspan 9; rowspan 7; ninePatch "Content/TestAtlas" "window_background.9"]
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