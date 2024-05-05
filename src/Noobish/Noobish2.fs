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




[<Struct>]
type TableSpan = {
    Rowspan: int
    Colspan: int
}

[<Struct>]
type Fill = {
    Horizontal: bool
    Vertical: bool
}

[<Struct>]
type Margin = {
    Top: float32
    Right: float32
    Bottom: float32
    Left: float32
}

[<Struct>]
type Padding = {
    Top: float32
    Right: float32
    Bottom: float32
    Left: float32
}

[<Struct>]
type Scroll = {
    Horizontal: bool
    Vertical: bool
}

[<Struct>]
type Size = {Width: float32; Height: float32}

[<Struct>]
type Position = {X: float32; Y: float32}


[<Struct>]
type NoobishRectangle = {
    X: float32
    Y: float32
    Width: float32
    Height: float32
} with
    member r.Left with get() = r.X
    member r.Right with get() = r.X + r.Width
    member r.Top with get() = r.Y
    member r.Bottom with get() = r.Y + r.Height


module DrawUI = 


    let createRectangle (x: float32) (y:float32) (width: float32) (height: float32) =
        Rectangle (int (x), int (y), int (width), int (height))

    let drawDrawable (textureAtlas: NoobishTextureAtlas) (spriteBatch: SpriteBatch)  (position: Vector2) (size: Vector2) (layer: float32) (color: Color) (drawables: NoobishDrawable[]) =
        for drawable in drawables do
            match drawable with
            | NoobishDrawable.Texture _ -> failwith "Texture not supported for cursor."
            | NoobishDrawable.NinePatch(tid) ->
                let texture = textureAtlas.[tid]

                spriteBatch.DrawAtlasNinePatch2(
                    texture,
                    Rectangle(int position.X, int position.Y, int size.X, int size.Y),
                    color,
                    layer)
            | NoobishDrawable.NinePatchWithColor(tid, color) ->
                let texture = textureAtlas.[tid]

                spriteBatch.DrawAtlasNinePatch2(
                    texture,
                    Rectangle(int position.X, int position.Y, int size.X, int size.Y),
                    color,
                    layer)


[<Struct>]
[<CustomEquality; NoComparison>]
type UIComponentId = {
    Index: int
    Id: Guid
} with 
    override this.Equals other =
        match other with
        | :? UIComponentId as p -> (this :> IEquatable<UIComponentId>).Equals p
        | _ -> false   
        
    override this.GetHashCode () = this.Id.GetHashCode() 

    static member op_Equality(this : UIComponentId, other : UIComponentId) =
        this.Id.Equals other.Id
    
    interface IEquatable<UIComponentId> with 
        member this.Equals (other: UIComponentId) =
            this.Id.Equals other.Id

module UIComponentId =
    let empty: UIComponentId = { Index = -1; Id = Guid.Empty }


type OnClickEvent = {
    SourceId: UIComponentId
}

[<RequireQualifiedAccess>]
type Layout =
| LinearHorizontal 
| LinearVertical
| Grid of cols: int * rows: int
| Relative of UIComponentId
| None

type Noobish2(maxCount: int) =

    let mutable previousMouseState: MouseState = Microsoft.Xna.Framework.Input.Mouse.GetState()

    let mutable previousKeyState: KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState()
    let mutable waitForLayout = true

    let drawQueue = PriorityQueue<int, int>()

    let toCalculateSize = PriorityQueue<int, int>()
    let toLayout = PriorityQueue<int, int>()


    let free = PriorityQueue<int, int>(Array.init maxCount (fun i -> struct(i,i)))



    member val Id = Array.create maxCount UIComponentId.empty

    member val ThemeId = Array.create maxCount ""

    member val ParentId = Array.create maxCount UIComponentId.empty

    member val Children = Array.init maxCount (fun _ -> ResizeArray())

    member val Visible = Array.create maxCount true 
    member val Enabled = Array.create maxCount true 

    member val Block = Array.create maxCount false

    member val Text = Array.create maxCount ""
    member val TextAlign = Array.create maxCount NoobishTextAlignment.Left
    member val Textwrap = Array.create maxCount false

    member val Layer = Array.create maxCount 0
    member val Bounds = Array.create<NoobishRectangle> maxCount {X = 0f; Y = 0f; Width = 0f; Height = 0f}
    member val MinSize = Array.create maxCount {Width = 0f; Height = 0f}
    member val ContentSize = Array.create maxCount {Width = 0f; Height = 0f}
    member val RelativePosition = Array.create maxCount {X = 0f; Y = 0f}

    member val Fill = Array.create<Fill> maxCount ({Horizontal = false; Vertical = false})

    member val Padding = Array.create<Padding> maxCount {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Margin = Array.create<Margin> maxCount {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Layout = Array.create maxCount Layout.None

    member val GridSpan = Array.create maxCount ({Rowspan = 1; Colspan = 1})

    member val WantsOnClick = Array.create maxCount false
    member val OnClick = Array.init<OnClickEvent -> unit> maxCount (fun _event -> ignore)

    member val WantsKeyTyped = Array.create maxCount false 
    member val OnKeyTyped = Array.create<OnClickEvent -> char -> unit> maxCount (fun _ _ -> ()) 


    member val WantsKeyPressed = Array.create maxCount false 
    member val OnKeyPressed = Array.create<OnClickEvent -> Keys -> unit> maxCount (fun _ _ -> ()) 

    member val FocusedElementId = UIComponentId.empty with get, set
    member val FocusedTime = TimeSpan.Zero with get, set

    member val Cursor = 0 with get, set 

    member val WantsFocus = Array.create maxCount false 
    member val OnFocus = Array.create<OnClickEvent -> bool -> unit> maxCount (fun _ _ -> ())

    member val Scroll = Array.create<Scroll> maxCount {Horizontal = true; Vertical = true}

    member val Toggled = Array.create maxCount false 

    member val ScreenWidth: float32 = 0f with get, set 
    member val ScreenHeight: float32 = 0f with get, set

    member val Debug = false with get, set

    member val private Count = 0 with get, set

    member this.IsActive(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && this.Id.[index].Id.Equals cid.Id then 
            index
        else 
            -1
    member this.GetIndex(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && this.Id.[index].Id.Equals cid.Id then 
            index
        else 
            -1

    member private this.Create(themeId: string): UIComponentId =
        if free.Count = 0 then failwith "Out of free components."

        let i = free.Dequeue() 
        let cid: UIComponentId = {Index = i; Id = Guid.NewGuid()}
        this.Id.[i] <- cid
        this.ThemeId.[i] <- themeId
        if this.ParentId.[i] <> UIComponentId.empty then failwith "crap"
        this.Enabled.[i] <- true 
        this.Visible.[i] <- true 
        this.Layer.[i] <- 1
        this.Layout.[i] <- Layout.None
        this.GridSpan.[i] <- {Colspan = 1; Rowspan = 1}
        this.MinSize.[i] <- {Width = 0f; Height = 0f}

        this.WantsOnClick.[i] <- false 
        this.OnClick.[i] <- ignore
        
        this.Count <- this.Count + 1

        cid

    member this.Overlaypane(rcid: UIComponentId) =
        let cid = this.Create "Division"
        this.Layout.[cid.Index] <- Layout.Relative rcid
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Window() =
        let cid = this.Create "Panel"
        this.Layout.[cid.Index] <- Layout.LinearVertical
        cid

    member this.WindowWithGrid (cols: int) (rows: int) =
        let cid = this.Create "Window"
        this.Layout.[cid.Index] <- Layout.Grid(cols, rows)
        cid

    member this.Header (t: string) = 
        let cid = this.Create "Header1"
        this.Text.[cid.Index] <- t
        this.Block.[cid.Index] <- true

        cid

    member this.Label (t: string) = 
        let cid = this.Create "Label"
        this.Text.[cid.Index] <- t

        cid

    member this.Textbox (t: string) (onTextChanged: (OnClickEvent)-> string-> unit) = 
        let cid = this.Create "TextBox"

        this.WantsFocus.[cid.Index] <- true 
        this.OnFocus.[cid.Index] <- (
            fun event focus -> ()
                
        )

        this.WantsKeyTyped.[cid.Index] <- true 
        this.OnKeyTyped.[cid.Index] <- (fun event typed ->
            let text = this.Text.[cid.Index]
            if int typed = 8 then // backspace
                if text.Length > 0 && this.Cursor > 0 then
                    this.Text.[cid.Index] <- text.Remove(this.Cursor - 1, 1)
                    this.Cursor <- this.Cursor - 1
            elif int typed = 13 then
                this.FocusedElementId <- UIComponentId.empty
                this.Cursor <- 0
            elif int typed = 127 then // deleted
                if text.Length > 0 && this.Cursor < text.Length then
                    this.Text.[cid.Index] <- text.Remove(this.Cursor, 1)
            else
                this.Text.[cid.Index] <- text.Insert(this.Cursor, typed.ToString())
                this.Cursor <- this.Cursor + 1
        )
        this.WantsKeyPressed.[cid.Index] <- true 
        this.OnKeyPressed.[cid.Index] <- (fun event key ->
            let text = this.Text.[cid.Index]
            if key = Microsoft.Xna.Framework.Input.Keys.Left then 
                let newCursorPos = this.Cursor - 1 
                this.Cursor <- max 0 newCursorPos
            elif key = Microsoft.Xna.Framework.Input.Keys.Right then 
                let newCursorPos = this.Cursor + 1 
                this.Cursor <- min text.Length newCursorPos
        )

        this.Text.[cid.Index] <- t



        cid

    member this.Button (t: string) (onClick: OnClickEvent -> unit) = 
        let cid = this.Create "Button"
        this.Text.[cid.Index] <- t
        this.WantsOnClick.[cid.Index] <- true
        this.OnClick.[cid.Index] <- onClick
        cid

    member this.Space () = 
        let cid = this.Create "Space"
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Div () = 
        let cid = this.Create "Division"
        this.Block.[cid.Index] <- true
        this.SetLayout (Layout.LinearVertical) cid.Index

        cid

    member this.Grid (rows: int, cols: int) = 
        let cid = this.Create "Grid"
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.PanelWithGrid (rows: int, cols: int) = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.SetGrid (rows: int, cols: int) (cid: UIComponentId)= 
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        cid   

    member this.PanelHorizontal () = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.LinearHorizontal) cid.Index
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.PanelVertical () = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.LinearVertical) cid.Index
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.HorizontalRule () = 
        let cid = this.Create "HorizontalRule"
        this.Id.[cid.Index] <- cid
        this.Block.[cid.Index] <- true
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = false} 
        cid

    member this.Combobox<'T> (items: 'T[]) (onValueChanged: OnClickEvent -> 'T -> unit)= 
        let cid = this.Create "Combobox"
        this.Text.[cid.Index] <- items.[0].ToString()

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
                                this.Text.[cid.Index] <- i.ToString(); onValueChanged ({SourceId=cid}) i
                        ) |> this.SetFillHorizontal
                )
            )
            
        overlayPaneId 
        |> this.SetChildren [| overlayWindowId |]
        |> this.SetLayer 225
        |> this.SetVisible false 
        |> ignore


        this.WantsOnClick.[cid.Index] <- true
        this.OnClick.[cid.Index] <- (fun event -> 
            this.SetVisible true overlayPaneId |> ignore
        )

        cid

    member this.SetSize (width: int, height: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.MinSize.[index] <- {Width = float32 width; Height = float32 height}
        cid

    member this.SetLayer (layer: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 

            let children = this.Children.[cid.Index]
            if children.Count > 0 then 
                let previousLayer = this.Layer.[index]

                for j = 0 to children.Count - 1 do 
                    let ccid = children.[j]
                    let deltaLayer = this.Layer.[ccid.Index] - previousLayer
                    this.SetLayer (layer + deltaLayer) children.[j] |> ignore

            this.Layer.[index] <- layer
        cid


    member this.SetVisible (v: bool) (cid: UIComponentId): UIComponentId = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Visible.[index] <- v
            
            let children = this.Children.[cid.Index]
            for j = 0 to children.Count - 1 do 
                this.SetVisible v children.[j] |> ignore
        cid

    member this.SetEnabled (v: bool) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Enabled.[index] <- v
        cid

    member this.SetPosition (x: int, y: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Bounds.[index] <- {this.Bounds.[index] with X = float32 x; Y = float32 y}
        cid

    member private this.SetLayout (layout: Layout) (index: int) =
        this.Layout.[index] <- layout

    member this.SetRowspan (rowspan: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.GridSpan.[index] <- {this.GridSpan.[index] with Rowspan = rowspan}
        cid

    member this.SetColspan (colspan: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.GridSpan.[index] <- {this.GridSpan.[index] with Colspan = colspan}
        cid

    member this.SetMargin (margin: int) (cid: UIComponentId) =
        let margin = float32 margin
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Margin.[index] <- {Top = margin; Right = margin; Bottom = margin; Left = margin}
        cid

    member this.FillHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Fill.[index] <- {this.Fill.[index] with Horizontal = true}
        cid

    member this.SetFillHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Fill.[index] <- {this.Fill.[index] with Horizontal = true}
        cid

    member this.FillVertical (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Fill.[index] <- {this.Fill.[index] with Vertical = true}
        cid
    member this.SetFill (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Fill.[index] <- {Horizontal = true; Vertical = true}
        cid

    member this.SetScrollHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Scroll.[index] <- {this.Scroll.[index] with Horizontal = true}
        cid

    member this.SetScrollVertical (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Scroll.[index] <- {this.Scroll.[index] with Vertical = true}
        cid


    member this.SetScroll (horizontal: bool) (vertical: bool) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Scroll.[index] <- {this.Scroll.[index] with Horizontal = horizontal; Vertical = vertical}
        cid

    member this.SetToggled (t: bool) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Toggled.[index] <- t
        cid

    member this.SetOnClick (onClick: OnClickEvent -> unit) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.WantsOnClick.[index] <- true
            this.OnClick.[index] <- onClick
        cid

    member this.BumpLayer (layer: int) (children: IReadOnlyList<UIComponentId>) = 
        for i = 0 to children.Count - 1 do 
            let cid = children.[i]
            let childLayer = this.Layer.[cid.Index] + layer
            this.Layer.[cid.Index] <- childLayer
            this.BumpLayer childLayer this.Children.[cid.Index]


    member this.SetChildren (cs: UIComponentId[]) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index > -1 then 
            for i = 0 to cs.Length - 1 do 
                let childId = cs[i]
                if this.ParentId.[childId.Index] <> UIComponentId.empty then failwith "what"
                this.ParentId.[childId.Index] <- cid
            let childrenIds = this.Children.[index]
            childrenIds.AddRange cs

            let layer = this.Layer.[index]
            this.BumpLayer layer (childrenIds :> IReadOnlyList<UIComponentId>)
        cid

    member this.CalculateMinSize (content: ContentManager) (styleSheet: NoobishStyleSheet) (i: int) = 

        let minSize = this.MinSize.[i]

        let struct(contentWidth, contentHeight) = 
            match this.Layout.[i] with 
            | Layout.LinearHorizontal -> 
                let children = this.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]

                    let childPadding = this.Padding.[cid.Index]
                    let childMargin = this.Margin.[cid.Index]
                    let childSize = this.ContentSize.[cid.Index]
                    width <- width + (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- max height (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                struct(width, height)
            | Layout.LinearVertical -> 
                let children = this.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]

                    let childPadding = this.Padding.[cid.Index]
                    let childMargin = this.Margin.[cid.Index]
                    let childSize = this.ContentSize.[cid.Index]
                    width <- max width (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- height + (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                struct (width, height)

            | Layout.Grid(cols, rows) -> 
                let children = this.Children.[i]

                let mutable maxColWidth = 0f
                let mutable maxRowHeight = 0f

                for i = 0 to children.Count - 1 do
                    let cid = children.[i]

                    let gridSpan = this.GridSpan.[cid.Index]
                    let childPadding = this.Padding.[cid.Index]
                    let childMargin = this.Margin.[cid.Index]
                    let childSize = this.ContentSize.[cid.Index]
                    maxColWidth <- max maxColWidth ((childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right) / float32 gridSpan.Colspan)
                    maxRowHeight <- max maxRowHeight ((childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom) / float32 gridSpan.Rowspan)

                struct(maxColWidth * float32 cols, maxRowHeight * float32 rows)
            | Layout.Relative (rcid) -> 
                let pcid = this.ParentId.[rcid.Index]
                struct(0f, 0f)
            | Layout.None -> 
                let mutable width = 0f
                let mutable height = 0f

                let text = this.Text.[i]
                if not (String.IsNullOrWhiteSpace text) then
                    let themeId = this.ThemeId.[i]

                    let fontId = styleSheet.GetFont themeId "default"
                    let font = content.Load<NoobishFont> fontId
                    let fontSize = int(float32 (styleSheet.GetFontSize themeId "default"))

                    let struct (contentWidth, contentHeight) =
                        if this.Textwrap.[i] then
                            struct(minSize.Width, minSize.Height)
                        else
                            NoobishFont.measureSingleLine font fontSize text

                    width <- max width (float32 contentWidth)
                    height <- max height (float32 contentHeight)
                struct (width, height)


        this.ContentSize.[i] <- {Width = contentWidth; Height = contentHeight}

    member this.LayoutComponent (content: ContentManager) (styleSheet: NoobishStyleSheet) (startX: float32) (startY: float32) (parentWidth: float32) (parentHeight: float32) (i: int) = 

        let fill = this.Fill.[i]
        let text = this.Text.[i]
        let margin = this.Margin.[i]
        let padding = this.Padding.[i]
        let minSize = this.MinSize.[i]
        let contentSize = this.ContentSize.[i]

        let maxWidth = parentWidth - margin.Left - margin.Right - padding.Left - padding.Right
        let maxHeight = parentHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom

        let mutable width = if fill.Horizontal then maxWidth else min (max contentSize.Width minSize.Width) maxWidth
        let mutable height = if fill.Vertical then maxHeight else min (max contentSize.Height minSize.Height) maxHeight

        if not (String.IsNullOrWhiteSpace text) then
            let themeId = this.ThemeId.[i]
            let paddedWidth = maxWidth

            let fontId = styleSheet.GetFont themeId "default"
            let font = content.Load<NoobishFont> fontId
            let fontSize = int(float32 (styleSheet.GetFontSize themeId "default"))

            let struct (contentWidth, contentHeight) =
                if this.Textwrap.[i] then
                    NoobishFont.measureMultiLine font fontSize paddedWidth text
                else
                    NoobishFont.measureSingleLine font fontSize text

            width <- max width (float32 contentWidth)
            height <- max height (float32 contentHeight)


        this.Bounds[i] <- {
            this.Bounds.[i] with  
                X = startX
                Y = startY
                Width = width + margin.Left + margin.Right + padding.Left + padding.Right
                Height = height + margin.Top + margin.Bottom + padding.Top + padding.Bottom
        }

        match this.Layout.[i] with 
        | Layout.LinearHorizontal -> 
            let children = this.Children.[i]
            let mutable childX = startX + margin.Left + padding.Left
            let childY = startY + margin.Top + padding.Top
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY width height cid.Index

                let childSize = this.Bounds.[cid.Index]
                childX <- childX + childSize.Width

        | Layout.LinearVertical -> 
            let children = this.Children.[i]
            let childX = startX + margin.Left + padding.Left
            let mutable childY = startY + margin.Top + padding.Top
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY width height cid.Index

                let childSize = this.Bounds.[cid.Index]
                childY <- childY + childSize.Height

        | Layout.Grid(cols, rows) -> 
            let startX = startX + margin.Left + padding.Left
            let startY = startY + margin.Top + padding.Top

            let children = this.Children.[i]
            let colWidth = width / float32 cols
            let rowHeight = height  / float32 rows
            let mutable col = 0
            let mutable row = 0

            let cellUsed = Array2D.create cols rows false

            for i = 0 to children.Count - 1 do
                let cid = children.[i]
                let childStartX = (startX + (float32 col) * (colWidth))
                let childStartY = (startY + (float32 row) * (rowHeight))

                let gridSpan = this.GridSpan.[cid.Index]

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
            let pcid = this.ParentId.[rcid.Index]
            let parentBounds = this.Bounds.[pcid.Index]

            let relativeBounds = this.Bounds.[rcid.Index]
            let startX = relativeBounds.X
            let startY = relativeBounds.Y
            
            let children = this.Children.[i]
            for i = 0 to children.Count - 1 do 
                let ccid = children.[i]
                let relativePosition = this.RelativePosition.[ccid.Index]
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
        let themeId = this.ThemeId.[i]
        let layer = 1f - float32 (this.Layer.[i] + 32) / 255f


        let fontId = styleSheet.GetFont themeId "default"
        let font = content.Load<NoobishFont>  fontId
        let fontSize = styleSheet.GetFontSize themeId "default"

        let bounds = this.Bounds.[i]
        let margin = this.Margin.[i]
        let padding = this.Padding.[i]
        let contentStartX = bounds.X + margin.Left + padding.Left
        let contentStartY = bounds.Y +  margin.Top + padding.Top
        let contentWidth = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom
        let bounds: Internal.NoobishRectangle = {X = contentStartX; Y = contentStartY; Width = contentWidth; Height = contentHeight}

        let text = this.Text.[i]
        let textAlign = this.TextAlign.[i]
        let textWrap = this.Textwrap.[i]

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
            if this.WantsFocus.[i] && this.FocusedElementId.Id = this.Id.[i].Id then 
                "focused"
            else 
                "default"

        let color = Color.White

        let themeId = this.ThemeId.[i]
        let bounds = this.Bounds.[i]
        let margin = this.Margin.[i]
        let layer = this.Layer.[i]
        let contentStartX = bounds.X + margin.Left
        let contentStartY = bounds.Y +  margin.Top 
        let contentWidth = bounds.Width - margin.Left - margin.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom

        let drawables = styleSheet.GetDrawables themeId cstate

        let position = Vector2(contentStartX, contentStartY)
        let size = Vector2(contentWidth, contentHeight)

        let layer = 1f - (float32 layer / 255f)

        DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables


    member this.DrawText (content: ContentManager) (styleSheet: NoobishStyleSheet) (textBatch: TextBatch) (i: int) =
        let text = this.Text.[i]
        if text.Length > 0 then 
            let themeId = this.ThemeId.[i]
            let layer = this.Layer.[i]

            let layer = (1f - float32 (layer + 1) / 255.0f)

            let fontId = styleSheet.GetFont themeId "default"
            let font = content.Load<NoobishFont> fontId

            let fontSize = (styleSheet.GetFontSize themeId "default")

            let bounds = this.Bounds.[i]
            let margin = this.Margin.[i]
            let padding = this.Padding.[i]
            let contentStartX = bounds.X + padding.Left + margin.Left
            let contentStartY = bounds.Y + padding.Top + margin.Top
            let contentWidth = bounds.Width - padding.Left - padding.Right - margin.Left - margin.Right
            let contentHeight = bounds.Height - padding.Top - padding.Bottom - margin.Left - margin.Right
            let bounds: Internal.NoobishRectangle = {X = contentStartX; Y = contentStartY; Width = contentWidth; Height = contentHeight}
            let textWrap = this.Textwrap.[i]
            let textAlign = this.TextAlign.[i]
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


        let visible = this.Visible.[i]
        if visible then 
            let textureAtlas = content.Load(styleSheet.TextureAtlasId)

            let bounds = this.Bounds.[i]
            let margin = this.Margin.[i]
            let padding = this.Padding.[i]
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

            if this.FocusedElementId = this.Id.[i] then 
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
        if waitForLayout && this.Count > 0 then 

            for i = 0 to this.Count - 1 do 
                let themeId = this.ThemeId.[i]

                let (top, left, bottom, right) = styleSheet.GetMargin themeId "default"
                this.Margin.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                let (top, left, bottom, right) = styleSheet.GetPadding themeId "default"
                this.Padding.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                (* Calculate size for all the components going from topmost to bottom. *)
                toCalculateSize.Enqueue(i, - this.Layer.[i])

                (* Run layout only for root components. *)
                if this.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, this.Layer.[i])

            while toCalculateSize.Count > 0 do 
                let i = toCalculateSize.Dequeue()
                this.CalculateMinSize content styleSheet i 

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.LayoutComponent content styleSheet 0f 0f this.ScreenWidth this.ScreenHeight i

            waitForLayout <- false



        for i = 0 to this.Count - 1 do 
            let layer = this.Layer.[i]
            drawQueue.Enqueue(i, layer)

        let styleSheet = content.Load<Noobish.Styles.NoobishStyleSheet> styleSheetId
        while drawQueue.Count > 0 do 
            let i = drawQueue.Dequeue()
            this.DrawComponent graphics content spriteBatch textBatch styleSheet false i gameTime



    member this.ComponentContains (x: float32) (y: float32) (i: int) = 
        let bounds = this.Bounds.[i]

        x > bounds.X && x < bounds.X + bounds.Width && y > bounds.Y && y < bounds.Y + bounds.Height

    member this.Hover (x: float32) (y: float32) (gameTime: GameTime) (i: int)  =
        if this.ComponentContains x y i then 
            Log.Logger.Information ("Mouse Hover inside component {ComponentId}", i)

            let children = this.Children.[i]
            for j = 0 to children.Count - 1 do 
                this.Hover x y gameTime children.[j].Index

    member this.Click (x: float32) (y: float32) (gameTime: GameTime) (i: int): bool  =
        if this.Visible.[i] && this.Enabled.[i]&& this.ComponentContains x y i then 
            Log.Logger.Information ("Mouse click inside component {ComponentId}", i)

            let children = this.Children.[i]


            let mutable found = false
            let mutable j = 0
            while not found && j < children.Count do 
                let handled = this.Click x y gameTime children.[j].Index
                if handled then 
                    found <- true 
                j <- j + 1
            

            if not found && this.WantsOnClick.[i] then 
                found <- true
                this.OnClick.[i] ({SourceId = this.Id.[i]})
            
            if not found  && this.WantsFocus.[i] then
                found <- true 
                this.FocusedElementId <- this.Id.[i]
                this.Cursor <- this.Text.[i].Length
                this.FocusedTime <- gameTime.TotalGameTime
                this.OnFocus.[i] ({SourceId = this.Id.[i]}) true
            
            found



        else 
            false


    member this.KeyTyped (c: char) = 
        let focusedIndex = this.GetIndex this.FocusedElementId
        if focusedIndex <> -1 && this.WantsKeyTyped.[focusedIndex] then 
            this.OnKeyTyped.[focusedIndex] {SourceId = this.Id.[focusedIndex]} c


    member this.ProcessMouse(gameTime: GameTime) =

        let mouseState =  Microsoft.Xna.Framework.Input.Mouse.GetState()

        let x = float32 mouseState.X
        let y = float32 mouseState.Y
        for i = 0 to this.Count - 1 do 
            if this.ParentId.[i] = UIComponentId.empty then 
                this.Hover x y gameTime i 


        let leftPressed = previousMouseState.LeftButton = ButtonState.Pressed && mouseState.LeftButton = ButtonState.Pressed

        if leftPressed then 
            this.FocusedElementId <- UIComponentId.empty
            for i = 0 to this.Count - 1 do 
                if this.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, -this.Layer.[i])

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.Click x y gameTime i |> ignore

        previousMouseState <- mouseState

    member this.ProcessKeys (gameTime: GameTime) = 
        let keyState = Keyboard.GetState()

        let index = this.GetIndex(this.FocusedElementId)
        if index <> -1 && this.WantsKeyPressed.[index] then 
            if previousKeyState.IsKeyDown(Keys.Left) && keyState.IsKeyUp (Keys.Left) then 
                this.OnKeyPressed.[index] {SourceId = this.Id.[index]} Keys.Left

            if previousKeyState.IsKeyDown(Keys.Right) && keyState.IsKeyUp (Keys.Right) then 
                this.OnKeyPressed.[index] {SourceId = this.Id.[index]} Keys.Right

        previousKeyState <- keyState
    member this.Update (gameTime: GameTime) = 
        this.ProcessMouse(gameTime)
        this.ProcessKeys(gameTime)




    member this.Clear() =
        this.FocusedElementId <- UIComponentId.empty
        for i = 0 to this.Count - 1 do 
            free.Enqueue(i, i)
            this.Id.[i] <- UIComponentId.empty
            this.ThemeId.[i] <- ""

            this.ParentId.[i] <- UIComponentId.empty

            this.Visible.[i] <- true 
            this.Enabled.[i] <- true 

            this.Block.[i] <- false

            this.Text.[i] <- ""
            this.Textwrap.[i] <- false

            this.Bounds.[i] <- {X = 0f; Y = 0f; Width = 0f; Height = 0f}
            this.MinSize.[i] <- {Width = 0f; Height = 0f}
            this.ContentSize.[i] <- {Width = 0f; Height = 0f}
            this.RelativePosition.[i] <- {X = 0f; Y = 0f}
            this.Fill.[i] <- {Horizontal = false; Vertical = false}
            this.Padding.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
            this.Margin.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

            this.Layer.[i] <- -1
            this.Layout.[i] <- Layout.None

            this.GridSpan.[i] <- {Rowspan = 1; Colspan = 1}

            this.WantsOnClick.[i] <- false
            this.OnClick.[i] <- ignore

            this.WantsKeyTyped.[i] <- false 
            this.OnKeyTyped.[i] <- (fun _ _ ->())

            this.WantsKeyPressed.[i] <- false 
            this.OnKeyPressed.[i] <- (fun _ _ ->())

            this.WantsFocus.[i] <- false 
            this.OnFocus.[i] <- (fun _ _ ->())

            this.Children.[i].Clear()
        this.Count <- 0

        waitForLayout <- true


