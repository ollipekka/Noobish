namespace Noobish

open Noobish
open Microsoft.Xna.Framework

module NoobishV2 =
    let beginFrame (page: string) (components: NoobishComponentsV2) =

        let ctx = components.AcquireContext()
        ctx.Reset(components.RunningId, page)
        ctx

    let endFrame (rootWidth: float32) (rootHeight: float32) (parentCtx: ComponentContextV2) =
        NoobishLayoutV2.layoutFrame parentCtx.Components rootWidth rootHeight
        parentCtx.ParentId

    let private createId (parentCtx: ComponentContextV2) (index: int) (localId: uint16) =
        let ns = NamespaceHash.fromPage parentCtx.Page
        let generation = uint16 parentCtx.Components.RunningId
        UIComponentIdV2.create ns generation (uint16 index) localId

    let createComponent (themeId: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        let components = parentCtx.Components
        let index = components.Count
        let cid = createId parentCtx index localId
        components.Id.[index] <- cid
        components.ThemeId.[index] <- themeId
        components.ParentId.[index] <- parentCtx.ParentId
        components.Count <- components.Count + 1
        components.RunningId <- components.RunningId + 1

        let ctx = parentCtx.Components.AcquireContext()
        ctx.ComponentId <- cid 
        ctx.FrameId <- parentCtx.FrameId
        ctx.ParentId <- parentCtx.ComponentId
        ctx.Page <- parentCtx.Page
        ctx

    let header (text: string) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Header" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.Block.[index] <- true
        ctx

    let label (text: string) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Label" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx

    let paragraph (text: string) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Paragraph" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.Textwrap.[index] <- true
        ctx.Components.TextAlign.[index] <- NoobishAlignment.TopLeft
        ctx.Components.Block.[index] <- true
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = false}
        ctx

    let textbox (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "TextBox" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.WantsTextChanged.[index] <- true
        ctx

    let button (text: string) (localId: uint16) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Button" localId parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.WantsText.[index] <- true
        ctx.Components.Text.[index] <- text
        ctx.Components.WantsOnClick.[index] <- true
        ctx.Components.WantsOnPress.[index] <- true
        ctx

    let space (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Space" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        ctx


    let canvas (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Division" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Layout.[index] <- LayoutV2.Relative ctx.ComponentId
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        ctx

    let beginPanel (parentCtx: ComponentContextV2) = 
        let ctx = createComponent "Panel" 0us parentCtx

        let index = int ctx.ComponentId.Index

        ctx.Components.Block.[index] <- true
        ctx.Components.Layout.[index] <- LayoutV2.LinearVertical
        
        ctx

    let endPanel (parentCtx: ComponentContextV2) = 

        parentCtx.ParentId

    let beginStackVertical (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Division" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Layout.[index] <- LayoutV2.LinearVertical
        ctx

    let endStackVertical (ctx: ComponentContextV2) =
        ctx.ParentId

    let beginStackHorizontal (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Division" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Block.[index] <- true
        ctx.Components.Layout.[index] <- LayoutV2.LinearHorizontal
        ctx

    let endStackHorizontal (ctx: ComponentContextV2) =
        ctx.ParentId

    let beginGrid (cols: int, rows: int) (parentCtx: ComponentContextV2) =
        let ctx = createComponent "Division" 0us parentCtx
        let index = int ctx.ComponentId.Index
        ctx.Components.Layout.[index] <- LayoutV2.Grid(cols, rows)
        ctx.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        ctx

    let endGrid (ctx: ComponentContextV2) =
        ctx.ParentId
