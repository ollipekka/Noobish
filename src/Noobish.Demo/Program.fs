// Learn more about F# at http://fsharp.org

open System

open Serilog
open Serilog.Configuration
open Serilog.Sinks

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

open Noobish
open Noobish.Styles
open Noobish.Fonts

let loremIpsum1 =
    "Scroll me!\n\n Lorem\nipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."

let loremIpsum2 =
    "Part 2\n Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?"

type DemoMessage =
    | ShowContainers
    | ShowNoobish2Demo
    | ShowButtons
    | ShowText
    | ShowSliders
    | ShowGithub
    | SliderValueChanged of float32
    | ChangePadding of int
    | ChangeMargin of int
    | ComboboxValueChanged of string
    | FeaturesChanged of string
    | ToggleDebug
    | ToggleDarkMode
    | ToggleLightMode
    | SelectListItem of int

type StyleMode = LightMode | DarkMode

type ViewState = | Noobish2Demo | Containers | Buttons | Text | Slider | Github

type DemoModel = {
    State: ViewState
    StyleMode: StyleMode
    Padding: int
    Margin: int
    ComboboxValue: string
    SliderAValue: float32
    FeatureText: string
    ListModel: int[]
    SelectedListItemIndex: int
} with
    member m.SelectedListItem with get() = m.ListModel.[m.SelectedListItemIndex]

module Noobish2Demo = 

    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) model dispatch =
        let nui2 = game.Noobish2
        
        let window2 = 
            nui2.Window()
            |> nui2.SetSize (150, 200)
            |> nui2.Children [|
                nui2.Header "Hello"
                nui2.HorizontalRule()
                nui2.Button "One" (fun _ -> ( Log.Logger.Information "One")) 
                    |> nui2.FillHorizontal
                nui2.Button "Two" (fun _ -> ( Log.Logger.Information "Two"))
                nui2.Button "Three" (fun _ -> (Log.Logger.Information "Three"))
            |]
        
        
        let window3 = 
            nui2.Window()
            |> nui2.SetPosition (200, 0)
            |> nui2.Children [|
                nui2.Header "Hello 2"
                nui2.HorizontalRule()
                nui2.Button "One 2" (fun _ -> (Log.Logger.Information "One 2"))
                nui2.Button "Two 2" (fun _ -> (Log.Logger.Information "Two 2"))
                nui2.Button "Three 2" (fun _ -> (Log.Logger.Information "Three 2"))
            |] 

        let window3 = 
            nui2.Window()
            |> nui2.SetGrid(2, 2)
            |> nui2.SetPosition (0, 200)
            |> nui2.Children [|
                nui2.Button "1" (fun _ -> (Log.Logger.Information "1"))
                    |> nui2.SetColspan 1 
                    |> nui2.SetRowspan 1 
                    |> nui2.SetFill
                nui2.Button "2" (fun _ -> (Log.Logger.Information "2"))
                    |>nui2.SetColspan 1 
                    |> nui2.SetRowspan 1
                nui2.Button "3" (fun _ -> (Log.Logger.Information "3"))
                    |>nui2.SetColspan 1 
                    |> nui2.SetRowspan 1
                nui2.Button "44444" (fun _ -> (Log.Logger.Information "4444"))
                    |>nui2.SetColspan 1 
                    |> nui2.SetRowspan 1
            |] 
        ()


module Text =

    let view model _dispatch =

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
                                paragraph [text loremIpsum2; textTopCenter;]
                            ]
                            [
                                rowspan 1
                            ]
                        scroll
                            [
                                paragraph [text $"Could scroll, but won't.\n Here's the Noobish manifesto:\n %s{model.FeatureText}"]

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
                                h1 [text "Header 1"; block]
                                h2 [text "Header 2"; block]
                                h3 [text "Header 3"; block]
                            ]
                            [

                            ]
                        div
                            [
                                label [text "Font size 16"; block]
                                label [text "Regular"; block]
                                label [text "Bold"; block]
                                label [text "Italic"; block]
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

        let createListLabel index =
            div
                [
                    label [ text $"Item %i{index}"; toggled (model.SelectedListItemIndex = index); fillHorizontal; onClick (fun _ -> dispatch (SelectListItem index))] |> themePrefix "List";
                ]
                [block; fillHorizontal; toggled (model.SelectedListItemIndex = index)]
                |> themePrefix "List" |> themeSuffix (if index % 2 = 0 then "Even" else "Odd")
        [
            grid 2 2
                [
                panel
                    [
                        div [
                                h2 [text "Hello"; ];
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
                                        padding 0
                                        margin 0
                                        relativePosition 15 -25

                                    ]
                            ]
                            [
                                fill
                                text model.ComboboxValue; onTextChange (fun v -> dispatch (ComboboxValueChanged v))
                            ]
                    ]
                    [

                    ]
                panel
                    [
                        h2 [text $"List: Item %i{model.SelectedListItem}"]
                        hr []
                        scroll
                            (model.ListModel |> Array.map createListLabel |> Array.toList)
                            []
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
    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) model dispatch =

        let ui = game.Noobish2

    
        ui.Grid(2, 2)
        |> ui.Children 
            [|
                ui.PanelVertical ()
                |> ui.Children [|
                    ui.Button "Padding 0" (fun gameTime -> dispatch (ChangePadding 0))
                        |> ui.SetFillHorizontal
                    ui.Button "Padding 5" (fun gameTime -> dispatch (ChangePadding 5)) 
                        |> ui.SetFillHorizontal
                    ui.Button "Padding 10" (fun gameTime -> dispatch (ChangePadding 10))
                        |> ui.SetFillHorizontal
                    ui.Button "Padding 15" (fun gameTime -> dispatch (ChangePadding 15))
                        |> ui.SetFillHorizontal
                |]
                ui.PanelVertical ()
                |> ui.Children [|
                    ui.Button "Margin 0" (fun gameTime -> dispatch (ChangeMargin 0))
                        |> ui.SetFillHorizontal
                    ui.Button "Margin 5" (fun gameTime -> dispatch (ChangeMargin 5)) 
                        |> ui.SetFillHorizontal
                    ui.Button "Margin 10" (fun gameTime -> dispatch (ChangeMargin 10))
                        |> ui.SetFillHorizontal
                    ui.Button "Margin 15" (fun gameTime -> dispatch (ChangeMargin 15))
                        |> ui.SetFillHorizontal
                |]
                ui.PanelVertical ()
                |> ui.Children [|
                    ui.Combobox [| "One"; "Two"; "Three" |] (fun event value -> Log.Logger.Information("Value changed {Value}", value))
                    ui.Textbox "what" (fun event value -> Log.Logger.Information("Text changed {value}", value))
                    ui.Button "8" ignore 
                    ui.Button "9" ignore
                |]
                ui.PanelHorizontal ()
                |> ui.Children [|
                        ui.Header "Three"
                    |]
            |]


module Slider =
    let view model dispatch =

        let option children =
            div
                children
                [
                    fillHorizontal
                    block
                ]

        let content =
            [
                option
                    [
                        label [ text "OPTION 1"; fillHorizontal; ];
                        slider [ sliderRange 0.0f 100.0f; sliderStep 1.0f; fillHorizontal; ];
                    ];
                option
                    [
                        label [ text "OPTION 2"; fillHorizontal; ];
                        slider [ sliderRange 0.0f 10.0f; sliderStep 1.0f; fillHorizontal; ];

                    ];
                 option
                    [
                        label [ text "OPTION 3"; fillHorizontal; ];
                        slider [ sliderRange 0.0f 10.0f; sliderStep 1.0f; fillHorizontal; ];

                    ];
                option
                    [
                        label [ text "OPTION 4"; fillHorizontal; ];
                        slider [ sliderRange 0.0f 10.0f; sliderStep 1.0f; fillHorizontal; ];

                    ];
                option
                    [
                        label [ text "OPTION 5"; fillHorizontal; ];
                        slider [ sliderRange 0.0f 10.0f; sliderStep 1.0f; fillHorizontal; ];

                    ];
                option
                    [
                        label [ text "OPTION 6"; fillHorizontal; ];
                        slider [ sliderRange 0.0f 10.0f; sliderStep 1.0f; fillHorizontal; ];
                    ];
                option
                    [
                        label [ text "OPTION 7"; fillHorizontal; ];
                        slider [ sliderRange 0.0f 10.0f; sliderStep 1.0f; fillHorizontal; ];

                    ];
                option
                    [
                        label [ text "OPTION 8"; fillHorizontal; ];
                        slider [ sliderRange 0.0f 10.0f; sliderStep 1.0f; fillHorizontal; ];

                    ];
            ]

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
                panel
                    [
                    scroll
                        content
                        [
                            fill
                        ]
                    ]

                    []

                ]
                [

                ]

        ]




module Github =
    let view model dispatch =
        [
            grid 10 8
                [
                    space [colspan 10; rowspan 2]
                    space [colspan 2; rowspan 4]
                    panel
                        [
                            h2 [text "Welcome to Noobish"; textCenter]
                            hr []
                            grid 6 4
                                [
                                    checkbox [text "FSharp"; toggled true; onCheckBoxValueChange(fun v -> printfn "Hello %b" v); colspan 2; rowspan 1]
                                    checkbox [text "MonoGame"; toggled true; colspan 2; rowspan 1]
                                    checkbox [text "Elmish"; toggled true; colspan 2; rowspan 1]
                                    label [text "Coolness:"; colspan 1; rowspan 1; textLeft]
                                    slider [sliderValue 80f; fillHorizontal; colspan 5; rowspan 1]
                                    label [text "Features:"; colspan 1; rowspan 1; textLeft]
                                    textbox [ text model.FeatureText; onTextChange (fun v -> dispatch (FeaturesChanged v)); textLeft; colspan 5; rowspan 1]
                                    button [text "Report a bug"; colspan 2]
                                    button [text "Contribute"; colspan 2]
                                    button [text "Fork"; colspan 2]
                                ]
                                [

                                ]

                        ]
                        [
                            colspan 6; rowspan 4;
                        ]
                    space [colspan 2; rowspan 4]
                    space [colspan 10; rowspan 2]
                ]
                [

                ]
        ]

let init (game: NoobishGame<unit, DemoMessage, DemoModel>) () =
    { State = Buttons; ComboboxValue = "Option 1"; Padding = 5; Margin = 5; SliderAValue = 25.0f; StyleMode = DarkMode; FeatureText = "functional, extendable, net6.0 and cross-platform."; ListModel = Array.init 21 id; SelectedListItemIndex = 0}, Cmd.ofMsg(ShowButtons)

let update (game: Game) (message: DemoMessage) (model: DemoModel) (gameTime: GameTime)=
    match message with
    | ShowNoobish2Demo ->
        {model with State = Noobish2Demo}, Cmd.none
    | ShowButtons ->
        {model with State = Buttons}, Cmd.none
    | ShowContainers ->
        {model with State = Containers}, Cmd.none
    | ShowText ->
        {model with State = Text}, Cmd.none
    | ShowSliders ->
        {model with State = Slider}, Cmd.none
    | ShowGithub ->
        {model with State = Github}, Cmd.none
    | SliderValueChanged v ->
        {model with SliderAValue = v}, Cmd.none
    | ComboboxValueChanged v ->
        {model with ComboboxValue = v}, Cmd.none
    | ChangePadding padding ->
        {model with Padding = padding}, Cmd.none
    | ChangeMargin margin ->
        {model with Margin = margin}, Cmd.none
    | FeaturesChanged s ->
        {model with FeatureText = s}, Cmd.none
    | ToggleDebug ->
        let nui = game.Services.GetService<NoobishUI>()
        nui.Settings.Debug <- (not nui.Settings.Debug)
        model, Cmd.none
    | ToggleLightMode ->
        let nui = game.Services.GetService<NoobishUI>()
        nui.StyleSheetId <- "Light/Light"
        {model with StyleMode = LightMode}, Cmd.none
    | ToggleDarkMode ->
        let nui = game.Services.GetService<NoobishUI>()
        nui.StyleSheetId <- "Dark/Dark"
        {model with StyleMode = DarkMode}, Cmd.none
    | SelectListItem index ->
        {model with SelectedListItemIndex = index}, Cmd.none

let view game (model: DemoModel) dispatch =

    let title, content2, content  =
        match model.State with
        | Noobish2Demo ->
            Noobish2Demo.view game model dispatch
            "Noobish2Demo", UIComponentId.empty, []
        | Buttons ->
            let content = Buttons.view game model dispatch
            "Buttons", content, []
        | Containers -> 
            let content= Containers.view model dispatch
            "Containers", UIComponentId.empty, content
        | Text -> 
            let content = Text.view model dispatch
            "Labels", UIComponentId.empty, content
        | Slider -> 
            let content = Slider.view model dispatch
            "Slider", UIComponentId.empty, content
        | Github -> 
            let content = Github.view model dispatch
            "Github", UIComponentId.empty, content

    let ui = game.Noobish2

    let contentId = 
        if content2 = UIComponentId.empty then 
            ui.Space()
        else 
            content2

    let gridId = 
        ui.Grid(8, 12)
        |> ui.Children [|
            ui.PanelHorizontal() 
            |> ui.SetRowspan 1
            |> ui.SetColspan 3
            |> ui.Children [|
                ui.Header "Noobish"
            |]
            ui.PanelWithGrid(1, 12)
            |> ui.SetRowspan 1 
            |> ui.SetColspan 9
            |> ui.Children [|
                ui.Header title
                |> ui.SetColspan 6
                
                ui.Button "Dark" (fun e -> dispatch ToggleDarkMode) 
                |> ui.SetFill
                |> ui.SetColspan 2     
                |> ui.SetToggled (model.StyleMode = DarkMode )          
                ui.Button "Light" (fun e -> dispatch ToggleLightMode) 
                |> ui.SetFill
                |> ui.SetToggled (model.StyleMode = LightMode )       
                |> ui.SetColspan 2
                ui.Button "Debug" (fun e -> dispatch ToggleDebug) 
                |> ui.SetFill
                |> ui.SetColspan 2
                |> ui.SetToggled (ui.Debug)       
                
            |]
            ui.PanelVertical() 
            |> ui.SetRowspan 7
            |> ui.SetColspan 3
            |> ui.Children [|
                ui.Button "Buttons" (fun gameTime -> dispatch ShowButtons)
                |> ui.FillHorizontal 
                |> ui.SetToggled (model.State = Buttons)
                ui.Button "Text" (fun gameTime -> dispatch ShowText)
                |> ui.FillHorizontal 
                |> ui.SetToggled (model.State = Text)
                ui.Button "Containers" (fun gameTime -> dispatch ShowContainers)
                |> ui.FillHorizontal 
                |> ui.SetToggled (model.State = Containers)
                ui.Button "Slider" (fun gameTime -> dispatch ShowSliders)
                |> ui.FillHorizontal 
                |> ui.SetToggled (model.State = Slider)
                ui.Button "Github" (fun gameTime -> dispatch ShowGithub)
                |> ui.FillHorizontal 
                |> ui.SetToggled (model.State = Github)
            |]
            contentId
            |> ui.SetRowspan 7 
            |> ui.SetColspan 9
        |]

    [
        [
            grid 12 8
                [
                    panel [h1 [text "Noobish"; fill]] [colspan 3; rowspan 1]
                    panelWithGrid 12 1
                        [
                            h1 [text title; fill; colspan 6];
                            button
                                [
                                    localizedText ("Localization/TestBundle", "Dark");
                                    toggled (model.StyleMode = DarkMode);
                                    fill;
                                    onClick (fun gameTime -> dispatch ToggleDarkMode)
                                    keyboardShortcut (ctrl NoobishKeyId.B)
                                    colspan 2
                                ]
                            button
                                [
                                    localizedText ("Localization/TestBundle", "Light");
                                    toggled (model.StyleMode = LightMode);
                                    fill;
                                    onClick (fun gameTime -> dispatch ToggleLightMode)
                                    keyboardShortcut (ctrl NoobishKeyId.L)
                                    colspan 2
                                ]
                            button
                                [
                                    text "Debug";
                                    toggled false; //model.UI.Settings.Debug;
                                    fill;
                                    onClick (fun gameTime -> dispatch ToggleDebug)
                                    keyboardShortcut (alt NoobishKeyId.D)
                                    colspan 2
                                ]
                        ]
                        [
                            colspan 9;
                            rowspan 1
                        ]
                    space [colspan 3; rowspan 7;]
                    panel content [colspan 9; rowspan 7;]
                ]
                [
                    padding 10
                ]
        ]
    ]

let tick game (model) (gameTime: GameTime) = ()
let draw game (model) (gameTime: GameTime) = ()


[<EntryPoint>]
let main argv =
    let logger =
        LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger()

    Log.Logger <- logger
    Program2.create ignore init update view tick draw
        |> Program2.withContentRoot "Content/"
        |> Program2.withTheme "Dark/Dark"
        |> Program2.withPreferHalfPixelOffset true
        |> Program2.withResolution 1280 720
        |> Program2.withMouseVisible true
        |> Program2.run
    0 // return an integer exit code