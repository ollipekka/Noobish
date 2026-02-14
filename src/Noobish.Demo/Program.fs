open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish


type ComponentId = 
| Text = 1us
| TextInput = 2us
| Buttons = 3us
| Checkbox = 4us
| Slider = 5us
| Grid = 6us
| Scroll = 7us
| TextClip = 8us
| Mouse = 9us

let loremIpsum1 =
    "Scroll me!\n\n Lorem\nipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum."

let loremIpsum2 =
    "Part 2\n Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem. Ut enim ad minima veniam, quis nostrum exercitationem ullam corporis suscipit laboriosam, nisi ut aliquid ex ea commodi consequatur? Quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur, vel illum qui dolorem eum fugiat quo voluptas nulla pariatur?"


module ComponentId = 
    let toLocalId (cid: ComponentId) = LanguagePrimitives.EnumToValue cid 
    let ofLocalId (v: uint16): ComponentId = 
        LanguagePrimitives.EnumOfValue v

module TextDemo =
    type Model() =
        member val LabelText = "Sample label" with get, set
        member val ParagraphText = "Sample paragraph text to show wrapping and layout." with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginGrid (2, 2)
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.beginLabel "Intro"
                    |> NoobishV2.setMinHeight 32f
                    |> NoobishV2.endLabel
                |> NoobishV2.beginHorizontalRule
                    |> NoobishV2.endHorizontalRule
                |> NoobishV2.beginParagraph model.ParagraphText
                    |> NoobishV2.setMinHeight 64f
                    |> NoobishV2.endParagraph
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.beginLabel "Details"
                    |> NoobishV2.setMinHeight 32f
                    |> NoobishV2.endLabel
                |> NoobishV2.beginHorizontalRule
                    |> NoobishV2.endHorizontalRule
                |> NoobishV2.beginParagraph model.ParagraphText
                    |> NoobishV2.setMinHeight 64f
                    |> NoobishV2.endParagraph
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.beginLabel "Notes"
                    |> NoobishV2.setMinHeight 32f
                    |> NoobishV2.endLabel
                |> NoobishV2.beginHorizontalRule
                    |> NoobishV2.endHorizontalRule
                |> NoobishV2.beginParagraph model.ParagraphText
                    |> NoobishV2.setMinHeight 64f
                    |> NoobishV2.endParagraph
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.beginLabel "Summary"
                    |> NoobishV2.setMinHeight 32f
                    |> NoobishV2.endLabel
                |> NoobishV2.beginHorizontalRule
                    |> NoobishV2.endHorizontalRule
                |> NoobishV2.beginParagraph model.ParagraphText
                    |> NoobishV2.setMinHeight 64f
                    |> NoobishV2.endParagraph
                |> NoobishV2.endPanel
            |> NoobishV2.endGrid

module TextInputDemo =
    [<Literal>]
    let TopTextboxId = 401us

    [<Literal>]
    let BottomTextboxId = 402us

    type Model() =
        member val TopText = "Type here..." with get, set
        member val BottomText = "Type here too..." with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginGrid (1, 2)
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginLabel "Textbox A"
                    |> NoobishV2.endLabel
                |> NoobishV2.beginTextbox model.TopText TopTextboxId
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setMinHeight 40f
                    |> NoobishV2.endTextbox
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginLabel "Textbox B"
                    |> NoobishV2.endLabel
                |> NoobishV2.beginTextbox model.BottomText BottomTextboxId
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setMinHeight 40f
                    |> NoobishV2.endTextbox
                |> NoobishV2.endPanel
            |> NoobishV2.endGrid

module ButtonsDemo =
    [<Literal>]
    let PrimaryButtonId = 101us
    [<Literal>]
    let OverlayButtonId = 102us
    [<Literal>]
    let OverlayRootId = 103us
    [<Literal>]
    let OverlayScrimId = 104us

    type Model() =
        member val PrimaryPressed = false with get, set
        member val ShowOverlay = false with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginButton "Primary button" PrimaryButtonId
            |> NoobishV2.setMinHeight 48f
            |> NoobishV2.setFill {Horizontal = false; Vertical = false}
            |> NoobishV2.setWantsToggle true
            |> NoobishV2.setToggled model.PrimaryPressed
            |> NoobishV2.endButton
        |> NoobishV2.beginButton "Show scrim" OverlayButtonId
            |> NoobishV2.setMinHeight 48f
            |> NoobishV2.setFill {Horizontal = false; Vertical = false}
            |> NoobishV2.endButton

module CheckboxDemo =
    [<Literal>]
    let CheckboxId = 201us

    type Model() =
        member val IsChecked = false with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginCheckbox "Sample checkbox" CheckboxId
            |> NoobishV2.setToggled model.IsChecked
            |> NoobishV2.endCheckbox

module SliderDemo =
    [<Literal>]
    let SliderId = 301us
    [<Literal>]
    let ProgressButtonId = 302us
    [<Literal>]
    let SquareProgressButtonId = 303us

    type Model() =
        member val Value = 50f with get, set
        member val ProgressButtonCount = 0 with get, set
        member val ProgressButtonValue = 0f with get, set
        member val SquareProgressButtonCount = 0 with get, set
        member val SquareProgressButtonValue = 0f with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginGrid (2, 3)
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginLabel "Slider"
                    |> NoobishV2.endLabel
                |> NoobishV2.beginSlider (0f, 100f) 1f model.Value SliderId
                    |> NoobishV2.setMinHeight 40f
                    |> NoobishV2.endSlider
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginLabel "Progress"
                    |> NoobishV2.endLabel
                |> NoobishV2.beginProgressBar (model.Value / 100f)
                    |> NoobishV2.setProgressSegments 4
                    |> NoobishV2.endProgressBar
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setColspan 2
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginLabel "Combined"
                    |> NoobishV2.endLabel
                |> NoobishV2.beginSlider (0f, 100f) 1f model.Value SliderId
                    |> NoobishV2.setMinHeight 40f
                    |> NoobishV2.endSlider
                |> NoobishV2.beginProgressBar (1f - model.Value / 100f)
                    |> NoobishV2.setProgressSegments 10
                    |> NoobishV2.endProgressBar
                |> NoobishV2.endPanel
                |> NoobishV2.beginGrid (2, 1)
                    |> NoobishV2.setColspan 2
                    |> NoobishV2.beginButton $"Radial Progress ({model.ProgressButtonCount})" ProgressButtonId
                        |> NoobishV2.setMinHeight 72f
                        |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                        |> NoobishV2.setProgress model.ProgressButtonValue
                        |> NoobishV2.setProgressStyle NoobishProgressStyle.Radial
                        |> NoobishV2.setProgressSegments 4
                        |> NoobishV2.endButton
                    |> NoobishV2.beginButton $"Square Pie Progress ({model.SquareProgressButtonCount})" SquareProgressButtonId
                        |> NoobishV2.setMinHeight 72f
                        |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                        |> NoobishV2.setProgress model.SquareProgressButtonValue
                        |> NoobishV2.setProgressStyle NoobishProgressStyle.RadialSquare
                        |> NoobishV2.setProgressSegments 8
                        |> NoobishV2.endButton
                |> NoobishV2.endGrid
            |> NoobishV2.endGrid

module GridDemo =
    type Model() =
        member val Header = "Grid Layout" with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginGrid (3, 3)
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setRowspan 2
                |> NoobishV2.setColspan 2
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginHeader model.Header
                    |> NoobishV2.endHeader
                |> NoobishV2.beginParagraph "Spans 2 columns and 2 rows."
                    |> NoobishV2.endParagraph
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginLabel "Top Right"
                    |> NoobishV2.endLabel
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginLabel "Bottom Left"
                    |> NoobishV2.endLabel
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setColspan 3
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginParagraph "Spans three columns."
                    |> NoobishV2.endParagraph
                |> NoobishV2.endPanel
            |> NoobishV2.endGrid

[<RequireQualifiedAccess>]
module ScrollDemo =
    type Model() =
        member val Text = loremIpsum1 + "\n\n" + loremIpsum2 with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginPanel
            |> NoobishV2.setFill {Horizontal = true; Vertical = true}
            |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
            |> NoobishV2.setScrollVertical
            |> NoobishV2.beginParagraph model.Text
                |> NoobishV2.setMinWidth 320f
                |> NoobishV2.setFillHorizontal
                |> NoobishV2.endParagraph
            |> NoobishV2.endPanel

module TextClipDemo =
    type Model() =
        member val Text = "Click a ConstructionVehicle to see abilities." with get, set
        member val LongWordText = "Supercalifragilisticexpialidocious should clip." with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginStackHorizontal
            |> NoobishV2.beginPanel
                |> NoobishV2.setMinWidth 148f
                |> NoobishV2.setMinHeight 120f
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginHeader "Clipped Word"
                    |> NoobishV2.endHeader
                |> NoobishV2.beginParagraph model.Text
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.endParagraph
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setMinWidth 148f
                |> NoobishV2.setMinHeight 120f
                |> NoobishV2.setPadding {NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f}
                |> NoobishV2.beginHeader "Very Long Word"
                    |> NoobishV2.endHeader
                |> NoobishV2.beginParagraph model.LongWordText
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.endParagraph
                |> NoobishV2.endPanel
            |> NoobishV2.endStackHorizontal

module MouseDemo =
    [<Literal>]
    let LeftButtonId = 701us
    [<Literal>]
    let RightButtonId = 702us
    [<Literal>]
    let MiddleButtonId = 703us
    [<Literal>]
    let XButton1Id = 704us
    [<Literal>]
    let XButton2Id = 705us

    type Model() =
        member val LeftCount = 0 with get, set
        member val RightCount = 0 with get, set
        member val MiddleCount = 0 with get, set
        member val XButton1Count = 0 with get, set
        member val XButton2Count = 0 with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginPanel
            |> NoobishV2.setFill { Horizontal = true; Vertical = true }
            |> NoobishV2.setPadding { NoobishPadding.Top = 12f; Right = 12f; Bottom = 12f; Left = 12f }
            |> NoobishV2.beginParagraph "Click each button with the matching mouse button."
                |> NoobishV2.endParagraph
            |> NoobishV2.beginButton $"Left ({model.LeftCount})" LeftButtonId
                |> NoobishV2.setMinHeight 42f
                |> NoobishV2.endButton
            |> NoobishV2.beginButton $"Right ({model.RightCount})" RightButtonId
                |> NoobishV2.setMinHeight 42f
                |> NoobishV2.endButton
            |> NoobishV2.beginButton $"Middle ({model.MiddleCount})" MiddleButtonId
                |> NoobishV2.setMinHeight 42f
                |> NoobishV2.endButton
            |> NoobishV2.beginButton $"XButton1 ({model.XButton1Count})" XButton1Id
                |> NoobishV2.setMinHeight 42f
                |> NoobishV2.endButton
            |> NoobishV2.beginButton $"XButton2 ({model.XButton2Count})" XButton2Id
                |> NoobishV2.setMinHeight 42f
                |> NoobishV2.endButton
            |> NoobishV2.endPanel

[<RequireQualifiedAccess>]
type DemoPage =
| Text
| TextInput
| Buttons
| Checkbox
| Slider
| Grid
| Scroll
| TextClip
| Mouse

[<RequireQualifiedAccess>]
type DemoSubModel =
| Text of TextDemo.Model
| TextInput of TextInputDemo.Model
| Buttons of ButtonsDemo.Model
| Checkbox of CheckboxDemo.Model
| Slider of SliderDemo.Model
| Grid of GridDemo.Model
| Scroll of ScrollDemo.Model
| TextClip of TextClipDemo.Model
| Mouse of MouseDemo.Model

type DemoModel () =
    let text = TextDemo.Model()
    let textInput = TextInputDemo.Model()
    let buttons = ButtonsDemo.Model()
    let checkbox = CheckboxDemo.Model()
    let slider = SliderDemo.Model()
    let grid = GridDemo.Model()
    let scroll = ScrollDemo.Model()
    let textClip = TextClipDemo.Model()
    let mouse = MouseDemo.Model()
    let mutable viewState = DemoPage.Text
    let mutable view = DemoSubModel.Text text

    member _.ViewState
        with get() = viewState
        and private set value = viewState <- value

    member _.View
        with get() = view
        and private set value = view <- value

    member _.Text = text
    member _.TextInput = textInput
    member _.Buttons = buttons
    member _.Checkbox = checkbox
    member _.Slider = slider
    member _.Grid = grid
    member _.Scroll = scroll
    member _.TextClip = textClip
    member _.Mouse = mouse

    member this.SetViewState(value: DemoPage) =
        viewState <- value
        view <-
            match value with
            | DemoPage.Text -> DemoSubModel.Text text
            | DemoPage.TextInput -> DemoSubModel.TextInput textInput
            | DemoPage.Buttons -> DemoSubModel.Buttons buttons
            | DemoPage.Checkbox -> DemoSubModel.Checkbox checkbox
            | DemoPage.Slider -> DemoSubModel.Slider slider
            | DemoPage.Grid -> DemoSubModel.Grid grid
            | DemoPage.Scroll -> DemoSubModel.Scroll scroll
            | DemoPage.TextClip -> DemoSubModel.TextClip textClip
            | DemoPage.Mouse -> DemoSubModel.Mouse mouse

let private buildUi (ui: NoobishUserInterface) (width: float32) (height: float32) (model: DemoModel)=

    let rootCtx =
        ui.BeginFrame "Demo/Simple"
        |> NoobishV2.beginStackHorizontal
            
            |> NoobishV2.setFill {Horizontal = true; Vertical = true}
            |> NoobishV2.beginPanel
                |> NoobishV2.setMinWidth 220f
                |> NoobishV2.setFill {Horizontal = false; Vertical = true}
                |> NoobishV2.beginHeader "Components"
                    |> NoobishV2.endHeader
                |> NoobishV2.horizontalRule
                |> NoobishV2.beginButton "Text" (ComponentId.toLocalId ComponentId.Text)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true 
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Text)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Text Input" (ComponentId.toLocalId ComponentId.TextInput)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true 
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.TextInput)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Buttons" (ComponentId.toLocalId ComponentId.Buttons)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true 
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Buttons)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Checkbox" (ComponentId.toLocalId ComponentId.Checkbox)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Checkbox)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Slider" (ComponentId.toLocalId ComponentId.Slider)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Slider)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Grid" (ComponentId.toLocalId ComponentId.Grid)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Grid)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Scroll" (ComponentId.toLocalId ComponentId.Scroll)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Scroll)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Text Clip" (ComponentId.toLocalId ComponentId.TextClip)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.TextClip)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Mouse" (ComponentId.toLocalId ComponentId.Mouse)
                    |> NoobishV2.setFillHorizontal
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Mouse)
                    |> NoobishV2.endButton
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.beginHeader "Preview"
                    |> NoobishV2.endHeader
                |> NoobishV2.horizontalRule

    let previewCtx =
        match model.View with
        | DemoSubModel.Text subModel ->
            TextDemo.buildUi subModel rootCtx
        | DemoSubModel.TextInput subModel ->
            TextInputDemo.buildUi subModel rootCtx
        | DemoSubModel.Buttons subModel ->
            ButtonsDemo.buildUi subModel rootCtx
        | DemoSubModel.Checkbox subModel ->
            CheckboxDemo.buildUi subModel rootCtx
        | DemoSubModel.Slider subModel ->
            SliderDemo.buildUi subModel rootCtx
        | DemoSubModel.Grid subModel ->
            GridDemo.buildUi subModel rootCtx
        | DemoSubModel.Scroll subModel ->
            ScrollDemo.buildUi subModel rootCtx
        | DemoSubModel.TextClip subModel ->
            TextClipDemo.buildUi subModel rootCtx
        | DemoSubModel.Mouse subModel ->
            MouseDemo.buildUi subModel rootCtx

    let endCtx =
        previewCtx
        |> NoobishV2.endPanel
        |> NoobishV2.endStackHorizontal

    let finalCtx =
        if model.Buttons.ShowOverlay then
            endCtx
            |> NoobishV2.beginOverlayRoot ButtonsDemo.OverlayRootId
                |> NoobishV2.beginGrid (9, 6)
                    |> NoobishV2.beginSpace |> NoobishV2.setColspan 9 |> NoobishV2.endSpace
                    |> NoobishV2.beginSpace |> NoobishV2.setRowspan 5 |> NoobishV2.endSpace
                    |> NoobishV2.beginOverlayScrim ButtonsDemo.OverlayScrimId
                        |> NoobishV2.setColspan 7 |> NoobishV2.setRowspan 4
                        |> NoobishV2.endOverlayScrim
                |> NoobishV2.endOverlayRoot
        else
            endCtx

    ui.EndFrame(width, height, finalCtx)


type SimpleDemoGame() as game =
    inherit Game()

    let graphics = 
        let gdm = new GraphicsDeviceManager(game)

        gdm.PreferMultiSampling <- true
        gdm.PreferHalfPixelOffset <- true
        gdm
        
    let ui = NoobishUserInterface(64)
    let renderer = NoobishMonoGameRendererV2()
    let inputState = NoobishInputState()

    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable textBatch = Unchecked.defaultof<TextBatch>
    let mutable renderContext = Unchecked.defaultof<NoobishMonoGameRenderContext>

    let demoModel = DemoModel()

    let styleSheetId = "Dark/Dark"
    let fontEffectId = "MSDFFontEffect"
    let mutable styleSheet = Unchecked.defaultof<Noobish.Styles.NoobishStyleSheet>
    let mutable measureProvider = Unchecked.defaultof<NoobishMonoGameMeasureProvider>

    do
        game.Content.RootDirectory <- "Content"
        game.IsMouseVisible <- true

    override _.Initialize() =
        base.Initialize()
        game.Window.TextInput.Add(fun e ->
            inputState.EnqueueTextInput e.Character
        )
        let screenWidth = float32 game.GraphicsDevice.Viewport.Width
        let screenHeight = float32 game.GraphicsDevice.Viewport.Height
        ()

    override _.LoadContent() =
        spriteBatch <- new SpriteBatch(game.GraphicsDevice)
        let fontEffect = game.Content.Load<Effect>(fontEffectId)
        textBatch <- new TextBatch(game.GraphicsDevice, struct(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height), fontEffect, 1024)
        renderContext <- new NoobishMonoGameRenderContext(game.GraphicsDevice, game.Content, spriteBatch, textBatch)
        styleSheet <- game.Content.Load<Noobish.Styles.NoobishStyleSheet>(styleSheetId)
        measureProvider <- new NoobishMonoGameMeasureProvider(game.Content, styleSheet)

    override this.Update(gameTime) =
        inputState.Update()

        let lastClicked = ComponentId.ofLocalId (ui.GetClicked())
        match lastClicked with 
        | ComponentId.Text -> 
            demoModel.SetViewState DemoPage.Text
        | ComponentId.TextInput ->
            demoModel.SetViewState DemoPage.TextInput
        | ComponentId.Buttons -> 
            demoModel.SetViewState DemoPage.Buttons
        | ComponentId.Checkbox -> 
            demoModel.SetViewState DemoPage.Checkbox
        | ComponentId.Slider ->
            demoModel.SetViewState DemoPage.Slider
        | ComponentId.Grid ->
            demoModel.SetViewState DemoPage.Grid
        | ComponentId.Scroll ->
            demoModel.SetViewState DemoPage.Scroll
        | ComponentId.TextClip ->
            demoModel.SetViewState DemoPage.TextClip
        | ComponentId.Mouse ->
            demoModel.SetViewState DemoPage.Mouse
        | _ -> ()

        match demoModel.ViewState with
        | DemoPage.Buttons ->
            if ui.WasClicked(ButtonsDemo.PrimaryButtonId, NoobishMouseButtonId.Left) then
                demoModel.Buttons.PrimaryPressed <- not demoModel.Buttons.PrimaryPressed
            if ui.WasClicked(ButtonsDemo.OverlayButtonId, NoobishMouseButtonId.Left) then
                demoModel.Buttons.ShowOverlay <- true
            if ui.WasClicked(ButtonsDemo.OverlayScrimId, NoobishMouseButtonId.Left) then
                demoModel.Buttons.ShowOverlay <- false
        | DemoPage.Checkbox ->
            if ui.WasClicked(CheckboxDemo.CheckboxId, NoobishMouseButtonId.Left) then
                demoModel.Checkbox.IsChecked <- not demoModel.Checkbox.IsChecked
        | DemoPage.Slider ->
            match ui.TryGetSliderChanged SliderDemo.SliderId with
            | ValueSome value -> demoModel.Slider.Value <- value
            | ValueNone -> ()
            if ui.WasClicked(SliderDemo.ProgressButtonId, NoobishMouseButtonId.Left) then
                demoModel.Slider.ProgressButtonCount <- min 10 (demoModel.Slider.ProgressButtonCount + 1)
                demoModel.Slider.ProgressButtonValue <- min 1f (demoModel.Slider.ProgressButtonValue + 0.1f)
            if ui.WasClicked(SliderDemo.ProgressButtonId, NoobishMouseButtonId.Right) then
                demoModel.Slider.ProgressButtonCount <- max 0 (demoModel.Slider.ProgressButtonCount - 1)
                demoModel.Slider.ProgressButtonValue <- max 0f (demoModel.Slider.ProgressButtonValue - 0.1f)
            if ui.WasClicked(SliderDemo.SquareProgressButtonId, NoobishMouseButtonId.Left) then
                demoModel.Slider.SquareProgressButtonCount <- min 10 (demoModel.Slider.SquareProgressButtonCount + 1)
                demoModel.Slider.SquareProgressButtonValue <- min 1f (demoModel.Slider.SquareProgressButtonValue + 0.1f)
            if ui.WasClicked(SliderDemo.SquareProgressButtonId, NoobishMouseButtonId.Right) then
                demoModel.Slider.SquareProgressButtonCount <- max 0 (demoModel.Slider.SquareProgressButtonCount - 1)
                demoModel.Slider.SquareProgressButtonValue <- max 0f (demoModel.Slider.SquareProgressButtonValue - 0.1f)
        | DemoPage.Grid -> ()
        | DemoPage.Scroll -> ()
        | DemoPage.Text -> ()
        | DemoPage.TextClip -> ()
        | DemoPage.Mouse ->
            if ui.WasClicked(MouseDemo.LeftButtonId, NoobishMouseButtonId.Left) then
                demoModel.Mouse.LeftCount <- demoModel.Mouse.LeftCount + 1
            if ui.WasClicked(MouseDemo.RightButtonId, NoobishMouseButtonId.Right) then
                demoModel.Mouse.RightCount <- demoModel.Mouse.RightCount + 1
            if ui.WasClicked(MouseDemo.MiddleButtonId, NoobishMouseButtonId.Middle) then
                demoModel.Mouse.MiddleCount <- demoModel.Mouse.MiddleCount + 1
            if ui.WasClicked(MouseDemo.XButton1Id, NoobishMouseButtonId.XButton1) then
                demoModel.Mouse.XButton1Count <- demoModel.Mouse.XButton1Count + 1
            if ui.WasClicked(MouseDemo.XButton2Id, NoobishMouseButtonId.XButton2) then
                demoModel.Mouse.XButton2Count <- demoModel.Mouse.XButton2Count + 1
        | DemoPage.TextInput ->
            match ui.TryGetTextChanged TextInputDemo.TopTextboxId with
            | ValueSome text -> demoModel.TextInput.TopText <- text
            | ValueNone -> ()
            match ui.TryGetTextChanged TextInputDemo.BottomTextboxId with
            | ValueSome text -> demoModel.TextInput.BottomText <- text
            | ValueNone -> ()

        base.Update(gameTime)

    override _.Draw(gameTime) =
        game.GraphicsDevice.Clear(Color.Black)

        let screenWidth = float32 game.GraphicsDevice.Viewport.Width
        let screenHeight = float32 game.GraphicsDevice.Viewport.Height
        buildUi ui screenWidth screenHeight demoModel
        NoobishV2MonoGame.processFrameWith measureProvider ui.Components screenWidth screenHeight inputState ui.InputBuffer
        renderer.Draw ui.Components renderContext styleSheetId gameTime

        base.Draw(gameTime)

[<EntryPoint>]
let main _argv =
    use game = new SimpleDemoGame()
    game.Run()
    0
