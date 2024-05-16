[<AutoOpen>]
module Noobish.Slider

open Noobish

type NoobishComponents with 

    member this.CalculatePinPosition (parentCid: UIComponentId) (pinCid: UIComponentId) (rangeStart: float32, rangeEnd: float32) (value: float32):Position =
        let pinSize = this.ContentSize.[pinCid.Index]
        let pinPadding = this.Padding.[pinCid.Index]
        let pinWidth = pinSize.Width + pinPadding.Left + pinPadding.Right
        let pinHeight = pinSize.Height + pinPadding.Top + pinPadding.Bottom
        let pinPos = (value - rangeStart) / (rangeEnd - rangeStart)
        let parentBounds = this.Bounds.[parentCid.Index]
        let parentMargin = this.Margin.[parentCid.Index]
        let parentPadding = this.Padding.[parentCid.Index]
        
        let parentWidth = parentBounds.Width - parentMargin.Left - parentMargin.Right - parentPadding.Left - parentPadding.Right
        let parentHeight = parentBounds.Height - parentMargin.Top - parentMargin.Bottom - parentPadding.Top - parentPadding.Bottom

        {X = parentWidth * pinPos - pinWidth / 2f; Y = parentHeight / 2f - parentMargin.Top - pinHeight / 2f}

type Noobish2 with 

    member this.Slider<'T> (rangeStart: float32, rangeEnd: float32) (step: float32) (value: float32) (onValueChanged: float32 -> unit) =

        let calcaulateSliderValue (cid: UIComponentId) (position: Position) = 
            let bounds = this.Components.Bounds.[cid.Index]

            let relative = (position.X - bounds.X) / (bounds.Width)
            let newValue = rangeStart + (relative * (rangeEnd - rangeStart))
            let steppedNewValue = truncate(newValue / step) * step
            Noobish.Internal.clamp steppedNewValue rangeStart rangeEnd

        

        let cid = this.Create "Slider"
        this.Components.Layout[cid.Index] <- Layout.Relative(cid)
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = false}
        this.Components.WantsOnPress.[cid.Index] <- true
        this.Components.OnPress.[cid.Index] <- (fun cid position -> 
            let v = calcaulateSliderValue cid position
            onValueChanged v
        )
        let sliderPin = 
            this.Create "SliderPin"
            |> this.SetConstrainToParentBounds false
            |> this.SetRelativePositionFunc (fun (rcid: UIComponentId) (ccid: UIComponentId) -> 
                this.Components.CalculatePinPosition cid ccid (rangeStart, rangeEnd) value
            ) (*
            |> this.SetOnPress (fun pinCid position -> 
                let pcid = this.Components.ParentId.[pinCid.Index]
                let v = calcaulateSliderValue pcid position
                onValueChanged v  )*)



        this.SetChildren [| sliderPin |] cid |> ignore

        cid
