open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish


type ComponentId = 
| LabelsAndParagraphs= 1us
| Buttons = 2us
| Checkbox = 3us
| Slider = 6us


module ComponentId = 
    let toLocalId (cid: ComponentId) = LanguagePrimitives.EnumToValue cid 
    let ofLocalId (v: uint16): ComponentId = 
        LanguagePrimitives.EnumOfValue v

let private buildUi (components: NoobishComponentsV2) (width: float32) (height: float32)=

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
                |> NoobishV2.beginButton "Labels and Paragraphs" (ComponentId.toLocalId ComponentId.LabelsAndParagraphs)
                    |> NoobishV2.setMinHeight 28f
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Buttons" (ComponentId.toLocalId ComponentId.Buttons)
                    |> NoobishV2.setMinHeight 28f
                    |> NoobishV2.endButton
                |> NoobishV2.beginButton "Checkbox" (ComponentId.toLocalId ComponentId.Checkbox)
                    |> NoobishV2.setMinHeight 28f
                    |> NoobishV2.endButton
                |> NoobishV2.endPanel
            |> NoobishV2.beginPanel
                |> NoobishV2.setFill {Horizontal = true; Vertical = true}
                |> NoobishV2.setPadding {NoobishPadding.Top = 24f; Right = 24f; Bottom = 24f; Left = 24f}
                |> NoobishV2.beginHeader "Preview"
                    |> NoobishV2.setMinHeight 40f
                    |> NoobishV2.endHeader
                |> NoobishV2.beginLabel "Sample label"
                    |> NoobishV2.setMinHeight 32f
                    |> NoobishV2.endLabel
                |> NoobishV2.beginButton "Sample button" 1us
                    |> NoobishV2.setMinHeight 48f
                    |> NoobishV2.setFill {Horizontal = false; Vertical = false}
                    |> NoobishV2.setWantsToggle true
                    |> NoobishV2.endButton
                |> NoobishV2.beginCheckbox "Sample checkbox" 2us
                    |> NoobishV2.setMinHeight 40f
                    |> NoobishV2.endCheckbox
                |> NoobishV2.beginSlider (0f, 100f) 1f 50f (ComponentId.toLocalId ComponentId.Slider)
                    |> NoobishV2.setMinHeight 40f
                    |> NoobishV2.endSlider
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

    let styleSheetId = "Dark/Dark"
    let fontEffectId = "MSDFFontEffect"

    do
        game.Content.RootDirectory <- "Content"
        game.IsMouseVisible <- true

    override _.Initialize() =
        base.Initialize()
        let screenWidth = float32 game.GraphicsDevice.Viewport.Width
        let screenHeight = float32 game.GraphicsDevice.Viewport.Height
        buildUi components screenWidth screenHeight
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
        NoobishMeasureV2.measureFrame game.Content styleSheet components
        NoobishLayoutV2.layoutFrame components screenWidth screenHeight
        NoobishInputV2.process inputState components inputBuffer

        let lastClicked = ComponentId.ofLocalId inputBuffer.LastClickedLocalId
        match lastClicked with 
        | ComponentId.LabelsAndParagraphs -> 
            System.Console.WriteLine("Clicked: LabelsAndParagraphs")
        | ComponentId.Buttons -> 
            System.Console.WriteLine("Clicked: Buttons")
        | ComponentId.Checkbox -> 
            System.Console.WriteLine("Clicked: Checkbox")
        | _ -> ()

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
