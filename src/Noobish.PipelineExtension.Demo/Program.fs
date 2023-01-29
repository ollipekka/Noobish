// Learn more about F# at http://fsharp.org

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open Noobish.TextureAtlas

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

    let mutable previousKeyboardState = Unchecked.defaultof<KeyboardState>
    let mutable keyboardState = Unchecked.defaultof<KeyboardState>

    let mutable previousMouseState= Unchecked.defaultof<MouseState>
    let mutable mouseState = Unchecked.defaultof<MouseState>

    let mutable previousTouchState = Unchecked.defaultof<TouchCollection>
    let mutable touchState = Unchecked.defaultof<TouchCollection>

    override this.Initialize() =

        base.Initialize()
        this.GraphicsDevice.PresentationParameters.RenderTargetUsage <- RenderTargetUsage.PreserveContents
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)


    override this.LoadContent() =

        ()

    override _this.UnloadContent() = ()

    override this.Update gameTime =
        base.Update(gameTime)


    override this.Draw (gameTime) =
        let sheet = this.Content.Load<TextureAtlas>"Content/TestAtlas"

        spriteBatch.Begin(samplerState = SamplerState.PointClamp)


        spriteBatch.DrawAtlasNinePatch(
            sheet.["window_background.9"],
            Vector2(10f, 10f),
            1000f,
            500f,
            Color.White,
            0f,
            Vector2(4f, 4f),
            SpriteEffects.None,
            0f )

        spriteBatch.DrawAtlasTexture(
            sheet.["orange64x64"],
            Vector2(200f, 200f),
            Color.White,
            0f,
            Vector2.One,
            SpriteEffects.None,
            0f )


        spriteBatch.DrawAtlasNinePatch(
            sheet.["purple32x32.9"],
            Vector2(400f, 200f),
            300f,
            150f,
            Color.White,
            0f,
            Vector2(1f, 1f),
            SpriteEffects.None,
            0f )

        spriteBatch.DrawAtlasNinePatch(
            sheet.["window_background.9"],
            Vector2(600f, 300f),
            500f,
            200f,
            Color.DarkRed,
            0f,
            Vector2(1f, 1f),
            SpriteEffects.None,
            0f )

        spriteBatch.DrawAtlasTexture(
            sheet.["gold8x8"],
            Vector2(402f, 202f),
            Color.White,
            0f,
            Vector2.One,
            SpriteEffects.None,
            0f )

        spriteBatch.End()
        base.Draw(gameTime)



[<EntryPoint>]
let main argv =
    use game = new DemoGame ()

    game.Run()
    0 // return an integer exit code