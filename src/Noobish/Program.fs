namespace Noobish

open System
open System.Collections.Generic

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

type Dispatch2<'msg> = 'msg -> unit

[<RequireQualifiedAccess>]
type Cmd2<'msg> =
| Single of message: 'msg
| Batch of messages: list<Cmd2<'msg>>
| None

module Cmd2 =
    let ofMsg<'msg> (m: 'msg) = Cmd2<'msg>.Single(m)
    let batch<'msg> (l: list<Cmd2<'msg>>) = Cmd2<'msg>.Batch l
    let none = Cmd2.None

    let rec unpack (array: ResizeArray<'msg>) (cmd: Cmd2<'msg>) =
        match cmd with
        | Cmd2.Single(msg) ->
            array.Add msg
        | Cmd2.Batch (msgs) ->
            for msg in msgs do
                unpack array msg
        | Cmd2.None -> ()


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
}

module NoobishInputState =
    let updateDevice<'InputDevice> (device: 'InputDevice) (deviceState: NoobishInputDeviceState<'InputDevice>) =
        {deviceState with Previous = deviceState.Current; Current = device}

[<AbstractClass>]
type NoobishGame<'arg, 'msg, 'model>() as game =

    inherit Game()



    let tempMessages = ResizeArray()
    let mutable state: 'model = Unchecked.defaultof<'model>
    let mutable textBatch = Unchecked.defaultof<TextBatch>
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let graphicsDeviceManager = new GraphicsDeviceManager(game)
    do
        graphicsDeviceManager.GraphicsProfile <- GraphicsProfile.HiDef
        #if !__MOBILE__
        #endif
        graphicsDeviceManager.SynchronizeWithVerticalRetrace <- true
        graphicsDeviceManager.SupportedOrientations <-
            DisplayOrientation.LandscapeLeft ||| DisplayOrientation.LandscapeRight;

    let mutable nui = Unchecked.defaultof<NoobishUI>

    abstract member ServiceInit: Game -> unit

    abstract member InitInternal: 'arg -> 'model * Cmd2<'msg>

    abstract member UpdateInternal: 'msg -> 'model -> ('model*Cmd2<'msg>)

    abstract member ViewInternal: 'model -> Dispatch2<'msg> -> list<list<NoobishElement>>

    abstract member TickInternal: 'model -> GameTime -> unit

    abstract member DrawInternal: 'model -> GameTime -> unit


    member _this.State with get() = state
    member _this.UI with get() = nui
    member val GraphicsDeviceManager = graphicsDeviceManager
    member val Messages = ResizeArray<'msg>()
    member val Termination: ('msg -> bool) * ('model -> unit) = (fun _ -> false), (ignore) with get, set
    member val OnError = fun (text, ex) ->  System.Console.Error.WriteLine("{0}: {1}", text, ex) with get, set

    member _s.ScreenWidth with get() = game.GraphicsDevice.Viewport.Width
    member _s.ScreenHeight with get() = game.GraphicsDevice.Viewport.Height

    member val Theme = "" with get, set

    member s.SetScreenSize width height =
        graphicsDeviceManager.PreferredBackBufferWidth <- width
        graphicsDeviceManager.PreferredBackBufferHeight <- height

    member _this.SetState s =
        state <- s

    member val Input = Unchecked.defaultof<NoobishInputState> with get, set

    member this.Dispatch (msg: 'msg) =
        this.Messages.Add msg

    override this.Initialize() =
        base.Initialize()

        graphicsDeviceManager.ApplyChanges()

        this.ServiceInit (game :> Game)


        this.Window.TextInput.Add(fun e ->
            NoobishMonoGame.keyTyped nui e.Character
        )

        let settings: NoobishSettings = {
            Pixel = "Pixel"
            Locale = "en"
            Debug = false
        }

        nui <- NoobishMonoGame.create game.Content this.Theme this.ScreenWidth this.ScreenHeight settings
        game.Services.AddService nui
        this.GraphicsDevice.PresentationParameters.RenderTargetUsage <- RenderTargetUsage.PreserveContents
        spriteBatch <- new SpriteBatch(this.GraphicsDevice)

        let fontEffect = this.Content.Load<Effect>("MSDFFontEffect")
        textBatch <- new TextBatch(this.GraphicsDevice, fontEffect, 1024)

    override this.LoadContent() =
        ()

    override _this.UnloadContent() = ()

    override this.Update gameTime =
        base.Update(gameTime)

        this.Input <- {
            Keyboard = NoobishInputState.updateDevice (Keyboard.GetState()) this.Input.Keyboard
            Mouse = NoobishInputState.updateDevice (Mouse.GetState()) this.Input.Mouse
            Touch = NoobishInputState.updateDevice (TouchPanel.GetState()) this.Input.Touch
        }

        #if __MOBILE__
        NoobishMonoGame.updateMobile nui this.Input.Touch.Previous this.Input.Touch.Current gameTime
        #else
        NoobishMonoGame.updateMouse nui this.Input.Mouse.Previous this.Input.Mouse.Current gameTime
        NoobishMonoGame.updateKeyboard nui this.Input.Keyboard.Previous this.Input.Keyboard.Current gameTime
        #endif

        this.TickInternal state gameTime

        while this.Messages.Count > 0 do
            tempMessages.Clear()
            tempMessages.AddRange this.Messages
            this.Messages.Clear()
            for msg in tempMessages do

                let model', cmd = this.UpdateInternal msg state
                state <- model'

                Cmd2.unpack this.Messages cmd
        if tempMessages.Count > 0 then

            tempMessages.Clear()

            let rec getElements (elements: Dictionary<string, NoobishLayoutElement>) (overlays: ResizeArray<NoobishLayoutElement>) (e: NoobishLayoutElement) =
                elements.[e.Id] <- e

                if e.Overlay then
                    overlays.Add e

                for e2 in e.Children do
                    getElements elements overlays e2

            let dispatch = this.Dispatch
            let layers = this.ViewInternal state dispatch

            let width = (float32 nui.Width)
            let height = (float32 nui.Height)

            nui.State.BeginUpdate()
            nui.Layers <- layers |> List.mapi (fun i components -> Logic.layout nui.Content nui.StyleSheet nui.Settings nui.State (i + 1) width height components) |> List.toArray
            nui.State.EndUpdate()

            nui.Elements.Clear()

            let overlays = ResizeArray<NoobishLayoutElement>()

            for layer in nui.Layers do
                for e in layer do
                    getElements nui.Elements overlays e

            nui.Layers <- Array.concat [nui.Layers; [| overlays.ToArray() |]]

        nui.State.ProcessEvents()



    override this.Draw (gameTime) =
        base.Draw(gameTime)
        this.GraphicsDevice.Clear(Color.Black)
        this.DrawInternal state gameTime
        NoobishMonoGame.draw game.Content game.GraphicsDevice spriteBatch textBatch nui gameTime.TotalGameTime

type SimpleNoobishGame<'arg, 'msg, 'model>(
    serviceInit: Game -> unit,
    init: SimpleNoobishGame<'arg, 'msg, 'model> ->'arg -> ('model * Cmd2<'msg>),
    update: SimpleNoobishGame<'arg, 'msg, 'model> -> 'msg -> 'model -> ('model * Cmd2<'msg>),
    view: SimpleNoobishGame<'arg, 'msg, 'model> -> 'model -> Dispatch2<'msg> -> list<list<NoobishElement>>,
    tick: SimpleNoobishGame<'arg, 'msg, 'model> -> 'model -> GameTime -> unit,
    draw: SimpleNoobishGame<'arg, 'msg, 'model> -> 'model -> GameTime -> unit) =
    inherit NoobishGame<'arg, 'msg, 'model>()


    override this.ServiceInit game =
        serviceInit game
    override this.InitInternal arg =
        init this arg
    override this.UpdateInternal message model =
        update this message model
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

    let withTheme<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> theme (game: 'T) =
        game.Theme <- theme
        game
    let withScreenSize<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> width height (game: 'T) =
        game.SetScreenSize width height
        game

    let withPreferHalfPixelOffset<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> useOffset (game: 'T) =
        game.GraphicsDeviceManager.PreferHalfPixelOffset <- useOffset
        game

    let withMouseVisible<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> b (game: 'T) =
        game.IsMouseVisible <- b
        game

    let withTermination<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> predicate terminate (game: 'T) =
        game.Termination <- predicate, terminate

    let runWithArg<'arg, 'msg, 'model, 'T when 'T :> NoobishGame<'arg, 'msg, 'model>> (arg: 'arg) (game: 'T)  =
        let model, cmd = game.InitInternal arg
        game.SetState model

        Cmd2.unpack game.Messages cmd
        game.Run()

    let withTextInput<'msg, 'model, 'T when 'T :> NoobishGame<unit, 'msg, 'model>> (game: 'T) =
        game.Window.TextInput.Add(fun e ->
            NoobishMonoGame.keyTyped game.UI e.Character
        )
        game

    let run<'msg, 'model, 'T when 'T :> NoobishGame<unit, 'msg, 'model>> (game: 'T) =
        runWithArg () game
