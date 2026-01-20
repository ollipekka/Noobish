namespace Noobish

open Microsoft.Xna.Framework.Content
open Noobish.Styles

module NoobishV2MonoGame =
    let processFrameWith
        (measureProvider: INoobishMeasureProvider)
        (components: NoobishComponentsV2)
        (rootWidth: float32)
        (rootHeight: float32)
        (inputState: INoobishInputState)
        (inputBuffer: InputBufferV2) =
        NoobishMeasureV2.measureFrame measureProvider components
        NoobishLayoutV2.layoutFrame components rootWidth rootHeight
        NoobishMeasureV2.measureFramePostLayout measureProvider components
        NoobishLayoutV2.layoutFrame components rootWidth rootHeight
        NoobishInputV2.ProcessInput inputState components inputBuffer

    let processFrame
        (content: ContentManager)
        (styleSheet: NoobishStyleSheet)
        (components: NoobishComponentsV2)
        (rootWidth: float32)
        (rootHeight: float32)
        (inputState: INoobishInputState)
        (inputBuffer: InputBufferV2) =
        let measureProvider =
            NoobishMonoGameMeasureProvider(content, styleSheet) :> INoobishMeasureProvider
        processFrameWith measureProvider components rootWidth rootHeight inputState inputBuffer
