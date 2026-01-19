namespace Noobish

open Noobish
open Microsoft.Xna.Framework.Content
open Noobish.Styles

module NoobishV2 =
    let private overlayRootLayer = 200
    let private overlayScrimLayer = 201
    let private overlayPanelLayer = 202
    let beginFrame (page: string) (components: INoobishComponents2) =
        if components.Count <> 0 then
            invalidOp "beginFrame requires a cleared component store. Call components.Clear() between frames."

        let ctx = components.AcquireContext()
        let namespaceId = NamespaceHash.fromPage page
        ctx.Reset(components.RunningId, page, namespaceId)
        ctx.ComponentId <- UIComponentIdV2.empty

        ctx

    let endFrame (rootWidth: float32) (rootHeight: float32) (parentCtx: ComponentContextV2) =
        NoobishLayoutV2.layoutFrame parentCtx.Components rootWidth rootHeight

    let processFrame
        (content: ContentManager)
        (styleSheet: NoobishStyleSheet)
        (components: NoobishComponentsV2)
        (rootWidth: float32)
        (rootHeight: float32)
        (inputState: INoobishInputState)
        (inputBuffer: InputBufferV2) =
        NoobishMeasureV2.measureFrame content styleSheet components
        NoobishLayoutV2.layoutFrame components rootWidth rootHeight
        NoobishMeasureV2.measureFramePostLayout content styleSheet components
        NoobishLayoutV2.layoutFrame components rootWidth rootHeight
        NoobishInputV2.ProcessInput inputState components inputBuffer
        

    let private createId (parentCtx: ComponentContextV2) (index: int) (localId: uint16) =
        let generation = uint16 parentCtx.Components.RunningId
        UIComponentIdV2.create parentCtx.NamespaceId generation (uint16 index) localId

    let createComponent (themeId: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        let components = parentCtx.Components
        let index = components.Count
        let cid = createId parentCtx index localId
        let parentId = parentCtx.ComponentId
        components.Id.[index] <- cid
        components.ThemeId.[index] <- themeId
        components.ParentId.[index] <- parentId
        if parentId <> UIComponentIdV2.empty then
            let parentIndex = int parentId.Index
            components.Children.[parentIndex].Add cid
        components.Count <- components.Count + 1
        components.RunningId <- components.RunningId + 1

        let ctx = parentCtx.Components.AcquireContext()
        ctx.ComponentId <- cid 
        ctx.FrameId <- parentCtx.FrameId
        ctx.ParentId <- parentId
        ctx.Page <- parentCtx.Page
        ctx

    let private endScope (ctx: ComponentContextV2) =
        let parentId = ctx.ParentId
        ctx.ComponentId <- parentId
        if parentId <> UIComponentIdV2.empty then
            ctx.ParentId <- ctx.Components.ParentId.[int parentId.Index]
        else
            ctx.ParentId <- UIComponentIdV2.empty
        ctx

    let setFill (fill: Fill) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.Fill.[index] <- fill
        ctx

    let setFillHorizontal (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        let fill = ctx.Components.Fill.[index]
        ctx.Components.Fill.[index] <- {fill with Horizontal = true}
        ctx

    let setFillVertical (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        let fill = ctx.Components.Fill.[index]
        ctx.Components.Fill.[index] <- {fill with Vertical = true}
        ctx

    let setLayer (layer: int) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.Layer.[index] <- layer
        ctx

    let setScroll (scroll: Scroll) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.Scroll.[index] <- scroll
        ctx

    let setScrollHorizontal (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        let scroll = ctx.Components.Scroll.[index]
        ctx.Components.Scroll.[index] <- {scroll with Horizontal = true}
        ctx

    let setScrollVertical (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        let scroll = ctx.Components.Scroll.[index]
        ctx.Components.Scroll.[index] <- {scroll with Vertical = true}
        ctx

    let setToggled (value: bool) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.Toggled.[index] <- value
        ctx

    let setWantsToggle (value: bool) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsToggle.[index] <- value
        ctx

    let setPadding (padding: NoobishPadding) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.Padding.[index] <- padding
        ctx.Components.PaddingOverride.[index] <- true
        ctx

    let setMargin (margin: NoobishMargin) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.Margin.[index] <- margin
        ctx.Components.MarginOverride.[index] <- true
        ctx

    let setMinSize (size: NoobishSize) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.MinSize.[index] <- size
        ctx

    let setMinWidth (width: float32) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        let size = ctx.Components.MinSize.[index]
        ctx.Components.MinSize.[index] <- {size with Width = width}
        ctx

    let setMinHeight (height: float32) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        let size = ctx.Components.MinSize.[index]
        ctx.Components.MinSize.[index] <- {size with Height = height}
        ctx

    let setRowspan (rowspan: int) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        let span = ctx.Components.GridSpan.[index]
        ctx.Components.GridSpan.[index] <- {span with Rowspan = rowspan}
        ctx

    let setColspan (colspan: int) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        let span = ctx.Components.GridSpan.[index]
        ctx.Components.GridSpan.[index] <- {span with Colspan = colspan}
        ctx

    let beginHeader (text: string) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Header" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.Block.[index] <- true
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = false}
        ctx

    let endHeader (ctx: ComponentContextV2) =
        endScope ctx

    let header (text: string) (parentCtx: ComponentContextV2) =
        beginHeader text parentCtx |> endHeader

    let beginLabel (text: string) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Label" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx

    let endLabel (ctx: ComponentContextV2) =
        endScope ctx

    let label (text: string) (parentCtx: ComponentContextV2) =
        beginLabel text parentCtx |> endLabel

    let beginParagraph (text: string) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Paragraph" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.Textwrap.[index] <- true
        ctx.Components.TextAlign.[index] <- NoobishAlignment.TopLeft
        ctx.Components.Block.[index] <- true
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = false}
        ctx

    let endParagraph (ctx: ComponentContextV2) =
        endScope ctx

    let paragraph (text: string) (parentCtx: ComponentContextV2) =
        beginParagraph text parentCtx |> endParagraph

    let beginTextbox (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "TextBox" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.WantsTextChanged.[index] <- true
        ctx

    let endTextbox (ctx: ComponentContextV2) =
        endScope ctx

    let textbox (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        beginTextbox text localId parentCtx |> endTextbox

    let beginButton (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Button" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.WantsOnClick.[index] <- true
        ctx.Components.WantsOnPress.[index] <- true
        ctx

    let endButton (ctx: ComponentContextV2) =
        endScope ctx

    let button (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        beginButton text localId parentCtx |> endButton

    let beginCheckbox (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        let containerCtx = createComponent "Division" 0us parentCtx
        let containerIndex = int containerCtx.ComponentId.Index
        containerCtx.Components.Block.[containerIndex] <- true
        containerCtx.Components.Layout.[containerIndex] <- LayoutV2.LinearHorizontal

        let boxCtx = createComponent "Checkbox" localId containerCtx
        let boxIndex = int boxCtx.ComponentId.Index
        boxCtx.Components.WantsOnClick.[boxIndex] <- true
        boxCtx.Components.WantsOnPress.[boxIndex] <- true
        boxCtx.Components.WantsToggle.[boxIndex] <- true

        let labelCtx = createComponent "Label" 0us containerCtx
        let labelIndex = int labelCtx.ComponentId.Index
        labelCtx.Components.WantsText.[labelIndex] <- true
        labelCtx.Components.Text.[labelIndex] <- text
        labelCtx.Components.TextAlign.[labelIndex] <- NoobishAlignment.Left
        labelCtx.Components.Fill.[labelIndex] <- {Horizontal = true; Vertical = true}

        boxCtx

    let endCheckbox (ctx: ComponentContextV2) =
        endScope ctx |> endScope

    let checkbox (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        beginCheckbox text localId parentCtx |> endCheckbox

    let beginSlider (rangeStart: float32, rangeEnd: float32) (step: float32) (value: float32) (localId: uint16) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Slider" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsOnPress.[index] <- true
        ctx.Components.WantsSlider.[index] <- true
        ctx.Components.SliderMin.[index] <- rangeStart
        ctx.Components.SliderMax.[index] <- rangeEnd
        ctx.Components.SliderStep.[index] <- step
        ctx.Components.SliderValue.[index] <- value
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = false}
        ctx

    let endSlider (ctx: ComponentContextV2) =
        endScope ctx

    let slider (rangeStart: float32, rangeEnd: float32) (step: float32) (value: float32) (localId: uint16) (parentCtx: ComponentContextV2) =
        beginSlider (rangeStart, rangeEnd) step value localId parentCtx |> endSlider

    let beginProgressBar (value: float32) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "ProgressBar" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = false}
        ctx.Components.WantsProgress.[index] <- true
        ctx.Components.ProgressValue.[index] <- value
        ctx.Components.ProgressSegments.[index] <- 1
        ctx

    let endProgressBar (ctx: ComponentContextV2) =
        endScope ctx

    let progressBar (value: float32) (parentCtx: ComponentContextV2) =
        beginProgressBar value parentCtx |> endProgressBar

    let setProgress (value: float32) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.ProgressValue.[index] <- value
        ctx

    let setProgressSegments (segments: int) (ctx: ComponentContextV2) =
        let index = int ctx.ComponentId.Index
        ctx.Components.ProgressSegments.[index] <- if segments < 1 then 1 else segments
        ctx

    let beginSpace (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Space" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        ctx

    let endSpace (ctx: ComponentContextV2) =
        endScope ctx

    let space (parentCtx: ComponentContextV2) =
        beginSpace parentCtx |> endSpace

    let beginCanvas (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Division" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Layout.[index] <- LayoutV2.Relative ctx.ComponentId
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        ctx

    let endCanvas (ctx: ComponentContextV2) =
        endScope ctx

    let canvas (parentCtx: ComponentContextV2) =
        beginCanvas parentCtx |> endCanvas

    let beginOverlayRoot (localId: uint16) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Overlay" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Layout.[index] <- LayoutV2.Relative ctx.ComponentId
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        ctx.Components.Layer.[index] <- overlayRootLayer
        ctx

    let endOverlayRoot (ctx: ComponentContextV2) =
        endScope ctx

    let beginOverlayScrim (localId: uint16) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Panel" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        ctx.Components.Padding.[index] <- NoobishPadding.empty
        ctx.Components.PaddingOverride.[index] <- true
        ctx.Components.WantsOnClick.[index] <- true
        ctx.Components.WantsOnPress.[index] <- true
        ctx.Components.Layer.[index] <- overlayScrimLayer
        ctx

    let endOverlayScrim (ctx: ComponentContextV2) =
        endScope ctx

    let beginOverlayPanel (localId: uint16) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Panel" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Layout.[index] <- LayoutV2.LinearVertical
        ctx.Components.Layer.[index] <- overlayPanelLayer
        ctx

    let endOverlayPanel (ctx: ComponentContextV2) =
        endScope ctx

    let beginPanel (parentCtx: ComponentContextV2) = 
        let ctx = createComponent "Panel" 0us parentCtx

        let index = int ctx.ComponentId.Index

        ctx.Components.Block.[index] <- true
        ctx.Components.Layout.[index] <- LayoutV2.LinearVertical
        
        ctx

    let endPanel (ctx: ComponentContextV2) =
        endScope ctx

    let beginStackVertical (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Division" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Layout.[index] <- LayoutV2.LinearVertical
        ctx

    let endStackVertical (ctx: ComponentContextV2) =
        endScope ctx

    let beginStackHorizontal (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Division" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Layout.[index] <- LayoutV2.LinearHorizontal
        ctx

    let endStackHorizontal (ctx: ComponentContextV2) =
        endScope ctx

    let beginGrid (cols: int, rows: int) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Division" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Layout.[index] <- LayoutV2.Grid(cols, rows)
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        ctx

    let endGrid (ctx: ComponentContextV2) =
        endScope ctx

    let beginHorizontalRule (parentCtx: ComponentContextV2) =
        let ctx = createComponent "HorizontalRule" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = false}
        ctx

    let endHorizontalRule (ctx: ComponentContextV2) =
        endScope ctx

    let horizontalRule (parentCtx: ComponentContextV2) =
        beginHorizontalRule parentCtx |> endHorizontalRule
