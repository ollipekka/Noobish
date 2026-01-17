open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish


type ComponentId = 
| Text = 1us
| Layouts = 2us
| Buttons = 3us
| Checkbox = 4us
| Slider = 5us


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
        |> NoobishV2.beginLabel model.LabelText
            |> NoobishV2.setMinHeight 32f
            |> NoobishV2.endLabel
        |> NoobishV2.beginParagraph model.ParagraphText
            |> NoobishV2.setMinHeight 64f
            |> NoobishV2.endParagraph

module ButtonsDemo =
    [<Literal>]
    let PrimaryButtonId = 101us

    type Model() =
        member val PrimaryPressed = false with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginButton "Primary button" PrimaryButtonId
            |> NoobishV2.setMinHeight 48f
            |> NoobishV2.setFill {Horizontal = false; Vertical = false}
            |> NoobishV2.setWantsToggle true
            |> NoobishV2.setToggled model.PrimaryPressed
            |> NoobishV2.endButton

module CheckboxDemo =
    [<Literal>]
    let CheckboxId = 201us

    type Model() =
        member val IsChecked = false with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginCheckbox "Sample checkbox" CheckboxId
            |> NoobishV2.setMinHeight 40f
            |> NoobishV2.setToggled model.IsChecked
            |> NoobishV2.endCheckbox

module SliderDemo =
    [<Literal>]
    let SliderId = 301us

    type Model() =
        member val Value = 50f with get, set

    let buildUi (model: Model) (parentCtx: ComponentContextV2) =
        parentCtx
        |> NoobishV2.beginSlider (0f, 100f) 1f model.Value SliderId
            |> NoobishV2.setMinHeight 40f
            |> NoobishV2.endSlider

[<RequireQualifiedAccess>]
type DemoPage =
| Text
| Buttons
| Checkbox
| Slider

[<RequireQualifiedAccess>]
type DemoSubModel =
| Text of TextDemo.Model
| Buttons of ButtonsDemo.Model
| Checkbox of CheckboxDemo.Model
| Slider of SliderDemo.Model

type DemoModel () =
    let text = TextDemo.Model()
    let buttons = ButtonsDemo.Model()
    let checkbox = CheckboxDemo.Model()
    let slider = SliderDemo.Model()
    let mutable viewState = DemoPage.Text
    let mutable view = DemoSubModel.Text text

    member _.ViewState
        with get() = viewState
        and private set value = viewState <- value

    member _.View
        with get() = view
        and private set value = view <- value

    member _.Text = text
    member _.Buttons = buttons
    member _.Checkbox = checkbox
    member _.Slider = slider

    member this.SetViewState(value: DemoPage) =
        viewState <- value
        view <-
            match value with
            | DemoPage.Text -> DemoSubModel.Text text
            | DemoPage.Buttons -> DemoSubModel.Buttons buttons
            | DemoPage.Checkbox -> DemoSubModel.Checkbox checkbox
            | DemoPage.Slider -> DemoSubModel.Slider slider

let private buildUi (components: NoobishComponentsV2) (width: float32) (height: float32) (model: DemoModel)=

    components.Clear()
    let rootCtx =
        NoobishV2.beginFrame "Demo/Simple" components
        |> NoobishV2.beginStackHorizontal
            |> NoobishV2.setFill {Horizontal = true; Vertical = true}
            |> NoobishV2.beginPanel
                |> NoobishV2.setMinWidth 220f
                |> NoobishV2.setFill {Horizontal = false; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 16f; Right = 16f; Bottom = 16f; Left = 16f}
                |> NoobishV2.beginHeader "Components"
                    |> NoobishV2.setMinHeight 32f
                    |> NoobishV2.endHeader
                |> NoobishV2.beginButton "Labels and Paragraphs" (ComponentId.toLocalId ComponentId.Text)
                    |> NoobishV2.setMinHeight 28f
                    |> NoobishV2.setWantsToggle true 
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Text)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Buttons" (ComponentId.toLocalId ComponentId.Buttons)
                    |> NoobishV2.setMinHeight 28f
                    |> NoobishV2.setWantsToggle true 
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Buttons)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Checkbox" (ComponentId.toLocalId ComponentId.Checkbox)
                    |> NoobishV2.setMinHeight 28f
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Checkbox)
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Slider" (ComponentId.toLocalId ComponentId.Slider)
                    |> NoobishV2.setMinHeight 28f
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.setToggled (model.ViewState = DemoPage.Slider)
                    |> NoobishV2.endButton
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 24f; Right = 24f; Bottom = 24f; Left = 24f}
                |> NoobishV2.beginHeader "Preview"
                    |> NoobishV2.setMinHeight 40f
                    |> NoobishV2.endHeader

    let previewCtx =
        match model.View with
        | DemoSubModel.Text subModel ->
            TextDemo.buildUi subModel rootCtx
        | DemoSubModel.Buttons subModel ->
            ButtonsDemo.buildUi subModel rootCtx
        | DemoSubModel.Checkbox subModel ->
            CheckboxDemo.buildUi subModel rootCtx
        | DemoSubModel.Slider subModel ->
            SliderDemo.buildUi subModel rootCtx

    previewCtx
        |> NoobishV2.endPanel
        |> NoobishV2.endStackHorizontal
        |> NoobishV2.endFrame width height


type SimpleDemoGame() as game =
    inherit Game()

    let graphics = 
        let gdm = new GraphicsDeviceManager(game)

        gdm.PreferMultiSampling <- true
        gdm.PreferHalfPixelOffset <- true
        gdm
        
    let components = NoobishComponentsV2(64)
    let renderer = NoobishMonoGameRendererV2()
    let inputBuffer = InputBufferV2(64)
    let inputState = NoobishInputState()

    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable textBatch = Unchecked.defaultof<TextBatch>

    let demoModel = DemoModel()

    let styleSheetId = "Dark/Dark"
    let fontEffectId = "MSDFFontEffect"

    do
        game.Content.RootDirectory <- "Content"
        game.IsMouseVisible <- true

    override _.Initialize() =
        base.Initialize()
        let screenWidth = float32 game.GraphicsDevice.Viewport.Width
        let screenHeight = float32 game.GraphicsDevice.Viewport.Height
        ()

    override _.LoadContent() =
        spriteBatch <- new SpriteBatch(game.GraphicsDevice)
        let fontEffect = game.Content.Load<Effect>(fontEffectId)
        textBatch <- new TextBatch(game.GraphicsDevice, struct(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height), fontEffect, 1024)

    override this.Update(gameTime) =
        let screenWidth = float32 game.GraphicsDevice.Viewport.Width
        let screenHeight = float32 game.GraphicsDevice.Viewport.Height
        inputState.Update()

        let styleSheet = game.Content.Load<Noobish.Styles.NoobishStyleSheet>(styleSheetId)
        
        buildUi components screenWidth screenHeight demoModel
        NoobishMeasureV2.measureFrame game.Content styleSheet components
        NoobishLayoutV2.layoutFrame components screenWidth screenHeight
        NoobishInputV2.process inputState components inputBuffer

        let lastClicked = ComponentId.ofLocalId inputBuffer.LastClickedLocalId
        match lastClicked with 
        | ComponentId.Text -> 
            demoModel.SetViewState DemoPage.Text
        | ComponentId.Buttons -> 
            demoModel.SetViewState DemoPage.Buttons
        | ComponentId.Checkbox -> 
            demoModel.SetViewState DemoPage.Checkbox
        | ComponentId.Slider ->
            demoModel.SetViewState DemoPage.Slider
        | _ -> ()

        match demoModel.ViewState with
        | DemoPage.Buttons ->
            if inputBuffer.WasClicked ButtonsDemo.PrimaryButtonId then
                demoModel.Buttons.PrimaryPressed <- not demoModel.Buttons.PrimaryPressed
        | DemoPage.Checkbox ->
            if inputBuffer.WasClicked CheckboxDemo.CheckboxId then
                demoModel.Checkbox.IsChecked <- not demoModel.Checkbox.IsChecked
        | DemoPage.Slider ->
            match inputBuffer.TryGetSliderChanged SliderDemo.SliderId with
            | ValueSome value -> demoModel.Slider.Value <- value
            | ValueNone -> ()
        | DemoPage.Text -> ()

        base.Update(gameTime)

    override _.Draw(gameTime) =
        game.GraphicsDevice.Clear(Color.Black)

        renderer.Draw components game.GraphicsDevice game.Content spriteBatch textBatch styleSheetId gameTime

        base.Draw(gameTime)

[<EntryPoint>]
let main _argv =
    use game = new SimpleDemoGame()
    game.Run()
    0
