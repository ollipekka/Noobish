namespace Noobish

open System
open System.Collections.Generic

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

type Dispatch2<'msg> = 'msg -> unit

type Cmd2<'msg> = 'msg list

module Cmd2 =
    let ofMsg m = [m]
    let batch l = l

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

type NoobishGame<'arg, 'model, 'msg, 'systems>(subSystemInit: NoobishGame<'arg, 'model, 'msg, 'systems> -> 'systems, init: 'arg -> 'model * Cmd2<'msg>, update: 'msg -> 'model -> 'model*Cmd2<'msg>, view: 'model -> Dispatch2<'msg> -> list<list<NoobishElement>>, tick: 'model -> GameTime -> unit, draw: 'model -> GameTime -> unit) as game =
    inherit Game()

    let tempMessages = ResizeArray()
    let mutable systems: 'systems = Unchecked.defaultof<'systems>
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
    member val GraphicsDeviceManager = graphicsDeviceManager
    member val Init = init
    member val Messages = ResizeArray<'msg>()
    member val Termination: ('msg -> bool) * ('model -> unit) = (fun _ -> false), (ignore) with get, set
    member val OnError = fun (text, ex) ->  System.Console.Error.WriteLine("{0}: {1}", text, ex) with get, set

    member _s.ScreenWidth with get() = game.GraphicsDevice.Viewport.Width
    member _s.ScreenHeight with get() = game.GraphicsDevice.Viewport.Height

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

        systems <- subSystemInit this


        this.Window.TextInput.Add(fun e ->
            NoobishMonoGame.keyTyped nui e.Character
        )

        let settings: NoobishSettings = {
            Pixel = "Pixel"
            Locale = "en"
            Debug = false
        }

        nui <- NoobishMonoGame.create game.Content "Dark/Dark" this.ScreenWidth this.ScreenHeight settings

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

        while this.Messages.Count > 0 do
            tempMessages.AddRange this.Messages
            this.Messages.Clear()
            for msg in tempMessages do

                let model', cmd = update msg state
                state <- model'

                for c in cmd do
                    this.Messages.Add msg

        if tempMessages.Count > 0 then

            tempMessages.Clear()

            let rec getElements (elements: Dictionary<string, NoobishLayoutElement>) (overlays: ResizeArray<NoobishLayoutElement>) (e: NoobishLayoutElement) =
                elements.[e.Id] <- e

                if e.Overlay then
                    overlays.Add e

                for e2 in e.Children do
                    getElements elements overlays e2

            let dispatch = this.Dispatch
            let layers = view state dispatch

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
        NoobishMonoGame.draw game.Content game.GraphicsDevice spriteBatch textBatch nui gameTime.TotalGameTime

        draw state gameTime




module Program2 =
    let create subSystemInit init update view tick draw =
        new NoobishGame<'arg, 'msg, 'model, 'systems>(subSystemInit, init, update, view, tick, draw)

    let withContentRoot root (game: NoobishGame<'arg, 'msg, 'model, 'systems>) =
        game.Content.RootDirectory <- root
        game
    let withScreenSize width height (game: NoobishGame<'arg, 'msg, 'model, 'systems>) =
        game.SetScreenSize width height
        game

    let withPreferHalfPixelOffset useOffset (game: NoobishGame<'arg, 'msg, 'model, 'systems>) =
        game.GraphicsDeviceManager.PreferHalfPixelOffset <- useOffset
        game

    let withMouseVisible b (game: NoobishGame<'arg, 'msg, 'model, 'systems>) =
        game.IsMouseVisible <- b
        game

    let withTermination predicate terminate (game: NoobishGame<'arg, 'msg, 'model, 'systems>) =
        game.Termination <- predicate, terminate

    let runWithArg arg (game: NoobishGame<'arg, 'msg, 'model, 'systems>) =
        let model, cmd = game.Init arg
        game.SetState model
        for c in cmd do
            game.Messages.Add c
        game.Run()

    let run (game: NoobishGame<unit, 'msg, 'model, 'systems>) =
        runWithArg () game

