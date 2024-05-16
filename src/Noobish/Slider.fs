[<AutoOpen>]
module Noobish.Slider

open Noobish


type Noobish2 with 

    member this.Slider<'T> (rangeStart: float32, rangeEnd: float32) (step: float32) (value: float32) (onValueChanged: OnClickEvent -> float32 -> unit) =

        let cid = this.Create "Slider"
        this.Components.Layout[cid.Index] <- Layout.Relative(cid)
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = false}

        this.SetChildren [|
            this.Create "SliderPin"
            |> this.SetConstrainToParentBounds false
            |> this.SetRelativePositionFunc (fun (rcid: UIComponentId) ccid -> 
                let pinSize = this.Components.ContentSize.[ccid.Index]
                let pinPadding = this.Components.Padding.[ccid.Index]
                let pinWidth = pinSize.Width + pinPadding.Left + pinPadding.Right
                let pinHeight = pinSize.Height + pinPadding.Top + pinPadding.Bottom
                let pinPos = (value - rangeStart) / (rangeEnd - rangeStart)
                let parentBounds = this.Components.Bounds.[cid.Index]
                let parentMargin = this.Components.Margin.[cid.Index]
                let parentPadding = this.Components.Padding.[cid.Index]
                
                let parentWidth = parentBounds.Width - parentMargin.Left - parentMargin.Right - parentPadding.Left - parentPadding.Right
                let parentHeight = parentBounds.Height - parentMargin.Top - parentMargin.Bottom - parentPadding.Top - parentPadding.Bottom

                {X = parentWidth * pinPos - pinWidth / 2f; Y = parentHeight / 2f - parentMargin.Top - pinHeight / 2f}
            )
            
            

        |] cid |> ignore

        cid
