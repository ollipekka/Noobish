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
open Internal






type Noobish2(maxCount: int) =


    let rasterizerState = 
        let rasterizerState = new RasterizerState()
        rasterizerState.ScissorTestEnable <- true
        rasterizerState

    let mutable previousMouseState: MouseState = Microsoft.Xna.Framework.Input.Mouse.GetState()

    let mutable previousKeyState: KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState()
    let mutable waitForLayout = true

    let drawQueue = PriorityQueue<int, int>()

    let toCalculateSize = PriorityQueue<int, int>()
    let toLayout = PriorityQueue<int, int>()


    let free = PriorityQueue<int, int>(Array.init maxCount (fun i -> struct(i,i)))



    member val Components = NoobishFrame(256)

    member val Cursor = 0 with get, set 

    member val FocusedElementId = UIComponentId.empty with get, set
    member val FocusedTime = TimeSpan.Zero with get, set

    member val ScreenWidth: float32 = 0f with get, set 
    member val ScreenHeight: float32 = 0f with get, set

    member val Debug = false with get, set

    member this.IsActive(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && this.Components.Id.[index].Id.Equals cid.Id then 
            index
        else 
            -1
    member this.GetIndex(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && this.Components.Id.[index].Id.Equals cid.Id then 
            index
        else 
            -1

    member private this.Create(themeId: string): UIComponentId =
        if free.Count = 0 then failwith "Out of free components."

        let i = free.Dequeue() 
        let cid: UIComponentId = {Index = i; Id = Guid.NewGuid()}
        this.Components.Id.[i] <- cid
        this.Components.ThemeId.[i] <- themeId
        if this.Components.ParentId.[i] <> UIComponentId.empty then failwith "crap"
        this.Components.Enabled.[i] <- true 
        this.Components.Visible.[i] <- true 
        this.Components.Layer.[i] <- 1
        this.Components.Layout.[i] <- Layout.None
        this.Components.GridSpan.[i] <- {Colspan = 1; Rowspan = 1}
        this.Components.MinSize.[i] <- {Width = 0f; Height = 0f}

        this.Components.WantsOnClick.[i] <- false 
        this.Components.OnClick.[i] <- ignore
        this.Components.LastPressTime[i] <- TimeSpan.Zero
        
        this.Components.Count <- this.Components.Count + 1

        cid

    member this.Overlaypane(rcid: UIComponentId) =
        let cid = this.Create "Division"
        this.Components.Layout.[cid.Index] <- Layout.Relative rcid
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Window() =
        let cid = this.Create "Panel"
        this.Components.Layout.[cid.Index] <- Layout.LinearVertical
        cid

    member this.WindowWithGrid (cols: int, rows: int) =
        let cid = this.Create "Window"
        this.Components.Layout.[cid.Index] <- Layout.Grid(cols, rows)
        cid

    member this.Header (t: string) = 
        let cid = this.Create "Header1"
        this.Components.Text.[cid.Index] <- t
        this.Components.Block.[cid.Index] <- true

        cid

    member this.Label (t: string) = 
        let cid = this.Create "Label"
        this.Components.Text.[cid.Index] <- t

        cid

    member this.Paragraph (t: string) = 
        let cid = this.Create "Paragraph"
        this.Components.Text.[cid.Index] <- t
        this.Components.Textwrap.[cid.Index] <- true 
        this.Components.TextAlign.[cid.Index] <- NoobishTextAlignment.TopLeft 
        this.Components.Block.[cid.Index] <- true 
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = false}

        cid


    member this.Textbox (t: string) (onTextChanged: (OnClickEvent)-> string-> unit) = 
        let cid = this.Create "TextBox"

        this.Components.WantsFocus.[cid.Index] <- true 
        this.Components.OnFocus.[cid.Index] <- (
            fun event focus -> ()
                
        )

        this.Components.WantsKeyTyped.[cid.Index] <- true 
        this.Components.OnKeyTyped.[cid.Index] <- (fun event typed ->
            let text = this.Components.Text.[cid.Index]
            if int typed = 8 then // backspace
                if text.Length > 0 && this.Cursor > 0 then
                    this.Components.Text.[cid.Index] <- text.Remove(this.Cursor - 1, 1)
                    this.Cursor <- this.Cursor - 1
            elif int typed = 13 || int typed = 10 then 
                this.FocusedElementId <- UIComponentId.empty
                onTextChanged {SourceId = cid} this.Components.Text[cid.Index]
                this.Cursor <- 0
            elif int typed = 27 then
                this.FocusedElementId <- UIComponentId.empty
                this.Components.Text.[cid.Index] <- t
                this.Cursor <- 0
            elif int typed = 127 then // deleted
                if text.Length > 0 && this.Cursor < text.Length then
                    this.Components.Text.[cid.Index] <- text.Remove(this.Cursor, 1)
            else
                this.Components.Text.[cid.Index] <- text.Insert(this.Cursor, typed.ToString())
                this.Cursor <- this.Cursor + 1
        )
        this.Components.WantsKeyPressed.[cid.Index] <- true 
        this.Components.OnKeyPressed.[cid.Index] <- (fun event key ->
            let text = this.Components.Text.[cid.Index]
            if key = Microsoft.Xna.Framework.Input.Keys.Left then 
                let newCursorPos = this.Cursor - 1 
                this.Cursor <- max 0 newCursorPos
            elif key = Microsoft.Xna.Framework.Input.Keys.Right then 
                let newCursorPos = this.Cursor + 1 
                this.Cursor <- min text.Length newCursorPos
        )

        this.Components.Text.[cid.Index] <- t



        cid

    member this.Button (t: string) (onClick: OnClickEvent -> unit) = 
        let cid = this.Create "Button"
        this.Components.Text.[cid.Index] <- t
        this.Components.WantsOnClick.[cid.Index] <- true
        this.Components.OnClick.[cid.Index] <- onClick
        cid

    member this.Space () = 
        let cid = this.Create "Space"
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Div () = 
        let cid = this.Create "Division"
        this.Components.Block.[cid.Index] <- true
        this.SetLayout (Layout.LinearVertical) cid.Index

        cid

    member this.Grid (cols: int, rows: int) = 
        let cid = this.Create "Division"
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.PanelWithGrid (cols: int, rows: int) = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.PanelHorizontal () = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.LinearHorizontal) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.PanelVertical () = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.LinearVertical) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.HorizontalRule () = 
        let cid = this.Create "HorizontalRule"
        this.Components.Id.[cid.Index] <- cid
        this.Components.Block.[cid.Index] <- true
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = false} 
        cid

    member this.Combobox<'T> (items: 'T[]) (selectedIndex: int) (onValueChanged: OnClickEvent -> 'T -> unit)= 
        let cid = this.Create "Combobox"
        this.Components.Text.[cid.Index] <- items.[selectedIndex].ToString()

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
                                this.Components.Text.[cid.Index] <- i.ToString(); onValueChanged ({SourceId=cid}) i
                        ) |> this.SetFillHorizontal
                )
            )
            
        overlayPaneId 
        |> this.SetChildren [| overlayWindowId |]
        |> this.SetLayer 225
        |> this.SetVisible false 
        |> ignore


        this.Components.WantsOnClick.[cid.Index] <- true
        this.Components.OnClick.[cid.Index] <- (fun event -> 
            this.SetVisible true overlayPaneId |> ignore
        )

        cid

    member this.SetSize (width: int, height: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.MinSize.[index] <- {Width = float32 width; Height = float32 height}
        cid

    member this.SetLayer (layer: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 

            let children = this.Components.Children.[cid.Index]
            if children.Count > 0 then 
                let previousLayer = this.Components.Layer.[index]

                for j = 0 to children.Count - 1 do 
                    let ccid = children.[j]
                    let deltaLayer = this.Components.Layer.[ccid.Index] - previousLayer
                    this.SetLayer (layer + deltaLayer) children.[j] |> ignore

            this.Components.Layer.[index] <- layer
        cid


    member this.SetVisible (v: bool) (cid: UIComponentId): UIComponentId = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Visible.[index] <- v
            
            let children = this.Components.Children.[cid.Index]
            for j = 0 to children.Count - 1 do 
                this.SetVisible v children.[j] |> ignore
        cid

    member this.SetEnabled (v: bool) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Enabled.[index] <- v
        cid

    member this.SetPosition (x: int, y: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Bounds.[index] <- {this.Components.Bounds.[index] with X = float32 x; Y = float32 y}
        cid

    member private this.SetLayout (layout: Layout) (index: int) =
        this.Components.Layout.[index] <- layout

    member this.SetRowspan (rowspan: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.GridSpan.[index] <- {this.Components.GridSpan.[index] with Rowspan = rowspan}
        cid

    member this.SetColspan (colspan: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.GridSpan.[index] <- {this.Components.GridSpan.[index] with Colspan = colspan}
        cid

    member this.SetMargin (margin: int) (cid: UIComponentId) =
        let margin = float32 margin
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Margin.[index] <- {Top = margin; Right = margin; Bottom = margin; Left = margin}
        cid

    member this.FillHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Horizontal = true}
        cid

    member this.SetFillHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Horizontal = true}
        cid

    member this.FillVertical (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Vertical = true}
        cid
    member this.SetFill (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        cid

    member this.SetScrollHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Horizontal = true}
        cid

    member this.SetScrollVertical (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Vertical = true}
        cid


    member this.SetScroll (horizontal: bool) (vertical: bool) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Horizontal = horizontal; Vertical = vertical}
        cid

    member this.AlignText (textAlignment: NoobishTextAlignment) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.TextAlign.[index] <- textAlignment
        cid       

    member this.AlignTextTopLeft (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.TopLeft cid

    member this.AlignTextTop (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.TopCenter cid

    member this.AlignTextTopRight (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.TopRight cid


    member this.AlignTextLeft (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.Left cid

    member this.AlignTextCenter (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.Center cid

    member this.AlignTextRight (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.Right cid


    member this.AlignTextBottomLeft (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.BottomLeft cid

    member this.AlignTextBottomCenter (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.BottomCenter cid

    member this.AlignTextBottomRight (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.BottomRight cid

  
    member this.SetToggled (t: bool) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Toggled.[index] <- t
        cid

    member this.SetOnClick (onClick: OnClickEvent -> unit) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.WantsOnClick.[index] <- true
            this.Components.OnClick.[index] <- onClick
        cid

    member this.BumpLayer (layer: int) (children: IReadOnlyList<UIComponentId>) = 
        for i = 0 to children.Count - 1 do 
            let cid = children.[i]
            let childLayer = this.Components.Layer.[cid.Index] + layer
            this.Components.Layer.[cid.Index] <- childLayer
            this.BumpLayer childLayer this.Components.Children.[cid.Index]


    member this.SetChildren (cs: UIComponentId[]) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index > -1 then 
            for i = 0 to cs.Length - 1 do 
                let childId = cs[i]
                if this.Components.ParentId.[childId.Index] <> UIComponentId.empty then failwith "what"
                this.Components.ParentId.[childId.Index] <- cid
            let childrenIds = this.Components.Children.[index]
            childrenIds.AddRange cs

            let layer = this.Components.Layer.[index]
            this.BumpLayer layer (childrenIds :> IReadOnlyList<UIComponentId>)
        cid

    member this.CalculateContentSize (content: ContentManager) (styleSheet: NoobishStyleSheet) (i: int) =


        let struct(contentWidth, contentHeight) = 
            match this.Components.Layout.[i] with 
            | Layout.LinearHorizontal -> 
                let children = this.Components.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]

                    let childPadding = this.Components.Padding.[cid.Index]
                    let childMargin = this.Components.Margin.[cid.Index]
                    let childSize = this.Components.ContentSize.[cid.Index]
                    width <- width + (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- max height (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                struct(width, height)
            | Layout.LinearVertical -> 
                let children = this.Components.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]

                    let childPadding = this.Components.Padding.[cid.Index]
                    let childMargin = this.Components.Margin.[cid.Index]
                    let childSize = this.Components.ContentSize.[cid.Index]
                    width <- max width (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- height + (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                struct (width, height)

            | Layout.Grid(cols, rows) -> 
                let children = this.Components.Children.[i]

                let mutable maxColWidth = 0f
                let mutable maxRowHeight = 0f

                for i = 0 to children.Count - 1 do
                    let cid = children.[i]

                    let gridSpan = this.Components.GridSpan.[cid.Index]
                    let childPadding = this.Components.Padding.[cid.Index]
                    let childMargin = this.Components.Margin.[cid.Index]
                    let childSize = this.Components.ContentSize.[cid.Index]
                    maxColWidth <- max maxColWidth ((childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right) / float32 gridSpan.Colspan)
                    maxRowHeight <- max maxRowHeight ((childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom) / float32 gridSpan.Rowspan)

                struct(maxColWidth * float32 cols, maxRowHeight * float32 rows)
            | Layout.Relative (rcid) -> 
                let pcid = this.Components.ParentId.[rcid.Index]
                struct(0f, 0f)
            | Layout.None -> struct(0f,0f)
        this.Components.ContentSize.[i] <- {Width = contentWidth; Height = contentHeight}

    member this.LayoutComponent (content: ContentManager) (styleSheet: NoobishStyleSheet) (startX: float32) (startY: float32) (parentWidth: float32) (parentHeight: float32) (i: int) = 

        let fill = this.Components.Fill.[i]
        let scroll = this.Components.Scroll.[i]
        let text = this.Components.Text.[i]
        let margin = this.Components.Margin.[i]
        let padding = this.Components.Padding.[i]
        let minSize = this.Components.MinSize.[i]


        let maxWidth = parentWidth
        let maxHeight = parentHeight

        let viewportStartX = startX + margin.Left + padding.Left
        let viewportStartY = startY + margin.Top + padding.Top
        let viewportWidth = maxWidth - margin.Left - margin.Right - padding.Left - padding.Right
        let viewportHeight = maxHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom


        match this.Components.Layout.[i] with 
        | Layout.LinearHorizontal -> 
            let children = this.Components.Children.[i]
            let mutable totalWidth = viewportStartX
            let childY = viewportStartY
            let mutable maxChildHeight = 0f
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet totalWidth childY viewportWidth viewportHeight cid.Index

                let childBounds = this.Components.Bounds.[cid.Index]
                totalWidth <- childBounds.Right
                maxChildHeight <- max maxChildHeight childBounds.Height

            this.Components.ContentSize.[i] <- {
                Width = totalWidth
                Height = maxChildHeight
            }

            this.Components.Bounds.[i] <- {
                X = viewportStartX; 
                Y = viewportStartY; 
                Width = 
                    if fill.Horizontal then viewportWidth
                    elif scroll.Horizontal && totalWidth > viewportWidth then maxWidth
                    else totalWidth
                Height = 
                    if fill.Vertical then viewportHeight
                    elif scroll.Vertical && maxChildHeight > viewportHeight then maxHeight
                    else maxChildHeight 
            }

        | Layout.LinearVertical -> 
            let children = this.Components.Children.[i]
            let mutable totalHeight = viewportStartY
            let childX = viewportStartX
            let mutable maxChildWidth = 0f
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX totalHeight viewportWidth viewportHeight cid.Index

                let childBounds = this.Components.Bounds.[cid.Index]
                totalHeight <- childBounds.Bottom
                maxChildWidth <- max maxChildWidth childBounds.Width

            this.Components.ContentSize.[i] <- {
                Width = maxChildWidth
                Height = totalHeight
            }

            this.Components.Bounds.[i] <- {
                X = viewportStartX; 
                Y = viewportStartY; 
                Width = 
                    if fill.Horizontal then viewportWidth
                    elif scroll.Horizontal && maxChildWidth > viewportWidth then maxWidth
                    else maxChildWidth
                Height = 
                    if fill.Vertical then viewportHeight
                    elif scroll.Vertical && totalHeight > viewportHeight then maxHeight
                    else totalHeight 
            }

        | Layout.Grid(cols, rows) -> 
            let startX = viewportStartX
            let startY = viewportStartY

            let children = this.Components.Children.[i]
            let colWidth = viewportWidth / float32 cols
            let rowHeight = viewportHeight  / float32 rows
            let mutable col = 0
            let mutable row = 0

            let cellUsed = Array2D.create cols rows false

            for i = 0 to children.Count - 1 do
                let cid = children.[i]
                let childStartX = (startX + (float32 col) * (colWidth))
                let childStartY = (startY + (float32 row) * (rowHeight))

                let gridSpan = this.Components.GridSpan.[cid.Index]

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


            this.Components.ContentSize.[i] <- {
                Width = viewportWidth
                Height = viewportHeight
            }

            this.Components.Bounds.[i] <- {
                X = viewportStartX; 
                Y = viewportStartY; 
                Width = viewportWidth;
                Height = viewportHeight;
            }
        | Layout.Relative (rcid) -> 
            let pcid = this.Components.ParentId.[rcid.Index]
            let parentBounds = this.Components.Bounds.[pcid.Index]

            let relativeBounds = this.Components.Bounds.[rcid.Index]
            let startX = relativeBounds.X
            let startY = relativeBounds.Y
            
            let children = this.Components.Children.[i]
            for i = 0 to children.Count - 1 do 
                let ccid = children.[i]
                let relativePosition = this.Components.RelativePosition.[ccid.Index]
                let childStartX = startX + relativePosition.X 
                let childStartY = startY + relativePosition.Y 
                this.LayoutComponent content styleSheet childStartX childStartY parentBounds.Width parentBounds.Height ccid.Index

            this.Components.ContentSize.[i] <- {
                Width = viewportWidth
                Height = viewportHeight
            }

            this.Components.Bounds.[i] <- {
                X = viewportStartX; 
                Y = viewportStartY; 
                Width = viewportWidth;
                Height = viewportHeight;
            }

        | Layout.None -> 

            let struct(contentWidth, contentHeight) = 
                if not (String.IsNullOrWhiteSpace text) then
                    let themeId = this.Components.ThemeId.[i]

                    let fontId = styleSheet.GetFont themeId "default"
                    let font = content.Load<NoobishFont> fontId
                    let fontSize = int(float32 (styleSheet.GetFontSize themeId "default"))

                    if this.Components.Textwrap.[i] then
                        NoobishFont.measureMultiLine font fontSize viewportWidth text
                    else 
                        NoobishFont.measureSingleLine font fontSize text
                else 
                    struct(minSize.Width, minSize.Height)
                
            this.Components.ContentSize.[i] <- {Width = contentWidth; Height = contentHeight}
            this.Components.Bounds[i] <- {
                this.Components.Bounds.[i] with  
                    X = viewportStartX
                    Y = viewportStartY
                    Width = 
                        if fill.Horizontal then 
                            viewportWidth
                        else 
                            contentWidth + margin.Left + margin.Right + padding.Left + padding.Right
                    Height = 
                        if fill.Vertical then 
                            viewportHeight
                        else 
                            contentHeight + margin.Top + margin.Bottom + padding.Top + padding.Bottom
            }


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
        let themeId = this.Components.ThemeId.[i]
        let layer = 1f - float32 (this.Components.Layer.[i] + 32) / 255f


        let fontId = styleSheet.GetFont themeId "default"
        let font = content.Load<NoobishFont>  fontId
        let fontSize = styleSheet.GetFontSize themeId "default"

        let bounds = this.Components.Bounds.[i]
        let margin = this.Components.Margin.[i]
        let padding = this.Components.Padding.[i]
        let contentStartX = bounds.X + margin.Left + padding.Left
        let contentStartY = bounds.Y +  margin.Top + padding.Top
        let contentWidth = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom
        let bounds: Internal.NoobishRectangle = {X = contentStartX; Y = contentStartY; Width = contentWidth; Height = contentHeight}

        let text = this.Components.Text.[i]
        let textAlign = this.Components.TextAlign.[i]
        let textWrap = this.Components.Textwrap.[i]

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
            if this.Components.WantsFocus.[i] && this.FocusedElementId.Id = this.Components.Id.[i].Id then 
                "focused"
            else 
                "default"


        let themeId = this.Components.ThemeId.[i]
        let bounds = this.Components.Bounds.[i]
        let margin = this.Components.Margin.[i]
        let layer = this.Components.Layer.[i]
        let contentStartX = bounds.X + margin.Left
        let contentStartY = bounds.Y +  margin.Top 
        let contentWidth = bounds.Width - margin.Left - margin.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom

        let color =
            if this.Components.WantsFocus.[i] && this.FocusedElementId = this.Components.Id.[i] then
                styleSheet.GetColor themeId "focused"
            elif not this.Components.Enabled.[i] then
                styleSheet.GetColor themeId "disabled"
            elif this.Components.Toggled.[i] then
                styleSheet.GetColor themeId "toggled"
            else
                let lastPressTime = this.Components.LastPressTime.[i]
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
        let text = this.Components.Text.[i]
        if text.Length > 0 then 
            let themeId = this.Components.ThemeId.[i]
            let layer = this.Components.Layer.[i]

            let layer = (1f - float32 (layer + 1) / 255.0f)

            let fontId = styleSheet.GetFont themeId "default"
            let font = content.Load<NoobishFont> fontId

            let fontSize = (styleSheet.GetFontSize themeId "default")

            let bounds = this.Components.Bounds.[i]
            let margin = this.Components.Margin.[i]
            let padding = this.Components.Padding.[i]
            let contentStartX = bounds.X + padding.Left + margin.Left
            let contentStartY = bounds.Y + padding.Top + margin.Top
            let contentWidth = bounds.Width - padding.Left - padding.Right - margin.Left - margin.Right
            let contentHeight = bounds.Height - padding.Top - padding.Bottom - margin.Left - margin.Right
            let bounds: Internal.NoobishRectangle = {X = contentStartX; Y = contentStartY; Width = contentWidth; Height = contentHeight}
            let textWrap = this.Components.Textwrap.[i]
            let textAlign = this.Components.TextAlign.[i]
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


        let visible = this.Components.Visible.[i]
        if visible then 

            let pcid = this.Components.ParentId.[i]
            let parentViewport: NoobishRectangle =
                if pcid = UIComponentId.empty then 
                    {X = 0.0f; Y = 0.0f; Width = this.ScreenWidth; Height = this.ScreenHeight}
                else 
                    let padding = this.Components.Padding.[pcid.Index]
                    let margin = this.Components.Margin.[pcid.Index]
                    let bounds = this.Components.Bounds.[pcid.Index]
                    {
                        X = bounds.X + margin.Left + padding.Left
                        Y = bounds.Y + margin.Top + padding.Top
                        Width = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
                        Height = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom
                    }

            let textureAtlas = content.Load(styleSheet.TextureAtlasId)

            let bounds = this.Components.Bounds.[i]
            let margin = this.Components.Margin.[i]
            let padding = this.Components.Padding.[i]
            let contentStartX = bounds.X + margin.Left
            let contentStartY = bounds.Y + margin.Top
            let contentWidth = bounds.Width - margin.Left - margin.Right
            let contentHeight = bounds.Height - margin.Left - margin.Right


            let boundsWithMargin = ({
                    X = contentStartX
                    Y = contentStartY
                    Width = contentWidth
                    Height =contentHeight
            })
            let boundsWithMargin = boundsWithMargin.Clamp parentViewport

            let boundsWithMarginAndPadding = {

                    X = contentStartX + padding.Left
                    Y = contentStartY + padding.Top
                    Width = contentWidth - padding.Left - padding.Right
                    Height = contentHeight - padding.Top - padding.Bottom
            }
            let boundsWithMarginAndPadding = boundsWithMarginAndPadding.Clamp parentViewport

            let oldScissorRect = graphics.ScissorRectangle

            graphics.ScissorRectangle <- DrawUI.toRectangle boundsWithMargin 
            spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)

            this.DrawBackground styleSheet textureAtlas spriteBatch gameTime i

            if this.FocusedElementId = this.Components.Id.[i] then 
                this.DrawCursor styleSheet content textureAtlas spriteBatch i gameTime 0f 0f

            spriteBatch.End()

            graphics.ScissorRectangle <- DrawUI.toRectangle boundsWithMarginAndPadding

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
        if waitForLayout && this.Components.Count > 0 then 

            for i = 0 to this.Components.Count - 1 do 
                let themeId = this.Components.ThemeId.[i]

                let (top, left, bottom, right) = styleSheet.GetMargin themeId "default"
                this.Components.Margin.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                let (top, left, bottom, right) = styleSheet.GetPadding themeId "default"
                this.Components.Padding.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                (* Run layout only for root components. *)
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, this.Components.Layer.[i])


            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.LayoutComponent content styleSheet 0f 0f this.ScreenWidth this.ScreenHeight i

            waitForLayout <- false



        for i = 0 to this.Components.Count - 1 do 
            let layer = this.Components.Layer.[i]
            drawQueue.Enqueue(i, layer)

        let oldRasterizerState = graphics.RasterizerState
        graphics.RasterizerState <- rasterizerState

        let styleSheet = content.Load<Noobish.Styles.NoobishStyleSheet> styleSheetId
        while drawQueue.Count > 0 do 
            let i = drawQueue.Dequeue()
            this.DrawComponent graphics content spriteBatch textBatch styleSheet false i gameTime

        graphics.RasterizerState <- oldRasterizerState


    member this.ComponentContains (x: float32) (y: float32) (i: int) = 
        let bounds = this.Components.Bounds.[i]

        x > bounds.X && x < bounds.X + bounds.Width && y > bounds.Y && y < bounds.Y + bounds.Height

    member this.Hover (x: float32) (y: float32) (gameTime: GameTime) (i: int)  =
        if this.Components.Visible.[i] && this.Components.Enabled.[i]&& this.ComponentContains x y i then 
            Log.Logger.Debug ("Mouse hover inside component {ComponentId}", i)

            let children = this.Components.Children.[i]
            if children.Count = 0 then 
                this.Components.LastHoverTime.[i] <- gameTime.TotalGameTime
            else 
                for j = 0 to children.Count - 1 do 
                    this.Hover x y gameTime children.[j].Index

    member this.Click (x: float32) (y: float32) (gameTime: GameTime) (i: int): bool  =
        if this.Components.Visible.[i] && this.Components.Enabled.[i]&& this.ComponentContains x y i then 
            Log.Logger.Debug ("Mouse click inside component {ComponentId}", i)

            let children = this.Components.Children.[i]

            let mutable found = false
            let mutable j = 0
            while not found && j < children.Count do 
                if this.Click x y gameTime children.[j].Index then 
                    found <- true 
                j <- j + 1

            if not found && this.Components.WantsOnClick.[i] then 
                found <- true
                this.Components.OnClick.[i] ({SourceId = this.Components.Id.[i]})
            
            if not found  && this.Components.WantsFocus.[i] then
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


    member this.Press (x: float32) (y: float32) (gameTime: GameTime) (i: int): bool =

        if this.Components.Visible.[i] && this.Components.Enabled.[i]&& this.ComponentContains x y i then 
            Log.Logger.Debug ("Mouse press inside component {ComponentId}", i)
            let children = this.Components.Children.[i]

            if children.Count = 0 then 
                this.Components.LastPressTime.[i] <- gameTime.TotalGameTime
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
        for i = 0 to this.Components.Count - 1 do 
            if this.Components.ParentId.[i] = UIComponentId.empty then 
                this.Hover x y gameTime i 


        if mouseState.LeftButton = ButtonState.Pressed then 

            for i = 0 to this.Components.Count - 1 do 
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, -this.Components.Layer.[i])

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.Press x y gameTime i |> ignore

        elif mouseState.LeftButton = ButtonState.Released && previousMouseState.LeftButton = ButtonState.Pressed then 
            this.FocusedElementId <- UIComponentId.empty
            for i = 0 to this.Components.Count - 1 do 
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, -this.Components.Layer.[i])

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.Click x y gameTime i |> ignore

        previousMouseState <- mouseState

    member this.ProcessKeys (gameTime: GameTime) = 
        let keyState = Keyboard.GetState()

        let index = this.GetIndex(this.FocusedElementId)
        if index <> -1 && this.Components.WantsKeyPressed.[index] then 
            if previousKeyState.IsKeyDown(Keys.Left) && keyState.IsKeyUp (Keys.Left) then 
                this.Components.OnKeyPressed.[index] {SourceId = this.Components.Id.[index]} Keys.Left

            if previousKeyState.IsKeyDown(Keys.Right) && keyState.IsKeyUp (Keys.Right) then 
                this.Components.OnKeyPressed.[index] {SourceId = this.Components.Id.[index]} Keys.Right

        previousKeyState <- keyState
    member this.Update (gameTime: GameTime) = 
        this.ProcessMouse(gameTime)
        this.ProcessKeys(gameTime)




    member this.Clear() =
        this.FocusedElementId <- UIComponentId.empty
        for i = 0 to this.Components.Count - 1 do 
            free.Enqueue(i, i)
            this.Components.Id.[i] <- UIComponentId.empty
            this.Components.ThemeId.[i] <- ""

            this.Components.ParentId.[i] <- UIComponentId.empty

            this.Components.Visible.[i] <- true 
            this.Components.Enabled.[i] <- true 
            this.Components.Toggled.[i] <- false 
            this.Components.Hovered.[i] <- false 

            this.Components.Block.[i] <- false

            this.Components.Text.[i] <- ""
            this.Components.Textwrap.[i] <- false
            this.Components.TextAlign.[i] <- NoobishTextAlignment.Left

            this.Components.Bounds.[i] <- {X = 0f; Y = 0f; Width = 0f; Height = 0f}
            this.Components.MinSize.[i] <- {Width = 0f; Height = 0f}
            this.Components.ContentSize.[i] <- {Width = 0f; Height = 0f}
            this.Components.RelativePosition.[i] <- {X = 0f; Y = 0f}
            this.Components.Fill.[i] <- {Horizontal = false; Vertical = false}
            this.Components.Padding.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
            this.Components.Margin.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

            this.Components.Layer.[i] <- -1
            this.Components.Layout.[i] <- Layout.None

            this.Components.GridSpan.[i] <- {Rowspan = 1; Colspan = 1}

            this.Components.WantsOnClick.[i] <- false
            this.Components.OnClick.[i] <- ignore

            this.Components.WantsKeyTyped.[i] <- false 
            this.Components.OnKeyTyped.[i] <- (fun _ _ ->())

            this.Components.WantsKeyPressed.[i] <- false 
            this.Components.OnKeyPressed.[i] <- (fun _ _ ->())

            this.Components.WantsFocus.[i] <- false 
            this.Components.OnFocus.[i] <- (fun _ _ ->())

            this.Components.LastPressTime.[i] <- TimeSpan.Zero
            this.Components.LastHoverTime.[i] <- TimeSpan.Zero

            this.Components.Children.[i].Clear()
        this.Components.Count <- 0

        waitForLayout <- true


