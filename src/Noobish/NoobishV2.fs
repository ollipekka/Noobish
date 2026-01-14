namespace Noobish

open Noobish

module NoobishV2 =
    let beginFrame (page: string) (components: INoobishComponents2) =

        let ctx = components.AcquireContext()
        let namespaceId = NamespaceHash.fromPage page
        ctx.Reset(components.RunningId, page, namespaceId)
        ctx.ComponentId <- UIComponentIdV2.empty
        ctx

    let endFrame (rootWidth: float32) (rootHeight: float32) (parentCtx: ComponentContextV2) =
        NoobishLayoutV2.layoutFrame parentCtx.Components rootWidth rootHeight
        

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
        let ctx = createComponent "Checkbox" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.WantsOnClick.[index] <- true
        ctx.Components.WantsToggle.[index] <- true
        ctx

    let endCheckbox (ctx: ComponentContextV2) =
        endScope ctx

    let checkbox (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        beginCheckbox text localId parentCtx |> endCheckbox

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
