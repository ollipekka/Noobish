[<AutoOpen>]
module Noobish.Input


open System

open Serilog

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input

open Noobish.Internal

type Noobish with 


    member this.ComponentContains (x: float32) (y: float32) (scrollX: float32) (scrollY: float32) (i: int) = 
        let x = x - scrollX 
        let y = y - scrollY
        let bounds = this.Components.Bounds.[i]
        let left = bounds.X 
        let top = bounds.Y 
        let right = left + bounds.Width
        let bottom = top + bounds.Height
        
        not (x < left || x > right || y < top || y > bottom)

    member this.Hover (x: float32) (y: float32) (gameTime: GameTime) (i: int)  =
        if this.Components.Visible.[i] && this.Components.Enabled.[i]&& this.ComponentContains x y 0f 0f i then 
            Log.Logger.Debug ("Mouse hover inside component {ComponentId}", i)

            let children = this.Components.Children.[i]
            if children.Count = 0 then 
                this.Components.LastHoverTime.[i] <- gameTime.TotalGameTime
            else 
                for j = 0 to children.Count - 1 do 
                    this.Hover x y gameTime children.[j].Index

    member this.Click (x: float32) (y: float32) (gameTime: GameTime) (parentScrollX: float32) (parentScrollY: float32) (i: int): bool  =

        if this.Components.Visible.[i] && this.Components.Enabled.[i] then 
            Log.Logger.Debug ("Mouse click inside component {ComponentId}", i)

            let children = this.Components.Children.[i]

            let scrollX = parentScrollX + this.Components.ScrollX.[i]
            let scrollY = parentScrollY + this.Components.ScrollY.[i]

            let contains = this.ComponentContains x y parentScrollX parentScrollY i

            let mutable found = false
            let mutable j = 0
            while not found && j < children.Count do 
                let childIndex = children.[j].Index
                if this.Components.ConstrainToParentBounds.[childIndex] then
                    if contains && this.Click x y gameTime scrollX scrollY childIndex then 
                        found <- true 
                else 
                    if this.Click x y gameTime scrollX scrollY childIndex  then 
                        found <- true
                j <- j + 1

            if contains && not found && this.Components.WantsOnClick.[i] then 
                found <- true
                this.Components.OnClick.[i] (this.Components.Id.[i]) {X = x; Y = y} gameTime
            
            if contains && not found  && this.Components.WantsFocus.[i] then
                found <- true 
                this.FocusedElementId <- this.Components.Id.[i]
                this.Cursor <- this.Components.Text.[i].Length
                this.FocusedTime <- gameTime.TotalGameTime
                this.Components.OnFocus.[i] ({SourceId = this.Components.Id.[i]}) true
            
            found

        else 
            false


    member this.KeyTyped (c: char) = 
        let focusedIndex = this.GetIndex this.FocusedElementId
        if focusedIndex <> -1 && this.Components.WantsKeyTyped.[focusedIndex] then 
            Log.Logger.Debug ("Key typed {Key} for component {Component}", c, focusedIndex)
            this.Components.OnKeyTyped.[focusedIndex] {SourceId = this.Components.Id.[focusedIndex]} c


    member this.Press (x: float32) (y: float32) (gameTime: GameTime) (parentScrollX: float32) (parentScrollY: float32) (i: int): bool =
        if this.Components.Visible.[i] && this.Components.Enabled.[i] then 
            Log.Logger.Debug ("Mouse press inside component {ComponentId}", i)

            let children = this.Components.Children.[i]

            let scrollX = parentScrollX + this.Components.ScrollX.[i]
            let scrollY = parentScrollY + this.Components.ScrollY.[i]
            let mutable found = false
            let mutable j = 0
            while not found && j < children.Count do 
                if this.Press x y gameTime scrollX scrollY children.[j].Index then 
                    found <- true 
                j <- j + 1

            if not found && this.Components.WantsOnPress.[i] && this.ComponentContains x y parentScrollX parentScrollY i then 
                found <- true
                this.Components.LastPressTime.[i] <- gameTime.TotalGameTime
                this.Components.OnPress.[i] (this.Components.Id.[i]) {X = x; Y = y} gameTime
            
            found
        else
            false

    member this.Scroll
        (positionX: float32)
        (positionY: float32)
        (scale: float32)
        (time: TimeSpan)
        (scrollX: float32)
        (scrollY: float32)
        (i: int): bool =

        let scaleValue v = v * scale

        let mutable handled = false

        if this.Components.Enabled.[i] && this.Components.Visible.[i] && this.ComponentContains positionX positionY scrollX scrollY i then
            let handledByChild =
                let children = this.Components.Children.[i]
                let mutable handledByChild = false 
                for j = 0 to children.Count - 1 do 
                    if this.Scroll positionX positionY scale time scrollX scrollY children.[j].Index then 
                        handledByChild <- true 
                    else 
                        handledByChild <- false 
                handledByChild

            if not handledByChild then
                let contentSize = this.Components.ContentSize.[i]
                let bounds = this.Components.Bounds.[i]
                let scroll = this.Components.Scroll.[i]
                let padding = this.Components.Padding.[i]
                let margin = this.Components.Margin.[i]
                
                let contentWidth = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
                let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom

                if scroll.Horizontal && contentSize.Width > contentWidth then
                    this.Components.ScrollX.[i] <- this.Components.ScrollX.[i] + scrollX
                    this.Components.LastScrollTime.[i] <- time
                    handled <- true
                if scroll.Vertical && contentSize.Height > contentHeight then
                    let scaledScroll = scaleValue scrollY
                    let nextScroll = this.Components.ScrollY.[i] + scaledScroll
                    let minScroll = contentHeight - contentSize.Height


                    this.Components.ScrollY.[i] <- clamp nextScroll minScroll 0.0f
                    this.Components.LastScrollTime.[i] <- time
                    handled <- true
            else
                handled <- true

        handled



    member this.ProcessMouse(gameTime: GameTime) =
        let mouseState =  Microsoft.Xna.Framework.Input.Mouse.GetState()

        let x = float32 mouseState.X
        let y = float32 mouseState.Y
        for i = 0 to this.Components.Count - 1 do 
            if this.Components.ParentId.[i] = UIComponentId.empty then 
                this.Hover x y gameTime i 

        if mouseState.LeftButton = ButtonState.Pressed then 
            for i = 0 to this.Components.Count - 1 do 
                if this.Components.ParentId.[i] = UIComponentId.empty || not this.Components.ConstrainToParentBounds.[i] then 
                    this.ToProcess.Enqueue(i, -this.Components.Layer.[i])

            while this.ToProcess.Count > 0 do 
                let i = this.ToProcess.Dequeue()
                this.Press x y gameTime 0f 0f i |> ignore

        elif mouseState.LeftButton = ButtonState.Released && this.PreviousMouseState.LeftButton = ButtonState.Pressed then 
            this.FocusedElementId <- UIComponentId.empty
            for i = 0 to this.Components.Count - 1 do 
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    this.ToProcess.Enqueue(i, -this.Components.Layer.[i])

            while this.ToProcess.Count > 0 do 
                let i = this.ToProcess.Dequeue()
                this.Click x y gameTime 0f 0f i |> ignore

        let scrollWheelValue = mouseState.ScrollWheelValue - this.PreviousMouseState.ScrollWheelValue
        if scrollWheelValue <> 0 then
            let scroll = float32 scrollWheelValue / 2.0f
            let absScroll = abs scroll
            let sign = sign scroll |> float32
            let absScrollAmount = min absScroll (absScroll * float32 gameTime.ElapsedGameTime.TotalSeconds * 10.0f)

            for i = 0 to this.Components.Count - 1 do 
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    this.Scroll x y 1.0f gameTime.TotalGameTime 0.0f (- absScrollAmount * sign) i |> ignore

        this.PreviousMouseState <- mouseState

    member this.ProcessKeys (gameTime: GameTime) = 
        let keyState = Keyboard.GetState()

        let index = this.GetIndex(this.FocusedElementId)
        if index <> -1 && this.Components.WantsKeyPressed.[index] then 
            if this.PreviousKeyState.IsKeyDown(Keys.Left) && keyState.IsKeyUp (Keys.Left) then 
                this.Components.OnKeyPressed.[index] {SourceId = this.Components.Id.[index]} Keys.Left

            if this.PreviousKeyState.IsKeyDown(Keys.Right) && keyState.IsKeyUp (Keys.Right) then 
                this.Components.OnKeyPressed.[index] {SourceId = this.Components.Id.[index]} Keys.Right

        this.PreviousKeyState <- keyState

    member this.Update (gameTime: GameTime) = 
        this.ProcessMouse(gameTime)
        this.ProcessKeys(gameTime)
