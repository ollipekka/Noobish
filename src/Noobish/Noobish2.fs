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
| Default
| Grid of cols: int * rows: int
| OverlaySource
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

    let drawQueue = PriorityQueue<int, int>()
    let layoutQueue = PriorityQueue<int, int>()

    let sizeQueue = PriorityQueue<int, int>()


    let toLayout = ResizeArray<int>()


    let free = PriorityQueue<int, int>(Array.init maxCount (fun i -> struct(i,i)))



    let ids = Array.create maxCount UIComponentId.empty

    let themeIds = Array.create maxCount ""

    let parentIds = Array.create maxCount UIComponentId.empty

    let childrenIds = Array.init maxCount (fun _ -> ResizeArray())

    let block = Array.create maxCount false


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
        toLayout.Add i

        this.Count <- this.Count + 1
        
        cid

    member this.Window() =
        let cid = this.Create "Panel"
        this.Layout.[cid.Index] <- Layout.Default
        cid
    member this.WindowWithGrid (cols: int) (rows: int) =
        let cid = this.Create "Window"
        this.Layout.[cid.Index] <- Layout.Grid(cols, rows)
        cid



    member this.Header (t: string) = 
        let cid = this.Create "Header"
        this.Text.[cid.Index] <- t
        block.[cid.Index] <- true

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
        block.[cid.Index] <- true
        this.OnClick.[cid.Index] <- onClick
        cid

    member this.Space () = 
        let cid = this.Create "Space"
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Div () = 
        let cid = this.Create "Space"
        block.[cid.Index] <- true

        cid     
    member this.Grid (rows: int) (cols: int) = 
        let cid = this.Create "Space"
        this.Layout.[cid.Index] <- Layout.Grid(rows, cols)
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        
        cid    

    member this.HorizontalRule () = 
        let cid = this.Create "HorizontalRule"
        ids.[cid.Index] <- cid
        block.[cid.Index] <- true
        this.Fill.[cid.Index] <- {Horizontal = true; Vertical = false} 
        cid

    member this.SetMinWidth (minWidth: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.MinSize.[index] <- {this.MinSize.[index] with Width = float32 minWidth}
        cid

    member this.SetPosition (x: int, y: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Bounds.[index] <- {this.Bounds.[index] with X = float32 x; Y = float32 y}
        cid

    member this.WithGrid (cols: int, rows: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Layout.[index] <- Layout.Grid(cols, rows)
        cid

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

    member this.Children (cs: UIComponentId[]) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index > -1 then 
            for i = 0 to cs.Length - 1 do 
                let childId = cs[i]
                parentIds.[childId.Index] <- cid
                this.Layer.[childId.Index] <- this.Layer.[index] + this.Layer.[childId.Index]

            childrenIds.[index].AddRange cs
        cid

    member this.DefaultLayout (i: int) =
        let bounds = this.Bounds.[i]
        let padding = this.Padding.[i]
        let margin = this.Margin.[i]
        let contentX = bounds.X + margin.Left + padding.Left
        let contentY = bounds.Y + margin.Top + padding.Top
        let contentWidth = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom

        let children = childrenIds.[i]

        let mutable childX = 0f
        let mutable childY = 0f
        let mutable j = 0
        let mutable previousHeight = 0.0f
        while j < children.Count do 
            let cid = children.[j]

            let bounds = this.Bounds.[cid.Index]
            let fill = this.Fill.[cid.Index]
            let width = 
                if fill.Horizontal then 
                    contentWidth
                else 
                    bounds.Width 

            if (childX + width) <= contentWidth + 0.1f then 
                let newBounds = {bounds with X = contentX + childX; Y = contentY + childY; Width = width}
                this.Bounds.[cid.Index] <- newBounds
                childX <- childX + width
                previousHeight <- newBounds.Height
                j <- j + 1
            else 
                childX <- 0f
                if previousHeight < Single.Epsilon then failwith "Default layout will not work."
                childY <- childY + previousHeight

            
    member this.GridLayout (cols: int) (rows: int) (i: int) = 
        let bounds = this.Bounds.[i]
        let padding = this.Padding.[i]
        let margin = this.Margin.[i]
        let contentX = bounds.X + margin.Left + padding.Left
        let contentY = bounds.Y + margin.Top + padding.Top
        let contentWidth = bounds.Width - margin.Right - margin.Left - padding.Right - padding.Left
        let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom

        let children = childrenIds.[i]
        let colWidth = contentWidth / float32 cols
        let rowHeight = contentHeight  / float32 rows
        let mutable col = 0
        let mutable row = 0


        let cellUsed = Array2D.create cols rows false

        for i = 0 to children.Count - 1 do
            let cid = children.[i]
            let childBounds = this.Bounds.[cid.Index]
            let childStartX = (contentX + (float32 col) * (colWidth))
            let childStartY = (contentY + (float32 row) * (rowHeight))

            let gridSpan = this.GridSpan.[cid.Index]

            for c = col to col + gridSpan.Colspan - 1 do
                for r = row to row + gridSpan.Rowspan - 1 do
                    cellUsed.[c, r] <- true

            while row < rows && cellUsed.[col, row] do
                col <- col + gridSpan.Colspan
                if (col >= cols) then
                    col <- 0
                    row <- row + gridSpan.Rowspan

            let childFill = this.Fill.[cid.Index]

            let childWidth = 
                if childFill.Horizontal then 
                    float32 gridSpan.Colspan * colWidth 
                else 
                    childBounds.Width


            let childHeight = 
                if childFill.Vertical then 
                    float32 gridSpan.Rowspan * rowHeight 
                else 
                    childBounds.Height

            this.Bounds.[cid.Index] <- {X = childStartX; Y = childStartY; Width = childWidth; Height = childHeight}


        this.OverflowSize.[i] <- {Width = contentWidth; Height = contentHeight}

    member this.LayoutComponent (i: int) = 
        
        match this.Layout.[i] with 
        | Layout.Default -> 
            this.DefaultLayout i
        | Layout.Grid (cols, rows) ->
            this.GridLayout cols rows i
        | Layout.OverlaySource -> ()
        | Layout.Absolute -> ()
        | Layout.None -> ()


    member this.GetParentSize (index: int32) = 
        let pcid = parentIds.[index]
        if pcid.Index > -1 then 
            this.Bounds.[pcid.Index]
        else 
            {X = 0f; Y = 0f; Width = this.ScreenWidth; Height = this.ScreenHeight}

    member this.CalculateComponentSize (content: ContentManager) (styleSheet: NoobishStyleSheet) (i: int) = 

        let parentSize = this.GetParentSize i 

        let fill = this.Fill.[i]
        let text = this.Text.[i]
        let span = this.GridSpan.[i]
        let margin = this.Margin.[i]
        let padding = this.Padding.[i]
        let minSize = this.MinSize.[i]

        let maxWidth = parentSize.Width * float32 span.Colspan
        let mutable minWidth = minSize.Width
        let mutable minHeight = minSize.Height

        if not (String.IsNullOrWhiteSpace text) then
            let themeId = themeIds.[i]
            let paddedWidth = maxWidth - margin.Left - margin.Right - padding.Left - padding.Right

            let fontId = styleSheet.GetFont themeId "default"
            let font = content.Load<NoobishFont> fontId
            let fontSize = int(float32 (styleSheet.GetFontSize themeId "default"))

            let struct (contentWidth, contentHeight) =
                if this.Textwrap.[i] then
                    NoobishFont.measureMultiLine font fontSize paddedWidth text
                else
                    NoobishFont.measureSingleLine font fontSize text

            minWidth <- max minWidth (float32 contentWidth)
            minHeight <- max minHeight (float32 contentHeight)

        let layout = this.Layout.[i]

        match layout with 
        | Layout.Default -> 
            let children = childrenIds.[i]
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                let childBounds = this.Bounds.[cid.Index]
                minWidth <- max minWidth childBounds.Width
                minHeight <- minHeight + childBounds.Height
        | Layout.Grid(cols, rows) -> 

            let children = childrenIds.[i]
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                let childGridSpan = this.GridSpan.[cid.Index]
                let childBounds = this.Bounds.[cid.Index]
                let width = childBounds.Width / float32 childGridSpan.Colspan 
                let height = childBounds.Height / float32 childGridSpan.Rowspan
                minWidth <- max minWidth (width * float32 cols)
                minHeight <- max minHeight (height * float32 rows)
        | _ -> ()

        this.Bounds[i] <- {
            this.Bounds.[i] with 
                Width = 
                    if fill.Horizontal then
                        parentSize.Width * float32 span.Colspan
                    else minWidth + padding.Left + padding.Right + margin.Left + margin.Right
                Height = 
                    if fill.Vertical then
                        parentSize.Height * float32 span.Rowspan
                    else
                        minHeight + padding.Top + padding.Bottom + margin.Top + margin.Bottom
        }


    member this.LayoutComponents (content: ContentManager) (styleSheet: NoobishStyleSheet) =
        if toLayout.Count > 0 then 


            for i = 0 to toLayout.Count - 1 do 
                let themeId = themeIds.[i]
                let (top, left, bottom, right) = styleSheet.GetMargin themeId "default"
                this.Margin.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                let themeId = themeIds.[i]
                let (top, left, bottom, right) = styleSheet.GetPadding themeId "default"
                this.Padding.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}
            
            for i = 0 to toLayout.Count - 1 do 
                sizeQueue.Enqueue(i, -this.Layer.[toLayout.[i]])

            while sizeQueue.Count > 0 do 
                let i = sizeQueue.Dequeue()
                this.CalculateComponentSize content styleSheet i

            for i = 0 to toLayout.Count - 1 do 
                layoutQueue.Enqueue(i, this.Layer.[toLayout.[i]])

            while layoutQueue.Count > 0 do
                let i = layoutQueue.Dequeue()

                this.LayoutComponent i

            toLayout.Clear()

    member this.Update (content: ContentManager) (styleSheetId: string) = 
        let styleSheet = content.Load<Noobish.Styles.NoobishStyleSheet>(styleSheetId)
        this.LayoutComponents (content) (styleSheet)


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
        if this.ScreenWidth - screenWidth > Single.Epsilon || this.ScreenHeight - screenHeight > Single.Epsilon then
            this.ScreenWidth <- screenWidth
            this.ScreenHeight <- screenHeight
            // Relayout
            for i = 0 to this.Count - 1 do 
                toLayout.Add i
        
        for i = 0 to this.Count - 1 do 
            let layer = this.Layer.[i]
            drawQueue.Enqueue(i, layer)

        let styleSheet = content.Load<Noobish.Styles.NoobishStyleSheet> styleSheetId
        while drawQueue.Count > 0 do 
            let i = drawQueue.Dequeue()
            this.DrawComponent graphics content spriteBatch textBatch styleSheet false i gameTime





