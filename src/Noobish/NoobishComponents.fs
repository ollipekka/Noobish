namespace Noobish 

open System
open Serilog

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Noobish
open Noobish.Styles
open Noobish.TextureAtlas


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



module DrawUI = 
    open Internal


    let createRectangle (x: float32, y:float32, width: float32, height: float32) =
        Rectangle (int (x), int (y), int (width), int (height))


    let toRectangle (r: NoobishRectangle) =
        Rectangle (int (r.X), int (r.Y), int (r.Width), int (r.Height))

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
    let drawRectangle (spriteBatch: SpriteBatch) (pixel: Texture2D) (color: Color) (x: float32) (y:float32) (width: float32) (height: float32) =
        let origin = Vector2(0.0f, 0.0f)
        let startPos = Vector2(x, y)
        let scale = Vector2(width / float32 pixel.Width, height / float32 pixel.Height)

        spriteBatch.Draw(
            pixel,
            startPos,
            Nullable(Rectangle(0, 0, pixel.Width, pixel.Height)),
            color,
            0.0f,
            origin,
            scale,
            SpriteEffects.None,
            1.0f)

    let debugDrawBorders (spriteBatch: SpriteBatch) pixel (borderColor: Color) (bounds: NoobishRectangle) =
        let borderSize = 2f
        let widthWithoutBorders = float32 bounds.Width - borderSize

        //Left
        drawRectangle spriteBatch pixel borderColor (bounds.X) bounds.Y borderSize bounds.Height
        // Right
        drawRectangle spriteBatch pixel borderColor (bounds.X + bounds.Width - borderSize) bounds.Y  borderSize bounds.Height
        // Top
        drawRectangle spriteBatch pixel borderColor (bounds.X + borderSize) bounds.Y widthWithoutBorders borderSize
        // Bottom
        drawRectangle spriteBatch pixel borderColor (bounds.X + borderSize) ( bounds.Y + bounds.Height - borderSize) widthWithoutBorders borderSize

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

type NoobishComponents(count) = 

    member val Count = 0 with get, set
    member val Id = Array.create count UIComponentId.empty
    member val ThemeId = Array.create count ""
    member val ParentId = Array.create count UIComponentId.empty
    member val Children = Array.init count (fun _ -> ResizeArray<UIComponentId>())
    member val Visible = Array.create count true 
    member val Enabled = Array.create count true 
    member val Block = Array.create count false
    member val Text = Array.create count ""
    member val TextAlign = Array.create count NoobishTextAlignment.Left
    member val Textwrap = Array.create count false
    member val Layer = Array.create count 0
    member val Bounds = Array.create<Internal.NoobishRectangle> count {X = 0f; Y = 0f; Width = 0f; Height = 0f}
    member val MinSize = Array.create count {Width = 0f; Height = 0f}
    member val ContentSize = Array.create count {Width = 0f; Height = 0f}
    member val RelativePosition = Array.create count {X = 0f; Y = 0f}
    member val Fill = Array.create<Fill> count ({Horizontal = false; Vertical = false})
    member val PaddingOverride = Array.create count false
    member val Padding = Array.create<Padding> count {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val MarginOverride = Array.create count false
    member val Margin = Array.create<Margin> count {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Layout = Array.create count Layout.None
    member val GridSpan = Array.create count ({Rowspan = 1; Colspan = 1})
    member val WantsOnClick = Array.create count false
    member val OnClick = Array.init<OnClickEvent -> unit> count (fun _event -> ignore)
    member val LastPressTime = Array.create count TimeSpan.Zero
    member val LastHoverTime = Array.create count TimeSpan.Zero
    member val WantsKeyTyped = Array.create count false 
    member val OnKeyTyped = Array.create<OnClickEvent -> char -> unit> count (fun _ _ -> ()) 
    member val WantsKeyPressed = Array.create count false 
    member val OnKeyPressed = Array.create<OnClickEvent -> Keys -> unit> count (fun _ _ -> ()) 
    member val WantsFocus = Array.create count false 
    member val OnFocus = Array.create<OnClickEvent -> bool -> unit> count (fun _ _ -> ())
    member val Scroll = Array.create<Scroll> count {Horizontal = false; Vertical = false}
    member val ScrollX = Array.create count 0f
    member val ScrollY = Array.create count 0f
    member val LastScrollTime = Array.create count TimeSpan.Zero
    member val Toggled = Array.create count false 
    member val Hovered = Array.create count false 

    member this.CalculateContentSize (content: ContentManager) (styleSheet: NoobishStyleSheet) (parentWidth: float32) (parentHeight: float32) (i: int) =
        
        let padding = this.Padding.[i]
        let margin = this.Margin.[i]

        let viewportWidth = parentWidth - margin.Left - margin.Right - padding.Left - padding.Right
        let viewportHeight = parentHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom
        
        let contentSize =
            match this.Layout.[i] with 
            | Layout.LinearHorizontal -> 
                let children = this.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]


                    this.CalculateContentSize content styleSheet viewportWidth viewportHeight cid.Index

                    let childPadding = this.Padding.[cid.Index]
                    let childMargin = this.Margin.[cid.Index]
                    let childSize = this.ContentSize.[cid.Index]
                    width <- width + (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- max height (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                {Width = width; Height = height}
            | Layout.LinearVertical -> 
                let children = this.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]

                    this.CalculateContentSize content styleSheet viewportWidth viewportHeight cid.Index

                    let childPadding = this.Padding.[cid.Index]
                    let childMargin = this.Margin.[cid.Index]
                    let childSize = this.ContentSize.[cid.Index]
                    width <- max width (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- height + (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                {Width = width; Height = height}

            | Layout.Grid(cols, rows) -> 
                let children = this.Children.[i]

                let mutable maxColWidth = 0f
                let mutable maxRowHeight = 0f

                let cellWidth = viewportWidth / float32 cols 
                let cellHeight = viewportHeight / float32 rows

                for i = 0 to children.Count - 1 do
                    let cid = children.[i]
                    let cellspan = this.GridSpan.[cid.Index]
                    this.CalculateContentSize content styleSheet (cellWidth * float32 cellspan.Colspan) (cellHeight * float32 cellspan.Rowspan) cid.Index

                    let gridSpan = this.GridSpan.[cid.Index]
                    let childPadding = this.Padding.[cid.Index]
                    let childMargin = this.Margin.[cid.Index]
                    let childSize = this.ContentSize.[cid.Index]
                    maxColWidth <- max maxColWidth ((childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right) / float32 gridSpan.Colspan)
                    maxRowHeight <- max maxRowHeight ((childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom) / float32 gridSpan.Rowspan)

                {Width = maxColWidth * float32 cols; Height = maxRowHeight * float32 rows}
            | Layout.Relative (rcid) -> 
                let pcid = this.ParentId.[rcid.Index]
                
                let children = this.Children.[i]
                for i = 0 to children.Count - 1 do
                    let cid = children.[i]

                    this.CalculateContentSize content styleSheet viewportWidth viewportHeight cid.Index


                {Width = viewportHeight; Height = viewportHeight}
            | Layout.None ->
                let text = this.Text.[i]
                let minSize = this.MinSize.[i]
                let struct(contentWidth, contentHeight) = 
                    if not (String.IsNullOrWhiteSpace text) then
                        let themeId = this.ThemeId.[i]

                        let fontId = styleSheet.GetFont themeId "default"
                        let font = content.Load<NoobishFont> fontId
                        let fontSize = int(float32 (styleSheet.GetFontSize themeId "default"))

                        if this.Textwrap.[i] then
                            NoobishFont.measureMultiLine font fontSize parentWidth text
                        else 
                            NoobishFont.measureSingleLine font fontSize text
                    else 
                        struct(minSize.Width, minSize.Height)
                {Width = contentWidth; Height = contentHeight}
        this.ContentSize.[i] <- contentSize



    member this.LayoutComponent (content: ContentManager) (styleSheet: NoobishStyleSheet) (startX: float32) (startY: float32) (parentWidth: float32) (parentHeight: float32) (i: int) = 

        let fill = this.Fill.[i]
        let scroll = this.Scroll.[i]
        let text = this.Text.[i]
        let margin = this.Margin.[i]
        let padding = this.Padding.[i]
        let minSize = this.MinSize.[i]
        let contentSize = this.ContentSize.[i]


        let maxWidth = parentWidth
        let maxHeight = parentHeight

        let viewportStartX = startX + margin.Left + padding.Left
        let viewportStartY = startY + margin.Top + padding.Top

        let viewportWidth = 
            if fill.Horizontal then 
                maxWidth - margin.Left - margin.Right - padding.Left - padding.Right
            elif scroll.Horizontal && contentSize.Width > maxWidth - margin.Left - margin.Right - padding.Left - padding.Right then
                maxWidth - margin.Left - margin.Right - padding.Left - padding.Right
            else 
                contentSize.Width
        let viewportHeight = 
            if fill.Vertical then 
                maxHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom
            elif scroll.Vertical && contentSize.Height > maxHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom then
                maxHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom
            else 
                contentSize.Height

        this.Bounds.[i] <- {
            X = startX; 
            Y = startY; 
            Width = viewportWidth + margin.Left + margin.Right + padding.Left + padding.Right
            Height = viewportHeight + margin.Top + margin.Bottom + padding.Top + padding.Bottom
        }


        match this.Layout.[i] with 
        | Layout.LinearHorizontal -> 
            let children = this.Children.[i]
            let childY = viewportStartY
            let mutable childX = viewportStartX
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY viewportWidth viewportHeight cid.Index

                let childBounds = this.Bounds.[cid.Index]
                childX <- childX + childBounds.Width

        | Layout.LinearVertical -> 
            let children = this.Children.[i]
            let mutable childY = viewportStartY
            let childX = viewportStartX
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY viewportWidth viewportHeight cid.Index
                let childBounds = this.Bounds.[cid.Index]
                childY <- childY + childBounds.Height

        | Layout.Grid(cols, rows) -> 

            let children = this.Children.[i]
            let colWidth = viewportWidth / float32 cols
            let rowHeight = viewportHeight  / float32 rows
            let mutable col = 0
            let mutable row = 0

            let cellUsed = Array2D.create cols rows false

            for i = 0 to children.Count - 1 do
                let cid = children.[i]
                let childStartX = (viewportStartX + (float32 col) * (colWidth))
                let childStartY = (viewportStartY + (float32 row) * (rowHeight))

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
            
            
            let children = this.Children.[i]
            for i = 0 to children.Count - 1 do 
                let ccid = children.[i]
                let relativePosition = this.RelativePosition.[ccid.Index]
                let childStartX = relativeBounds.X + margin.Left + relativePosition.X 
                let childStartY = relativeBounds.Y + margin.Top + relativePosition.Y 
                this.LayoutComponent content styleSheet childStartX childStartY parentBounds.Width parentBounds.Height ccid.Index

        | Layout.None -> ()
            
