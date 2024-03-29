[<AutoOpen>]
module Noobish.Components


open System

open Microsoft.Xna.Framework

open Noobish
open Noobish.Styles
open Noobish.Internal
open Noobish.Localization


module ZIndex =
    let calculate (v: int) =
        let v = clamp v 0 255
        1f - (float32 v / 255f)

type NoobishAttribute =

    | Name of string
    | Padding of left: int * right: int * top: int * bottom: int
    | PaddingLeft of int
    | PaddingRight of int
    | PaddingTop of int
    | PaddingBottom of int

    | ColorOverride of Color
    | DisabledColorOverride of Color

    | Margin of left: int * right: int * top: int * bottom: int
    | MarginLeft of int
    | MarginRight of int
    | MarginTop of int
    | MarginBottom of int
    | ZIndex of int
    | Overlay

    | Text of string
    | ShowText
    | LocalizedText of bundleId: string * localizationKey: string
    | TextAlign of NoobishTextAlignment
    | TextWrap

    | SliderRange of min:float32 * max:float32
    | SliderValue of float32
    | SliderStep of float32
    | SliderValueChanged of (float32 -> unit)

    | Fill
    | FillHorizontal
    | FillVertical

    | OnClick of (GameTime -> unit)
    | OnClickInternal of (NoobishState -> NoobishLayoutElement -> unit)
    | OnPress of (Vector2 -> GameTime -> unit)
    | OnPressInternal of (NoobishState -> Vector2 -> NoobishLayoutElement -> unit)
    | OnTextChange of (string -> unit)
    | OnCheckboxValueChange of (bool -> unit)

    | DoNotConsumeMouseInput

    | Visible of bool
    | Enabled of bool
    | Toggled of bool
    | Selected of bool
    | Block
    | MinSize of width: int * height: int
    | Texture of NoobishTextureId
    | TextureEffect of NoobishTextureEffect
    | ImageSize of NoobishImageSize
    | TextureRotation of int
    | ScrollHorizontal
    | ScrollVertical
    | Scroll
    | Layout of NoobishLayout
    | RowSpan of int
    | ColSpan of int
    | RelativePosition of x: int * y: int
    | KeyboardShortcut of NoobishKeyboardShortcut
    | OnOpenKeyboard of ((string -> unit) -> unit)
    | KeyTypedEnabled

type NoobishElement = {
    ThemeId: string
    Children: list<NoobishElement>
    Attributes: list<NoobishAttribute>
}


// Attributes
let name n = Name n
let text (t: string) = Text t
let localizedText t = LocalizedText t

let textAlign = TextAlign
let textTopLeft = TextAlign NoobishTextAlignment.TopLeft
let textTopCenter = TextAlign NoobishTextAlignment.TopCenter
let textTopRight = TextAlign NoobishTextAlignment.TopRight
let textLeft = TextAlign NoobishTextAlignment.Left
let textCenter = TextAlign NoobishTextAlignment.Center
let textRight = TextAlign NoobishTextAlignment.Right
let textBottomLeft = TextAlign NoobishTextAlignment.BottomLeft
let textBottomCenter = TextAlign NoobishTextAlignment.BottomCenter
let textBottomRight = TextAlign NoobishTextAlignment.BottomRight
let textWrap = TextWrap

let private colorFromString (c: string): Color = 

    let v =
        if c.StartsWith "#" then
            int(c.Replace("#", "0x"))
        else
            int(c.Insert(0, "0x"))
    let r = (v >>> 24) &&& 255;
    let g = (v >>> 16) &&& 255;
    let b = (v >>> 8) &&& 255;
    let a = v &&& 255;

    (Color(r, g, b, a))

let color (c: string) =
    ColorOverride (colorFromString c)


let disabledColor (c: string) =
    DisabledColorOverride (colorFromString c)

let sliderRange min max = SliderRange(min, max)
let sliderValue v = SliderValue v
let sliderStep v = SliderStep v
let sliderOnValueChanged cb = SliderValueChanged cb

let texture t = Texture (NoobishTextureId.Basic t)
let ninePatch aid tid = Texture (NoobishTextureId.NinePatch(aid,tid))
let atlasTexture aid tid = Texture(NoobishTextureId.Atlas (aid, tid))

let imageSize s = ImageSize s
let textureEffect s = TextureEffect s
let textureFlipHorizontally = TextureEffect NoobishTextureEffect.FlipHorizontally
let textureFlipVertically = TextureEffect NoobishTextureEffect.FlipVertically
let textureBestFitMax = ImageSize NoobishImageSize.BestFitMax
let textureBestFitMin = ImageSize NoobishImageSize.BestFitMin
let textureStretch = ImageSize NoobishImageSize.Stretch
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
let onTextChange action = OnTextChange(action)
let onCheckBoxValueChange action = OnCheckboxValueChange(action)
let onPress action = OnPress(action)
let toggled value = Toggled (value)
let selected value = Selected (value)

let fill = Fill
let fillHorizontal = FillHorizontal
let fillVertical = FillVertical

let minSize w h = MinSize(w, h)
let enabled v = Enabled(v)

let visible v = Visible(v)

let hidden = Visible(false)

let scrollVertical = ScrollVertical
let scrollHorizontal = ScrollHorizontal
let scrollBoth = Scroll

let absoluteLayout = Layout (NoobishLayout.Absolute)
let gridLayout cols rows = Layout (NoobishLayout.Grid (cols, rows))
let stackLayout = Layout (NoobishLayout.Default)
let colspan s = ColSpan s
let rowspan s = RowSpan s

let relativePosition x y = RelativePosition(x, y)

let ctrl (nk: NoobishKeyId) = NoobishKeyboardShortcut.CtrlKeyPressed nk
let alt (nk: NoobishKeyId) = NoobishKeyboardShortcut.AltKeyPressed nk

let key (nk: NoobishKeyId) = NoobishKeyboardShortcut.KeyPressed nk

let keyboardShortcut k = KeyboardShortcut k

let onOpenKeyboard cb = OnOpenKeyboard cb

let private getText (attributes: list<NoobishAttribute>) =
    let mutable text = ""
    let mutable i = 0
    while i < attributes.Length - 1 do
        let a = attributes.[i]
        match a with
        | Text (text') ->
            text <- text'
            i <- attributes.Length
        | _ -> ()
        i <- i + 1
    text

let private isToggled (attributes: list<NoobishAttribute>) =
    let mutable toggled = false
    let mutable i = 0
    while i < attributes.Length - 1 do
        let a = attributes.[i]
        match a with
        | Toggled (t) ->
            toggled <- t
            i <- attributes.Length
        | _ -> ()
        i <- i + 1
    toggled

let themePrefix (prefix: string) (e: NoobishElement) =
    {e with ThemeId = $"%s{prefix}-%s{e.ThemeId}"}

let themeSuffix (suffix: string) (e: NoobishElement) =
    {e with ThemeId = $"%s{e.ThemeId}-%s{suffix}"}

// Components
let hr attributes = { ThemeId = "HorizontalRule"; Children = []; Attributes = minSize 0 2 :: fillHorizontal :: block :: attributes }
let label attributes = { ThemeId = "Label"; Children = []; Attributes = ShowText :: attributes }
let h1 attributes = { ThemeId = "Header1"; Children = []; Attributes = block :: ShowText :: attributes }
let h2 attributes = { ThemeId = "Header2"; Children = []; Attributes = block :: ShowText :: attributes }
let h3 attributes = { ThemeId = "Header3"; Children = []; Attributes = block :: ShowText :: attributes }
let textbox attributes = { ThemeId = "TextBox"; Children = []; Attributes = textAlign NoobishTextAlignment.TopLeft :: ShowText :: KeyTypedEnabled :: attributes }
let paragraph attributes = { ThemeId = "Paragraph"; Children = []; Attributes = textWrap :: textAlign NoobishTextAlignment.TopLeft :: ShowText :: attributes }
let header attributes = { ThemeId = "Header"; Children = []; Attributes = [fillHorizontal; block; ShowText] @ attributes }
let button attributes =  { ThemeId = "Button"; Children = []; Attributes = attributes }
let image attributes = { ThemeId = "Image"; Children = []; Attributes = attributes }
let option t = {ThemeId = "Button"; Children = []; Attributes = [text t; ShowText; block] }
let canvas children attributes = { ThemeId = "Canvas"; Children = children; Attributes = Layout(NoobishLayout.Absolute) :: attributes }
let scroll children attributes =
    { ThemeId = "Scroll"; Children = children; Attributes = [stackLayout; fill; scrollVertical; ] @ attributes}

let space attributes = { ThemeId = "Space"; Children = []; Attributes = DoNotConsumeMouseInput :: fill :: attributes}

let panel children attributes =
    { ThemeId = "Panel"; Children = children; Attributes = stackLayout :: block :: attributes}
let panelWithGrid cols rows children attributes = { ThemeId = "Panel"; Children = children; Attributes = gridLayout cols rows :: block :: fill :: attributes}
let grid cols rows children attributes = { ThemeId = "Division"; Children = children; Attributes = gridLayout cols rows :: fill :: DoNotConsumeMouseInput :: attributes}
let div children attributes = { ThemeId = "Division"; Children = children; Attributes = DoNotConsumeMouseInput :: stackLayout :: attributes}
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

let slider attributes =
    let handlePress (state: NoobishState) (position: Vector2) (c: NoobishLayoutElement) =

        let cs = state.ElementStateById.[c.Id]

        cs.Model
            |> Option.map(
                fun m ->
                    match m with
                    | Slider s' ->
                        let bounds = c.Content
                        let relative = (position.X - bounds.X) / (bounds.Width)
                        let newValue = s'.Min + (relative * s'.Max - s'.Min)
                        let steppedNewValue = truncate(newValue / s'.Step) * s'.Step
                        clamp steppedNewValue s'.Min s'.Max
                    | _ -> failwith "Not a slider")
            |> Option.iter (fun v ->
                state.QueueEvent c.Id (ChangeSliderValue v)
            )
    {ThemeId = "Slider"; Children = []; Attributes = [(OnPressInternal handlePress)] @ attributes}


let combobox children attributes =
    let children' = children |> List.map(fun c' ->
        let onClick = OnClickInternal (
            fun state c ->
                let cp = state.[c.ParentId]
                state.QueueEvent c.ParentId Hide
                state.QueueEvent cp.ParentId (InvokeTextChange c.Text)
            )
        {c' with Attributes = onClick :: c'.Attributes}
    )

    let dropdown = panel children' [ hidden; ZIndex(255); Overlay; Margin(0,0,0,0);]

    let onClickInternal: NoobishAttribute = OnClickInternal(fun state c ->
        for child in c.Children do
            state.QueueEvent child.Id ToggleVisibility
    )

    {ThemeId = "Combobox"; Children = [dropdown]; Attributes = Layout(NoobishLayout.OverlaySource) :: onClickInternal :: attributes}

let checkbox attributes =
    let labelText = getText attributes
    let isChecked = (isToggled attributes)

    let onClickInternal = OnClickInternal(fun state e ->

        state.QueueEvent e.Id Toggle
        state.QueueEvent e.Id (InvokeCheckBoxValueChange)
    )

    let onCheckBoxValueChange =
        attributes
            |> List.tryFind(
                function
                | OnCheckboxValueChange(_) -> true
                | _ -> false)
            |> Option.defaultValue (OnCheckboxValueChange(ignore))

    let check = { ThemeId = "CheckBox"; Children = []; Attributes = [toggled isChecked; onClickInternal; onCheckBoxValueChange] }

    let attributes =
        attributes
            |> List.filter(
                function
                | Text(_) -> false
                | Toggled(_) -> false
                | OnCheckboxValueChange(_) -> false
                | _ -> true
            )
    div
        [
            check; label [fillVertical; text labelText; textAlign NoobishTextAlignment.Left]
        ]
        (fill :: attributes)



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

module Logic =
    open Noobish
    open Noobish.Internal
    open Microsoft.Xna.Framework.Content

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



    let private createNoobishLayoutElement (styleSheet: NoobishStyleSheet) (content: ContentManager) (settings: NoobishSettings) (state: NoobishState) (zIndex: int) (parentId: string) (parentPath: string) (parentWidth: float32) (parentHeight: float32) (startX: float32) (startY: float32) (themeId: string) (attributes: list<NoobishAttribute>) =

        let cid = $"%s{parentPath}/%s{themeId}-(%g{startX},%g{startY})"
        let cstate = "default"

        let toFloat (left, right, top, bottom) =
            (float32 left, float32 right, float32 top, float32 bottom)

        let mutable name = ""
        let mutable elementState = NoobishElementState.Empty
        let mutable zIndex = zIndex
        let mutable overlay = false

        let mutable textAlign = styleSheet.GetTextAlignment themeId cstate
        let mutable text = ""
        let mutable showText = false
        let mutable textWrap = false
        let mutable paddingTop, paddingRight, paddingBottom, paddingLeft = toFloat (styleSheet.GetPadding themeId cstate)
        let mutable marginTop, marginRight, marginBottom, marginLeft = toFloat (styleSheet.GetMargin themeId cstate)

        let mutable fillHorizontal = false
        let mutable fillVertical = false

        let mutable isBlock = false
        let mutable onClick: GameTime -> unit = ignore
        let mutable onClickInternal: NoobishState -> NoobishLayoutElement -> unit = (fun _ _ -> ())
        let mutable consumedButtons = [|NoobishMouseButtonId.Left; NoobishMouseButtonId.Middle; NoobishMouseButtonId.Right|]
        let mutable consumedKeys = ResizeArray<NoobishKeyId>()
        let mutable keyTypedEnabled = false

        let mutable onPress: Vector2 -> GameTime -> unit = (fun _ _ -> ())
        let mutable onPressInternal: NoobishState -> Vector2 -> NoobishLayoutElement -> unit = (fun _ _ _ -> ())

        let mutable onTextChange: string -> unit = ignore
        let mutable onCheckboxValueChange: bool -> unit = ignore

        let mutable color = styleSheet.GetColor cid cstate
        let mutable disabledColor = styleSheet.GetColor cid "disabled"

        let mutable texture = NoobishTextureId.None
        let mutable textureEffect = NoobishTextureEffect.None
        let mutable imageSize = NoobishImageSize.BestFitMax
        let mutable textureRotation = 0

        let mutable scrollHorizontal = false
        let mutable scrollVertical = false

        let mutable layout = NoobishLayout.None

        let mutable rowspan = 1
        let mutable colspan = 1

        let mutable minWidth = 0f
        let mutable minHeight = 0f

        let mutable relativeX = 0.0f
        let mutable relativeY = 0.0f

        let mutable model: option<NoobishComponentModel> = None

        let mutable keyboardShortcut = NoobishKeyboardShortcut.NoShortcut

        for a in attributes do
            match a with
            | Name v ->
                name <- v
            | Padding (top, right, bottom, left) ->
                paddingLeft <- float32 left
                paddingRight <- float32 right
                paddingTop <- float32 top
                paddingBottom <- float32 bottom
            | PaddingLeft left ->
                paddingLeft <- float32 left
            | PaddingRight right ->
                paddingRight <- float32 right
            | PaddingTop top ->
                paddingTop <- float32 top
            | PaddingBottom bottom ->
                paddingBottom <- float32 bottom
            | Margin (top, right, bottom, left) ->
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
                marginBottom <- float32 bottom

            | ColorOverride c ->
                color <- c
            | DisabledColorOverride c ->
                disabledColor <- c
            | MinSize (width, height) ->
                minWidth <- float32 width
                minHeight <- float32 height
            // Text
            | Text(value) ->
                text <- value
            | ShowText -> 
                showText <- true
            | LocalizedText(bundleId, keyId) ->
                let localBundleId = $"{bundleId}-{settings.Locale}"
                let bundle = content.Load<NoobishLocalizationBundle> localBundleId

                text <- bundle.GetLocalizedText keyId


            | TextAlign (value) -> textAlign <- value
            | TextWrap -> textWrap <- true
            // Slider
            | SliderRange (min, max) ->

                if model.IsNone then
                    model <- Some(Slider{ Min = 0.0f; Max = 100.0f; Step = 0.1f; Value = 0.0f; OnValueChanged = ignore})

                model <- model
                    |> Option.map(
                        function
                        | Slider(s) ->
                            Slider ({s with Min = min; Max = max})
                        | _ -> failwith "Model is not a slider."
                    )
            | SliderValue (v) ->
                if model.IsNone then
                    model <- Some (Slider{ Min = 0.0f; Max = 100.0f; Step = 0.1f; Value = 0.0f; OnValueChanged = ignore})

                model <- model
                    |> Option.map(
                        function
                        | Slider(s) ->
                            Slider ({s with Value = v})
                        | _ -> failwith "Model is not a slider."
                    )
            | SliderStep (v) ->
                if model.IsNone then
                    model <- Some (Slider{ Min = 0.0f; Max = 100.0f; Step = 0.1f; Value = 0.0f; OnValueChanged = ignore})

                model <- model
                    |> Option.map(
                        function
                        | Slider(s) ->
                            Slider ({s with Step = v})
                        | _ -> failwith "Model is not a slider."
                    )
            | SliderValueChanged (cb) ->
                if model.IsNone then
                    model <- Some (Slider{ Min = 0.0f; Max = 100.0f; Step = 0.1f; Value = 0.0f; OnValueChanged = ignore})

                model <- model
                    |> Option.map(
                        function
                        | Slider(s) ->
                            Slider ({s with OnValueChanged = cb})
                        | _ -> failwith "Model is not a slider."
                    )
            | OnClick(v) ->
                onClick <- v
            | OnClickInternal(v) ->
                onClickInternal <- v
            | OnPress(v) ->
                onPress <- v
            | OnPressInternal(v) ->
                onPressInternal <- v
            | OnTextChange(v) ->
                onTextChange <- v
            | OnCheckboxValueChange(v) ->
                onCheckboxValueChange <- v
            | DoNotConsumeMouseInput ->
                consumedButtons <- [||]
            | Enabled (value) ->
                if (not value) && not (NoobishElementState.isSet elementState NoobishElementState.Disabled) then
                    elementState <- NoobishElementState.disable elementState
            | Visible (value) ->
                if (not value) && not (NoobishElementState.isSet elementState NoobishElementState.Hidden) then
                    elementState <- NoobishElementState.hide elementState
            | Toggled(value) ->
                if value && not (NoobishElementState.isSet elementState NoobishElementState.Toggled) then
                    elementState <- NoobishElementState.toggle elementState
            | Selected(value) ->
                if value && not (NoobishElementState.isSet elementState NoobishElementState.Selected) then
                    elementState <- NoobishElementState.select elementState
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
            | Block -> isBlock <- true
            | Texture (t) ->
                texture <- t
            | TextureEffect (t) ->
                textureEffect <- t
            | ImageSize (s) ->
                imageSize <- s
            | TextureRotation (t) ->
                textureRotation <- t
            | Scroll ->
                scrollHorizontal <- true
                scrollVertical <- true
                // Scroll needs to fill the whole area.
                fillHorizontal <- true
                fillVertical <- true
            | ScrollHorizontal ->
                scrollHorizontal <- true
            | ScrollVertical ->
                scrollVertical <- true
            | Layout (s) ->
                layout <- s
            | ColSpan (cs) ->
                colspan <- cs
                // Fill the whole cell. User has asked the component to fill the area.
                fillHorizontal <- true
                fillVertical <- true
            | RowSpan (rs) ->
                rowspan <- rs
                // Fill the whole cell. User has asked the component to fill the area.
                fillHorizontal <- true
                fillVertical <- true
            | RelativePosition (x, y) ->
                relativeX <- float32 x
                relativeY <- float32 y
            | KeyboardShortcut k ->
                keyboardShortcut <- k

                let keyId =
                    match k with
                    | NoobishKeyboardShortcut.KeyPressed(k) -> k
                    | NoobishKeyboardShortcut.CtrlKeyPressed(k) -> k
                    | NoobishKeyboardShortcut.AltKeyPressed(k) -> k
                    | NoobishKeyboardShortcut.NoShortcut -> failwith "User logic error. Can't have no shortcut here."

                consumedKeys.Add keyId
            | KeyTypedEnabled ->
                keyTypedEnabled <- true
                if model.IsNone then
                    model <- Some(Textbox{ Text = ""; Cursor = 0; OnOpenKeyboard = fun _ -> ()})
            | OnOpenKeyboard (o) ->
                keyTypedEnabled <- true
                if model.IsNone then
                    model <- Some(Textbox{ Text = ""; Cursor = 0; OnOpenKeyboard = fun _ -> ()})

                model <- model |> Option.map(
                    fun model' ->
                        match model' with
                        | Textbox(model'') ->
                            Textbox({model'' with OnOpenKeyboard = o})
                        | _ -> model'
                )



        let maxWidth = parentWidth * float32 colspan

        model |> Option.iter (fun model' ->
            match model' with
            | Slider (_s) ->
                minWidth <- maxWidth - paddingLeft - paddingRight - marginLeft - marginRight
                let pinHeight = styleSheet.GetHeight "SliderPin" "default"
                let barHeight = styleSheet.GetHeight "Slider" "default"
                minHeight <- minHeight + max pinHeight barHeight
            | Combobox (_c) -> ()
            | Textbox (_t) -> ()
        )

        if showText || not (String.IsNullOrWhiteSpace(text)) then
            model <- model |> Option.map(
                fun model' ->
                    match model' with
                    | Textbox(model'') ->
                        Textbox({model'' with Text = text; Cursor = text.Length})
                    | _ -> model'
            )

        if showText || not (String.IsNullOrWhiteSpace text) then
            let paddedWidth = maxWidth - marginLeft - marginRight - paddingLeft - paddingRight

            let fontId = styleSheet.GetFont themeId "default"
            let font = content.Load<NoobishFont> fontId
            let fontSize = int(float32 (styleSheet.GetFontSize themeId "default"))

            let struct (contentWidth, contentHeight) =
                if textWrap then
                    NoobishFont.measureMultiLine font fontSize paddedWidth text
                else
                    NoobishFont.measureSingleLine font fontSize text

            minWidth <- float32 contentWidth
            minHeight <- float32 contentHeight

        let width =
            if fillHorizontal then
                parentWidth * float32 colspan
            else minWidth + paddingLeft + paddingRight + marginLeft + marginRight
        let height =
            if fillVertical then
                parentHeight * float32 rowspan
            else
                minHeight + paddingTop + paddingBottom + marginTop + marginBottom


        let path = sprintf "%s/%s" parentPath themeId
        //let text = if String.IsNullOrWhiteSpace textLines then [||] else textLines.Split '\n'

        elementState, {
            Id = cid
            ParentId = parentId
            Path = path
            ThemeId = themeId
            ZIndex = zIndex
            Overlay = overlay

            FillHorizontal = fillHorizontal
            FillVertical = fillVertical

            TextAlignment = textAlign

            Text = text
            TextWrap = textWrap

            Model = model

            KeyboardShortcut = keyboardShortcut

            Image =
                if texture <> NoobishTextureId.None then
                    Some (
                        {
                            Texture = texture
                            TextureEffect = textureEffect
                            ImageSize = imageSize
                            Rotation = textureRotation
                        }
                    )
                else
                    None

            StartX = startX
            StartY = startY
            RelativeX = relativeX
            RelativeY = relativeY
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

            OnClickInternal = (fun c gameTime ->
                onClickInternal state c
                onClick gameTime)

            OnPressInternal = (fun mousePos c gameTime ->
                onPressInternal state mousePos c
                onPress mousePos gameTime)

            OnTextChange = onTextChange
            OnCheckBoxValueChange = onCheckboxValueChange
            ConsumedMouseButtons = consumedButtons |> Seq.distinct |> Seq.toArray
            ConsumedKeys = consumedKeys |> Seq.distinct |> Seq.toArray

            KeyTypedEnabled = keyTypedEnabled

            Layout = layout
            ColSpan = colspan
            RowSpan = rowspan

            Color = color
            ColorDisabled = disabledColor

            ScrollHorizontal = scrollHorizontal
            ScrollVertical = scrollVertical
            OverflowWidth = width
            OverflowHeight = height

            Children = [||]
        }

    let isGrid (attributes: list<NoobishAttribute>) =
        attributes
            |> List.exists(
                function
                | Layout (l) ->
                    match l with
                    | NoobishLayout.Grid(_) -> true
                    | _ -> false
                | _ -> false)



    let rec private layoutElement
        (content: ContentManager)
        (styleSheet: NoobishStyleSheet)
        (settings: NoobishSettings)
        (state: NoobishState)
        (zIndex: int)
        (parentId: string)
        (parentPath: string)
        (startX: float32)
        (startY: float32)
        (parentWidth: float32)
        (parentHeight: float32)
        (c: NoobishElement): NoobishLayoutElement  =

        let parentComponentState, parentComponent = createNoobishLayoutElement styleSheet content settings state zIndex parentId parentPath parentWidth parentHeight startX startY c.ThemeId c.Attributes


        let mutable offsetX = 0.0f
        let mutable offsetY = 0.0f

        let newChildren = ResizeArray<NoobishLayoutElement>()

        let calculateChildHeight () =
            let childHeight = newChildren |> Seq.fold (fun acc c -> acc + c.OuterHeight) 0.0f
            childHeight

        let calculateChildWidth () =
            if Seq.isEmpty newChildren then
                0.0f
            else
                newChildren |> Seq.map (fun c -> c.OuterWidth) |> Seq.max

        let parentComponent =
            match parentComponent.Layout with
            | NoobishLayout.Default ->

                // In this layout, actual parent component height might be zero at this point. We don't know. Use available height.
                let availableWidth  = (parentWidth * float32 parentComponent.ColSpan) - parentComponent.MarginHorizontal - parentComponent.PaddingHorizontal
                let availableHeight = (parentHeight * float32 parentComponent.RowSpan) - parentComponent.MarginVertical - parentComponent.PaddingVertical

                let parentBounds = parentComponent.Content
                for i = 0 to c.Children.Length - 1 do
                    let child = c.Children.[i]
                    let childStartX = parentBounds.X + offsetX
                    let childStartY = parentBounds.Y + offsetY
                    let childWidth = if parentComponent.ScrollHorizontal then availableWidth else availableWidth - offsetX
                    let childHeight = if parentComponent.ScrollVertical then availableHeight else availableHeight - offsetY



                    let path = sprintf "%s:%i" parentComponent.Path i
                    let childComponent = layoutElement content styleSheet settings state zIndex parentComponent.Id path childStartX childStartY childWidth childHeight child

                    newChildren.Add(childComponent)

                    let childEndX = (offsetX + childComponent.OuterWidth)
                    if childComponent.IsBlock || (childEndX + parentComponent.PaddingHorizontal) >= parentBounds.Width then
                        offsetX <- 0.0f
                        offsetY <- offsetY + childComponent.OuterHeight
                    else
                        offsetX <- childEndX

                let width =
                    if parentComponent.FillHorizontal then
                        availableWidth + parentComponent.PaddingHorizontal + parentComponent.MarginHorizontal
                    else
                        let childWidth = calculateChildWidth() + parentComponent.PaddingHorizontal + parentComponent.MarginHorizontal
                        clamp childWidth 0f availableWidth

                let overflowHeight = calculateChildHeight()
                let height =
                    if parentComponent.FillVertical then
                        availableHeight + parentComponent.PaddingVertical + parentComponent.MarginVertical
                    else
                        let childHeight = overflowHeight + parentComponent.PaddingVertical + parentComponent.MarginVertical
                        clamp childHeight 0f availableHeight

                let overflowWidth = if parentComponent.ScrollHorizontal then offsetX else parentComponent.ContentWidth

                {parentComponent with
                    OuterWidth = width
                    OuterHeight = height
                    OverflowWidth = overflowWidth
                    OverflowHeight = overflowHeight
                    Children = newChildren.ToArray()}
            | NoobishLayout.Grid (cols, rows) ->

                let parentBounds = parentComponent.Content
                let colWidth = parentBounds.Width / float32 cols
                let rowHeight = parentBounds.Height / float32 rows
                let mutable col = 0
                let mutable row = 0

                let bump colspan rowspan =
                    col <- col + colspan

                    if (col >= cols) then
                        col <- 0
                        row <- row + rowspan


                let cellUsed = Array2D.create cols rows false

                for child in c.Children do
                    let childStartX = (parentBounds.X + (float32 col) * (colWidth))
                    let childStartY = (parentBounds.Y + (float32 row) * (rowHeight))
                    let path = sprintf "%s:grid(%i,%i)" parentComponent.Path col row
                    let childComponent = layoutElement content styleSheet settings state (zIndex + 1) parentComponent.Id path childStartX childStartY colWidth rowHeight child

                    newChildren.Add({
                        childComponent with
                            OuterWidth = (colWidth * float32 childComponent.ColSpan)
                            OuterHeight = (rowHeight * float32 childComponent.RowSpan)
                            })

                    for c = col to col + childComponent.ColSpan - 1 do
                        for r = row to row + childComponent.RowSpan - 1 do
                            cellUsed.[c, r] <- true

                    while row < rows && cellUsed.[col, row] do
                        bump childComponent.ColSpan childComponent.RowSpan

                {parentComponent with
                    Children = newChildren.ToArray()
                    OverflowWidth = parentComponent.ContentWidth
                    OverflowHeight = parentComponent.ContentHeight}
            | NoobishLayout.OverlaySource ->

                if c.Children.Length <> 1 then failwith "Can only pop open one at a time."

                let path = sprintf "%s:overlay" parentComponent.Path
                let childComponent = layoutElement content styleSheet settings state (zIndex + 1) parentComponent.Id path parentComponent.X parentComponent.Y 800f 600f c.Children.[0]

                {parentComponent with Children = [|childComponent|]}
            | NoobishLayout.Absolute ->
                let parentBounds = parentComponent.Content

                for i = 0 to c.Children.Length - 1 do
                    let childStartX = parentBounds.X + parentBounds.Width / 2.0f
                    let childStartY = parentBounds.Y + parentBounds.Height / 2.0f

                    let childWidth = parentBounds.Width
                    let childHeight = parentBounds.Height

                    let path = sprintf "%s:absolute:%i" parentComponent.Path i
                    let childComponent = layoutElement content styleSheet settings state (zIndex + 1) parentComponent.Id path childStartX childStartY childWidth childHeight c.Children.[i]
                    newChildren.Add({ childComponent with StartX = childComponent.StartX; StartY = childComponent.StartY })

                {parentComponent with Children = newChildren.ToArray()}
            | NoobishLayout.None ->
                parentComponent
        state.UpdateState parentComponent parentComponentState
        parentComponent

    let layout (content: ContentManager) (styleSheet: NoobishStyleSheet) (settings: NoobishSettings) (state: NoobishState) (layer: int) (width: float32) (height: float32) (elements: list<NoobishElement>) =

        let path = sprintf "layer-%i" layer
        elements
            |> List.map(fun c ->
                layoutElement content styleSheet settings state (layer * 64) "" path 0.0f 0.0f width height c
            ) |> List.toArray


