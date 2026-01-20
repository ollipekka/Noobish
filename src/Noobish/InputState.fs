namespace Noobish

type INoobishInputState =
    abstract PointerX: float32
    abstract PointerY: float32
    abstract ScrollWheelDelta: float32
    abstract IsPrimaryClick: unit -> bool
    abstract IsPrimaryDown: unit -> bool
    abstract IsSecondaryClick: unit -> bool
    abstract IsKeyPressed: NoobishKeyId -> bool
    abstract ConsumeTextInput: unit -> struct(char[] * int)
