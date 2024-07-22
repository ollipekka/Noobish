[<AutoOpen>]
module Noobish.Slider

open Noobish

open Microsoft.Xna.Framework

type NoobishComponents with 

    member internal this.CalculatePinPosition (parentCid: int<UIComponentId>) (pinCid: int<UIComponentId>) (relativeX: float32) (relativeY: float32) (rangeStart: float32, rangeEnd: float32) (value: float32):NoobishPosition =
        let pinSize = this.ContentSize.[pinCid |> UIComponentId.index]
        let pinBounds = this.Bounds.[pinCid |> UIComponentId.index]
        let pinPadding = this.Padding.[pinCid |> UIComponentId.index]
        let pinWidth = pinSize.Width + pinPadding.Left + pinPadding.Right
        let pinHeight = pinSize.Height + pinPadding.Top + pinPadding.Bottom
        let pinPos = (value - rangeStart) / (rangeEnd - rangeStart)
        let parentBounds = this.Bounds.[parentCid |> UIComponentId.index]
        let parentMargin = this.Margin.[parentCid |> UIComponentId.index]
        let parentPadding = this.Padding.[parentCid |> UIComponentId.index]
        
        let parentWidth = parentBounds.Width - parentMargin.Left - parentMargin.Right
        let parentHeight = parentBounds.Height - parentMargin.Top - parentMargin.Bottom

        let x, y = 
            let parentContainerId = this.ParentId.[parentCid |> UIComponentId.index]
        
            if parentContainerId <> UIComponentId.empty then 
                let parentContainerIndex = parentContainerId |> UIComponentId.index
                let parentContainerMargin = this.Margin.[parentContainerIndex]
                let parentContainerPadding = this.Padding.[parentContainerIndex]
                let parentContainerBounds = this.Bounds.[parentContainerIndex]
                let parentContainerWidth = parentContainerBounds.Width - parentContainerMargin.Left - parentContainerMargin.Right - parentContainerPadding.Left - parentContainerPadding.Right
                let parentContainerHeight = parentContainerBounds.Height - parentContainerMargin.Top - parentContainerMargin.Bottom - parentContainerPadding.Top - parentContainerPadding.Bottom      
                match this.Layout.[parentContainerIndex] with 
                | Layout.Grid(cols, rows) ->
                    let rowWidth = (parentContainerWidth / float32 cols)
                    let rowHeight = (parentContainerHeight / float32 rows)
                    match this.GridCellAlignment.[parentCid |> UIComponentId.index] with 
                    | NoobishAlignment.Left -> 
                        parentBounds.X, parentBounds.Y  + parentMargin.Top + parentMargin.Bottom - pinHeight / 2f
                    | NoobishAlignment.Center -> 
                        parentBounds.X, parentBounds.Y + parentMargin.Top + parentMargin.Bottom - pinHeight / 2f
                    | _ -> 0f, 0f//parentBounds.X, parentBounds.Y + rowHeight / 2f - parentMargin.Top - parentMargin.Bottom - pinHeight / 2f
                | _ -> parentBounds.X, parentBounds.Y + parentHeight / 2f - parentMargin.Top - pinHeight / 2f
            else 
                0f, 0f

        {X = x + parentMargin.Left + parentWidth * pinPos - pinWidth / 2f; Y = y}



type Noobish with 

    member this.Slider<'T> (rangeStart: float32, rangeEnd: float32) (step: float32) (value: float32) (onValueChanged: float32 -> unit) =

        let calcaulateSliderValue (cid: int<UIComponentId>) (position: NoobishPosition) = 
            let bounds: Internal.NoobishRectangle = this.Components.Bounds.[cid |> UIComponentId.index]

            let relative = (position.X - bounds.X) / (bounds.Width)
            let newValue = rangeStart + (relative * (rangeEnd - rangeStart))
            let steppedNewValue = truncate(newValue / step) * step
            Noobish.Internal.clamp steppedNewValue rangeStart rangeEnd

        

        let cid = this.Create "Slider"
        this.Components.Layout[cid |> UIComponentId.index] <- Layout.Relative(cid)
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = false}
        this.Components.GridCellAlignment.[cid |> UIComponentId.index] <- NoobishAlignment.Center
        let sliderPin = 
            this.Create "SliderPin"
            |> this.SetConstrainToParentBounds false
            |> this.SetRelativePositionFunc (fun (rcid: int<UIComponentId>) (ccid: int<UIComponentId>) (relativeX: float32) (relativeY: float32) -> 
                this.Components.CalculatePinPosition rcid ccid relativeX relativeY (rangeStart, rangeEnd) value
            )

        this.AddChild sliderPin cid |> ignore

        this.SetOnPress (fun (cid: int<UIComponentId>) (position: NoobishPosition) (_gameTime: GameTime) -> 
            let v = calcaulateSliderValue cid position

            let offset = this.Components.CalculatePinPosition cid sliderPin 0f 0f (rangeStart,rangeEnd) v

            let bounds = this.Components.Bounds[sliderPin |> UIComponentId.index]
            this.Components.Bounds[sliderPin |> UIComponentId.index] <- {bounds with X = offset.X}
            onValueChanged v
        ) cid |> ignore

        cid
