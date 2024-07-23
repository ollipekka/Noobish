[<AutoOpen>]
module Noobish.ProgressBar

open Noobish


type Noobish with 

    member private this.ProgressBarDash (progress: float32) =

        let cid = this.Create "ProgressBar-Dash"
        this.Components.Layout[cid |> UIComponentId.index] <- Layout.Stack
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = false}

        let pcid = this.Create "ProgressBar-Progress"
        this.Components.Fill.[pcid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        this.Components.GridCellAlignment.[pcid |> UIComponentId.index] <- NoobishAlignment.Left
        this.Components.WidthPercentage.[pcid |> UIComponentId.index] <- progress 

        let pcid2 = this.Create "ProgressBar-Mask"
        this.Components.Fill.[pcid2 |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        this.Components.GridCellAlignment.[pcid2 |> UIComponentId.index] <- NoobishAlignment.Left

        this.AddChild pcid cid |> ignore
        this.AddChild pcid2 cid |> ignore

        cid

    member this.ProgressBar (dashes: int) (progress: float32) =

        let cid = 
            this.Create("ProgressBar")
            |> this.SetGridLayout(dashes, 1)
            |> this.SetPadding 0
            |> this.SetMarginLeft 0
            |> this.SetMarginRight 0

        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = false}
        let progressPerDash = 1.0f / float32 dashes 

        let filledDashes = int (progress / progressPerDash)
        for _i = 0 to filledDashes - 1 do 
            let pcid = 
                this.ProgressBarDash 1f
            this.AddChild pcid cid |> ignore 

        let remainingProgress = progress - progressPerDash * float32 filledDashes

        if remainingProgress > 0f then 
            let pcid = this.ProgressBarDash (remainingProgress / progressPerDash)
            this.AddChild pcid cid |> ignore 
        
        let remainingDashes = dashes - filledDashes - if remainingProgress > 0f then 1 else 0
        for _i = 0 to remainingDashes - 1 do 
            let pcid = this.ProgressBarDash 0f
            this.AddChild pcid cid |> ignore 

        cid

