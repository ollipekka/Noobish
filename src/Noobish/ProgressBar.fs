[<AutoOpen>]
module Noobish.ProgressBar

open Noobish


type Noobish with 

    member this.ProgressBar<'T> (rangeStart: float32, rangeEnd: float32) (step: float32) (value: float32) =
        let length = rangeEnd - rangeStart

        let progress = (value - rangeStart) / (rangeEnd - rangeStart)

        let cid = this.Create "ProgressBar"
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
