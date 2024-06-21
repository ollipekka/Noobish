namespace Noobish

open Serilog 
open System
open System.Collections.Generic

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open Noobish.Styles

type Dispatch2<'msg> = 'msg -> unit

[<RequireQualifiedAccess>]
type Cmd<'msg> =
| Single of message: 'msg
| Batch of messages: list<Cmd<'msg>>
| None

module Cmd =
    let ofMsg<'msg> (m: 'msg) = Cmd<'msg>.Single(m)
    let batch<'msg> (l: list<Cmd<'msg>>) = Cmd<'msg>.Batch l
    let none = Cmd.None
    let rec map<'msg1, 'msg2> (projection: 'msg1 -> 'msg2) (msg: Cmd<'msg1>): Cmd<'msg2> =
        match msg with
        | Cmd.Single(msg) ->
            Cmd.Single (projection msg)
        | Cmd.Batch (msgs) ->
            Cmd.Batch(msgs |> List.map (fun msg -> map projection msg ))
        | Cmd.None -> Cmd.None

    let rec unpack (array: ResizeArray<'msg>) (cmd: Cmd<'msg>) =
        match cmd with
        | Cmd.Single(msg) ->
            array.Add msg
        | Cmd.Batch (msgs) ->
            for msg in msgs do
                unpack array msg
        | Cmd.None -> ()

[<Struct>]
type NoobishInputDeviceState<'InputDevice> = {
    Current: 'InputDevice
    Previous: 'InputDevice
}

[<Struct>]
type NoobishInputState = {
    Keyboard: NoobishInputDeviceState<KeyboardState>
    Mouse: NoobishInputDeviceState<MouseState>
    Touch: NoobishInputDeviceState<TouchCollection>
} with
    member this.IsKeyPressed (k:Keys) =
        this.Keyboard.Previous.IsKeyDown k && this.Keyboard.Current.IsKeyUp k

    member this.IsLeftMouseButtonPressed () =
        this.Mouse.Previous.LeftButton = ButtonState.Pressed && this.Mouse.Current.LeftButton = ButtonState.Released

    member this.IsRightMouseButtonPressed () =
        this.Mouse.Previous.LeftButton = ButtonState.Pressed && this.Mouse.Current.LeftButton = ButtonState.Released


module NoobishInputState =
    let updateDevice<'InputDevice> (device: 'InputDevice) (deviceState: NoobishInputDeviceState<'InputDevice>) =
        {deviceState with Previous = deviceState.Current; Current = device}

[<AbstractClass>]
type NoobishGame<'arg, 'msg, 'model>() as game =

    inherit Game()


    let mutable virtualResolution: voption<struct(int*int)> = ValueNone

    let tempMessages = ResizeArray()
    let mutable state: 'model = Unchecked.defaultof<'model>
    let mutable textBatch = Unchecked.defaultof<TextBatch>
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable renderTarget = Unchecked.defaultof<RenderTarget2D>

    abstract member ServiceInit: Game -> unit

    abstract member InitInternal: 'arg -> 'model * Cmd<'msg>

    abstract member UpdateInternal: 'msg -> 'model -> GameTime -> ('model*Cmd<'msg>)

    abstract member ViewInternal: 'model -> Dispatch2<'msg> -> UIComponentId

    abstract member TickInternal: 'model -> GameTime -> unit

    abstract member DrawInternal: 'model -> GameTime -> unit

    member val Noobish = Noobish(1024)
    member _this.TextBatch with get() = textBatch
    member _this.State with get() = state


    member val StyleSheetId = "Invalid" with get, set

    member val Debug = false with get, set

    member this.VirtualResolution with get() =
        match virtualResolution with
        | ValueSome(virtualResolution) -> virtualResolution
        | ValueNone -> struct(this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height)
    member val GraphicsDeviceManager =
        let graphicsDeviceManager = new GraphicsDeviceManager(game)
        graphicsDeviceManager.GraphicsProfile <- GraphicsProfile.HiDef
        graphicsDeviceManager.PreferMultiSampling <- true
        graphicsDeviceManager.PreferHalfPixelOffset <- true
        graphicsDeviceManager.SynchronizeWithVerticalRetrace <- true
        graphicsDeviceManager.SupportedOrientations <-
            DisplayOrientation.LandscapeLeft ||| DisplayOrientation.LandscapeRight;

        graphicsDeviceManager
    member val Messages = ResizeArray<'msg>()
    member val Termination: ('msg -> bool) = (fun _ -> false) with get, set
    member val OnError = fun (text, ex) ->  System.Console.Error.WriteLine("{0}: {1}", text, ex) with get, set

    member _s.ScreenWidth with get() = game.GraphicsDevice.Viewport.Width
    member _s.ScreenHeight with get() = game.GraphicsDevice.Viewport.Height

    member val FontEffectId = "MSDFFontEffect" with get, set

    member this.SetResolution width height =
        this.GraphicsDeviceManager.PreferredBackBufferWidth <- width
        this.GraphicsDeviceManager.PreferredBackBufferHeight <- height

    member s.SetVirtualResolution width height =
        virtualResolution <- ValueSome(struct(width, height))

    member _this.SetState s =
        state <- s

    member val Input = Unchecked.defaultof<NoobishInputState> with get, set

    member this.Dispatch (msg: 'msg) =
        this.Messages.Add msg

    override this.Initialize() =

        base.Initialize()



        this.GraphicsDevice.PresentationParameters.MultiSampleCount <- 4
        this.GraphicsDevice.PresentationParameters.RenderTargetUsage <- RenderTargetUsage.PreserveContents
        this.GraphicsDeviceManager.ApplyChanges();

        this.ServiceInit (game :> Game)


        this.Window.TextInput.Add(fun e ->
            this.Noobish.KeyTyped (e.Character)
        )


        let pixel = new Texture2D(this.GraphicsDevice, 1, 1)
        pixel.SetData<Color> [|Color.White|]

        let struct(virtualWidth, virtualHeight) = this.VirtualResolution

        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        let fontEffect = this.Content.Load<Effect>(this.FontEffectId)
        textBatch <- new TextBatch(this.GraphicsDevice, this.VirtualResolution, fontEffect, 1024)

        renderTarget <- new RenderTarget2D(this.GraphicsDevice, virtualWidth, virtualHeight)


    override this.LoadContent() =
        ()

    override _this.UnloadContent() = ()

    override this.Update gameTime =
        base.Update(gameTime)

        if this.IsActive then 
            this.Input <- {
                Keyboard = NoobishInputState.updateDevice (Keyboard.GetState()) this.Input.Keyboard
                Mouse = NoobishInputState.updateDevice (Mouse.GetState()) this.Input.Mouse
                Touch = NoobishInputState.updateDevice (TouchPanel.GetState()) this.Input.Touch
            }

            this.Noobish.Update gameTime

        this.TickInternal state gameTime

        while this.Messages.Count > 0 do
            tempMessages.Clear()
            tempMessages.AddRange this.Messages
            this.Messages.Clear()
            for msg in tempMessages do

                let model', cmd = this.UpdateInternal msg state gameTime
                state <- model'

                Cmd.unpack this.Messages cmd

        if tempMessages.Count > 0 then
            tempMessages.Clear()
            this.Noobish.Clear()
            this.ViewInternal state this.Dispatch |> ignore


    override this.Draw (gameTime) =
        base.Draw(gameTime)

        this.GraphicsDevice.SetRenderTarget(renderTarget)
        this.GraphicsDevice.Clear(Color.Transparent)
        this.Noobish.Draw game.GraphicsDevice game.Content spriteBatch textBatch game.StyleSheetId this.Debug gameTime 
        this.GraphicsDevice.SetRenderTarget(null)

        this.GraphicsDevice.Clear(Color.Black)
        this.DrawInternal state gameTime


        spriteBatch.Begin()
        spriteBatch.Draw(renderTarget, Rectangle(0, 0, game.GraphicsDevice.Viewport.Width, game.GraphicsDevice.Viewport.Height), Color.White)
        spriteBatch.End()


type SimpleNoobishGame<'arg, 'msg, 'model>(
    serviceInit: Game -> unit,
    init: SimpleNoobishGame<'arg, 'msg, 'model> ->'arg -> ('model * Cmd<'msg>),
    update: SimpleNoobishGame<'arg, 'msg, 'model> -> 'msg -> 'model -> GameTime -> ('model * Cmd<'msg>),
    view: SimpleNoobishGame<'arg, 'msg, 'model> -> 'model -> Dispatch2<'msg> -> UIComponentId,
    tick: SimpleNoobishGame<'arg, 'msg, 'model> -> 'model -> GameTime -> unit,
    draw: SimpleNoobishGame<'arg, 'msg, 'model> -> 'model -> GameTime -> unit) =
    inherit NoobishGame<'arg, 'msg, 'model>()


    override this.ServiceInit game =
        serviceInit game
    override this.InitInternal arg =
        init this arg
    override this.UpdateInternal message model gameTime =
        update this message model gameTime
    override this.ViewInternal model dispatch =
        view this model dispatch
    override this.TickInternal model gameTime =
        tick this model gameTime
    override this.DrawInternal (model: 'model) (gameTime: GameTime) =
        draw this model gameTime


module Program2 =

    let customGame<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> (game: 'T) =
        game
    let create<'arg, 'msg, 'model> serviceInit init update view tick draw =
        let game = new SimpleNoobishGame<'arg, 'msg, 'model>(serviceInit, init, update, view, tick, draw)
        customGame game

    let withContentRoot<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> root (game:  'T) =
        game.Content.RootDirectory <- root
        game

    let withStyleSheet<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> (styleSheetId: string) (game: 'T) =
        game.StyleSheetId <- styleSheetId
        game

    let withFontEffectId<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> fontEffectId (game: 'T) =
        game.FontEffectId <- fontEffectId
        game

    let withResolution<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> width height (game: 'T) =
        game.SetResolution width height
        game
    let withVirtualResolution<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> width height (game: 'T) =
        game.SetVirtualResolution width height
        game

    let withPreferHalfPixelOffset<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> useOffset (game: 'T) =
        game.GraphicsDeviceManager.PreferHalfPixelOffset <- useOffset
        game

    let withMouseVisible<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> b (game: 'T) =
        game.IsMouseVisible <- b
        game

    let runWithArg<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> (arg: 'arg) (game: 'T)  =
        let model, cmd = game.InitInternal arg
        game.SetState model

        let terminate = Cmd.unpack game.Messages cmd
        game.Run()

    let withTextInput<'msg, 'model, 'T when 'T :> NoobishGame<unit, 'msg, 'model>> (game: 'T) =
        game.Window.TextInput.Add(fun e ->
            game.Noobish.KeyTyped e.Character
        )
        game

    let run<'msg, 'model, 'T when 'T :> NoobishGame<unit, 'msg, 'model>> (game: 'T) =
        runWithArg () game
