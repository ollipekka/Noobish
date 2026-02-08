namespace Noobish

type NoobishUserInterface(capacity: int) =
    let components = NoobishComponentsV2(capacity)
    let inputBuffer = InputBufferV2(capacity)

    member _.Components = components
    member _.InputBuffer = inputBuffer

    member _.BeginFrame(page: string) =
        components.Clear()
        NoobishV2.beginFrame page components

    member _.EndFrame(rootWidth: float32, rootHeight: float32, frameCtx: ComponentContextV2) =
        NoobishV2.endFrame rootWidth rootHeight frameCtx

    member _.ProcessInput(input: INoobishInputState) =
        NoobishInputV2.ProcessInput input components inputBuffer

    member _.ReleaseContext(ctx: ComponentContextV2) =
        components.ReleaseContext ctx

    member _.PointerConsumed
        with get() = inputBuffer.PointerConsumed

    member _.KeyboardConsumed
        with get() = inputBuffer.KeyboardConsumed

    member _.WasClicked(localId: uint16) =
        inputBuffer.WasClicked localId

    member _.WasPressed(localId: uint16) =
        inputBuffer.WasPressed localId

    member _.WasReleased(localId: uint16) =
        inputBuffer.WasReleased localId

    member _.IsDown(localId: uint16) =
        inputBuffer.IsDown localId

    member _.TryGetTextChanged(localId: uint16) =
        inputBuffer.TryGetTextChanged localId

    member _.TryGetSliderChanged(localId: uint16) =
        inputBuffer.TryGetSliderChanged localId

    member _.GetClicked() =
        inputBuffer.GetClicked()

    member _.GetPressed() =
        inputBuffer.GetPressed()

    member _.TryGetClicked() =
        inputBuffer.TryGetClicked()

    member _.TryGetPressed() =
        inputBuffer.TryGetPressed()

    member _.TryGetBounds(localId: uint16) =
        inputBuffer.TryGetBounds(components, localId)

    member _.TryGetSize(localId: uint16) =
        inputBuffer.TryGetSize(components, localId)
