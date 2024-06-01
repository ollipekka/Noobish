[<AutoOpen>]
module Noobish.Slider

open Noobish

open Microsoft.Xna.Framework

type NoobishComponents with 

    member internal this.CalculatePinPosition (parentCid: UIComponentId) (pinCid: UIComponentId) (relativeX: float32) (relativeY: float32) (rangeStart: float32, rangeEnd: float32) (value: float32):Position =
        let pinSize = this.ContentSize.[pinCid.Index]
        let pinBounds = this.Bounds.[pinCid.Index]
        let pinPadding = this.Padding.[pinCid.Index]
        let pinWidth = pinSize.Width + pinPadding.Left + pinPadding.Right
        let pinHeight = pinSize.Height + pinPadding.Top + pinPadding.Bottom
        let pinPos = (value - rangeStart) / (rangeEnd - rangeStart)
        let parentBounds = this.Bounds.[parentCid.Index]
        let parentMargin = this.Margin.[parentCid.Index]
        let parentPadding = this.Padding.[parentCid.Index]
        
        let parentWidth = parentBounds.Width - parentMargin.Left - parentMargin.Right - parentPadding.Left - parentPadding.Right
        let parentHeight = parentBounds.Height - parentMargin.Top - parentMargin.Bottom - parentPadding.Top - parentPadding.Bottom

        let x, y = 
            let parentContainerId = this.ParentId.[parentCid.Index]
        
            if parentContainerId <> UIComponentId.empty then 
                let parentContainerMargin = this.Margin.[parentContainerId.Index]
                let parentContainerPadding = this.Padding.[parentContainerId.Index]
                let parentContainerBounds = this.Bounds.[parentContainerId.Index]
                let parentContainerWidth = parentContainerBounds.Width - parentContainerMargin.Left - parentContainerMargin.Right - parentContainerPadding.Left - parentContainerPadding.Right
                let parentContainerHeight = parentContainerBounds.Height - parentContainerMargin.Top - parentContainerMargin.Bottom - parentContainerPadding.Top - parentContainerPadding.Bottom      
                match this.Layout.[parentContainerId.Index] with 
                | Layout.Grid(cols, rows) ->
                    let rowHeight = (parentContainerHeight / float32 rows)
                    match this.GridCellAlignment.[parentCid.Index] with 
                    | NoobishAlignment.Left -> 
                        parentBounds.X + parentMargin.Left, parentBounds.Y + rowHeight / 2f - parentMargin.Top - pinHeight / 2f
                    | _ -> 0f, 0f
                | _ -> parentBounds.X + parentMargin.Left, parentBounds.Y + parentHeight / 2f - pinHeight / 2f
            else 
                0f, 0f

        {X = x + parentWidth * pinPos - pinWidth / 2f; Y = y}

    member internal this.CalculatePinX(parentCid: UIComponentId) (pinCid: UIComponentId) (position: Position) : float32 =

        let pinSize = this.ContentSize.[pinCid.Index]
        let pinPadding = this.Padding.[pinCid.Index]
        let pinWidth = pinSize.Width + pinPadding.Left + pinPadding.Right

        let bounds = this.Bounds.[pinCid.Index]

        (position.X - bounds.X - pinWidth / 2f)


type Noobish with 

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
        let sliderPin = 
            this.Create "SliderPin"
            |> this.SetConstrainToParentBounds false
            |> this.SetRelativePositionFunc (fun (rcid: UIComponentId) (ccid: UIComponentId) (relativeX: float32) (relativeY: float32) -> 
                this.Components.CalculatePinPosition rcid ccid relativeX relativeY (rangeStart, rangeEnd) value
            )


        this.SetChildren [| sliderPin |] cid |> ignore


        this.SetOnPress (fun (cid: UIComponentId) (position: Position) (_gameTime: GameTime) -> 
            let v = calcaulateSliderValue cid position
            let delta = v - value


            let offset = this.Components.CalculatePinX cid sliderPin position

            let bounds = this.Components.Bounds[sliderPin.Index]
            this.Components.Bounds[sliderPin.Index] <- {bounds with X = bounds.X + offset}
            onValueChanged v
        ) cid |> ignore

        cid
