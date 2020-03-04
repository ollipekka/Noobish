namespace Noobish
open System
open System.Collections.Generic
type Alignment =
| Top
| Bottom
| Left
| Right
| Center

type Fill =
| NoFill
| FillParent
| FillHorizontal
| FillVertical


[<RequireQualifiedAccess>]
type NoobishScroll = Vertical | Horizontal | Both

[<RequireQualifiedAccess>]
type NoobishTexture =
    | NinePatch of string
    | Basic of string

type Attribute =
| Name of string
| Padding of left: int * right: int* top: int * bottom: int
| PaddingLeft of int
| PaddingRight of int
| PaddingTop of int
| PaddingBottom of int

| Margin of left: int * right: int* top: int * bottom: int
| MarginLeft of int
| MarginRight of int
| MarginTop of int
| MarginBottom of int

| Alignment of Alignment
| Text of string
| TextFont of string
| TextHorizontalAlign of NoobishHorizontalTextAlign
| TextVerticalAlign of NoobishVerticalTextAlign
| TextColor of int
| TextWrap
| OnClick of (unit -> unit)
| Toggled of bool
| Fill of Fill
| Block
| MinSize of widht: int * height: int
| FgColor of int
| Enabled of bool
| DisabledColor of int
| BorderColor of int
| BorderSize of int
| Texture of NoobishTexture
| TextureColor of int
| TextureColorDisabled of int
| TextureSize of NoobishTextureSize
| Scroll of NoobishScroll
| Layout of NoobishLayout
| RowSpan of int
| ColSpan of int

type Component = {
    ThemeId: string
    Children: list<Component>
    Attributes: list<Attribute>
}

[<RequireQualifiedAccess>]
type ComponentState =
    Normal | Toggled

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

type LayoutComponentState = {
    mutable State: ComponentState
    mutable PressedTime: TimeSpan
    mutable ScrolledTime: TimeSpan

    mutable ScrollX: float32
    mutable ScrollY: float32
}

type LayoutComponent = {
    Id: string
    Name: string
    ThemeId: string
    Enabled: bool
    TextHorizontalAlignment: NoobishHorizontalTextAlign
    TextVerticalAlignment: NoobishVerticalTextAlign

    Text: string[]
    TextFont: string
    TextColor: int
    TextColorDisabled: int
    TextWrap: bool

    Texture: string
    TextureColor: int
    TextureColorDisabled:int
    TextureSize: NoobishTextureSize

    Color: int
    ColorDisabled: int
    PressedColor: int
    HoverColor: int
    StartX: float32
    StartY: float32
    OuterWidth: float32
    OuterHeight: float32
    IsBlock: bool
    PaddingLeft: float32
    PaddingRight: float32
    PaddingTop: float32
    PaddingBottom: float32
    MarginLeft: float32
    MarginRight: float32
    MarginTop: float32
    MarginBottom: float32

    BorderSize: float32
    BorderColor: int
    BorderColorDisabled: int

    ScrollHorizontal: bool
    ScrollVertical: bool
    OverflowWidth: float32
    OverflowHeight: float32

    OnClick: unit -> unit

    Layout: NoobishLayout
    ColSpan: int
    RowSpan: int

    Children: LayoutComponent[]
} with
    member l.PaddedWidth with get() = l.OuterWidth - l.PaddingLeft - l.PaddingRight - l.MarginLeft - l.MarginRight
    member l.PaddedHeight with get() = l.OuterHeight - l.PaddingBottom - l.PaddingTop - l.MarginTop - l.MarginBottom

    member l.Width with get() = l.OuterWidth - l.MarginLeft - l.MarginRight
    member l.Height with get() = l.OuterHeight - l.MarginTop - l.MarginBottom

    member l.RectangleWithPadding =
        {
            X = l.StartX + l.PaddingLeft + l.MarginLeft
            Y = l.StartY + l.PaddingTop + l.MarginTop
            Width = l.PaddedWidth
            Height = l.PaddedHeight
        }

    member l.OuterRectangle with get() =
        {
            X = l.StartX
            Y = l.StartY
            Width = l.OuterWidth
            Height = l.OuterHeight
        }

    member l.RectangleWithMargin with get() =
        {
            X = l.StartX + l.MarginLeft
            Y = l.StartY + l.MarginTop
            Width = l.Width
            Height = l.Height
        }

    member l.Contains x y scrollX scrollY =
        let startX = l.StartX + l.MarginLeft + scrollX
        let endX = startX + l.Width
        let startY = l.StartY + l.MarginTop + scrollY
        let endY = startY + l.Height
        not (x < startX || x > endX || y < startY || y > endY)


module Components =

    // Attributes
    let name v = Name v
    let text value = Text(value)
    let textFont f = TextFont(f)
    let textColor c = TextColor (c)
    let textHorizontalLeft = TextHorizontalAlign(NoobishHorizontalTextAlign.Left)
    let textHorizontalRight = TextHorizontalAlign(NoobishHorizontalTextAlign.Right)
    let textHorizontalCenter = TextHorizontalAlign(NoobishHorizontalTextAlign.Center)
    let textVerticalTop = TextVerticalAlign(NoobishVerticalTextAlign.Top)
    let textVerticalBottom = TextVerticalAlign(NoobishVerticalTextAlign.Bottom)
    let textVerticalCenter = TextVerticalAlign(NoobishVerticalTextAlign.Center)
    let textWrap = TextWrap

    let texture t = Texture (NoobishTexture.Basic t)
    let ninePatch t = Texture (NoobishTexture.NinePatch t)
    let textureColor c = TextureColor c
    let textureSize s = TextureSize s

    let paddingLeft lp = PaddingLeft lp
    let paddingRight rp = PaddingRight rp
    let paddingTop tp = PaddingTop tp
    let paddingBottom bp = PaddingBottom bp
    let padding value = Padding(value, value, value, value)
    let marginLeft lm = MarginLeft lm
    let marginRight rm = MarginRight rm
    let marginTop tm = MarginTop tm
    let marginBottom bm = MarginBottom bm
    let margin value = Margin(value, value, value, value)
    let top = Alignment(Top)
    let bottom = Alignment(Bottom)
    let center = Alignment(Center)
    let left = Alignment(Left)
    let right = Alignment(Right)
    let block = Block
    let onClick action = OnClick(action)
    let toggled value = Toggled (value)
    let fill = Fill(FillParent)
    let fillHorizontal = Fill(FillHorizontal)
    let fillVertical = Fill(FillVertical)
    let minSize w h = MinSize(w, h)
    let color c = FgColor (c)
    let enabled v = Enabled(v)

    let borderSize v = BorderSize(v)
    let borderColor c = BorderColor(c)
    let scrollVertical = Scroll (NoobishScroll.Vertical)
    let scrollHorizontal = Scroll (NoobishScroll.Horizontal)
    let scrollBoth = Scroll (NoobishScroll.Both)

    let gridLayout cols rows = Layout (NoobishLayout.Grid (cols, rows))
    let colspan s = ColSpan s
    let rowspan s = RowSpan s

    // Components
    let hr attributes = { ThemeId = "HorizontalRule"; Children = []; Attributes = minSize 0 2 :: block :: Margin(5, 5, 0, 0) :: attributes }
    let label attributes = { ThemeId = "Label"; Children = []; Attributes = attributes }
    let paragraph attributes ={ ThemeId = "Paragraph"; Children = []; Attributes = textWrap :: textVerticalTop :: textHorizontalLeft :: attributes }
    let header attributes = { ThemeId = "Header"; Children = []; Attributes = [fillHorizontal; block] @ attributes }
    let button attributes =  { ThemeId = "Button"; Children = []; Attributes = attributes }
    let image attributes = { ThemeId = "Image"; Children = []; Attributes = attributes}
    let scroll children attributes = { ThemeId = "Scroll"; Children = children; Attributes = attributes}
    let panel children attributes = { ThemeId = "Panel"; Children = children; Attributes = block :: attributes}
    let panelWithGrid cols rows children attributes = { ThemeId = "Panel"; Children = children; Attributes = gridLayout cols rows :: block :: attributes}
    let grid cols rows children attributes = { ThemeId = "Division"; Children = children; Attributes = gridLayout cols rows :: attributes}
    let div children attributes = { ThemeId = "Division"; Children = children; Attributes = attributes}
    let space attributes = { ThemeId = "Space"; Children = []; Attributes = attributes}


module Logic =
    let splitLines (measureString: string -> int * int) width (text: string) =
        let lines = ResizeArray<string>()
        let mutable leftIndex = 0
        let mutable rightIndex = 0
        let mutable cursor = 0
        let mutable lastIndex = text.Length - 1
        while (rightIndex < lastIndex) do
            let nextSpace = text.IndexOf (' ', cursor)
            rightIndex <- if nextSpace = -1 then lastIndex else nextSpace

            let substr = text.[leftIndex..rightIndex]
            let (textWidth, _textHeight) = measureString substr

            cursor <- rightIndex + 1
            if (float32 textWidth) > width || rightIndex = lastIndex then
                lines.Add(substr)
                leftIndex <- cursor


        lines.ToArray()

    let createLayoutComponentState () =
        {
            State = ComponentState.Normal
            PressedTime = TimeSpan.Zero
            ScrolledTime = TimeSpan.Zero

            ScrollX = 0.0f
            ScrollY = 0.0f
        }

    let private createLayoutComponent (theme: Theme) (measureText: string -> string -> int*int) (scale:float32) (parentWidth: float32) (parentHeight: float32) (startX: float32) (startY: float32) rowspan colspan (themeId: string) (attributes: list<Attribute>) =

        let scale (v: float32) = v * scale
        let scaleTuple (left, right, top, bottom) =
            (scale (float32 left), scale (float32 right), scale (float32 top), scale (float32 bottom))

        let theme = if theme.ComponentThemes.ContainsKey themeId then theme.ComponentThemes.[themeId] else theme.ComponentThemes.["Empty"]
        let mutable name = ""
        let mutable enabled = true
        let mutable disabledColor = theme.ColorDisabled
        let mutable textVerticalAlign = theme.TextVerticalAlignment
        let mutable textHorizontalAlign = theme.TextHorizontalAlignment
        let mutable text = ""
        let mutable textFont = theme.TextFont
        let mutable textColor = theme.TextColor
        let mutable textColorDisabled = theme.TextColorDisabled
        let mutable textWrap = false
        let mutable color = theme.Color
        let mutable pressedColor = theme.PressedColor
        let mutable hoverColor = theme.HoverColor
        let mutable paddingLeft, paddingRight, paddingTop, paddingBottom = scaleTuple theme.Padding
        let mutable marginLeft, marginRight, marginTop, marginBottom = scaleTuple theme.Margin


        let mutable minWidth, minHeight = 0.0f, 0.0f
        let mutable isBlock = false
        let mutable fill = NoFill
        let mutable alignment = Left
        let mutable onClick = fun () -> ()
        let mutable state = ComponentState.Normal
        let mutable borderSize = scale (float32 theme.BorderSize)
        let mutable borderColor = theme.BorderColor
        let mutable borderColorDisabled = theme.BorderColorDisabled

        let mutable texture = ""
        let mutable textureColor = theme.TextureColor
        let mutable textureColorDisabled = theme.TextColorDisabled
        let mutable textureSize = NoobishTextureSize.BestFitMax

        let mutable scrollHorizontal = false
        let mutable scrollVertical = false

        let mutable layout = NoobishLayout.Default

        let mutable rowspan = rowspan
        let mutable colspan = colspan

        for a in attributes do
            match a with
            | Name v ->
                name <- v
            | Padding (left, right, top, bottom) ->
                paddingLeft <- scale (float32 left)
                paddingRight <- scale (float32 right)
                paddingTop <- scale (float32 top)
                paddingBottom <- scale (float32 bottom)
            | PaddingLeft left ->
                paddingLeft <- scale (float32 left)
            | PaddingRight right ->
                paddingRight <- scale (float32 right)
            | PaddingTop top ->
                paddingTop <- scale (float32 top)
            | PaddingBottom bottom ->
                paddingBottom <- scale (float32 bottom)
            | Margin (left, right, top, bottom) ->
                marginLeft <- scale (float32 left)
                marginRight <- scale (float32 right)
                marginTop <- scale (float32 top)
                marginBottom <- scale (float32 bottom)
            | MarginLeft left ->
                marginLeft <- scale (float32 left)
            | MarginRight right ->
                marginLeft <- scale (float32 right)
            | MarginTop top ->
                marginTop <- scale (float32 top)
            | MarginBottom bottom ->
                marginBottom <- scale (float32 bottom)
            | MinSize (width, height) ->
                minWidth <- scale (float32 width)
                minHeight <- scale (float32 height)
            | Alignment(value) ->
                alignment <- value
            // Text
            | Text(value) -> text <- value
            | TextFont(value) -> textFont <- value
            | TextHorizontalAlign (value) -> textHorizontalAlign <- value
            | TextVerticalAlign (value) -> textVerticalAlign <- value
            | TextColor (c) -> textColor <- c
            | TextWrap -> textWrap <- true
            // Border
            | BorderSize(v) -> borderSize <- scale (float32 v)
            | BorderColor(c) -> borderColor <-c
            | OnClick(v) -> onClick <- v
            | Toggled(value) ->
                if value then
                    state <- ComponentState.Toggled
                else ()
            | Fill(value) -> fill <- value
            | FgColor (c) -> color <- c
            | Block -> isBlock <- true
            | Enabled (v) -> enabled <- v
            | DisabledColor(c) -> disabledColor <- c
            | Texture (t) ->
                match t with
                | NoobishTexture.Basic(t') -> texture <- t'
                | NoobishTexture.NinePatch (_t') -> raise (NotImplementedException("No support for NinePatch yet!"))
            | TextureColor (c) -> textureColor <- c
            | TextureColorDisabled (c) -> textureColorDisabled <- c
            | TextureSize (s) ->
                textureSize <- s
            | Scroll (s) ->
                match s with
                | NoobishScroll.Horizontal ->
                    scrollHorizontal <- true
                | NoobishScroll.Vertical ->
                    scrollVertical <- true
                | NoobishScroll.Both ->
                    scrollHorizontal <- true
                    scrollVertical <- true
            | Layout (s) ->
                layout <- s
            | ColSpan (cs) -> colspan <- cs
            | RowSpan (rs) -> rowspan <- rs

        let mutable startPosX = startX
        let mutable startPosY = startY

        if not (String.IsNullOrWhiteSpace text) then
            let (textWidth, textHeight) = measureText textFont text
            minWidth <- max minWidth ((float32 textWidth + paddingLeft + paddingRight + marginLeft + marginRight))
            minHeight <- max minHeight ((float32 textHeight + paddingTop + paddingBottom + marginTop + marginBottom))

        let maxWidth = parentWidth
        let maxHeight = parentHeight

        match fill with
        | FillParent ->
            minWidth <- parentWidth
            minHeight <- parentHeight
        | FillHorizontal ->
            minWidth <- parentWidth
        | FillVertical ->
            minHeight <- parentHeight
        | NoFill ->()

        let width =
            if colspan > 0 then parentWidth * float32 colspan
            elif minWidth <= Single.Epsilon then maxWidth
            else (min minWidth maxWidth)

        let height =
            if rowspan > 0 then parentHeight * float32 rowspan
            elif minHeight <= Single.Epsilon then maxHeight else (min minHeight maxHeight)

        match alignment with
        | Center ->
            startPosX <- startPosX + parentWidth / 2.0f - width / 2.0f
            startPosY <- startPosY + parentHeight / 2.0f - height / 2.0f
        | Top -> ()
        | Bottom ->
            startPosY <- startPosY + parentHeight -  height - marginLeft - marginRight
        | Left -> ()
        | Right ->
            let margins = marginRight - marginLeft
            startPosX <- startPosX + parentWidth -  width - margins


        let cid = sprintf "%s%s%s%s-%g-%g-%g-%g-%i-%i" text texture themeId name startPosX startPosY width height colspan rowspan
        let paddedWidth = width - marginLeft - marginRight - paddingLeft - paddingRight
        let textLines = if textWrap then splitLines (measureText textFont) paddedWidth text else [|text|]

        printfn "%s %f %f" cid width height
        {
            Id = cid
            Name = name
            ThemeId = themeId
            Enabled = enabled
            TextVerticalAlignment = textVerticalAlign
            TextHorizontalAlignment = textHorizontalAlign
            Text = textLines
            TextFont = textFont
            TextColor = textColor
            TextColorDisabled = textColorDisabled
            TextWrap = textWrap

            Texture = texture
            TextureColor = textureColor
            TextureColorDisabled = textureColorDisabled
            TextureSize = textureSize

            BorderSize = borderSize
            BorderColor = borderColor
            BorderColorDisabled = borderColorDisabled

            StartX = startPosX
            StartY = startPosY
            OuterWidth = width
            OuterHeight = height
            IsBlock = isBlock
            PaddingLeft = paddingLeft
            PaddingRight = paddingRight
            PaddingTop = paddingTop
            PaddingBottom = paddingBottom
            MarginLeft = marginLeft
            MarginRight = marginRight
            MarginTop = marginTop
            MarginBottom = marginBottom

            OnClick = onClick

            Layout = layout
            ColSpan = colspan
            RowSpan = rowspan

            Color = color
            ColorDisabled = disabledColor

            PressedColor = pressedColor
            HoverColor = hoverColor

            ScrollHorizontal = scrollHorizontal
            ScrollVertical = scrollVertical
            OverflowWidth = width - marginLeft - marginRight - paddingLeft - paddingRight
            OverflowHeight = height - marginTop - marginBottom - marginLeft - marginRight

            Children = [||]
        }

    let rec private layoutComponent
        (measureText: string -> string -> int*int)
        (theme: Theme)
        (scale: float32)
        (startX: float32)
        (startY: float32)
        (colspan: int)
        (rowspan: int)
        (parentWidth: float32)
        (parentHeight: float32)
        (c: Component): LayoutComponent  =

        let parentComponent = createLayoutComponent theme measureText scale parentWidth parentHeight startX startY colspan rowspan c.ThemeId c.Attributes
        let mutable offsetX = 0.0f
        let mutable offsetY = 0.0f

        let newChildren = ResizeArray<LayoutComponent>()

        let parentBounds = parentComponent.RectangleWithPadding

        match parentComponent.Layout with
        | NoobishLayout.Default ->
            for child in c.Children do
                let childStartX = parentBounds.X + offsetX
                let childStartY = parentBounds.Y + offsetY
                let childWidth = if parentComponent.ScrollHorizontal then parentBounds.Width else parentBounds.Width - offsetX
                let childHeight = if parentComponent.ScrollVertical then parentBounds.Height else parentBounds.Height - offsetY
                let childComponent = layoutComponent measureText theme scale childStartX childStartY 0 0 childWidth childHeight child
                newChildren.Add(childComponent)


                let childEndX = offsetX + childComponent.OuterWidth
                if childComponent.IsBlock || (childEndX + parentComponent.PaddingLeft + parentComponent.PaddingRight) >= parentBounds.Width then
                    offsetY <- offsetY + childComponent.OuterHeight
                    offsetX <- 0.0f

                else
                    offsetX <- childEndX

            {parentComponent with
                OverflowWidth = if parentComponent.ScrollHorizontal then offsetX else parentComponent.PaddedWidth
                OverflowHeight = if parentComponent.ScrollVertical then offsetY else parentComponent.PaddedHeight
                Children = newChildren.ToArray()}
        | NoobishLayout.Grid (cols, rows) ->
            let colWidth = (float32 parentBounds.Width / float32 cols)
            let rowHeight = (float32 parentBounds.Height / float32 rows)

            let mutable col = 0
            let mutable row = 0

            let bump colspan rowspan =
                col <- col + colspan

                if (col >= cols) then
                    col <- 0
                    row <- row + rowspan

            let notFinished () = row < rows

            let cellUsed = Array2D.create cols rows false

            for child in c.Children do
                let childStartX = parentBounds.X + (float32 col) * colWidth
                let childStartY = parentBounds.Y + (float32 row) * rowHeight
                let childWidth = colWidth
                let childHeight = rowHeight
                let childComponent = layoutComponent measureText theme scale childStartX childStartY 1 1 childWidth childHeight child
                newChildren.Add(childComponent)

                for c = col to col + childComponent.ColSpan - 1 do
                    for r = row to row + childComponent.RowSpan - 1 do
                        cellUsed.[c, r] <- true

                while notFinished() && cellUsed.[col, row] do
                    bump childComponent.ColSpan childComponent.RowSpan
                    printfn "%s %i %i" childComponent.ThemeId col row

            {parentComponent with
                Children = newChildren.ToArray()
                OverflowWidth = parentComponent.PaddedWidth
                OverflowHeight = parentComponent.PaddedHeight}

    let layout (measureText: string -> string -> int*int) (theme: Theme) (scale: float32) (width: float32) (height: float32)  (components: list<Component>) =
        components
            |> List.map(fun c ->
                layoutComponent measureText theme scale 0.0f 0.0f 0 0 width height c
            ) |> List.toArray


