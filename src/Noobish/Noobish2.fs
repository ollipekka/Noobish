namespace Noobish

open System
open System.Collections.Generic

open Serilog

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Noobish
open Noobish.Styles
open Noobish.TextureAtlas






type Noobish2(maxCount: int) =

    let mutable previousMouseState: MouseState = Microsoft.Xna.Framework.Input.Mouse.GetState()

    let mutable previousKeyState: KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState()
    let mutable waitForLayout = true

    let drawQueue = PriorityQueue<int, int>()

    let toCalculateSize = PriorityQueue<int, int>()
    let toLayout = PriorityQueue<int, int>()


    let free = PriorityQueue<int, int>(Array.init maxCount (fun i -> struct(i,i)))


    let frames = Array.init 2 (fun _ -> NoobishFrame (255))

    let mutable previousFrameIndex = 0
    let mutable activeFrameIndex = 0

    member private this.Active with get () = frames.[activeFrameIndex]

    member val Cursor = 0 with get, set 

    member val FocusedElementId = UIComponentId.empty with get, set
    member val FocusedTime = TimeSpan.Zero with get, set

    member val ScreenWidth: float32 = 0f with get, set 
    member val ScreenHeight: float32 = 0f with get, set

    member val Debug = false with get, set

    member this.IsActive(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && this.Active.Id.[index].Id.Equals cid.Id then 
            index
        else 
            -1
    member this.GetIndex(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && this.Active.Id.[index].Id.Equals cid.Id then 
            index
        else 
            -1

    member private this.Create(themeId: string): UIComponentId =
        if free.Count = 0 then failwith "Out of free components."

        let i = free.Dequeue() 
        let cid: UIComponentId = {Index = i; Id = Guid.NewGuid()}
        this.Active.Id.[i] <- cid
        this.Active.ThemeId.[i] <- themeId
        if this.Active.ParentId.[i] <> UIComponentId.empty then failwith "crap"
        this.Active.Enabled.[i] <- true 
        this.Active.Visible.[i] <- true 
        this.Active.Layer.[i] <- 1
        this.Active.Layout.[i] <- Layout.None
        this.Active.GridSpan.[i] <- {Colspan = 1; Rowspan = 1}
        this.Active.MinSize.[i] <- {Width = 0f; Height = 0f}

        this.Active.WantsOnClick.[i] <- false 
        this.Active.OnClick.[i] <- ignore
        this.Active.LastPressTime[i] <- TimeSpan.Zero
        
        this.Active.Count <- this.Active.Count + 1

        cid

    member this.Overlaypane(rcid: UIComponentId) =
        let cid = this.Create "Division"
        this.Active.Layout.[cid.Index] <- Layout.Relative rcid
        this.Active.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Window() =
        let cid = this.Create "Panel"
        this.Active.Layout.[cid.Index] <- Layout.LinearVertical
        cid

    member this.WindowWithGrid (cols: int) (rows: int) =
        let cid = this.Create "Window"
        this.Active.Layout.[cid.Index] <- Layout.Grid(cols, rows)
        cid

    member this.Header (t: string) = 
        let cid = this.Create "Header1"
        this.Active.Text.[cid.Index] <- t
        this.Active.Block.[cid.Index] <- true

        cid

    member this.Label (t: string) = 
        let cid = this.Create "Label"
        this.Active.Text.[cid.Index] <- t

        cid

    member this.Textbox (t: string) (onTextChanged: (OnClickEvent)-> string-> unit) = 
        let cid = this.Create "TextBox"

        this.Active.WantsFocus.[cid.Index] <- true 
        this.Active.OnFocus.[cid.Index] <- (
            fun event focus -> ()
                
        )

        this.Active.WantsKeyTyped.[cid.Index] <- true 
        this.Active.OnKeyTyped.[cid.Index] <- (fun event typed ->
            let text = this.Active.Text.[cid.Index]
            if int typed = 8 then // backspace
                if text.Length > 0 && this.Cursor > 0 then
                    this.Active.Text.[cid.Index] <- text.Remove(this.Cursor - 1, 1)
                    this.Cursor <- this.Cursor - 1
            elif int typed = 13 || int typed = 10 then 
                this.FocusedElementId <- UIComponentId.empty
                onTextChanged {SourceId = cid} this.Active.Text[cid.Index]
                this.Cursor <- 0
            elif int typed = 27 then
                this.FocusedElementId <- UIComponentId.empty
                this.Active.Text.[cid.Index] <- t
                this.Cursor <- 0
            elif int typed = 127 then // deleted
                if text.Length > 0 && this.Cursor < text.Length then
                    this.Active.Text.[cid.Index] <- text.Remove(this.Cursor, 1)
            else
                this.Active.Text.[cid.Index] <- text.Insert(this.Cursor, typed.ToString())
                this.Cursor <- this.Cursor + 1
        )
        this.Active.WantsKeyPressed.[cid.Index] <- true 
        this.Active.OnKeyPressed.[cid.Index] <- (fun event key ->
            let text = this.Active.Text.[cid.Index]
            if key = Microsoft.Xna.Framework.Input.Keys.Left then 
                let newCursorPos = this.Cursor - 1 
                this.Cursor <- max 0 newCursorPos
            elif key = Microsoft.Xna.Framework.Input.Keys.Right then 
                let newCursorPos = this.Cursor + 1 
                this.Cursor <- min text.Length newCursorPos
        )

        this.Active.Text.[cid.Index] <- t



        cid

    member this.Button (t: string) (onClick: OnClickEvent -> unit) = 
        let cid = this.Create "Button"
        this.Active.Text.[cid.Index] <- t
        this.Active.WantsOnClick.[cid.Index] <- true
        this.Active.OnClick.[cid.Index] <- onClick
        cid

    member this.Space () = 
        let cid = this.Create "Space"
        this.Active.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Div () = 
        let cid = this.Create "Division"
        this.Active.Block.[cid.Index] <- true
        this.SetLayout (Layout.LinearVertical) cid.Index

        cid

    member this.Grid (rows: int, cols: int) = 
        let cid = this.Create "Grid"
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        this.Active.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.PanelWithGrid (rows: int, cols: int) = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        this.Active.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.SetGrid (rows: int, cols: int) (cid: UIComponentId)= 
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        cid   

    member this.PanelHorizontal () = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.LinearHorizontal) cid.Index
        this.Active.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.PanelVertical () = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.LinearVertical) cid.Index
        this.Active.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.HorizontalRule () = 
        let cid = this.Create "HorizontalRule"
        this.Active.Id.[cid.Index] <- cid
        this.Active.Block.[cid.Index] <- true
        this.Active.Fill.[cid.Index] <- {Horizontal = true; Vertical = false} 
        cid

    member this.Combobox<'T> (items: 'T[]) (selectedIndex: int) (onValueChanged: OnClickEvent -> 'T -> unit)= 
        let cid = this.Create "Combobox"
        this.Active.Text.[cid.Index] <- items.[selectedIndex].ToString()

        let overlayPaneId =
            this.Overlaypane cid 
            |> this.SetOnClick (fun event ->  
                this.SetVisible false event.SourceId |> ignore)

        let overlayWindowId =
            this.Window()
            |> this.SetChildren (
                items |> Array.map (
                    fun i -> 
                        this.Button (i.ToString()) (
                            fun (event) -> 
                                this.Active.Text.[cid.Index] <- i.ToString(); onValueChanged ({SourceId=cid}) i
                        ) |> this.SetFillHorizontal
                )
            )
            
        overlayPaneId 
        |> this.SetChildren [| overlayWindowId |]
        |> this.SetLayer 225
        |> this.SetVisible false 
        |> ignore


        this.Active.WantsOnClick.[cid.Index] <- true
        this.Active.OnClick.[cid.Index] <- (fun event -> 
            this.SetVisible true overlayPaneId |> ignore
        )

        cid

    member this.SetSize (width: int, height: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.MinSize.[index] <- {Width = float32 width; Height = float32 height}
        cid

    member this.SetLayer (layer: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 

            let children = this.Active.Children.[cid.Index]
            if children.Count > 0 then 
                let previousLayer = this.Active.Layer.[index]

                for j = 0 to children.Count - 1 do 
                    let ccid = children.[j]
                    let deltaLayer = this.Active.Layer.[ccid.Index] - previousLayer
                    this.SetLayer (layer + deltaLayer) children.[j] |> ignore

            this.Active.Layer.[index] <- layer
        cid


    member this.SetVisible (v: bool) (cid: UIComponentId): UIComponentId = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Visible.[index] <- v
            
            let children = this.Active.Children.[cid.Index]
            for j = 0 to children.Count - 1 do 
                this.SetVisible v children.[j] |> ignore
        cid

    member this.SetEnabled (v: bool) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Enabled.[index] <- v
        cid

    member this.SetPosition (x: int, y: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Bounds.[index] <- {this.Active.Bounds.[index] with X = float32 x; Y = float32 y}
        cid

    member private this.SetLayout (layout: Layout) (index: int) =
        this.Active.Layout.[index] <- layout

    member this.SetRowspan (rowspan: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.GridSpan.[index] <- {this.Active.GridSpan.[index] with Rowspan = rowspan}
        cid

    member this.SetColspan (colspan: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.GridSpan.[index] <- {this.Active.GridSpan.[index] with Colspan = colspan}
        cid

    member this.SetMargin (margin: int) (cid: UIComponentId) =
        let margin = float32 margin
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Margin.[index] <- {Top = margin; Right = margin; Bottom = margin; Left = margin}
        cid

    member this.FillHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Fill.[index] <- {this.Active.Fill.[index] with Horizontal = true}
        cid

    member this.SetFillHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Fill.[index] <- {this.Active.Fill.[index] with Horizontal = true}
        cid

    member this.FillVertical (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Fill.[index] <- {this.Active.Fill.[index] with Vertical = true}
        cid
    member this.SetFill (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Fill.[index] <- {Horizontal = true; Vertical = true}
        cid

    member this.SetScrollHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Scroll.[index] <- {this.Active.Scroll.[index] with Horizontal = true}
        cid

    member this.SetScrollVertical (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Scroll.[index] <- {this.Active.Scroll.[index] with Vertical = true}
        cid


    member this.SetScroll (horizontal: bool) (vertical: bool) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Scroll.[index] <- {this.Active.Scroll.[index] with Horizontal = horizontal; Vertical = vertical}
        cid

    member this.SetToggled (t: bool) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.Toggled.[index] <- t
        cid

    member this.SetOnClick (onClick: OnClickEvent -> unit) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Active.WantsOnClick.[index] <- true
            this.Active.OnClick.[index] <- onClick
        cid

    member this.BumpLayer (layer: int) (children: IReadOnlyList<UIComponentId>) = 
        for i = 0 to children.Count - 1 do 
            let cid = children.[i]
            let childLayer = this.Active.Layer.[cid.Index] + layer
            this.Active.Layer.[cid.Index] <- childLayer
            this.BumpLayer childLayer this.Active.Children.[cid.Index]


    member this.SetChildren (cs: UIComponentId[]) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index > -1 then 
            for i = 0 to cs.Length - 1 do 
                let childId = cs[i]
                if this.Active.ParentId.[childId.Index] <> UIComponentId.empty then failwith "what"
                this.Active.ParentId.[childId.Index] <- cid
            let childrenIds = this.Active.Children.[index]
            childrenIds.AddRange cs

            let layer = this.Active.Layer.[index]
            this.BumpLayer layer (childrenIds :> IReadOnlyList<UIComponentId>)
        cid

    member this.CalculateMinSize (content: ContentManager) (styleSheet: NoobishStyleSheet) (i: int) = 

        let minSize = this.Active.MinSize.[i]

        let struct(contentWidth, contentHeight) = 
            match this.Active.Layout.[i] with 
            | Layout.LinearHorizontal -> 
                let children = this.Active.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]

                    let childPadding = this.Active.Padding.[cid.Index]
                    let childMargin = this.Active.Margin.[cid.Index]
                    let childSize = this.Active.ContentSize.[cid.Index]
                    width <- width + (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- max height (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                struct(width, height)
            | Layout.LinearVertical -> 
                let children = this.Active.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]

                    let childPadding = this.Active.Padding.[cid.Index]
                    let childMargin = this.Active.Margin.[cid.Index]
                    let childSize = this.Active.ContentSize.[cid.Index]
                    width <- max width (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- height + (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                struct (width, height)

            | Layout.Grid(cols, rows) -> 
                let children = this.Active.Children.[i]

                let mutable maxColWidth = 0f
                let mutable maxRowHeight = 0f

                for i = 0 to children.Count - 1 do
                    let cid = children.[i]

                    let gridSpan = this.Active.GridSpan.[cid.Index]
                    let childPadding = this.Active.Padding.[cid.Index]
                    let childMargin = this.Active.Margin.[cid.Index]
                    let childSize = this.Active.ContentSize.[cid.Index]
                    maxColWidth <- max maxColWidth ((childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right) / float32 gridSpan.Colspan)
                    maxRowHeight <- max maxRowHeight ((childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom) / float32 gridSpan.Rowspan)

                struct(maxColWidth * float32 cols, maxRowHeight * float32 rows)
            | Layout.Relative (rcid) -> 
                let pcid = this.Active.ParentId.[rcid.Index]
                struct(0f, 0f)
            | Layout.None -> 
                let mutable width = 0f
                let mutable height = 0f

                let text = this.Active.Text.[i]
                if not (String.IsNullOrWhiteSpace text) then
                    let themeId = this.Active.ThemeId.[i]

                    let fontId = styleSheet.GetFont themeId "default"
                    let font = content.Load<NoobishFont> fontId
                    let fontSize = int(float32 (styleSheet.GetFontSize themeId "default"))

                    let struct (contentWidth, contentHeight) =
                        if this.Active.Textwrap.[i] then
                            struct(minSize.Width, minSize.Height)
                        else
                            NoobishFont.measureSingleLine font fontSize text

                    width <- max width (float32 contentWidth)
                    height <- max height (float32 contentHeight)
                struct (width, height)


        this.Active.ContentSize.[i] <- {Width = contentWidth; Height = contentHeight}

    member this.LayoutComponent (content: ContentManager) (styleSheet: NoobishStyleSheet) (startX: float32) (startY: float32) (parentWidth: float32) (parentHeight: float32) (i: int) = 

        let fill = this.Active.Fill.[i]
        let text = this.Active.Text.[i]
        let margin = this.Active.Margin.[i]
        let padding = this.Active.Padding.[i]
        let minSize = this.Active.MinSize.[i]
        let contentSize = this.Active.ContentSize.[i]

        let maxWidth = parentWidth - margin.Left - margin.Right - padding.Left - padding.Right
        let maxHeight = parentHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom

        let mutable width = if fill.Horizontal then maxWidth else min (max contentSize.Width minSize.Width) maxWidth
        let mutable height = if fill.Vertical then maxHeight else min (max contentSize.Height minSize.Height) maxHeight

        this.Active.Bounds[i] <- {
            this.Active.Bounds.[i] with  
                X = startX
                Y = startY
                Width = width + margin.Left + margin.Right + padding.Left + padding.Right
                Height = height + margin.Top + margin.Bottom + padding.Top + padding.Bottom
        }

        match this.Active.Layout.[i] with 
        | Layout.LinearHorizontal -> 
            let children = this.Active.Children.[i]
            let mutable childX = startX + margin.Left + padding.Left
            let childY = startY + margin.Top + padding.Top
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY width height cid.Index

                let childSize = this.Active.Bounds.[cid.Index]
                childX <- childX + childSize.Width

        | Layout.LinearVertical -> 
            let children = this.Active.Children.[i]
            let childX = startX + margin.Left + padding.Left
            let mutable childY = startY + margin.Top + padding.Top
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY width height cid.Index

                let childSize = this.Active.Bounds.[cid.Index]
                childY <- childY + childSize.Height

        | Layout.Grid(cols, rows) -> 
            let startX = startX + margin.Left + padding.Left
            let startY = startY + margin.Top + padding.Top

            let children = this.Active.Children.[i]
            let colWidth = width / float32 cols
            let rowHeight = height  / float32 rows
            let mutable col = 0
            let mutable row = 0

            let cellUsed = Array2D.create cols rows false

            for i = 0 to children.Count - 1 do
                let cid = children.[i]
                let childStartX = (startX + (float32 col) * (colWidth))
                let childStartY = (startY + (float32 row) * (rowHeight))

                let gridSpan = this.Active.GridSpan.[cid.Index]

                for c = col to col + gridSpan.Colspan - 1 do
                    for r = row to row + gridSpan.Rowspan - 1 do
                        cellUsed.[c, r] <- true

                while row < rows && cellUsed.[col, row] do
                    col <- col + gridSpan.Colspan
                    if (col >= cols) then
                        col <- 0
                        row <- row + gridSpan.Rowspan

                let childWidth = float32 gridSpan.Colspan * colWidth 
                let childHeight =  float32 gridSpan.Rowspan * rowHeight
             
                this.LayoutComponent content styleSheet childStartX childStartY childWidth childHeight cid.Index
        | Layout.Relative (rcid) -> 
            let pcid = this.Active.ParentId.[rcid.Index]
            let parentBounds = this.Active.Bounds.[pcid.Index]

            let relativeBounds = this.Active.Bounds.[rcid.Index]
            let startX = relativeBounds.X
            let startY = relativeBounds.Y
            
            let children = this.Active.Children.[i]
            for i = 0 to children.Count - 1 do 
                let ccid = children.[i]
                let relativePosition = this.Active.RelativePosition.[ccid.Index]
                let childStartX = startX + relativePosition.X 
                let childStartY = startY + relativePosition.Y 
                this.LayoutComponent content styleSheet childStartX childStartY parentBounds.Width parentBounds.Height ccid.Index


        | Layout.None -> ()

    member this.DrawCursor
        (styleSheet: NoobishStyleSheet)
        (content: ContentManager)
        (textureAtlas: NoobishTextureAtlas)
        (spriteBatch: SpriteBatch)
        (i: int)
        (gameTime: GameTime)
        (scrollX: float32)
        (scrollY: float32) =
        let time = float32 gameTime.TotalGameTime.Seconds
        let themeId = this.Active.ThemeId.[i]
        let layer = 1f - float32 (this.Active.Layer.[i] + 32) / 255f


        let fontId = styleSheet.GetFont themeId "default"
        let font = content.Load<NoobishFont>  fontId
        let fontSize = styleSheet.GetFontSize themeId "default"

        let bounds = this.Active.Bounds.[i]
        let margin = this.Active.Margin.[i]
        let padding = this.Active.Padding.[i]
        let contentStartX = bounds.X + margin.Left + padding.Left
        let contentStartY = bounds.Y +  margin.Top + padding.Top
        let contentWidth = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom
        let bounds: Internal.NoobishRectangle = {X = contentStartX; Y = contentStartY; Width = contentWidth; Height = contentHeight}

        let text = this.Active.Text.[i]
        let textAlign = this.Active.TextAlign.[i]
        let textWrap = this.Active.Textwrap.[i]

        let textBounds = NoobishFont.calculateCursorPosition font fontSize textWrap bounds scrollX scrollY textAlign this.Cursor text


        let timeFocused = gameTime.TotalGameTime - this.FocusedTime
        let blinkProgress = Cursor.blink timeFocused

        let cursorColor = styleSheet.GetColor "Cursor" "default"
        let color = Color.Lerp(cursorColor, Color.Transparent, float32 blinkProgress)

        let drawables = styleSheet.GetDrawables "Cursor" "default"
        let position = Vector2(bounds.X + textBounds.Width, textBounds.Y)

        let cursorWidth = styleSheet.GetWidth "Cursor" "default"
        if cursorWidth = 0f then failwith "Cursor:defaul width is 0"
        let size = Vector2(cursorWidth, textBounds.Height)
        DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables

    member this.DrawBackground (styleSheet: NoobishStyleSheet) (textureAtlas: NoobishTextureAtlas) (spriteBatch: SpriteBatch) (gameTime: GameTime) (i: int)=

        let cstate =
            if this.Active.WantsFocus.[i] && this.FocusedElementId.Id = this.Active.Id.[i].Id then 
                "focused"
            else 
                "default"


        let themeId = this.Active.ThemeId.[i]
        let bounds = this.Active.Bounds.[i]
        let margin = this.Active.Margin.[i]
        let layer = this.Active.Layer.[i]
        let contentStartX = bounds.X + margin.Left
        let contentStartY = bounds.Y +  margin.Top 
        let contentWidth = bounds.Width - margin.Left - margin.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom

        let color =
            if this.Active.WantsFocus.[i] && this.FocusedElementId = this.Active.Id.[i] then
                styleSheet.GetColor themeId "focused"
            elif not this.Active.Enabled.[i] then
                styleSheet.GetColor themeId "disabled"
            elif this.Active.Toggled.[i] then
                styleSheet.GetColor themeId "toggled"
            else
                let lastPressTime = this.Active.LastPressTime.[i]
                let progress = 1.0 - min ((gameTime.TotalGameTime - lastPressTime).TotalSeconds / 0.05) 1.0

                let color = styleSheet.GetColor themeId "default"
                let pressedColor = styleSheet.GetColor themeId "toggled"
                let finalColor = Color.Lerp(color, pressedColor, float32 progress)

                finalColor

        let drawables = styleSheet.GetDrawables themeId cstate

        let position = Vector2(contentStartX, contentStartY)
        let size = Vector2(contentWidth, contentHeight)

        let layer = 1f - (float32 layer / 255f)

        DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables


    member this.DrawText (content: ContentManager) (styleSheet: NoobishStyleSheet) (textBatch: TextBatch) (i: int) =
        let text = this.Active.Text.[i]
        if text.Length > 0 then 
            let themeId = this.Active.ThemeId.[i]
            let layer = this.Active.Layer.[i]

            let layer = (1f - float32 (layer + 1) / 255.0f)

            let fontId = styleSheet.GetFont themeId "default"
            let font = content.Load<NoobishFont> fontId

            let fontSize = (styleSheet.GetFontSize themeId "default")

            let bounds = this.Active.Bounds.[i]
            let margin = this.Active.Margin.[i]
            let padding = this.Active.Padding.[i]
            let contentStartX = bounds.X + padding.Left + margin.Left
            let contentStartY = bounds.Y + padding.Top + margin.Top
            let contentWidth = bounds.Width - padding.Left - padding.Right - margin.Left - margin.Right
            let contentHeight = bounds.Height - padding.Top - padding.Bottom - margin.Left - margin.Right
            let bounds: Internal.NoobishRectangle = {X = contentStartX; Y = contentStartY; Width = contentWidth; Height = contentHeight}
            let textWrap = this.Active.Textwrap.[i]
            let textAlign = this.Active.TextAlign.[i]
            let textBounds =
                    NoobishFont.calculateBounds font fontSize false bounds 0f 0f textAlign text

            let textColor = styleSheet.GetFontColor themeId "default"
            if textWrap then
                textBatch.DrawMultiLine font fontSize bounds.Width (Vector2(textBounds.X, textBounds.Y)) layer textColor text
            else
                textBatch.DrawSingleLine font fontSize (Vector2(textBounds.X, textBounds.Y)) layer textColor text

    member this.DrawComponent 
        (graphics: GraphicsDevice) 
        (content: ContentManager)
        (spriteBatch: SpriteBatch)
        (textBatch: TextBatch)
        (styleSheet: NoobishStyleSheet)
        (debug: bool)
        (i: int)
        (gameTime: GameTime) =


        let visible = this.Active.Visible.[i]
        if visible then 
            let textureAtlas = content.Load(styleSheet.TextureAtlasId)

            let bounds = this.Active.Bounds.[i]
            let margin = this.Active.Margin.[i]
            let padding = this.Active.Padding.[i]
            let contentStartX = bounds.X + padding.Left + margin.Left
            let contentStartY = bounds.Y + padding.Top + margin.Top
            let contentWidth = bounds.Width - padding.Left - padding.Right - margin.Left - margin.Right
            let contentHeight = bounds.Height - padding.Top - padding.Bottom - margin.Left - margin.Right

            let parentBounds = {X = 0f; Y = 0f; Width = 25000f; Height = 25000f}

            let outerRectangle =
                let startX = contentStartX // + totalScrollX
                let startY = contentStartY // + totalScrollY
                let sourceStartX = max startX (float32 parentBounds.X)
                let sourceStartY = max startY (float32 parentBounds.Y)
                let sourceWidth = min (contentWidth) (float32 parentBounds.Right - startX)
                let sourceHeight = min (contentHeight) (float32 parentBounds.Bottom - startY)
                DrawUI.createRectangle
                    sourceStartX
                    sourceStartY
                    ((min sourceWidth (float32 parentBounds.Width)) + 2f)
                    ((min sourceHeight (float32 parentBounds.Height)) + 2f)

            let oldScissorRect = graphics.ScissorRectangle

            graphics.ScissorRectangle <- outerRectangle
            spriteBatch.Begin(rasterizerState = RasterizerState.CullCounterClockwise, samplerState = SamplerState.PointClamp)

            this.DrawBackground styleSheet textureAtlas spriteBatch gameTime i

            if this.FocusedElementId = this.Active.Id.[i] then 
                this.DrawCursor styleSheet content textureAtlas spriteBatch i gameTime 0f 0f

            spriteBatch.End()

            this.DrawText content styleSheet textBatch i

            graphics.ScissorRectangle <- oldScissorRect

    member this.Draw 
        (graphics: GraphicsDevice) 
        (content: ContentManager)
        (spriteBatch: SpriteBatch)
        (textBatch: TextBatch)
        (styleSheetId: string)
        (debug: bool)
        (gameTime: GameTime) = 
        let screenWidth = float32 graphics.Viewport.Width
        let screenHeight = float32 graphics.Viewport.Height
        if abs (this.ScreenWidth - screenWidth) > Single.Epsilon || abs (this.ScreenHeight - screenHeight )> Single.Epsilon then
            this.ScreenWidth <- screenWidth
            this.ScreenHeight <- screenHeight
            // Relayout
            waitForLayout <- true

        

        let styleSheet = content.Load<Noobish.Styles.NoobishStyleSheet>(styleSheetId)
        if waitForLayout && this.Active.Count > 0 then 

            for i = 0 to this.Active.Count - 1 do 
                let themeId = this.Active.ThemeId.[i]

                let (top, left, bottom, right) = styleSheet.GetMargin themeId "default"
                this.Active.Margin.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                let (top, left, bottom, right) = styleSheet.GetPadding themeId "default"
                this.Active.Padding.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                (* Calculate size for all the components going from topmost to bottom. *)
                toCalculateSize.Enqueue(i, - this.Active.Layer.[i])

                (* Run layout only for root components. *)
                if this.Active.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, this.Active.Layer.[i])

            while toCalculateSize.Count > 0 do 
                let i = toCalculateSize.Dequeue()
                this.CalculateMinSize content styleSheet i 

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.LayoutComponent content styleSheet 0f 0f this.ScreenWidth this.ScreenHeight i

            let previous = frames.[previousFrameIndex]
            let current = frames.[activeFrameIndex]
            if previous.Count = current.Count then 
                
                let mutable drift = false

                let mutable i = 0
                while not drift && i < current.Count - 1 do 
                    if previous.ThemeId.[i] = current.ThemeId.[i] && 
                     previous.ContentSize.[i] = current.ContentSize.[i] then 

                        current.LastPressTime.[i] <- previous.LastPressTime.[i]
                        current.LastHoverTime.[i] <- previous.LastHoverTime.[i]    

                        i <- i + 1
                    else 
                        drift <- true


            waitForLayout <- false



        for i = 0 to this.Active.Count - 1 do 
            let layer = this.Active.Layer.[i]
            drawQueue.Enqueue(i, layer)

        let styleSheet = content.Load<Noobish.Styles.NoobishStyleSheet> styleSheetId
        while drawQueue.Count > 0 do 
            let i = drawQueue.Dequeue()
            this.DrawComponent graphics content spriteBatch textBatch styleSheet false i gameTime



    member this.ComponentContains (x: float32) (y: float32) (i: int) = 
        let bounds = this.Active.Bounds.[i]

        x > bounds.X && x < bounds.X + bounds.Width && y > bounds.Y && y < bounds.Y + bounds.Height

    member this.Hover (x: float32) (y: float32) (gameTime: GameTime) (i: int)  =
        if this.Active.Visible.[i] && this.Active.Enabled.[i]&& this.ComponentContains x y i then 
            Log.Logger.Debug ("Mouse hover inside component {ComponentId}", i)

            let children = this.Active.Children.[i]
            if children.Count = 0 then 
                this.Active.LastHoverTime.[i] <- gameTime.TotalGameTime
            else 
                for j = 0 to children.Count - 1 do 
                    this.Hover x y gameTime children.[j].Index

    member this.Click (x: float32) (y: float32) (gameTime: GameTime) (i: int): bool  =
        if this.Active.Visible.[i] && this.Active.Enabled.[i]&& this.ComponentContains x y i then 
            Log.Logger.Debug ("Mouse click inside component {ComponentId}", i)

            let children = this.Active.Children.[i]

            let mutable found = false
            let mutable j = 0
            while not found && j < children.Count do 
                if this.Click x y gameTime children.[j].Index then 
                    found <- true 
                j <- j + 1

            if not found && this.Active.WantsOnClick.[i] then 
                found <- true
                this.Active.OnClick.[i] ({SourceId = this.Active.Id.[i]})
            
            if not found  && this.Active.WantsFocus.[i] then
                found <- true 
                this.FocusedElementId <- this.Active.Id.[i]
                this.Cursor <- this.Active.Text.[i].Length
                this.FocusedTime <- gameTime.TotalGameTime
                this.Active.OnFocus.[i] ({SourceId = this.Active.Id.[i]}) true
            
            found

        else 
            false


    member this.KeyTyped (c: char) = 
        let focusedIndex = this.GetIndex this.FocusedElementId
        if focusedIndex <> -1 && this.Active.WantsKeyTyped.[focusedIndex] then 
            Log.Logger.Debug ("Key typed {Key} for component {Component}", c, focusedIndex)
            this.Active.OnKeyTyped.[focusedIndex] {SourceId = this.Active.Id.[focusedIndex]} c


    member this.Press (x: float32) (y: float32) (gameTime: GameTime) (i: int): bool =

        if this.Active.Visible.[i] && this.Active.Enabled.[i]&& this.ComponentContains x y i then 
            Log.Logger.Debug ("Mouse press inside component {ComponentId}", i)
            let children = this.Active.Children.[i]

            if children.Count = 0 then 
                this.Active.LastPressTime.[i] <- gameTime.TotalGameTime
                true 
            else 

            let mutable handled = false
            let mutable j = 0

            while not handled && j < children.Count do
                let cid = children.[j]

                if this.Press x y gameTime cid.Index then 
                    handled <- true 

                j <- j + 1
            handled
        else 
            false


    member this.ProcessMouse(gameTime: GameTime) =

        let mouseState =  Microsoft.Xna.Framework.Input.Mouse.GetState()

        let x = float32 mouseState.X
        let y = float32 mouseState.Y
        for i = 0 to this.Active.Count - 1 do 
            if this.Active.ParentId.[i] = UIComponentId.empty then 
                this.Hover x y gameTime i 


        if mouseState.LeftButton = ButtonState.Pressed then 

            for i = 0 to this.Active.Count - 1 do 
                if this.Active.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, -this.Active.Layer.[i])

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.Press x y gameTime i |> ignore

        elif mouseState.LeftButton = ButtonState.Released && previousMouseState.LeftButton = ButtonState.Pressed then 
            this.FocusedElementId <- UIComponentId.empty
            for i = 0 to this.Active.Count - 1 do 
                if this.Active.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, -this.Active.Layer.[i])

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.Click x y gameTime i |> ignore

        previousMouseState <- mouseState

    member this.ProcessKeys (gameTime: GameTime) = 
        let keyState = Keyboard.GetState()

        let index = this.GetIndex(this.FocusedElementId)
        if index <> -1 && this.Active.WantsKeyPressed.[index] then 
            if previousKeyState.IsKeyDown(Keys.Left) && keyState.IsKeyUp (Keys.Left) then 
                this.Active.OnKeyPressed.[index] {SourceId = this.Active.Id.[index]} Keys.Left

            if previousKeyState.IsKeyDown(Keys.Right) && keyState.IsKeyUp (Keys.Right) then 
                this.Active.OnKeyPressed.[index] {SourceId = this.Active.Id.[index]} Keys.Right

        previousKeyState <- keyState
    member this.Update (gameTime: GameTime) = 
        this.ProcessMouse(gameTime)
        this.ProcessKeys(gameTime)




    member this.Clear() =
        this.FocusedElementId <- UIComponentId.empty
        //previousFrameIndex <- activeFrameIndex
        //activeFrameIndex <- activeFrameIndex % frames.Length
        for i = 0 to this.Active.Count - 1 do 
            free.Enqueue(i, i)
            this.Active.Id.[i] <- UIComponentId.empty
            this.Active.ThemeId.[i] <- ""

            this.Active.ParentId.[i] <- UIComponentId.empty

            this.Active.Visible.[i] <- true 
            this.Active.Enabled.[i] <- true 
            this.Active.Toggled.[i] <- false 
            this.Active.Hovered.[i] <- false 

            this.Active.Block.[i] <- false

            this.Active.Text.[i] <- ""
            this.Active.Textwrap.[i] <- false

            this.Active.Bounds.[i] <- {X = 0f; Y = 0f; Width = 0f; Height = 0f}
            this.Active.MinSize.[i] <- {Width = 0f; Height = 0f}
            this.Active.ContentSize.[i] <- {Width = 0f; Height = 0f}
            this.Active.RelativePosition.[i] <- {X = 0f; Y = 0f}
            this.Active.Fill.[i] <- {Horizontal = false; Vertical = false}
            this.Active.Padding.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
            this.Active.Margin.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

            this.Active.Layer.[i] <- -1
            this.Active.Layout.[i] <- Layout.None

            this.Active.GridSpan.[i] <- {Rowspan = 1; Colspan = 1}

            this.Active.WantsOnClick.[i] <- false
            this.Active.OnClick.[i] <- ignore

            this.Active.WantsKeyTyped.[i] <- false 
            this.Active.OnKeyTyped.[i] <- (fun _ _ ->())

            this.Active.WantsKeyPressed.[i] <- false 
            this.Active.OnKeyPressed.[i] <- (fun _ _ ->())

            this.Active.WantsFocus.[i] <- false 
            this.Active.OnFocus.[i] <- (fun _ _ ->())

            this.Active.LastPressTime.[i] <- TimeSpan.Zero
            this.Active.LastHoverTime.[i] <- TimeSpan.Zero

            this.Active.Children.[i].Clear()
        this.Active.Count <- 0

        waitForLayout <- true


