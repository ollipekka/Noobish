open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Noobish
open Noobish.Styles

let private buildUi (components: NoobishComponentsV2) =
    let rootCtx =
        NoobishV2.beginFrame "Demo/Simple" components
        |> NoobishV2.beginPanel

    let rootId = rootCtx.ComponentId
    let headerCtx = NoobishV2.beginHeader "Noobish V2" rootCtx
    let bodyCtx = NoobishV2.beginParagraph "A tiny demo screen to grow from." rootCtx
    let buttonCtx = NoobishV2.beginButton "Get Started" 1us rootCtx

    let rootIndex = int rootId.Index
    let headerIndex = int headerCtx.ComponentId.Index
    let bodyIndex = int bodyCtx.ComponentId.Index
    let buttonIndex = int buttonCtx.ComponentId.Index

    components.Fill.[rootIndex] <- {Horizontal = true; Vertical = true}
    components.Padding.[rootIndex] <- {NoobishPadding.Top = 24f; Right = 24f; Bottom = 24f; Left = 24f}

    components.MinSize.[headerIndex] <- {Width = 0f; Height = 48f}
    components.MinSize.[bodyIndex] <- {Width = 0f; Height = 96f}
    components.MinSize.[buttonIndex] <- {Width = 0f; Height = 40f}
    components.Fill.[buttonIndex] <- {Horizontal = true; Vertical = false}

    components.ReleaseContext headerCtx
    components.ReleaseContext bodyCtx
    components.ReleaseContext buttonCtx
    components.ReleaseContext rootCtx

    rootId

let private buildUi2 (components: NoobishComponentsV2) (width: float32) (height: float32)=

    NoobishV2.beginFrame "Demo/Simple" components
        |> NoobishV2.beginPanel
            |> NoobishV2.setFill {Horizontal = true; Vertical = true}
            |> NoobishV2.setPadding {NoobishPadding.Top = 24f; Right = 24f; Bottom = 24f; Left = 24f}
            |> NoobishV2.beginHeader "Noobish V2"
                |> NoobishV2.setMinSize {Width = 0f; Height = 48f}
                |> NoobishV2.endHeader
            |> NoobishV2.beginParagraph "A tiny demo screen to grow from."
                |> NoobishV2.setMinSize {Width = 0f; Height = 96f}
                |> NoobishV2.endParagraph
            |> NoobishV2.beginButton "Get Started" 1us
                |> NoobishV2.setMinSize {Width = 0f; Height = 40f}
                |> NoobishV2.setFill {Horizontal = true; Vertical = false}
                |> NoobishV2.endButton
            |> NoobishV2.endPanel
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
        buildUi2 components screenWidth screenHeight
        ()

    override _.LoadContent() =
        spriteBatch <- new SpriteBatch(game.GraphicsDevice)
        let fontEffect = game.Content.Load<Effect>(fontEffectId)
        textBatch <- new TextBatch(game.GraphicsDevice, struct(game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height), fontEffect, 1024)

    override _.Draw(gameTime) =
        game.GraphicsDevice.Clear(Color.Black)

        let screenWidth = float32 game.GraphicsDevice.Viewport.Width
        let screenHeight = float32 game.GraphicsDevice.Viewport.Height
        NoobishLayoutV2.layoutFrame components screenWidth screenHeight

        renderer.Draw components game.GraphicsDevice game.Content spriteBatch textBatch styleSheetId gameTime

        base.Draw(gameTime)

[<EntryPoint>]
let main _argv =
    use game = new SimpleDemoGame()
    game.Run()
    0
