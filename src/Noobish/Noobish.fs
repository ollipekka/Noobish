namespace Noobish

open System


module Components =

    type NoobishSettings = {
        Scale: float32
        Pixel: string
        DefaultFont: string
        FontPrefix: string
        GraphicsPrefix: string
    }

    [<RequireQualifiedAccess>]
    type NoobishAlignment =
    | Top
    | Bottom
    | Left
    | Right
    | Center

    [<RequireQualifiedAccess>]
    type NoobishFill =
    | Horizontal | Vertical | Both

    [<RequireQualifiedAccess>]
    type NoobishSizeHint =
    | Content
    | Fill of NoobishFill

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

    | Alignment of NoobishAlignment
    | Text of string
    | TextFont of string
    | TextAlign of NoobishTextAlign
    | TextColor of int
    | TextWrap
    | SizeHint of NoobishSizeHint
    | OnClick of (unit -> unit)
    | Toggled of bool
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

    // Attributes
    let name v = Name v
    let text value = Text(value)
    let textFont f = TextFont(f)
    let textColor c = TextColor (c)
    let textAlign v = TextAlign (v)
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
    let top = Alignment(NoobishAlignment.Top)
    let bottom = Alignment(NoobishAlignment.Bottom)
    let center = Alignment(NoobishAlignment.Center)
    let left = Alignment(NoobishAlignment.Left)
    let right = Alignment(NoobishAlignment.Right)
    let block = Block
    let onClick action = OnClick(action)
    let toggled value = Toggled (value)

    let fill = SizeHint (NoobishSizeHint.Fill (NoobishFill.Both))
    let fillHorizontal = SizeHint (NoobishSizeHint.Fill (NoobishFill.Horizontal))
    let fillVertical = SizeHint (NoobishSizeHint.Fill (NoobishFill.Vertical))

    let sizeContent = SizeHint NoobishSizeHint.Content
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
    let paragraph attributes ={ ThemeId = "Paragraph"; Children = []; Attributes = textWrap :: textAlign TopLeft :: sizeContent :: attributes }
    let header attributes = { ThemeId = "Header"; Children = []; Attributes = [fillHorizontal; block] @ attributes }
    let button attributes =  { ThemeId = "Button"; Children = []; Attributes = attributes }
    let image attributes = { ThemeId = "Image"; Children = []; Attributes = attributes}
    let scroll children attributes = { ThemeId = "Scroll"; Children = children; Attributes = fill :: attributes}
    let panel children attributes = { ThemeId = "Panel"; Children = children; Attributes = block :: fill :: attributes}
    let panelWithGrid cols rows children attributes = { ThemeId = "Panel"; Children = children; Attributes = gridLayout cols rows :: block :: fill:: attributes}
    let grid cols rows children attributes = { ThemeId = "Division"; Children = children; Attributes = gridLayout cols rows :: fill :: attributes}
    let div children attributes = { ThemeId = "Division"; Children = children; Attributes = fill :: attributes}
    let space attributes = { ThemeId = "Space"; Children = []; Attributes = fill :: attributes}



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
    Toggled: bool
    TextAlignment: NoobishTextAlign

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



module Logic =
    open Components
    let splitLines (measureString: string -> int * int) width (text: string) =
        let width = int width


        let words = text.Split [|' '|]

        let mutable line = ""

        let mutable newText = ""

        let addLine () =
            printfn "Adding %s" line
            newText <- sprintf "%s%s\n" newText (line.Trim())
            line <- ""


        for word in words do
            let (lineWidth, _lineHeight) = measureString (line + word)

            let lineBreak = word.IndexOf '\n'

            if lineBreak > - 1 then
                let parts = word.Split '\n'
                for part in parts do
                    if part <> "" then
                        line <- sprintf "%s%s " line part
                    else
                        line <- sprintf "%s\n" line
                        addLine ()

                if line.Length > 0 then
                    addLine()
            else
                if (lineWidth > width) then
                    addLine ()

                line <- sprintf "%s%s " line word


        if line.Length > 0 then
            addLine()

        newText

    let createLayoutComponentState () =
        {
            State = ComponentState.Normal
            PressedTime = TimeSpan.Zero
            ScrolledTime = TimeSpan.Zero

            ScrollX = 0.0f
            ScrollY = 0.0f
        }

    let private createLayoutComponent (theme: Theme) (measureText: string -> string -> int*int) (settings: NoobishSettings) (parentWidth: float32) (parentHeight: float32) (startX: float32) (startY: float32) rowspan colspan (themeId: string) (attributes: list<Attribute>) =

        let scale (v: float32) = v * settings.Scale
        let scaleTuple (left, right, top, bottom) =
            (scale (float32 left), scale (float32 right), scale (float32 top), scale (float32 bottom))

        let theme = if theme.ComponentThemes.ContainsKey themeId then theme.ComponentThemes.[themeId] else theme.ComponentThemes.["Empty"]
        let mutable name = ""
        let mutable enabled = true
        let mutable toggled = false
        let mutable disabledColor = theme.ColorDisabled
        let mutable textAlign = theme.TextAlignment
        let mutable text = ""
        let mutable textFont = if theme.TextFont <> "" then sprintf "%s%s" settings.FontPrefix theme.TextFont else ""
        let mutable textColor = theme.TextColor
        let mutable textColorDisabled = theme.TextColorDisabled
        let mutable textWrap = false
        let mutable color = theme.Color
        let mutable pressedColor = theme.PressedColor
        let mutable hoverColor = theme.HoverColor
        let mutable paddingLeft, paddingRight, paddingTop, paddingBottom = scaleTuple theme.Padding
        let mutable marginLeft, marginRight, marginTop, marginBottom = scaleTuple theme.Margin


        let mutable sizeHint = NoobishSizeHint.Content

        let mutable minWidth, minHeight = 0.0f, 0.0f
        let mutable isBlock = false
        let mutable alignment = NoobishAlignment.Left
        let mutable onClick = fun () -> ()
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
            | TextFont(value) -> textFont <- if value <> "" then sprintf "%s%s" settings.FontPrefix value else ""
            | TextAlign (value) -> textAlign <- value
            | TextColor (c) -> textColor <- c
            | TextWrap -> textWrap <- true
            // Border
            | BorderSize(v) -> borderSize <- scale (float32 v)
            | BorderColor(c) -> borderColor <-c
            | OnClick(v) -> onClick <- v
            | Toggled(value) ->
                toggled <- value
            | SizeHint(value) -> sizeHint <- value
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
        let mutable textLines = ""
        if not (String.IsNullOrWhiteSpace text) then
            let paddedWidth = parentWidth - marginLeft - marginRight - paddingLeft - paddingRight
            textLines <- if textWrap then splitLines (measureText textFont) paddedWidth text else text

            let (contentWidth, contentHeight) = measureText textFont textLines

            printfn "contentSize: %f %f %f %f" minWidth (float32 contentWidth) minHeight (float32 contentHeight)
            minWidth <- max minWidth ((float32 contentWidth + paddingLeft + paddingRight + marginLeft + marginRight))
            minHeight <- max minHeight ((float32 contentHeight + paddingTop + paddingBottom + marginTop + marginBottom))


        if not (String.IsNullOrEmpty texture) then

            let contentWidth = if colspan > 0 then parentWidth * float32 colspan else parentWidth
            let contentHeight = if rowspan > 0 then parentHeight * float32 rowspan else parentHeight

            minWidth <- max minWidth contentWidth
            minHeight <- max minHeight contentHeight

        let width, height =
            match sizeHint with
            | NoobishSizeHint.Content ->
                minWidth, minHeight
            | NoobishSizeHint.Fill (f) ->
                match f with
                | NoobishFill.Horizontal ->
                    let width = if colspan > 0 then parentWidth * float32 colspan else parentWidth
                    let height = minHeight
                    width, height
                | NoobishFill.Vertical ->
                    let width = minWidth
                    let height = if rowspan > 0 then parentHeight * float32 rowspan else parentHeight
                    width, height
                | NoobishFill.Both ->
                    let width = if colspan > 0 then parentWidth * float32 colspan else parentWidth
                    let height = if rowspan > 0 then parentHeight * float32 rowspan else parentHeight
                    width, height

        printfn "contentSize 2: %f %f %f %f" minWidth width minHeight height
        match alignment with
        | NoobishAlignment.Center ->
            startPosX <- startPosX + parentWidth / 2.0f - width / 2.0f
            startPosY <- startPosY + parentHeight / 2.0f - height / 2.0f
        | NoobishAlignment.Top -> ()
        | NoobishAlignment.Bottom ->
            startPosY <- startPosY + parentHeight -  height - marginLeft - marginRight
        | NoobishAlignment.Left -> ()
        | NoobishAlignment.Right ->
            let margins = marginRight - marginLeft
            startPosX <- startPosX + parentWidth -  width - margins



        let cid = sprintf "%s%s%s%s-%g-%g-%g-%g-%i-%i" text texture themeId name startPosX startPosY width height colspan rowspan

        printfn "%s %f %f" cid width height
        {
            Id = cid
            Name = name
            ThemeId = themeId
            Enabled = enabled
            Toggled = toggled
            TextAlignment = textAlign
            Text = textLines.Split '\n'
            TextFont = textFont
            TextColor = textColor
            TextColorDisabled = textColorDisabled
            TextWrap = textWrap

            Texture = if texture <> "" then sprintf "%s%s" settings.GraphicsPrefix texture else ""
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
        (settings: NoobishSettings)
        (startX: float32)
        (startY: float32)
        (colspan: int)
        (rowspan: int)
        (parentWidth: float32)
        (parentHeight: float32)
        (c: Component): LayoutComponent  =

        let parentComponent = createLayoutComponent theme measureText settings parentWidth parentHeight startX startY colspan rowspan c.ThemeId c.Attributes
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
                let childComponent = layoutComponent measureText theme settings childStartX childStartY 0 0 childWidth childHeight child
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
                let childComponent = layoutComponent measureText theme settings childStartX childStartY 1 1 childWidth childHeight child
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

    let layout (measureText: string -> string -> int*int) (theme: Theme) (settings: NoobishSettings) (width: float32) (height: float32)  (components: list<Component>) =
        components
            |> List.map(fun c ->
                layoutComponent measureText theme settings 0.0f 0.0f 0 0 width height c
            ) |> List.toArray


