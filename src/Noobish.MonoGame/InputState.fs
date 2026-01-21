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

    let mapKeyId keyId =
        let mutable value = Unchecked.defaultof<Keys>
        if keyMap.TryGetValue(keyId, &value) then
            ValueSome value
        else
            ValueNone

type NoobishInputState() =
    let mutable keyboardCurrent = Keyboard.GetState()
    let mutable keyboardPrevious = keyboardCurrent
    let mutable mouseCurrent = Mouse.GetState()
    let mutable mousePrevious = mouseCurrent
    let mutable touchCurrent = TouchPanel.GetState()
    let mutable touchPrevious = touchCurrent
    let mutable textBuffer = Array.zeroCreate<char> 16
    let mutable textCount = 0

    let mapKeyId = NoobishInputStateHelpers.mapKeyId

    member _.Update() =
        keyboardPrevious <- keyboardCurrent
        keyboardCurrent <- Keyboard.GetState()
        mousePrevious <- mouseCurrent
        mouseCurrent <- Mouse.GetState()
        touchPrevious <- touchCurrent
        touchCurrent <- TouchPanel.GetState()

    member _.EnqueueTextInput(value: char) =
        if textCount >= textBuffer.Length then
            let nextSize = max 16 (textBuffer.Length * 2)
            System.Array.Resize(&textBuffer, nextSize)
        textBuffer.[textCount] <- value
        textCount <- textCount + 1

    member _.PointerX = float32 mouseCurrent.X
    member _.PointerY = float32 mouseCurrent.Y
    member _.ScrollWheelDelta = float32 (mouseCurrent.ScrollWheelValue - mousePrevious.ScrollWheelValue)

    member _.IsPrimaryClick() =
        mousePrevious.LeftButton = ButtonState.Pressed && mouseCurrent.LeftButton = ButtonState.Released

    member _.IsPrimaryDown() =
        mouseCurrent.LeftButton = ButtonState.Pressed

    member _.IsSecondaryClick() =
        mousePrevious.RightButton = ButtonState.Pressed && mouseCurrent.RightButton = ButtonState.Released

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
        member this.IsPrimaryClick() = this.IsPrimaryClick()
        member this.IsPrimaryDown() = this.IsPrimaryDown()
        member this.IsSecondaryClick() = this.IsSecondaryClick()
        member this.IsKeyPressed keyId = this.IsKeyPressed keyId
        member this.ConsumeTextInput() = this.ConsumeTextInput()
