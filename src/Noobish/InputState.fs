namespace Noobish

type INoobishInputState =
    abstract PointerX: float32
    abstract PointerY: float32
    abstract ScrollWheelDelta: float32
    abstract IsMouseClick: NoobishMouseButtonId -> bool
    abstract IsMouseDown: NoobishMouseButtonId -> bool
    abstract IsKeyPressed: NoobishKeyId -> bool
    abstract ConsumeTextInput: unit -> struct(char[] * int)
