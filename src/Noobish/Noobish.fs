namespace Noobish

open System


module Components =
    [<RequireQualifiedAccess>]
    type NoobishKeyId =
    | Escape
    | Enter
    | None


    type ComponentMessage =
    | Show
    | Hide
    | ToggleVisibility
    | SetScrollX of float32
    | SetScrollY of float32
    | SetSliderValue of float32

    type ComponentChangeDispatch = (ComponentMessage -> unit)


    type NoobishSettings = {
        Scale: float32
        Pixel: string
        FontSettings: FontSettings
    }

    type Slider = {
        Min: float32
        Max: float32
        Step: float32
        OnValueChanged: float32 -> unit
        Value: float32
    }
    [<RequireQualifiedAccess>]
    type NoobishTexture =
        | None
        | NinePatch of string
        | Basic of string
        | Atlas of id: string * sx: int * sy: int * sw: int * sh: int

    [<RequireQualifiedAccess>]
    type NoobishTextureEffect =
        | None
        | FlipHorizontally
        | FlipVertically


    type Attribute =
    | Name of string
    | Padding of left: int * right: int * top: int * bottom: int
    | PaddingLeft of int
    | PaddingRight of int
    | PaddingTop of int
    | PaddingBottom of int

    | Margin of left: int * right: int * top: int * bottom: int
    | MarginLeft of int
    | MarginRight of int
    | MarginTop of int
    | MarginBottom of int
    | ZIndex of int
    | Overlay

    | Text of string
    | TextFont of string
    | TextSmall
    | TextLarge
    | TextAlign of NoobishTextAlign
    | TextColor of int
    | TextWrap

    | SliderRange of min:float32 * max:float32
    | SliderValue of float32
    | SliderStep of float32
    | SliderValueChanged of (float32 -> unit)

    | Fill
    | FillHorizontal
    | FillVertical

    | OnClick of (unit -> unit)
    | OnClickInternal of ((string -> ComponentMessage -> unit) -> unit)
    | OnPress of (struct(int*int) -> unit)
    | OnPressInternal of ((string -> ComponentMessage -> unit) -> struct(int*int) -> unit)
    | OnChange of (string -> unit)
    | Toggled of bool
    | Block
    | MinSize of width: int * height: int

    | Height of height: int
    | FgColor of int
    | Visible of bool
    | Enabled of bool
    | DisabledColor of int
    | BorderColor of int
    | BorderSize of int
    | Texture of NoobishTexture
    | TextureEffect of NoobishTextureEffect
    | TextureColor of int
    | TextureColorDisabled of int
    | TextureSize of NoobishTextureSize
    | TextureRotation of int
    | ScrollHorizontal
    | ScrollVertical
    | Scroll
    | ScrollBarColor of int
    | ScrollPinColor of int
    | ScrollBarThickness of int
    | ScrollPinThickness of int
    | Layout of NoobishLayout
    | RowSpan of int
    | ColSpan of int
    | RelativePosition of x: int * y: int
    | KeyboardShortcut of NoobishKeyId

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
    let textCenter = TextAlign (NoobishTextAlign.Center)
    let textWrap = TextWrap
    let textSmall = TextSmall
    let textLarge = TextLarge

    let sliderRange min max = SliderRange(min, max)
    let sliderValue v = SliderValue v
    let sliderStep v = SliderStep v
    let sliderOnValueChanged cb = SliderValueChanged cb

    let texture t = Texture (NoobishTexture.Basic t)
    let ninePatch t = Texture (NoobishTexture.NinePatch t)
    let atlasTexture t sx sy sw sh= Texture(NoobishTexture.Atlas (t, sx, sy, sw, sh))
    let textureColor c = TextureColor c
    let textureColorDisabled c = TextureColorDisabled c
    let textureSize s = TextureSize s
    let textureEffect s = TextureEffect s
    let textureFlipHorizontally = TextureEffect NoobishTextureEffect.FlipHorizontally
    let textureFlipVertically = TextureEffect NoobishTextureEffect.FlipVertically
    let textureBestFitMax = TextureSize NoobishTextureSize.BestFitMax
    let textureBestFitMin = TextureSize NoobishTextureSize.BestFitMin
    let textureRotation t = TextureRotation t
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
    let block = Block
    let onClick action = OnClick(action)
    let onChange action = OnChange(action)
    let toggled value = Toggled (value)

    let fill = Fill
    let fillHorizontal = FillHorizontal
    let fillVertical = FillVertical

    let minSize w h = MinSize(w, h)
    let height h = Height h
    let color c = FgColor (c)
    let enabled v = Enabled(v)

    let visible v = Visible(v)

    let hidden = Visible(false)

    let borderSize v = BorderSize(v)
    let borderColor c = BorderColor(c)
    let scrollVertical = ScrollVertical
    let scrollHorizontal = ScrollHorizontal
    let scrollBoth = Scroll

    let gridLayout cols rows = Layout (NoobishLayout.Grid (cols, rows))
    let stackLayout = Layout (NoobishLayout.Default)
    let colspan s = ColSpan s
    let rowspan s = RowSpan s

    let relativePosition x y = RelativePosition(x, y)

    let keyboardShortcut k = KeyboardShortcut k

    // Components
    let hr attributes = { ThemeId = "HorizontalRule"; Children = []; Attributes = minSize 0 2 :: fillHorizontal :: block :: attributes }
    let label attributes = { ThemeId = "Label"; Children = []; Attributes = attributes }
    let paragraph attributes = { ThemeId = "Paragraph"; Children = []; Attributes = textWrap :: textAlign TopLeft :: attributes }
    let header attributes = { ThemeId = "Header"; Children = []; Attributes = [fillHorizontal; block] @ attributes }
    let button attributes =  { ThemeId = "Button"; Children = []; Attributes = attributes }
    let image attributes = { ThemeId = "Image"; Children = []; Attributes = attributes}

    let option t = {ThemeId = "Button"; Children = []; Attributes = [text t; block] }

    let canvas children attributes = { ThemeId = "Image"; Children = children; Attributes = [] @ attributes}

    let slider attributes = {ThemeId = "Slider"; Children = []; Attributes = (sliderRange 0.0f 100.0f) :: attributes}

    let scroll children attributes =
        { ThemeId = "Scroll"; Children = children; Attributes = [stackLayout; fill; scrollVertical] @ attributes}

    let space attributes = { ThemeId = "Space"; Children = []; Attributes = fill :: attributes}

    let panel children attributes = { ThemeId = "Panel"; Children = children; Attributes = stackLayout :: block :: fill :: attributes}
    let panelWithGrid cols rows children attributes = { ThemeId = "Panel"; Children = children; Attributes = gridLayout cols rows :: block :: fill:: attributes}
    let grid cols rows children attributes = { ThemeId = "Division"; Children = children; Attributes = gridLayout cols rows :: fill :: attributes}
    let div children attributes = { ThemeId = "Division"; Children = children; Attributes = stackLayout :: attributes}
    let window children attributes =
        grid 12 8
            [
                space [colspan 12; rowspan 1]
                space [colspan 3; rowspan 7]
                panel children ([colspan 6; rowspan 6;] @ attributes)
            ]
            [

            ]
    let tree attributes = { ThemeId = "Tree"; Children = []; Attributes = fill::attributes}


    let combobox children attributes =

        let mutable onChange: string -> unit = ignore

        for a in attributes do
            match a with
            | OnChange (onChange') ->
                onChange <- onChange'
            | _ -> ()

        let name = children |> List.fold (fun acc c' ->
            let mutable text = ""
            for a in c'.Attributes do
                match a with
                | Text (text') ->
                    text <- text'
                | _ -> ()
            (sprintf "%s-%s" text acc)) "combobox-"

        let children' = children |> List.map(fun c' ->
            let mutable text = ""
            for a in c'.Attributes do
                match a with
                | Text (text') ->
                    text <- text'
                | _ -> ()

            let onClick = OnClickInternal (
                fun (dispatch) ->
                    dispatch name Hide
                    onChange (text)
                )
            {c' with Attributes = onClick :: c'.Attributes}
        )

        let dropdown = panel children' [ Name(name); hidden; ZIndex(10 * 255); Overlay; Margin(0,0,0,0);]

        {ThemeId = "Button"; Children = [dropdown]; Attributes = OnClickInternal(fun dispatch -> dispatch name ToggleVisibility ) :: attributes}

    let largeWindowWithGrid cols rows children attributes =
        grid 16 9
            [
                space [colspan 16; rowspan 1]
                space [colspan 1; rowspan 7]
                panelWithGrid cols rows children ([colspan 14; rowspan 7;] @ attributes)
            ]
            [

            ]

    let windowWithGrid cols rows children attributes =
        grid 12 8
            [
                space [colspan 12; rowspan 1]
                space [colspan 3; rowspan 7]
                panelWithGrid cols rows children ([colspan 6; rowspan 6;] @ attributes)
            ]
            [

            ]
open Components

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
    Name: string
    mutable Toggled: bool
    mutable Visible: bool
    mutable PressedTime: TimeSpan
    mutable ScrolledTime: TimeSpan

    mutable ScrollX: float32
    mutable ScrollY: float32

    mutable SliderValue: float32

    Version: Guid
    KeyboardShortcut: NoobishKeyId
}


type Texture = {
    Texture: NoobishTexture
    TextureEffect: NoobishTextureEffect
    TextureColor: int
    TextureColorDisabled:int
    TextureSize: NoobishTextureSize
    Rotation: int
}

type LayoutComponent = {
    Id: string
    Path: string
    Name: string
    ThemeId: string
    Enabled: bool
    Visible: bool
    Toggled: bool
    ZIndex: int
    Overlay: bool

    FillVertical: bool
    FillHorizontal: bool

    TextAlignment: NoobishTextAlign
    Text: string[]
    TextFont: string
    TextColor: int
    TextColorDisabled: int
    TextWrap: bool
    Texture: option<Texture>
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

    ScrollBarColor: int
    ScrollPinColor: int
    ScrollBarThickness: float32
    ScrollPinThickness: float32
    ScrollHorizontal: bool
    ScrollVertical: bool
    OverflowWidth: float32
    OverflowHeight: float32

    Slider: option<Slider>

    KeyboardShortcut: NoobishKeyId

    OnClickInternal: unit -> unit
    OnPressInternal: struct(int*int) -> unit
    OnChange: string -> unit

    Layout: NoobishLayout
    ColSpan: int
    RowSpan: int

    Children: LayoutComponent[]
} with
    member l.Hidden with get() = not l.Visible
    member l.PaddedWidth with get() = l.OuterWidth - l.PaddingLeft - l.PaddingRight - l.MarginLeft - l.MarginRight
    member l.PaddedHeight with get() = l.OuterHeight - l.PaddingBottom - l.PaddingTop - l.MarginTop - l.MarginBottom

    member l.Width with get() = l.OuterWidth - l.MarginLeft - l.MarginRight
    member l.Height with get() = l.OuterHeight - l.MarginTop - l.MarginBottom

    member l.RectangleWithPadding =
        {
            X = l.StartX + l.PaddingLeft + l.MarginLeft + l.BorderSize
            Y = l.StartY + l.PaddingTop + l.MarginTop + l.BorderSize
            Width = l.PaddedWidth - l.BorderSize * 2.0f
            Height = l.PaddedHeight - l.BorderSize * 2.0f
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
    let splitLines (measureString: string -> int * int) width (text: string) =

        let sections = text.Split [|'\n'|]

        let resultLines = ResizeArray<string>()

        for section in sections do

            let width = int width

            let words = section.Split [|' ';|]

            let mutable line = ""

            let addLine () =
                resultLines.Add(line.Trim())
                line <- ""

            for word in words do

                let (lineWidth, _lineHeight) = measureString (line + word)

                if (lineWidth > width) then
                    addLine ()

                line <- sprintf "%s%s " line word


            if line.Length > 0 then
                addLine()

        String.Join("\n", resultLines)

    let createLayoutComponentState (name) (keyboardShortcut) (version) visible =
        {
            Name = name
            Visible = visible
            Toggled = false
            PressedTime = TimeSpan.Zero
            ScrolledTime = TimeSpan.Zero

            ScrollX = 0.0f
            ScrollY = 0.0f

            SliderValue = 0.0f

            KeyboardShortcut = keyboardShortcut
            Version = version
        }

    let private createLayoutComponent (theme: Theme) (measureText: string -> string -> int*int) (settings: NoobishSettings) (mutateState: string -> ComponentMessage -> unit) (zIndex: int) (parentPath: string) (parentWidth: float32) (parentHeight: float32) (startX: float32) (startY: float32) (themeId: string) (attributes: list<Attribute>) =

        let scale (v: int) = float32 v * settings.Scale
        let scaleTuple (left, right, top, bottom) =
            (scale left, scale right, scale top, scale bottom)

        let theme = if theme.ComponentThemes.ContainsKey themeId then theme.ComponentThemes.[themeId] else theme.ComponentThemes.["Empty"]
        let mutable name = ""
        let mutable enabled = true
        let mutable visible = true
        let mutable toggled = false
        let mutable zIndex = zIndex
        let mutable overlay = false
        let mutable disabledColor = theme.ColorDisabled
        let mutable textAlign = theme.TextAlignment
        let mutable text = ""
        let mutable textFont = if theme.TextFont <> "" then theme.TextFont else settings.FontSettings.Normal
        let mutable textColor = theme.TextColor
        let mutable textColorDisabled = theme.TextColorDisabled
        let mutable textWrap = false
        let mutable color = theme.Color
        let mutable pressedColor = theme.PressedColor
        let mutable hoverColor = theme.HoverColor
        let mutable paddingLeft, paddingRight, paddingTop, paddingBottom = scaleTuple theme.Padding
        let mutable marginLeft, marginRight, marginTop, marginBottom = scaleTuple theme.Margin

        let mutable fillHorizontal = false
        let mutable fillVertical = false

        let mutable isBlock = false
        let mutable onClick: unit -> unit = ignore
        let mutable onClickInternal: (string -> ComponentMessage -> unit) -> unit = ignore

        let mutable onPress: struct(int*int) -> unit = ignore
        let mutable onPressInternal: (string -> ComponentMessage -> unit) -> (struct(int*int)) -> unit = (fun _ _ -> ())

        let mutable onChange: string -> unit = ignore
        let mutable borderSize = scale theme.BorderSize
        let mutable borderColor = theme.BorderColor
        let mutable borderColorDisabled = theme.BorderColorDisabled

        let mutable texture = NoobishTexture.None
        let mutable textureEffect = NoobishTextureEffect.None
        let mutable textureColor = theme.TextureColor
        let mutable textureColorDisabled = theme.TextColorDisabled
        let mutable textureSize = NoobishTextureSize.BestFitMax
        let mutable textureRotation = 0

        let mutable scrollHorizontal = false
        let mutable scrollVertical = false
        let mutable scrollBarColor = theme.ScrollBarColor
        let mutable scrollPinColor = theme.ScrollPinColor
        let mutable scrollBarThickness = scale theme.ScrollBarThickness
        let mutable scrollPinThickness = scale theme.ScrollPinThickness

        let mutable layout = NoobishLayout.None

        let mutable rowspan = 1
        let mutable colspan = 1

        let mutable minWidth = scale theme.Width
        let mutable minHeight = scale theme.Height

        let mutable relativeX = 0.0f
        let mutable relativeY = 0.0f

        let mutable slider: option<Slider> = None

        let mutable keyboardShortcut = NoobishKeyId.None

        for a in attributes do
            match a with
            | Name v ->
                name <- v
            | Padding (left, right, top, bottom) ->
                paddingLeft <- scale left
                paddingRight <- scale right
                paddingTop <- scale top
                paddingBottom <- scale bottom
            | PaddingLeft left ->
                paddingLeft <- scale left
            | PaddingRight right ->
                paddingRight <- scale right
            | PaddingTop top ->
                paddingTop <- scale top
            | PaddingBottom bottom ->
                paddingBottom <- float32 bottom
            | Margin (left, right, top, bottom) ->
                marginLeft <- float32 left
                marginRight <- float32 right
                marginTop <- float32 top
                marginBottom <- float32 bottom
            | MarginLeft left ->
                marginLeft <- float32 left
            | MarginRight right ->
                marginLeft <- float32 right
            | MarginTop top ->
                marginTop <- float32 top
            | MarginBottom bottom ->
                marginBottom <- scale bottom
            | MinSize (width, height) ->
                minWidth <- scale width
                minHeight <- scale height
            | Height height ->
                minHeight <- scale height
            // Text
            | Text(value) -> text <- value
            | TextFont(value) -> textFont <- value
            | TextAlign (value) -> textAlign <- value
            | TextColor (c) -> textColor <- c
            | TextWrap -> textWrap <- true
            | TextSmall -> textFont <- settings.FontSettings.Small
            | TextLarge -> textFont <- settings.FontSettings.Large
            // Slider
            | SliderRange (min, max) ->
                if slider.IsNone then
                    slider <- Some { Min = 0.0f; Max = 100.0f; Step = 0.1f; Value = 0.0f; OnValueChanged = ignore}

                slider <- slider
                    |> Option.map(fun s ->
                        {s with Min = min; Max = max}
                    )
            | SliderValue (v) ->
                if slider.IsNone then
                    slider <- Some { Min = 0.0f; Max = 100.0f; Step = 0.1f; Value = 0.0f; OnValueChanged = ignore}

                slider <- slider
                    |> Option.map(fun s ->
                        {s with Value = v}
                    )
            | SliderStep (v) ->
                if slider.IsNone then
                    slider <- Some { Min = 0.0f; Max = 100.0f; Step = 0.1f; Value = 0.0f; OnValueChanged = ignore}

                slider <- slider
                    |> Option.map(fun s ->
                        {s with Step = v}
                    )
            | SliderValueChanged (cb) ->
                if slider.IsNone then
                    slider <- Some { Min = 0.0f; Max = 100.0f; Step = 0.1f; Value = 0.0f; OnValueChanged = ignore}

                slider <- slider
                    |> Option.map(fun s ->
                        {s with OnValueChanged = cb}
                    )
            // Border
            | BorderSize(v) -> borderSize <- scale v
            | BorderColor(c) -> borderColor <-c
            | OnClick(v) -> onClick <- v
            | OnClickInternal(v) -> onClickInternal <- v
            | OnPress(v) -> onPress <- v
            | OnPressInternal(v) -> onPressInternal <- v
            | OnChange(v) -> onChange <- v
            | Toggled(value) ->
                toggled <- value
            | ZIndex(value) ->
                zIndex <- value
            | Overlay ->
                overlay <- true
            | Fill ->
                fillHorizontal <- true
                fillVertical <- true
            | FillHorizontal ->
                fillHorizontal <- true
            | FillVertical ->
                fillVertical <- true
            | FgColor (c) -> color <- c
            | Block -> isBlock <- true
            | Enabled (v) -> enabled <- v
            | Visible (v) -> visible <- v
            | DisabledColor(c) -> disabledColor <- c
            | Texture (t) ->
                texture <- t
            | TextureEffect (t) ->
                textureEffect <- t
            | TextureColor (c) -> textureColor <- c
            | TextureColorDisabled (c) -> textureColorDisabled <- c
            | TextureSize (s) ->
                textureSize <- s
            | TextureRotation (t) ->
                textureRotation <- t
            | Scroll ->
                scrollHorizontal <- true
                scrollVertical <- true
            |ScrollHorizontal ->
                scrollHorizontal <- true
            | ScrollVertical ->
                scrollVertical <- true
            | ScrollBarColor c -> scrollBarColor <- c
            | ScrollPinColor c -> scrollPinColor <- c
            | ScrollBarThickness h -> scrollBarThickness <- scale h
            | ScrollPinThickness h -> scrollPinThickness <- scale h
            | Layout (s) ->
                layout <- s
            | ColSpan (cs) -> colspan <- cs
            | RowSpan (rs) -> rowspan <- rs
            | RelativePosition (x, y) ->
                relativeX <- scale x
                relativeY <- scale y
            | KeyboardShortcut k ->
                keyboardShortcut <- k

        let maxWidth = ceil (parentWidth * float32 colspan)

        match slider with
        | Some(_slider') ->
            minWidth <- maxWidth - paddingLeft - paddingRight - marginLeft - marginRight
            let thickness =  max scrollBarThickness scrollPinThickness
            minHeight <- minHeight + thickness
        | None -> ()

        let mutable textLines = ""
        if not (String.IsNullOrWhiteSpace text) then
            let paddedWidth = maxWidth - marginLeft - marginRight - paddingLeft - paddingRight
            textLines <- if textWrap then splitLines (measureText textFont) paddedWidth text else text

            let (contentWidth, contentHeight) = measureText textFont textLines

            let paddedContentWidth = ((float32 contentWidth + paddingLeft + paddingRight + marginLeft + marginRight))
            let paddedContentHeight = ((float32 contentHeight + paddingTop + paddingBottom + marginTop + marginBottom))

            minWidth <- paddedContentWidth
            minHeight <- paddedContentHeight

        let width = (if fillHorizontal then ceil (parentWidth * float32 colspan) else minWidth + paddingLeft + paddingRight + marginLeft + marginRight)
        let height = (if fillVertical then ceil (parentHeight * float32 rowspan) else minHeight + paddingTop + paddingBottom + marginTop + marginBottom)

        let cid = sprintf "%s%A%s%s-%g-%g-%g-%g-%i-%i" text texture themeId name startX startY width height colspan rowspan

        if height < 0.0f then
            raise (InvalidOperationException (sprintf "Buggy behavior detected: height for a component %s is negative." themeId))

        let path = sprintf "%s/%s" parentPath themeId

        {
            Id = cid
            Path = path
            Name = name
            ThemeId = themeId
            Enabled = enabled
            Visible = visible
            Toggled = toggled
            ZIndex = zIndex
            Overlay = overlay

            FillHorizontal = fillHorizontal
            FillVertical = fillVertical

            TextAlignment = textAlign
            Text = textLines.Split '\n'
            TextFont = textFont
            TextColor = textColor
            TextColorDisabled = textColorDisabled
            TextWrap = textWrap

            Slider = slider

            KeyboardShortcut = keyboardShortcut

            Texture =
                if texture <> NoobishTexture.None then
                    Some (
                        {
                            Texture = texture
                            TextureEffect = textureEffect
                            TextureColor = textureColor
                            TextureColorDisabled = textureColorDisabled
                            TextureSize = textureSize
                            Rotation = textureRotation
                        }
                    )
                else
                    None

            BorderSize = borderSize
            BorderColor = borderColor
            BorderColorDisabled = borderColorDisabled

            StartX = startX + relativeX
            StartY = startY + relativeY
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

            OnClickInternal = (fun _ ->
                onClickInternal(mutateState)
                onClick())

            OnPressInternal = (fun mousePos ->
                onPressInternal (mutateState) mousePos
                onPress mousePos )

            OnChange = onChange

            Layout = layout
            ColSpan = colspan
            RowSpan = rowspan

            Color = color
            ColorDisabled = disabledColor

            PressedColor = pressedColor
            HoverColor = hoverColor

            ScrollHorizontal = scrollHorizontal
            ScrollVertical = scrollVertical
            ScrollBarColor = scrollBarColor
            ScrollPinColor = scrollPinColor
            ScrollBarThickness = scrollBarThickness
            ScrollPinThickness = scrollPinThickness
            OverflowWidth = width - marginLeft - marginRight - paddingLeft - paddingRight
            OverflowHeight = height - marginTop - marginBottom - marginLeft - marginRight

            Children = [||]
        }

    let isGrid (attributes: Attribute list) =
        attributes
            |> List.exists(
                function
                | Layout (l) ->
                    match l with
                    | NoobishLayout.Grid(_) -> true
                    | _ -> false
                | _ -> false)

    let rec private layoutComponent
        (measureText: string -> string -> int*int)
        (theme: Theme)
        (settings: NoobishSettings)
        (mutateState: string -> ComponentMessage -> unit)
        (zIndex: int)
        (parentPath: string)
        (startX: float32)
        (startY: float32)
        (parentWidth: float32)
        (parentHeight: float32)
        (c: Component): LayoutComponent  =

        let parentComponent = createLayoutComponent theme measureText settings mutateState zIndex parentPath parentWidth parentHeight startX startY c.ThemeId c.Attributes

        let mutable offsetX = 0.0f
        let mutable offsetY = 0.0f

        let newChildren = ResizeArray<LayoutComponent>()

        let calculateChildHeight () =
            let childHeight = newChildren |> Seq.fold (fun acc c -> acc + c.OuterHeight) 0.0f
            childHeight

        let calculateOverflowHeight () =
            let childHeight = newChildren |> Seq.fold (fun acc c -> acc + c.OverflowHeight) 0.0f
            childHeight

        let calculateChildWidth () =
            newChildren |> Seq.map (fun c -> c.OuterWidth) |> Seq.max

        match parentComponent.Layout with
        | NoobishLayout.Default ->

            let parentBounds = parentComponent.RectangleWithPadding
            for child in c.Children do
                let childStartX = parentBounds.X + offsetX
                let childStartY = parentBounds.Y + offsetY
                let childWidth = if parentComponent.ScrollHorizontal then parentBounds.Width else parentBounds.Width - offsetX
                let childHeight = if parentComponent.ScrollVertical then parentBounds.Height else parentBounds.Height - offsetY
                let childComponent = layoutComponent measureText theme settings mutateState zIndex parentComponent.Path childStartX childStartY childWidth childHeight child

                newChildren.Add(childComponent)

                let childEndX = offsetX + childComponent.OuterWidth
                if childComponent.IsBlock || (childEndX + parentComponent.PaddingLeft + parentComponent.PaddingRight) >= parentBounds.Width then
                    offsetX <- 0.0f
                    offsetY <- offsetY + childComponent.OuterHeight
                else
                    offsetX <- childEndX

            let width = if parentComponent.Overlay then calculateChildWidth() + parentComponent.PaddingLeft + parentComponent.PaddingRight + parentComponent.MarginLeft + parentComponent.MarginRight else parentComponent.OuterWidth
            let height = Utils.clamp (parentComponent.OuterHeight + calculateChildHeight()) 0f parentBounds.Height
            let overflowHeight = calculateOverflowHeight()
            let overflowWidth = if parentComponent.ScrollHorizontal then offsetX else parentComponent.PaddedWidth

            //if newChildren.Count > 0 && parentComponent.Visible && height <= 0.0f then raise(InvalidOperationException "Height can't be zero")
            {parentComponent with
                OuterWidth = width
                OuterHeight = height
                OverflowWidth = overflowWidth
                OverflowHeight = overflowHeight
                Children = newChildren.ToArray()}
        | NoobishLayout.Grid (cols, rows) ->

            let parentBounds = parentComponent.RectangleWithPadding
            let colWidth = parentBounds.Width / float32 cols
            let rowHeight = parentBounds.Height / float32 rows
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
                let childStartX = floor (parentBounds.X + (float32 col) * (colWidth))
                let childStartY = floor (parentBounds.Y + (float32 row) * (rowHeight))

                let childComponent = layoutComponent measureText theme settings mutateState  (zIndex + 1) parentComponent.Path childStartX childStartY colWidth rowHeight child

                newChildren.Add({
                    childComponent with
                        OuterHeight =
                            let height = (rowHeight * float32 childComponent.RowSpan)
                            if childComponent.FillVertical then
                                height
                            else
                                Utils.clamp childComponent.Height 0f height
                        OuterWidth =
                            let width = (colWidth * float32 childComponent.ColSpan)
                            if childComponent.FillHorizontal then
                                width
                            else
                                Utils.clamp childComponent.Width 0f width
                    })

                for c = col to col + childComponent.ColSpan - 1 do
                    for r = row to row + childComponent.RowSpan - 1 do
                        cellUsed.[c, r] <- true

                while notFinished() && cellUsed.[col, row] do
                    bump childComponent.ColSpan childComponent.RowSpan

            //let height =
            //    if parentBounds.Height <= 0.0f then calculateChildHeight() else parentComponent.OuterHeight
            //if height <= 0.0f then raise(InvalidOperationException "Height can't be zero")

            {parentComponent with
                Children = newChildren.ToArray()
                //OuterHeight = height
                OverflowWidth = parentComponent.PaddedWidth
                OverflowHeight = parentComponent.PaddedHeight}
        | NoobishLayout.None ->
            parentComponent


    let layout (measureText: string -> string -> int*int) (theme: Theme) (settings: NoobishSettings) (mutateState: string -> ComponentMessage -> unit) (layer: int) (width: float32) (height: float32) (components: list<Component>) =

        let path = sprintf "layer-%i" layer
        components
            |> List.map(fun c ->
                layoutComponent measureText theme settings mutateState (layer * 128) path 0.0f 0.0f width height c
            ) |> List.toArray


