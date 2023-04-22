// Learn more about F# at http://fsharp.org

open System

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

type ViewState = | Containers | Buttons | Text | Slider | Github

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
                                onTextChange (fun v -> dispatch (ComboboxValueChanged v))
                                block;
                            ]
                        textbox [
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
                        scroll
                            [
                                button [text "Button 1"; fillHorizontal]
                                button [text "Button 2"; fillHorizontal]
                                button [text "Button 3"; fillHorizontal]
                                button [text "Button 4"; fillHorizontal]
                                button [text "Button 5"; fillHorizontal]
                                button [text "Button 6"; fillHorizontal]
                                button [text "Button 7"; fillHorizontal]
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

[<EntryPoint>]
let main argv =
    let init () =
        { State = Buttons; ComboboxValue = "Option 1"; Padding = 5; Margin = 5; SliderAValue = 25.0f; StyleMode = DarkMode; FeatureText = "functional, extendable, net6.0 and cross-platform."; ListModel = Array.init 21 id; SelectedListItemIndex = 0}, [ShowButtons]

    let update (message: DemoMessage) (model: DemoModel) =
        match message with
        | ShowButtons ->
            {model with State = Buttons}, []
        | ShowContainers ->
            {model with State = Containers}, []
        | ShowText ->
            {model with State = Text}, []
        | ShowSliders ->
            {model with State = Slider}, []
        | ShowGithub ->
            {model with State = Github}, []
        | SliderValueChanged v ->
            {model with SliderAValue = v}, []
        | ComboboxValueChanged v ->
            {model with ComboboxValue = v}, []
        | ChangePadding padding ->
            {model with Padding = padding}, []
        | ChangeMargin margin ->
            {model with Margin = margin}, []
        | FeaturesChanged s ->
            {model with FeatureText = s}, []
        | ToggleDebug ->
            //model.UI.Settings.Debug <- (not model.UI.Settings.Debug)
            model, []
        | ToggleLightMode ->
            //nui.StyleSheet <- this.Content.Load<NoobishStyleSheet> "Light/Light"
            {model with StyleMode = LightMode}, []
        | ToggleDarkMode ->
            //nui.StyleSheet <- this.Content.Load<NoobishStyleSheet> "Dark/Dark"
            {model with StyleMode = DarkMode}, []
        | SelectListItem index ->
            {model with SelectedListItemIndex = index}, []

    let view (model: DemoModel) dispatch =

        let scrollItems =
            [
                button [text "Buttons"; onClick (fun () -> dispatch ShowButtons); fillHorizontal; toggled (model.State = Buttons); padding model.Padding; margin model.Margin; ]
                button [text "Text"; onClick (fun () -> dispatch ShowText); fillHorizontal; toggled (model.State = Text); padding model.Padding; margin model.Margin; ]
                button [text "Containers"; onClick (fun () -> dispatch ShowContainers); fillHorizontal; toggled (model.State = Containers); padding model.Padding; margin model.Margin; ]
                button [text "Slider"; onClick (fun () -> dispatch ShowSliders); fillHorizontal; toggled (model.State = Slider); padding model.Padding; margin model.Margin; ]
                button [text "Github"; onClick (fun () -> dispatch ShowGithub); fillHorizontal; toggled (model.State = Github); padding model.Padding; margin model.Margin; ]
            ]

        let title, content  =
            match model.State with
            | Buttons -> "Buttons", Buttons.view model dispatch
            | Containers -> "Containers", Containers.view model dispatch
            | Text -> "Labels", Text.view model dispatch
            | Slider -> "Slider", Slider.view model dispatch
            | Github -> "Github", Github.view model dispatch

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
                                        onClick (fun () -> dispatch ToggleDarkMode)
                                        colspan 2
                                    ]
                                button
                                    [
                                        localizedText ("Localization/TestBundle", "Light");
                                        toggled (model.StyleMode = LightMode);
                                        fill;
                                        onClick (fun () -> dispatch ToggleLightMode)
                                        colspan 2
                                    ]
                                button
                                    [
                                        text "Debug";
                                        toggled false; //model.UI.Settings.Debug;
                                        fill;
                                        onClick (fun () -> dispatch ToggleDebug)
                                        colspan 2
                                    ]
                            ]
                            [
                                colspan 9;
                                rowspan 1
                            ]
                        panel [scroll scrollItems [name "LeftMenu";]] [colspan 3; rowspan 7;]
                        panel content [colspan 9; rowspan 7;]
                    ]
                    [
                        padding 10
                    ]
            ]
        ]

    let tick (model) (gameTime: GameTime) = ()
    let draw (model) (gameTime: GameTime) = ()

    Program2.create ignore init update view tick draw
        |> Program2.withContentRoot "Content/"
        |> Program2.withPreferHalfPixelOffset true
        |> Program2.withScreenSize 1280 720
        |> Program2.withMouseVisible true
        |> Program2.run
    0 // return an integer exit code