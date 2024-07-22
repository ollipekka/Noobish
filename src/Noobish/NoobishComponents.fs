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

[<Measure>]
type UIComponentId 

module UIComponentId =
    let create (index: uint16) (id: uint16): int<UIComponentId> = 
        let indexShifted = (int index) <<< 16
        let packed = indexShifted ||| (int id)
        LanguagePrimitives.Int32WithMeasure<UIComponentId> packed

    let index (packed: int<UIComponentId>) = 
        let packed = (int) packed
        int (packed >>> 16)

    let empty = LanguagePrimitives.Int32WithMeasure<UIComponentId> 0xFFFFFFFF



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
type Scroll = {
    Horizontal: bool
    Vertical: bool
}

[<Struct>]
type NoobishSize = {Width: float32; Height: float32}

[<Struct>]
type NoobishPosition = {X: float32; Y: float32}

[<RequireQualifiedAccess>]
type RelativePosition = 
| None
| Func of pcid: (int<UIComponentId> -> int<UIComponentId> -> float32 -> float32 -> NoobishPosition)
| Position of p: NoobishPosition


[<RequireQualifiedAccess>]
type NoobishViewConstraint = 
| None
| Parent
| ParentOfParent



module DrawUI = 
    open Internal


    let createRectangle (x: float32, y:float32, width: float32, height: float32) =
        Rectangle (int (x), int (y), int (width), int (height))


    let toRectangle (r: NoobishRectangle) =
        Rectangle (int (r.X), int (r.Y), int (r.Width), int (r.Height))


    let toRotatedRectangle (r: NoobishRectangle) (phi: float32) = 
        if abs (phi) >= Single.Epsilon then 
            let rotation = Matrix.CreateRotationZ phi
            let halfWidth = r.Width / 2f
            let halfHeight = r.Height / 2f
            let center = Vector2(r.X + halfWidth, r.Y + halfHeight)
            let p1 = Vector2.Transform(Vector2(-halfWidth, -halfHeight), rotation)
            let p2 = Vector2.Transform(Vector2( halfWidth, -halfHeight), rotation)
            let p3 = Vector2.Transform(Vector2( halfWidth,  halfHeight), rotation)
            let p4 = Vector2.Transform(Vector2(-halfWidth,  halfHeight), rotation)

            let minX = min p1.X (min p2.X (min p3.X p4.X))
            let minY = min p1.Y (min p2.Y (min p3.Y p4.Y))
            let maxX = max p1.X (max p2.X (max p3.X p4.X))
            let maxY = max p1.Y (max p2.Y (max p3.Y p4.Y))
            Rectangle (int (round (center.X + minX)), int (round(center.Y + minY)), int (round(maxX - minX)), int (round(maxY - minY)))
        else 
            toRectangle r

    let calculateImageBounds (imageSize: NoobishImageSize) (imageAlign: NoobishAlignment) (bounds: NoobishRectangle) (textureWidth: int) (textureHeight: int) (scrollX: float32) (scrollY: float32) =
        match imageSize with
        | NoobishImageSize.Stretch ->
            createRectangle
                ((bounds.X + scrollX),
                (bounds.Y + scrollY),
                bounds.Width,
                bounds.Height)

        | NoobishImageSize.BestFitMax ->
            let ratio = max (bounds.Width / float32 textureWidth) (bounds.Height / float32 textureHeight)
            let width = ratio * float32 textureWidth
            let height = ratio * float32 textureHeight
            let padLeft = (bounds.Width - width) / 2.0f
            let padTop = (bounds.Height - height) / 2.0f
            createRectangle
                ((bounds.X + scrollX + padLeft),
                (bounds.Y + scrollY + padTop),
                width,
                height)

        | NoobishImageSize.BestFitMin ->
            let ratio = min (bounds.Width / float32 textureWidth) (bounds.Height / float32 textureHeight)
            let width = ratio * float32 textureWidth
            let height = ratio * float32 textureHeight
            let padLeft = (bounds.Width - width) / 2.0f
            let padTop = (bounds.Height - height) / 2.0f
            createRectangle
                ((bounds.X + scrollX + padLeft),
                (bounds.Y + scrollY + padTop),
                width,
                height)

        | NoobishImageSize.Original ->
            createRectangle
                ((bounds.X + scrollX),
                (bounds.Y + scrollY),
                bounds.Width,
                bounds.Height)


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

    let getTextureEfffect (t: NoobishTextureEffect) =
        if t = NoobishTextureEffect.FlipHorizontally then
            SpriteEffects.FlipHorizontally
        else if t = NoobishTextureEffect.FlipVertically then
            SpriteEffects.FlipVertically
        else
            SpriteEffects.None

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
    SourceId: int<UIComponentId>
}

[<RequireQualifiedAccess>]
type Layout =
| LinearHorizontal 
| LinearVertical
| Stack
| Grid of cols: int * rows: int
| Relative of int<UIComponentId>
| None

type NoobishComponents(count) = 
    

    let ignoreClick (_source: int<UIComponentId>) (_p: NoobishPosition) (_gameTime: GameTime) = ()
    let ignorePress (_source: int<UIComponentId>) (_p: NoobishPosition) (_gameTime: GameTime) = ()
    member val Count = 0 with get, set
    member val RunningId = 0 with get, set
    member val Id = Array.create count UIComponentId.empty
    member val ThemeId = Array.create count ""
    member val ParentId = Array.create count UIComponentId.empty
    member val Children = Array.init count (fun _ -> ResizeArray<int<UIComponentId>>())
    member val Visible = Array.create count true 
    member val Enabled = Array.create count true 
    member val Block = Array.create count false
    member val WantsText = Array.create count false 
    member val Text = Array.create count ""
    member val TextAlignOverride = Array.create count false
    member val TextAlign = Array.create count NoobishAlignment.Left
    member val Textwrap = Array.create count false

    member val Image = Array.create count ValueOption<NoobishTextureId>.None
    member val ImageAlign = Array.create count NoobishAlignment.Left

    member val ImageColorOverride = Array.create count false
    member val ImageColor = Array.create count Color.White
    member val ImageSize = Array.create count NoobishImageSize.Stretch
    member val ImageTextureEffect = Array.create count NoobishTextureEffect.None
    member val ImageRotation = Array.create count 0.0f

    member val Layer = Array.create count 0

    member val ConstrainToParentBounds = Array.create count true
    member val Bounds = Array.create<Internal.NoobishRectangle> count {X = 0f; Y = 0f; Width = 0f; Height = 0f}
    member val MinSizeOverride = Array.create count false
    member val MinSize = Array.create count {Width = 0f; Height = 0f}
    member val WidthPercentage = Array.create count 1f 
    member val HeightPercentage = Array.create count 1f

    member val ContentSize = Array.create count {Width = 0f; Height = 0f}
    member val RelativePosition = Array.create count RelativePosition.None

    member val Fill = Array.create<Fill> count ({Horizontal = false; Vertical = false})
    member val PaddingOverride = Array.create count false
    member val Padding = Array.create<NoobishPadding> count {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val MarginOverride = Array.create count false
    member val Margin = Array.create<NoobishMargin> count {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Layout = Array.create count Layout.None
    member val GridSpan = Array.create count ({Rowspan = 1; Colspan = 1})
    member val GridCellAlignment = Array.create count NoobishAlignment.None
    member val GridCells = Array.init count (fun _i -> ResizeArray<bool>(128))

    member val WantsOnPress = Array.create count false
    member val OnPress = Array.create<int<UIComponentId> -> NoobishPosition -> GameTime -> unit> count ignorePress
    member val WantsOnClick = Array.create count false
    member val OnClick = Array.create<int<UIComponentId> -> NoobishPosition -> GameTime -> unit> count ignoreClick
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


                    this.CalculateContentSize content styleSheet viewportWidth viewportHeight (cid |> UIComponentId.index)

                    let childPadding = this.Padding.[cid |> UIComponentId.index]
                    let childMargin = this.Margin.[cid |> UIComponentId.index]
                    let childSize = this.ContentSize.[cid |> UIComponentId.index]
                    width <- width + (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- max height (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                {Width = width; Height = height}
            | Layout.LinearVertical -> 
                let children = this.Children.[i]
                let mutable width = 0f
                let mutable height = 0f
                for j = 0 to children.Count - 1 do 
                    let cid = children.[j]

                    this.CalculateContentSize content styleSheet viewportWidth viewportHeight (cid |> UIComponentId.index)

                    let childPadding = this.Padding.[cid |> UIComponentId.index]
                    let childMargin = this.Margin.[cid |> UIComponentId.index]
                    let childSize = this.ContentSize.[cid |> UIComponentId.index]
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
                    let cellspan = this.GridSpan.[cid |> UIComponentId.index]
                    this.CalculateContentSize content styleSheet (cellWidth * float32 cellspan.Colspan) (cellHeight * float32 cellspan.Rowspan) (cid |> UIComponentId.index)

                    let gridSpan = this.GridSpan.[cid |> UIComponentId.index]
                    let childPadding = this.Padding.[cid |> UIComponentId.index]
                    let childMargin = this.Margin.[cid |> UIComponentId.index]
                    let childSize = this.ContentSize.[cid |> UIComponentId.index]
                    maxColWidth <- max maxColWidth ((childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right) / float32 gridSpan.Colspan)
                    maxRowHeight <- max maxRowHeight ((childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom) / float32 gridSpan.Rowspan)

                {Width = maxColWidth * float32 cols; Height = maxRowHeight * float32 rows}
            | Layout.Relative (rcid) -> 
                let children = this.Children.[i]
                for i = 0 to children.Count - 1 do
                    let cid = children.[i]
                    this.CalculateContentSize content styleSheet viewportWidth viewportHeight (cid |> UIComponentId.index)

                this.MinSize.[i]
            | Layout.Stack  -> 
                let children = this.Children.[i]
                let mutable width = 0f
                let mutable height= 0f
                for i = 0 to children.Count - 1 do
                    let cid = children.[i]
                    this.CalculateContentSize content styleSheet viewportWidth viewportHeight (cid |> UIComponentId.index)
                    let childPadding = this.Padding.[cid |> UIComponentId.index]
                    let childMargin = this.Margin.[cid |> UIComponentId.index]
                    let childSize = this.ContentSize.[cid |> UIComponentId.index]
                    width <- max width (childSize.Width + childPadding.Left + childPadding.Right + childMargin.Left + childMargin.Right)
                    height <- max height (childSize.Height + childPadding.Top + childPadding.Bottom + childMargin.Top + childMargin.Bottom)
                {Width = width; Height = height}
            | Layout.None ->
                let text = this.Text.[i]
                let minSize = this.MinSize.[i]
                let struct(contentWidth, contentHeight) = 
                    if not (String.IsNullOrWhiteSpace text) || this.WantsText.[i] then
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
                {Width = ceil contentWidth; Height = ceil contentHeight}
        this.ContentSize.[i] <- contentSize


    member private this.IsGridLayout(i) = 
        match this.Layout.[i] with 
        | Layout.Grid (_, _) -> true 
        | _ -> false

    member this.LayoutComponent (content: ContentManager) (styleSheet: NoobishStyleSheet) (startX: float32) (startY: float32) (parentWidth: float32) (parentHeight: float32) (i: int) = 
        let fill = this.Fill.[i]
        let scroll = this.Scroll.[i]
        let margin = this.Margin.[i]
        let padding = this.Padding.[i]
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

        let widthPercentage = this.WidthPercentage.[i]
        let heightPercentage = this.HeightPercentage.[i]

        this.Bounds.[i] <- {
            X = startX; 
            Y = startY; 
            Width = viewportWidth * widthPercentage + margin.Left + margin.Right + padding.Left + padding.Right
            Height = viewportHeight * heightPercentage + margin.Top + margin.Bottom + padding.Top + padding.Bottom
        }

        let parentId = this.ParentId.[i]
        if parentId <> UIComponentId.empty && (this.IsGridLayout (parentId |> UIComponentId.index)) then 
            match this.GridCellAlignment.[i] with 
            | NoobishAlignment.None -> ()
            | NoobishAlignment.Left ->
                let childBounds = this.Bounds.[i]
                this.Bounds.[i] <- {childBounds with Y = startY + parentHeight / 2f - childBounds.Height / 2f }
                
            | NoobishAlignment.Center ->
                let childBounds = this.Bounds.[i]
                this.Bounds.[i] <- {
                    childBounds with 
                        X = startX + parentWidth /2f - childBounds.Width / 2f; 
                        Y = startY + parentHeight / 2f - childBounds.Height / 2f }
                
            | _ -> ()


        match this.Layout.[i] with 
        | Layout.LinearHorizontal -> 
            let children = this.Children.[i]
            let childY = viewportStartY
            let mutable childX = viewportStartX

            for j = 0 to children.Count - 1 do 
                let cid = children.[j]

                let remainingHeight = 
                    if scroll.Horizontal then 
                        viewportWidth
                    else 
                        (viewportWidth - (childX - viewportStartX))

                this.LayoutComponent content styleSheet childX childY remainingHeight viewportHeight (cid |> UIComponentId.index)

                let childBounds = this.Bounds.[cid |> UIComponentId.index]
                childX <- childX + childBounds.Width

        | Layout.LinearVertical -> 
            let children = this.Children.[i]
            let mutable childY = viewportStartY
            let childX = viewportStartX
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]

                let remainingHeight = 
                    if scroll.Vertical then 
                        viewportHeight
                    else 
                        (viewportHeight - (childY - viewportStartY))
                this.LayoutComponent content styleSheet childX childY viewportWidth remainingHeight (cid |> UIComponentId.index)
                let childBounds = this.Bounds.[cid |> UIComponentId.index]
                childY <- childY + childBounds.Height

        | Layout.Grid(cols, rows) -> 

            let children = this.Children.[i]
            let colWidth = viewportWidth / float32 cols
            let rowHeight = viewportHeight  / float32 rows
            let mutable col = 0
            let mutable row = 0

            let gridCells = this.GridCells.[i]
            gridCells.Clear()
            for _i = 0 to cols * rows - 1 do gridCells.Add false 

            for i = 0 to children.Count - 1 do
                let cid = children.[i]

                let gridSpan = this.GridSpan.[cid |> UIComponentId.index]
                let childStartX = (viewportStartX + (float32 col) * (colWidth))
                let childStartY = (viewportStartY + (float32 row) * (rowHeight))

                let childWidth = float32 gridSpan.Colspan * colWidth 
                let childHeight =  float32 gridSpan.Rowspan * rowHeight
             

                for r = row to row + gridSpan.Rowspan - 1 do
                    for c = col to col + gridSpan.Colspan - 1 do
                        gridCells.[r * cols  + c] <- true

                while row < rows && gridCells.[row * cols + col] do
                    col <- col + gridSpan.Colspan
                    if (col >= cols) then
                        col <- 0
                        row <- row + gridSpan.Rowspan


                this.LayoutComponent content styleSheet childStartX childStartY childWidth childHeight (cid |> UIComponentId.index)
    
 
        | Layout.Relative (rcid) -> 
            let pcid = this.ParentId.[rcid |> UIComponentId.index]
            let parentBounds = this.Bounds.[pcid |> UIComponentId.index]

            let relativeBounds = this.Bounds.[rcid |> UIComponentId.index]
            
            let children = this.Children.[i]
            for i = 0 to children.Count - 1 do 
                let ccid = children.[i]

                let childStart =
                    match this.RelativePosition.[ccid |> UIComponentId.index] with 
                    | RelativePosition.None -> {X = relativeBounds.X + margin.Left; Y = relativeBounds.Y + margin.Top }
                    | RelativePosition.Position(relativePosition) -> 
                        
                        {X = (relativeBounds.X + relativePosition.X); Y = (relativeBounds.Y + relativePosition.Y)}
                    | RelativePosition.Func(f) ->
                        f rcid ccid (relativeBounds.X + margin.Left) (relativeBounds.Y + margin.Top)
                    
                this.LayoutComponent content styleSheet childStart.X childStart.Y parentBounds.Width parentBounds.Height (ccid |> UIComponentId.index)
        | Layout.Stack -> 
            let children = this.Children.[i]
            let childY = viewportStartY
            let childX = viewportStartX
            for j = 0 to children.Count - 1 do 
                let cid = children.[j]
                this.LayoutComponent content styleSheet childX childY viewportWidth viewportHeight (cid |> UIComponentId.index)
        | Layout.None -> ()
            

    member this.Clear() =

        for i = 0 to this.Count - 1 do 
            this.Id.[i] <- UIComponentId.empty
            this.ThemeId.[i] <- ""

            this.ParentId.[i] <- UIComponentId.empty

            this.Visible.[i] <- true 
            this.Enabled.[i] <- true 
            this.Toggled.[i] <- false 
            this.Hovered.[i] <- false 

            this.Block.[i] <- false

            this.WantsText.[i] <- false
            this.Text.[i] <- ""
            this.Textwrap.[i] <- false
            this.TextAlignOverride.[i] <- false 
            this.TextAlign.[i] <- NoobishAlignment.Left

            this.Image.[i] <- ValueNone
            this.ImageAlign.[i] <- NoobishAlignment.Left
            this.ImageColorOverride.[i] <- false
            this.ImageColor.[i] <- Color.White
            this.ImageSize.[i] <- NoobishImageSize.Stretch
            this.ImageTextureEffect.[i] <- NoobishTextureEffect.None
            this.ImageRotation.[i] <- 0.0f

            this.ConstrainToParentBounds.[i] <- true
            this.Bounds.[i] <- {X = 0f; Y = 0f; Width = 0f; Height = 0f}
            this.WidthPercentage.[i] <- 1f 
            this.HeightPercentage.[i] <- 1f
            this.MinSizeOverride.[i] <- false
            this.MinSize.[i] <- {Width = 0f; Height = 0f}
            this.ContentSize.[i] <- {Width = 0f; Height = 0f}
            this.RelativePosition.[i] <- RelativePosition.None

            this.Fill.[i] <- {Horizontal = false; Vertical = false}
            
            this.PaddingOverride.[i] <- false
            this.Padding.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

            this.MarginOverride.[i] <- false
            this.Margin.[i] <- {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}

            this.Layer.[i] <- -1
            this.Layout.[i] <- Layout.None

            this.GridSpan.[i] <- {Rowspan = 1; Colspan = 1}
            this.GridCellAlignment.[i] <- NoobishAlignment.None
            this.GridCells.[i].Clear()

            this.WantsOnPress.[i] <- false
            this.OnPress.[i] <- ignoreClick

            this.WantsOnClick.[i] <- false
            this.OnClick.[i] <- ignorePress

            this.WantsKeyTyped.[i] <- false 
            this.OnKeyTyped.[i] <- (fun _ _ ->())

            this.WantsKeyPressed.[i] <- false 
            this.OnKeyPressed.[i] <- (fun _ _ ->())

            this.WantsFocus.[i] <- false 
            this.OnFocus.[i] <- (fun _ _ ->())

            this.Scroll.[i] <-  {Horizontal = false; Vertical = false}
            this.ScrollX.[i] <-  0f
            this.ScrollY.[i] <-  0f
            this.LastScrollTime.[i] <- TimeSpan.Zero

            this.LastPressTime.[i] <- TimeSpan.Zero
            this.LastHoverTime.[i] <- TimeSpan.Zero

            this.Children.[i].Clear()
        this.Count <- 0
    