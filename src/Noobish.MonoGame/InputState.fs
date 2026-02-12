namespace Noobish

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch
open System.Collections.Generic

module internal NoobishInputStateHelpers =
    let private keyMap =
        let dict = Dictionary<NoobishKeyId, Keys>()
        dict.[NoobishKeyId.Escape] <- Keys.Escape
        dict.[NoobishKeyId.Enter] <- Keys.Enter
        dict.[NoobishKeyId.Space] <- Keys.Space
        dict.[NoobishKeyId.A] <- Keys.A
        dict.[NoobishKeyId.B] <- Keys.B
        dict.[NoobishKeyId.C] <- Keys.C
        dict.[NoobishKeyId.D] <- Keys.D
        dict.[NoobishKeyId.E] <- Keys.E
        dict.[NoobishKeyId.F] <- Keys.F
        dict.[NoobishKeyId.G] <- Keys.G
        dict.[NoobishKeyId.H] <- Keys.H
        dict.[NoobishKeyId.I] <- Keys.I
        dict.[NoobishKeyId.J] <- Keys.J
        dict.[NoobishKeyId.K] <- Keys.K
        dict.[NoobishKeyId.L] <- Keys.L
        dict.[NoobishKeyId.M] <- Keys.M
        dict.[NoobishKeyId.N] <- Keys.N
        dict.[NoobishKeyId.O] <- Keys.O
        dict.[NoobishKeyId.P] <- Keys.P
        dict.[NoobishKeyId.Q] <- Keys.Q
        dict.[NoobishKeyId.R] <- Keys.R
        dict.[NoobishKeyId.S] <- Keys.S
        dict.[NoobishKeyId.T] <- Keys.T
        dict.[NoobishKeyId.U] <- Keys.U
        dict.[NoobishKeyId.V] <- Keys.V
        dict.[NoobishKeyId.W] <- Keys.W
        dict.[NoobishKeyId.X] <- Keys.X
        dict.[NoobishKeyId.Y] <- Keys.Y
        dict.[NoobishKeyId.Z] <- Keys.Z
        dict.[NoobishKeyId.Left] <- Keys.Left
        dict.[NoobishKeyId.Right] <- Keys.Right
        dict

    let private mouseButtonMap =
        let dict = Dictionary<NoobishMouseButtonId, byte>()
        dict.[NoobishMouseButtonId.Left] <- 1uy
        dict.[NoobishMouseButtonId.Right] <- 2uy
        dict.[NoobishMouseButtonId.Middle] <- 4uy
        dict.[NoobishMouseButtonId.XButton1] <- 8uy
        dict.[NoobishMouseButtonId.XButton2] <- 16uy
        dict

    let mapKeyId keyId =
        let mutable value = Unchecked.defaultof<Keys>
        if keyMap.TryGetValue(keyId, &value) then
            ValueSome value
        else
            ValueNone

    let mapMouseButtonId buttonId =
        let mutable value = 0uy
        if mouseButtonMap.TryGetValue(buttonId, &value) then
            ValueSome value
        else
            ValueNone

    let isMouseButtonDown (state: MouseState) buttonId =
        match buttonId with
        | NoobishMouseButtonId.Left -> state.LeftButton = ButtonState.Pressed
        | NoobishMouseButtonId.Right -> state.RightButton = ButtonState.Pressed
        | NoobishMouseButtonId.Middle -> state.MiddleButton = ButtonState.Pressed
        | NoobishMouseButtonId.XButton1 -> state.XButton1 = ButtonState.Pressed
        | NoobishMouseButtonId.XButton2 -> state.XButton2 = ButtonState.Pressed
        | NoobishMouseButtonId.None -> false

type NoobishInputState internal (getKeyboardState: unit -> KeyboardState, getMouseState: unit -> MouseState, getTouchState: unit -> TouchCollection) =
    let mutable keyboardCurrent = getKeyboardState()
    let mutable keyboardPrevious = keyboardCurrent
    let mutable mouseCurrent = getMouseState()
    let mutable mousePrevious = mouseCurrent
    let mutable touchCurrent = getTouchState()
    let mutable touchPrevious = touchCurrent
    let mutable textBuffer = Array.zeroCreate<char> 16
    let mutable textCount = 0

    let mapKeyId = NoobishInputStateHelpers.mapKeyId
    let isMouseButtonDown = NoobishInputStateHelpers.isMouseButtonDown

    new () = NoobishInputState(Keyboard.GetState, Mouse.GetState, TouchPanel.GetState)

    member _.Update() =
        keyboardPrevious <- keyboardCurrent
        keyboardCurrent <- getKeyboardState()
        mousePrevious <- mouseCurrent
        mouseCurrent <- getMouseState()
        touchPrevious <- touchCurrent
        touchCurrent <- getTouchState()

    member _.EnqueueTextInput(value: char) =
        if textCount >= textBuffer.Length then
            let nextSize = max 16 (textBuffer.Length * 2)
            System.Array.Resize(&textBuffer, nextSize)
        textBuffer.[textCount] <- value
        textCount <- textCount + 1

    member _.PointerX = float32 mouseCurrent.X
    member _.PointerY = float32 mouseCurrent.Y
    member _.ScrollWheelDelta = float32 (mouseCurrent.ScrollWheelValue - mousePrevious.ScrollWheelValue)

    member _.IsMouseClick(buttonId: NoobishMouseButtonId) =
        isMouseButtonDown mousePrevious buttonId && not (isMouseButtonDown mouseCurrent buttonId)

    member _.IsMouseDown(buttonId: NoobishMouseButtonId) =
        isMouseButtonDown mouseCurrent buttonId

    member _.IsKeyPressed(keyId: NoobishKeyId) =
        match mapKeyId keyId with
        | ValueSome key -> keyboardPrevious.IsKeyDown key && keyboardCurrent.IsKeyUp key
        | ValueNone -> false

    member _.ConsumeTextInput() =
        let count = textCount
        textCount <- 0
        struct(textBuffer, count)

    interface INoobishInputState with
        member this.PointerX = this.PointerX
        member this.PointerY = this.PointerY
        member this.ScrollWheelDelta = this.ScrollWheelDelta
        member this.IsMouseClick buttonId = this.IsMouseClick buttonId
        member this.IsMouseDown buttonId = this.IsMouseDown buttonId
        member this.IsKeyPressed keyId = this.IsKeyPressed keyId
        member this.ConsumeTextInput() = this.ConsumeTextInput()
