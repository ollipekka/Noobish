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

    member this.ProgressBar (segments: int) (progress: float32) =

        let cid = 
            this.Create("ProgressBar")
            |> this.SetGridLayout(segments, 1)

        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = false}
        let progressPerSegment = 1.0f / float32 segments 

        let filledSegments = int (progress / progressPerSegment)
        for _i = 0 to filledSegments - 1 do 
            let pcid = 
                this.ProgressBarDash 1f
            this.AddChild pcid cid |> ignore 

        let remainingProgress = progress - progressPerSegment * float32 filledSegments

        if remainingProgress > 0f then 
            let pcid = this.ProgressBarDash (remainingProgress / progressPerSegment)
            this.AddChild pcid cid |> ignore 
        
        let remainingSegments = segments - filledSegments - if remainingProgress > 0f then 1 else 0
        for _i = 0 to remainingSegments - 1 do 
            let pcid = this.ProgressBarDash 0f
            this.AddChild pcid cid |> ignore 

        cid
