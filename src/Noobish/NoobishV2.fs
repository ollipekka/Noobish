namespace Noobish

open Noobish

module NoobishV2 =
    let beginFrame (page: string) (components: NoobishComponentsV2) =

        let ctx = components.AcquireContext()
        ctx.Reset(components.RunningId, page)
        ctx

    let endFrame (ctx: ComponentContextV2) =
        ctx

    let private createId (ctx: ComponentContextV2) (index: int) =
        let ns = NamespaceHash.fromPage ctx.Page
        let generation = uint16 ctx.Components.RunningId
        UIComponentIdV2.create ns generation (uint16 index) 0us

    let createComponent (themeId: string) (ctx: ComponentContextV2) =
        let components = ctx.Components
        let index = components.Count
        let cid = createId ctx index
        components.Id.[index] <- cid
        components.ThemeId.[index] <- themeId
        components.ParentId.[index] <- ctx.ParentId
        components.Count <- components.Count + 1
        components.RunningId <- components.RunningId + 1
        struct (ctx, cid)

    let beginPanel (ctx: ComponentContextV2) = 
        let panelCtx = ctx.Components.AcquireContext()
        
        panelCtx

    let endPanel (ctx: ComponentContextV2) = 
        ctx.ParentId
