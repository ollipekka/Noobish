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
    | ShowButtons
    | ShowList
    | ShowText
    | ShowSliders
    | ShowGithub
    | SliderValueChanged of float32
    | ChangePadding of int
    | ChangeMargin of int
    | ComboboxValueChanged of string
    | CheckboxValueChanged of bool
    | FeaturesChanged of string
    | ToggleDebug
    | ToggleDarkMode
    | ToggleLightMode
    | SelectListItem of int

type StyleMode = LightMode | DarkMode

type ViewState = Containers | Buttons | Text | List | Slider | Github

type DemoModel = {
    State: ViewState
    StyleMode: StyleMode
    Padding: int
    Margin: int
    ComboboxValue: string
    CheckboxValue: bool
    SliderAValue: float32
    FeatureText: string
    ListModel: int[]
    SelectedListItemIndex: int
} with
    member m.SelectedListItem with get() = m.ListModel.[m.SelectedListItemIndex]

module NoobishDemo = 

    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) model dispatch =
        let nui2 = game.Noobish
        
        let window2 = 
            nui2.Window()
            |> nui2.SetSize (150, 200)
            |> nui2.SetChildren [|
                nui2.Header "Hello"
                nui2.HorizontalRule()
                nui2.Button "One" (fun _ _ -> ( Log.Logger.Information "One")) 
                    |> nui2.FillHorizontal
                nui2.Button "Two" (fun _ _-> ( Log.Logger.Information "Two"))
                nui2.Button "Three" (fun _ _-> (Log.Logger.Information "Three"))
            |]
        
        
        let window3 = 
            nui2.Window()
            |> nui2.SetPosition (200, 0)
            |> nui2.SetChildren [|
                nui2.Header "Hello 2"
                nui2.HorizontalRule()
                nui2.Button "One 2" (fun _ _ -> (Log.Logger.Information "One 2"))
                nui2.Button "Two 2" (fun _ _ -> (Log.Logger.Information "Two 2"))
                nui2.Button "Three 2" (fun _ _ -> (Log.Logger.Information "Three 2"))
            |] 

        let window3 = 
            nui2.WindowWithGrid(2, 2)
            |> nui2.SetPosition (0, 200)
            |> nui2.SetChildren [|
                nui2.Button "1" (fun _ _ -> (Log.Logger.Information "1"))
                    |> nui2.SetColspan 1 
                    |> nui2.SetRowspan 1 
                    |> nui2.SetFill
                nui2.Button "2" (fun _ _ -> (Log.Logger.Information "2"))
                    |>nui2.SetColspan 1 
                    |> nui2.SetRowspan 1
                nui2.Button "3" (fun _ _ -> (Log.Logger.Information "3"))
                    |>nui2.SetColspan 1 
                    |> nui2.SetRowspan 1
                nui2.Button "44444" (fun _ _ -> (Log.Logger.Information "4444"))
                    |>nui2.SetColspan 1 
                    |> nui2.SetRowspan 1
            |] 
        ()


module Text =

    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) model _dispatch =
        let ui = game.Noobish
        ui.PanelWithGrid (2, 2) 
        |> ui.SetChildren [|
            ui.PanelWithGrid(3, 3)
            |> ui.SetChildren [|
                ui.Label "Top Left"
                    |> ui.SetFill
                    |> ui.AlignTextTopLeft
                ui.Label "Top"
                    |> ui.SetFill
                    |> ui.AlignTextTop
                ui.Label "Top Right"
                    |> ui.SetFill
                    |> ui.AlignTextTopRight
                ui.Label "Left"
                    |> ui.SetFill
                    |> ui.AlignTextLeft
                ui.Label "Center"
                    |> ui.SetFill
                    |> ui.AlignTextCenter
                ui.Label "Right"
                    |> ui.SetFill
                    |> ui.AlignTextRight
                ui.Label "Bottom Left"
                    |> ui.SetFill
                    |> ui.AlignTextBottomLeft
                ui.Label "Bottom Center"
                    |> ui.SetFill
                    |> ui.AlignTextBottomCenter
                ui.Label "Bottom Right"
                    |> ui.SetFill
                    |> ui.AlignTextBottomRight

            |]
            ui.PanelVertical()
                |> ui.SetScrollVertical
                |> ui.SetChildren [|
                    ui.Paragraph loremIpsum1
                    ui.Paragraph loremIpsum2
                |]  
            ui.PanelWithGrid (3, 1)
                |> ui.SetChildren [|
                    ui.Paragraph loremIpsum2 |> ui.AlignTextTopLeft |> ui.SetFill
                    ui.PanelVertical()
                        |> ui.SetScrollVertical
                        |> ui.SetChildren [|
                            ui.Paragraph loremIpsum1
                            |> ui.SetMargin 0 
                            |> ui.SetPadding 0
                        |]
                    ui.Paragraph $"Could scroll, but won't.\n Here's the Noobish manifesto:\n %s{model.FeatureText}" |> ui.SetFill
                |]
            ui.PanelWithGrid (1, 3)
                |> ui.SetChildren [|
                    ui.Paragraph loremIpsum2 |> ui.AlignTextTopLeft |> ui.SetFill
                    ui.DivVertical()
                        |> ui.SetScrollVertical
                        |> ui.SetChildren [|
                            ui.Paragraph loremIpsum1
                            |> ui.SetMargin 0 
                            |> ui.SetPadding 0
                        |]
                    ui.Paragraph $"Could scroll, but won't.\n Here's the Noobish manifesto:\n %s{model.FeatureText}" |> ui.SetFill
                |]
        |]
    



module List =
    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) (model: DemoModel) dispatch =
        let ui = game.Noobish
        ui.PanelWithGrid (2, 2) 
        |> ui.SetChildren [|
            ui.List model.ListModel model.SelectedListItem (fun value -> Log.Logger.Information ("Clicked {value}", value); dispatch (SelectListItem (model.ListModel |> Array.findIndex (fun v -> v = value))) )
        |]
    

module Containers =


    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) (model: DemoModel) dispatch =
        let ui = game.Noobish
        ui.PanelWithGrid (2, 2) 
        |> ui.SetChildren [|
            ui.PanelVertical()
            |> ui.SetChildren [|
                ui.DivVertical()
                    |> ui.FillHorizontal
                    |> ui.SetChildren [|
                        ui.Header "Hello"
                        ui.HorizontalRule()
                    |]
                ui.Button "Continue" (fun _ _ -> ()) |> ui.FillHorizontal |> ui.SetEnabled false
                ui.Button "Start" (fun _ _ -> ()) |> ui.FillHorizontal
                ui.Button "Options" (fun _ _ -> ()) |> ui.FillHorizontal


            |]
            ui.PanelVertical()
                |> ui.SetChildren [|
                    ui.DivHorizontal() 
                        |> ui.FillHorizontal
                        |> ui.SetChildren [|
                            ui.Label model.ComboboxValue
                        |]
                    ui.Canvas()
                        |> ui.SetChildren [|
                            ui.Button "0" (fun _ _ -> ()) |> ui.SetRelativePosition (50, 50)
                            ui.Button "1" (fun _ _ -> ()) |> ui.SetRelativePosition (25, 75)
                        |]

                |]
            ui.PanelVertical() 
                |> ui.SetChildren [|
                    ui.DivVertical()
                        |> ui.FillHorizontal
                        |> ui.SetChildren [|
                            ui.Header $"Selected: {model.SelectedListItemIndex}"
                            ui.HorizontalRule()
                        |]
                    ui.List model.ListModel model.SelectedListItemIndex (fun item -> dispatch (SelectListItem (model.ListModel |> Array.findIndex(fun i -> i = item))))
                |]
            ui.PanelVertical() 
                |> ui.SetChildren [|
                    ui.DivVertical()
                        |> ui.FillHorizontal
                        |> ui.SetChildren [|
                            ui.Checkbox "Option 1" model.CheckboxValue (fun v -> dispatch (CheckboxValueChanged v)) 
                            ui.Checkbox "Option 2" true ignore 
                        |]
                |]
        |]



module Buttons =
    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) model (dispatch: DemoMessage -> unit) =

        let ui = game.Noobish

    
        ui.Grid(2, 2)
        |> ui.SetChildren 
            [|
                ui.PanelVertical ()
                |> ui.SetChildren [|
                    ui.Button "Padding 0" (fun _ gameTime -> dispatch (ChangePadding 0))
                        |> ui.SetFillHorizontal
                    ui.Button "Padding 5" (fun _ gameTime -> dispatch (ChangePadding 5)) 
                        |> ui.SetFillHorizontal
                    ui.Button "Padding 10" (fun _ gameTime -> dispatch (ChangePadding 10))
                        |> ui.SetFillHorizontal
                    ui.Button "Padding 15" (fun _ gameTime -> dispatch (ChangePadding 15))
                        |> ui.SetFillHorizontal
                |]
                ui.PanelVertical ()
                |> ui.SetChildren [|
                    ui.Button "Margin 0" (fun _ gameTime -> dispatch (ChangeMargin 0))
                        |> ui.SetFillHorizontal
                    ui.Button "Margin 5" (fun _ gameTime -> dispatch (ChangeMargin 5)) 
                        |> ui.SetFillHorizontal
                    ui.Button "Margin 10" (fun _ gameTime -> dispatch (ChangeMargin 10))
                        |> ui.SetFillHorizontal
                    ui.Button "Margin 15" (fun _ gameTime -> dispatch (ChangeMargin 15))
                        |> ui.SetFillHorizontal
                |]
                let items = [| "Option 1"; "Option 2"; "Option 3" |]
                let selectedItemIndex = items |> Array.findIndex(fun item -> item = model.ComboboxValue)
                ui.PanelVertical ()
                |> ui.SetChildren [|
                    ui.Combobox items selectedItemIndex (fun value -> Log.Logger.Information("Value changed {Value}", value); dispatch (ComboboxValueChanged value))
                    ui.Textbox model.FeatureText (fun value -> Log.Logger.Information("Text changed {value}", value); dispatch (FeaturesChanged value))
                    ui.Button "8" (fun _ _ -> ())
                    ui.Button "9" (fun _ _ -> ())
                |]
                ui.PanelHorizontal ()
                |> ui.SetChildren [|
                        ui.Header "Three"
                    |]
            |]


module Slider =

    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) (model: DemoModel) dispatch =
            
            
        let ui = game.Noobish

        let gridId
            = ui.Grid(2,2)
            |> ui.SetChildren [|
                
                ui.PanelVertical() 
                    |> ui.SetChildren [|
                        ui.Header $"Slider A %g{model.SliderAValue}"
                        ui.Slider (0f, 100f) 1f model.SliderAValue (fun value -> dispatch (SliderValueChanged value))
                        ui.Header $"Slider B "
                        ui.Slider (0f, 10f) 0.1f 5.5f (fun value-> ())
                        ui.Header $"Slider C"
                        ui.Slider (50f, 100f) 2f 50f (fun value -> ())
                        ui.Header $"Slider D"
                        ui.Slider (0f, 25f) 5f 25f (fun value -> ())
                    |]
                ui.Panel ()
                    |> ui.SetGridLayout(1, 9)
                    |> ui.SetFill
                    |> ui.SetChildren [|
                        ui.DivVertical() 
                            |> ui.SetRowspan 2
                            |> ui.SetFill
                            |> ui.SetChildren [|
                                ui.Header ("Audio")
                                ui.HorizontalRule() 
                            |]
                        ui.Label ("Music") |> ui.FillHorizontal
                        ui.Slider (0f, 100f) 1.0f 50f (fun v -> ())
                            |> ui.SetFill

                        ui.Label ("Sounds") |> ui.FillHorizontal
                        ui.Slider (0f, 100f) 1.0f 75f (fun v -> ())
                            |> ui.FillHorizontal
                            

                        ui.Space() 

                        ui.Button ("Back") (fun _ _ -> ()) |> ui.SetFill |> ui.SetRowspan 2


                    |]
    
                ui.Panel() 
                    |> ui.SetFill
                    |> ui.SetGridLayout(1, 1)
                    |> ui.SetChildren[|
                        ui.Slider (0f, 100f) 1.0f 35f (fun v -> ())
                            |> ui.FillHorizontal
                    |]

            |]

        gridId




module Github =

    let view (game: NoobishGame<unit, DemoMessage, DemoModel>) (model: DemoModel) dispatch =
        let ui = game.Noobish
        let gridId = 
            ui.Grid(10, 8)
            |> ui.SetChildren [|
                ui.Space() 
                    |> ui.SetColspan 10
                    |> ui.SetRowspan 2
                ui.Space() 
                    |> ui.SetColspan 2
                    |> ui.SetRowspan 4
                ui.PanelVertical()
                    |> ui.SetColspan 6
                    |> ui.SetRowspan 4
                    |> ui.SetChildren [|
                        ui.Header "Welcome to Noobish" |> ui.AlignTextCenter
                        ui.HorizontalRule()
                        ui.Grid (6, 4)
                            |> ui.SetChildren [|
                                ui.Checkbox "FSharp" true ignore |> ui.SetColspan 2 
                                ui.Checkbox "MonoGame" true ignore |> ui.SetColspan 2
                                ui.Checkbox "Elmish" true ignore |> ui.SetColspan 2
                                ui.Label "Coolness:"
                                    |> ui.SetTextAlign NoobishAlignment.Left 
                                    |> ui.SetFill
                                ui.Slider (0f, 100f) 1f 80f ignore 
                                    |> ui.SetFillHorizontal
                                    |> ui.SetColspan 5
                                ui.Label "Features:"
                                    |> ui.SetTextAlign NoobishAlignment.Left 
                                    |> ui.SetFill
                                ui.Textbox model.FeatureText (fun v -> dispatch (FeaturesChanged v))
                                    |> ui.SetFillHorizontal
                                    |> ui.AlignTextLeft
                                    |> ui.SetColspan 5
                                ui.Button "Report a bug" (fun _ _ -> ())
                                    |> ui.AlignTextCenter
                                    |> ui.SetFillHorizontal
                                    |> ui.SetColspan 2
                                ui.Button "Contribute" (fun _ _ -> ()) 
                                    |> ui.AlignTextCenter
                                    |> ui.SetFillHorizontal
                                    |> ui.SetColspan 2
                                ui.Button "Fork" (fun _ _ -> ()) 
                                    |> ui.AlignTextCenter
                                    |> ui.SetFillHorizontal
                                    |> ui.SetColspan 2



                            |]

                    |]
            |]

        gridId

let init (game: NoobishGame<unit, DemoMessage, DemoModel>) () =
    { State = Buttons; ComboboxValue = "Option 1"; CheckboxValue = false; Padding = 5; Margin = 5; SliderAValue = 25.0f; StyleMode = DarkMode; FeatureText = "functional, extendable, net6.0 and cross-platform."; ListModel = Array.init 21 id; SelectedListItemIndex = 0}, Cmd.ofMsg(ShowButtons)

let update (game: NoobishGame<unit, DemoMessage, DemoModel>) (message: DemoMessage) (model: DemoModel) (gameTime: GameTime)=
    match message with
    | ShowButtons ->
        {model with State = Buttons}, Cmd.none
    | ShowContainers ->
        {model with State = Containers}, Cmd.none
    | ShowText ->
        {model with State = Text}, Cmd.none
    | ShowList ->
        {model with State = List}, Cmd.none
    | ShowSliders ->
        {model with State = Slider}, Cmd.none
    | ShowGithub ->
        {model with State = Github}, Cmd.none
    | SliderValueChanged v ->
        {model with SliderAValue = v}, Cmd.none
    | ComboboxValueChanged v ->
        {model with ComboboxValue = v}, Cmd.none
    | CheckboxValueChanged v ->
        {model with CheckboxValue = v}, Cmd.none
    | ChangePadding padding ->
        {model with Padding = padding}, Cmd.none
    | ChangeMargin margin ->
        {model with Margin = margin}, Cmd.none
    | FeaturesChanged s ->
        {model with FeatureText = s}, Cmd.none
    | ToggleDebug ->
        game.Noobish.Debug <- not (game.Noobish.Debug)
        model, Cmd.none
    | ToggleLightMode ->
        game.StyleSheetId <- "Light/Light"
        {model with StyleMode = LightMode}, Cmd.none
    | ToggleDarkMode ->
        game.StyleSheetId <- "Dark/Dark"
        {model with StyleMode = DarkMode}, Cmd.none
    | SelectListItem index ->
        {model with SelectedListItemIndex = index}, Cmd.none

let view game (model: DemoModel) dispatch =

    let title, content  =
        match model.State with
        | Buttons ->
            let content = Buttons.view game model dispatch
            "Buttons", content
        | Containers -> 
            let content = Containers.view game model dispatch
            "Containers", content
        | Text -> 
            let content = Text.view game model dispatch
            "Labels", content
        | List -> 
            let content = List.view game model dispatch
            "List", content
        | Slider -> 
            let content = Slider.view game model dispatch
            "Slider", content
        | Github -> 
            let content = Github.view game model dispatch
            "Github", content

    let ui = game.Noobish

    ui.Grid(12, 8)
    |> ui.SetChildren [|
        ui.PanelHorizontal() 
        |> ui.SetRowspan 1
        |> ui.SetColspan 3
        |> ui.SetChildren [|
            ui.Header "Noobish"
        |]
        ui.PanelWithGrid(12, 1)
        |> ui.SetRowspan 1 
        |> ui.SetColspan 9
        |> ui.SetChildren [|
            ui.Header title
            |> ui.SetColspan 6
            
            ui.Button "Dark" (fun e _ -> dispatch ToggleDarkMode) 
            |> ui.SetFill
            |> ui.SetColspan 2     
            |> ui.SetToggled (model.StyleMode = DarkMode )          
            ui.Button "Light" (fun e _ -> dispatch ToggleLightMode) 
            |> ui.SetFill
            |> ui.SetToggled (model.StyleMode = LightMode )       
            |> ui.SetColspan 2
            ui.Button "Debug" (
                fun e _ -> 
                    dispatch ToggleDebug
                ) 
            |> ui.SetFill
            |> ui.SetColspan 2
            |> ui.SetToggled (ui.Debug)       
            
        |]
        ui.PanelVertical() 
        |> ui.SetRowspan 7
        |> ui.SetColspan 3
        |> ui.SetChildren [|
            ui.Button "Buttons" (fun gameTime _ -> dispatch ShowButtons)
            |> ui.FillHorizontal 
            |> ui.SetToggled (model.State = Buttons)
            ui.Button "Text" (fun gameTime _ -> dispatch ShowText)
            |> ui.FillHorizontal 
            |> ui.SetToggled (model.State = Text)
            ui.Button "Containers" (fun gameTime _ -> dispatch ShowContainers)
            |> ui.FillHorizontal 
            |> ui.SetToggled (model.State = Containers)
            ui.Button "List" (fun gameTime _ -> dispatch ShowList)
            |> ui.FillHorizontal 
            |> ui.SetToggled (model.State = List)
            ui.Button "Slider" (fun gameTime _ -> dispatch ShowSliders)
            |> ui.FillHorizontal 
            |> ui.SetToggled (model.State = Slider)
            ui.Button "Github" (fun gameTime _ -> dispatch ShowGithub)
            |> ui.FillHorizontal 
            |> ui.SetToggled (model.State = Github)
        |]
        content
        |> ui.SetRowspan 7 
        |> ui.SetColspan 9
    |]


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
        |> Program2.withStyleSheet "Dark/Dark"
        |> Program2.withPreferHalfPixelOffset true
        |> Program2.withResolution 1280 720
        |> Program2.withMouseVisible true
        |> Program2.run
    0 // return an integer exit code