namespace Noobish

open System
open System.Collections.Generic

open Serilog

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics

open Noobish
open Noobish.Styles
open Noobish.TextureAtlas


type OnClickEvent = {
    SourceId: Guid
}

[<RequireQualifiedAccess>]
type Layout =
| LinearHorizontal 
| LinearVertical
| Grid of cols: int * rows: int
| Absolute
| None

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



type Noobish2(maxCount: int) =

    let mutable waitForLayout = true

    let drawQueue = PriorityQueue<int, int>()

    let toLayout = ResizeArray<int>()


    let free = PriorityQueue<int, int>(Array.init maxCount (fun i -> struct(i,i)))



    let ids = Array.create maxCount UIComponentId.empty

    let themeIds = Array.create maxCount ""

    let parentIds = Array.create maxCount UIComponentId.empty

    let childrenIds = Array.init maxCount (fun _ -> ResizeArray())

    member val Block = Array.create maxCount false
    member val Text = Array.create maxCount ""

    member val Textwrap = Array.create maxCount false

    member val Layer = Array.create maxCount 0
    member val Bounds = Array.create<NoobishRectangle> maxCount {X = 0f; Y = 0f; Width = 0f; Height = 0f}
    member val OverflowSize = Array.create maxCount {Width = 0f; Height = 0f}
    member val MinSize = Array.create maxCount {Width = 0f; Height = 0f}

    member val Fill = Array.create<Fill> maxCount ({Horizontal = false; Vertical = false})

    member val Padding = Array.create<Padding> maxCount {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Margin = Array.create<Margin> maxCount {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Layout = Array.create maxCount Layout.None

    member val GridSpan = Array.create maxCount ({Rowspan = 1; Colspan = 1})
    member val OnClick = Array.init<OnClickEvent -> unit> maxCount (fun _event -> ignore)
    member val Scroll = Array.create<Scroll> maxCount {Horizontal = true; Vertical = true}

    member val ScreenWidth: float32 = 0f with get, set 
    member val ScreenHeight: float32 = 0f with get, set

    member val private Count = 0 with get, set

    member this.IsActive(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && ids.[index].Id.Equals cid.Id then 
            index
        else 
            -1
    member this.GetIndex(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && ids.[index].Id.Equals cid.Id then 
            index
        else 
            -1

    member private this.Create(themeId: string): UIComponentId =
        if free.Count = 0 then failwith "Out of free components."

        let i = free.Dequeue() 
        let cid: UIComponentId = {Index = i; Id = Guid.NewGuid()}
        ids.[i] <- cid
        themeIds.[i] <- themeId
        this.Layer.[i] <- 1
        this.Layout.[i] <- Layout.None
        this.GridSpan.[i] <- {Colspan = 1; Rowspan = 1}
        this.MinSize.[i] <- {Width = 0f; Height = 0f}
        
        this.Count <- this.Count + 1

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
        let cid = this.Create "Header"
        this.Text.[cid.Index] <- t
        this.Block.[cid.Index] <- true

        cid

    member this.Label (t: string) = 
        let cid = this.Create "Label"
        this.Text.[cid.Index] <- t

        cid

    member this.Textbox (t: string) = 
        let cid = this.Create "Textbox"
        this.Text.[cid.Index] <- t

        cid

    member this.Button (t: string) (onClick: OnClickEvent -> unit) = 
        let cid = this.Create "Button"
        this.Text.[cid.Index] <- t
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
        ids.[cid.Index] <- cid
        this.Block.[cid.Index] <- true
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = false} 
        cid

    member this.SetSize (width: int, height: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.MinSize.[index] <- {Width = float32 width; Height = float32 height}
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

    member this.BumpLayer (layer: int) (childIds: IReadOnlyList<UIComponentId>) = 
        for i = 0 to childIds.Count - 1 do 
            let cid = childIds.[i]
            let childLayer = this.Layer.[cid.Index] + layer
            this.Layer.[cid.Index] <- childLayer
            this.BumpLayer childLayer childrenIds.[cid.Index]


    member this.Children (cs: UIComponentId[]) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index > -1 then 
            for i = 0 to cs.Length - 1 do 
                let childId = cs[i]
                parentIds.[childId.Index] <- cid
            let childrenIds = childrenIds.[index]
            childrenIds.AddRange cs

            let layer = this.Layer.[index]
            this.BumpLayer layer (childrenIds :> IReadOnlyList<UIComponentId>)
        cid

    member this.LayoutComponent (content: ContentManager) (styleSheet: NoobishStyleSheet) (startX: float32) (startY: float32) (parentWidth: float32) (parentHeight: float32) (i: int) = 

        let fill = this.Fill.[i]
        let text = this.Text.[i]
        let margin = this.Margin.[i]
        let padding = this.Padding.[i]
        let minSize = this.MinSize.[i]

        let maxWidth = parentWidth - margin.Left - margin.Right - padding.Left - padding.Right
        let maxHeight = parentHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom

        let mutable width = if fill.Horizontal then maxWidth else min minSize.Width maxWidth
        let mutable height = if fill.Vertical then maxHeight else min minSize.Height maxHeight

        if not (String.IsNullOrWhiteSpace text) then
            let themeId = themeIds.[i]
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

        let layout = this.Layout.[i]

        match layout with 
        | Layout.LinearHorizontal -> 
            let children = childrenIds.[i]
            let mutable childX = startX + margin.Left + padding.Left
            let childY = startY + margin.Top + padding.Top
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY width height cid.Index

                let childBounds = this.Bounds.[cid.Index]
                childX <- childX + childBounds.Width

        | Layout.LinearVertical -> 
            let children = childrenIds.[i]
            let childX = startX + margin.Left + padding.Left
            let mutable childY = startY + margin.Top + padding.Top
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY width height cid.Index

                let childBounds = this.Bounds.[cid.Index]
                childY <- childY + childBounds.Height

        | Layout.Grid(cols, rows) -> 
            let startX = startX + margin.Left + padding.Left
            let startY = startY + margin.Top + padding.Top

            let children = childrenIds.[i]
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
        | Layout.Absolute -> failwith "Not implemented"
        | Layout.None -> ()


    member this.DrawBackground (styleSheet: NoobishStyleSheet) (textureAtlas: NoobishTextureAtlas) (spriteBatch: SpriteBatch) (gameTime: GameTime) (i: int)=

        
        let cstate = "default"

        let color = Color.White

        let themeId = themeIds.[i]
        let bounds = this.Bounds.[i]
        let margin = this.Margin.[i]
        let layer = this.Layer.[i]
        let contentStartX = bounds.X + margin.Left
        let contentStartY = bounds.Y +  margin.Top 
        let contentWidth = bounds.Width - margin.Left - margin.Right
        let contentHeight = bounds.Height - margin.Left - margin.Right

        let drawables = styleSheet.GetDrawables themeId cstate

        let position = Vector2(contentStartX, contentStartY)
        let size = Vector2(contentWidth, contentHeight)

        let layer = 1f - (float32 layer / 255f)

        Log.Logger.Information("Drawing {ThemId} Background at {Layer}", themeId, layer)
        DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables


    member this.DrawText (content: ContentManager) (styleSheet: NoobishStyleSheet) (textBatch: TextBatch) (i: int) =
        let text = this.Text.[i]
        if text.Length > 0 then 
            let themeId = themeIds.[i]
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
            let textWrap = false
            let textBounds =
                    NoobishFont.calculateBounds font fontSize false bounds 0f 0f NoobishTextAlignment.Left text


            Log.Logger.Information("Drawing {ThemId} Text at {Layer}", themeId, layer)
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
                let themeId = themeIds.[i]
                let (top, left, bottom, right) = styleSheet.GetMargin themeId "default"
                this.Margin.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                let themeId = themeIds.[i]
                let (top, left, bottom, right) = styleSheet.GetPadding themeId "default"
                this.Padding.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

            for i = 0 to this.Count - 1 do 
                if parentIds.[i] = UIComponentId.empty then 
                    this.LayoutComponent content styleSheet 0f 0f this.ScreenWidth this.ScreenHeight i

            waitForLayout <- false



        for i = 0 to this.Count - 1 do 
            let layer = this.Layer.[i]
            drawQueue.Enqueue(i, layer)

        let styleSheet = content.Load<Noobish.Styles.NoobishStyleSheet> styleSheetId
        while drawQueue.Count > 0 do 
            let i = drawQueue.Dequeue()
            this.DrawComponent graphics content spriteBatch textBatch styleSheet false i gameTime


    member this.Clear() =
        for i = 0 to this.Count - 1 do 
            free.Enqueue(i, i)
            ids.[i] <- UIComponentId.empty
            themeIds.[i] <- ""

            parentIds.[i] <- UIComponentId.empty

            this.Block.[i] <- false

            this.Text.[i] <- ""
            this.Textwrap.[i] <- false

            this.Bounds.[i] <- {X = 0f; Y = 0f; Width = 0f; Height = 0f}
            this.OverflowSize.[i] <- {Width = 0f; Height = 0f}
            this.MinSize.[i] <- {Width = 0f; Height = 0f}
            this.Fill.[i] <- {Horizontal = false; Vertical = false}
            this.Padding.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
            this.Margin.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

            this.Layer.[i] <- -1
            this.Layout.[i] <- Layout.None

            this.GridSpan.[i] <- {Rowspan = 1; Colspan = 1}
            this.OnClick.[i] <- ignore

            childrenIds.[i].Clear()
        this.Count <- 0

        waitForLayout <- true


