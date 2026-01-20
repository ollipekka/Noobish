namespace Noobish

open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Input.Touch

type NoobishInputState() =
    let mutable keyboardCurrent = Keyboard.GetState()
    let mutable keyboardPrevious = keyboardCurrent
    let mutable mouseCurrent = Mouse.GetState()
    let mutable mousePrevious = mouseCurrent
    let mutable touchCurrent = TouchPanel.GetState()
    let mutable touchPrevious = touchCurrent
    let mutable textBuffer = Array.zeroCreate<char> 16
    let mutable textCount = 0

    let mapKeyId keyId =
        match keyId with
        | NoobishKeyId.Escape -> ValueSome Keys.Escape
        | NoobishKeyId.Enter -> ValueSome Keys.Enter
        | NoobishKeyId.Space -> ValueSome Keys.Space
        | NoobishKeyId.A -> ValueSome Keys.A
        | NoobishKeyId.B -> ValueSome Keys.B
        | NoobishKeyId.C -> ValueSome Keys.C
        | NoobishKeyId.D -> ValueSome Keys.D
        | NoobishKeyId.E -> ValueSome Keys.E
        | NoobishKeyId.F -> ValueSome Keys.F
        | NoobishKeyId.G -> ValueSome Keys.G
        | NoobishKeyId.H -> ValueSome Keys.H
        | NoobishKeyId.I -> ValueSome Keys.I
        | NoobishKeyId.J -> ValueSome Keys.J
        | NoobishKeyId.K -> ValueSome Keys.K
        | NoobishKeyId.L -> ValueSome Keys.L
        | NoobishKeyId.M -> ValueSome Keys.M
        | NoobishKeyId.N -> ValueSome Keys.N
        | NoobishKeyId.O -> ValueSome Keys.O
        | NoobishKeyId.P -> ValueSome Keys.P
        | NoobishKeyId.Q -> ValueSome Keys.Q
        | NoobishKeyId.R -> ValueSome Keys.R
        | NoobishKeyId.S -> ValueSome Keys.S
        | NoobishKeyId.T -> ValueSome Keys.T
        | NoobishKeyId.U -> ValueSome Keys.U
        | NoobishKeyId.V -> ValueSome Keys.V
        | NoobishKeyId.W -> ValueSome Keys.W
        | NoobishKeyId.X -> ValueSome Keys.X
        | NoobishKeyId.Y -> ValueSome Keys.Y
        | NoobishKeyId.Z -> ValueSome Keys.Z
        | NoobishKeyId.Left -> ValueSome Keys.Left
        | NoobishKeyId.Right -> ValueSome Keys.Right
        | NoobishKeyId.None -> ValueNone

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
