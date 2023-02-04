[<AutoOpen>]
module Noobish.Components

(*
    This file contains the user-facing interface. User should be able to take
    the library into use with a single import aside from where its set up.
*)

open Noobish
open Noobish.Internal
open Noobish.Styles
open System
open Microsoft.Xna.Framework

[<RequireQualifiedAccess>]
type NoobishTextureId =
    | None
    | NinePatch of atlasId: string * ninePatchId: string
    | Basic of string
    | Atlas of atlasId: string * textureId: string

[<RequireQualifiedAccess>]
type NoobishTextureEffect =
    | None
    | FlipHorizontally
    | FlipVertically

    type NoobishTexture = {
        Texture: NoobishTextureId
        TextureEffect: NoobishTextureEffect
        TextureColor: Color
        TextureColorDisabled: Color
        TextureSize: NoobishTextureSize
        Rotation: int
    }


type NoobishLayoutElement = {
    Id: string
    ParentId: string
    Path: string
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
    TextWrap: bool
    Texture: option<NoobishTexture>
    Color: Color
    ColorDisabled: Color

    StartX: float32
    StartY: float32
    RelativeX: float32
    RelativeY: float32
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

    ScrollHorizontal: bool
    ScrollVertical: bool
    OverflowWidth: float32
    OverflowHeight: float32

    Model: option<NoobishComponentModel>

    KeyboardShortcut: NoobishKeyId

    ConsumedMouseButtons: NoobishMouseButtonId[]
    ConsumedKeys: NoobishKeyId[]
    KeyTypedEnabled: bool
    OnClickInternal: NoobishLayoutElement -> unit
    OnPressInternal: struct(int*int) -> NoobishLayoutElement -> unit
    OnChange: string -> unit

    Layout: NoobishLayout
    ColSpan: int
    RowSpan: int

    Children: NoobishLayoutElement[]
} with
    member l.MarginVertical with get() = l.MarginTop + l.MarginBottom
    member l.MarginHorizontal with get() = l.MarginLeft + l.MarginRight
    member l.PaddingVertical with get() = l.PaddingTop + l.PaddingBottom
    member l.PaddingHorizontal with get() = l.PaddingRight+ l.PaddingLeft

    member l.Hidden with get() = not l.Visible
    member l.ContentStartX with get() = l.X + l.PaddingLeft + l.MarginLeft
    member l.ContentStartY with get() = l.Y + l.PaddingTop + l.MarginTop
    member l.ContentWidth with get() = l.OuterWidth - l.MarginHorizontal - l.PaddingHorizontal
    member l.ContentHeight with get() = l.OuterHeight - l.MarginVertical - l.PaddingVertical
    member l.X with get() = l.StartX + l.RelativeX
    member l.Y with get() = l.StartY + l.RelativeY
    member l.Width with get() = l.OuterWidth - l.MarginHorizontal
    member l.Height with get() = l.OuterHeight - l.MarginVertical

    member l.Content =
        {
            X = l.ContentStartX
            Y = l.ContentStartY
            Width = l.ContentWidth
            Height = l.ContentHeight
        }

    member l.ContentWithPadding with get() =
        {
            X = l.X + l.MarginLeft
            Y = l.Y + l.MarginTop
            Width = l.Width
            Height = l.Height
        }

    member l.ContentWithMargin with get() =
        {
            X = l.X
            Y = l.Y
            Width = l.OuterWidth
            Height = l.OuterHeight
        }

    member l.Contains x y scrollX scrollY =
        let startX = l.X + l.MarginLeft + scrollX
        let endX = startX + l.Width
        let startY = l.Y + l.MarginTop + scrollY
        let endY = startY + l.Height
        not (x < startX || x > endX || y < startY || y > endY)



type NoobishAttribute =

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
    | TextAlign of NoobishTextAlign
    | TextWrap

    | SliderRange of min:float32 * max:float32
    | SliderValue of float32
    | SliderStep of float32
    | SliderValueChanged of (float32 -> unit)

    | Fill
    | FillHorizontal
    | FillVertical

    | OnClick of (unit -> unit)
    | OnClickInternal of ((string -> ComponentMessage -> unit) -> NoobishLayoutElement -> unit)
    | OnPress of (struct(int*int) -> unit)
    | OnPressInternal of ((string -> ComponentMessage -> unit) -> struct(int*int) -> NoobishLayoutElement -> unit)
    | OnChange of (string -> unit)
    | Toggled of bool
    | Block
    | MinSize of width: int * height: int
    | Visible of bool
    | Enabled of bool
    | Texture of NoobishTextureId
    | TextureEffect of NoobishTextureEffect
    | TextureSize of NoobishTextureSize
    | TextureRotation of int
    | ScrollHorizontal
    | ScrollVertical
    | Scroll
    | Layout of NoobishLayout
    | RowSpan of int
    | ColSpan of int
    | RelativePosition of x: int * y: int
    | KeyboardShortcut of NoobishKeyId
    | OnOpenKeyboard of ((string -> unit) -> unit)
    | KeyTypedEnabled

type NoobishElement = {
    ThemeId: string
    Children: list<NoobishElement>
    Attributes: list<NoobishAttribute>
}


// Attributes
let name v = Name v
let text value = Text(value)

let textAlign v = TextAlign (v)
let textTopLeft = TextAlign NoobishTextAlign.TopLeft
let textTopCenter = TextAlign NoobishTextAlign.TopCenter
let textTopRight = TextAlign NoobishTextAlign.TopRight
let textLeft = TextAlign NoobishTextAlign.Left
let textCenter = TextAlign NoobishTextAlign.Center
let textRight = TextAlign NoobishTextAlign.Right
let textBottomLeft = TextAlign NoobishTextAlign.BottomRight
let textBottomCenter = TextAlign NoobishTextAlign.BottomCenter
let textBottomRight = TextAlign NoobishTextAlign.BottomRight
let textWrap = TextWrap

let sliderRange min max = SliderRange(min, max)
let sliderValue v = SliderValue v
let sliderStep v = SliderStep v
let sliderOnValueChanged cb = SliderValueChanged cb

let texture t = Texture (NoobishTextureId.Basic t)
let ninePatch aid tid = Texture (NoobishTextureId.NinePatch(aid,tid))
let atlasTexture aid tid = Texture(NoobishTextureId.Atlas (aid, tid))

let textureSize s = TextureSize s
let textureEffect s = TextureEffect s
let textureFlipHorizontally = TextureEffect NoobishTextureEffect.FlipHorizontally
let textureFlipVertically = TextureEffect NoobishTextureEffect.FlipVertically
let textureBestFitMax = TextureSize NoobishTextureSize.BestFitMax
let textureBestFitMin = TextureSize NoobishTextureSize.BestFitMin
let textureStretch = TextureSize NoobishTextureSize.Stretch
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
let onPress action = OnPress(action)
let toggled value = Toggled (value)

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

let keyboardShortcut k = KeyboardShortcut k

let onOpenKeyboard cb = OnOpenKeyboard cb

// Components
let hr attributes = { ThemeId = "HorizontalRule"; Children = []; Attributes = minSize 0 2 :: fillHorizontal :: block :: attributes }
let label attributes = { ThemeId = "Label"; Children = []; Attributes = attributes }
let textBox attributes = { ThemeId = "TextBox"; Children = []; Attributes = attributes @ [KeyTypedEnabled] }
let paragraph attributes = { ThemeId = "Paragraph"; Children = []; Attributes = textWrap :: textAlign NoobishTextAlign.TopLeft :: attributes }
let header attributes = { ThemeId = "Header"; Children = []; Attributes = [fillHorizontal; block] @ attributes }
let button attributes =  { ThemeId = "Button"; Children = []; Attributes = attributes }
let image attributes = { ThemeId = "Image"; Children = []; Attributes = attributes}

let option t = {ThemeId = "Button"; Children = []; Attributes = [text t; block] }

let canvas children attributes = { ThemeId = "Canvas"; Children = children; Attributes = Layout(NoobishLayout.Absolute) :: attributes}


let scroll children attributes =
    { ThemeId = "Scroll"; Children = children; Attributes = [stackLayout; fill; scrollVertical; ] @ attributes}

let space attributes = { ThemeId = "Space"; Children = []; Attributes = fill :: attributes}

let panel children attributes =
    { ThemeId = "Panel"; Children = children; Attributes = stackLayout :: block :: attributes}
let panelWithGrid cols rows children attributes = { ThemeId = "Panel"; Children = children; Attributes = gridLayout cols rows :: block :: fill :: attributes}
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

let slider attributes =
    let handlePress (dispatch) (struct(x: int, y: int)) (c: NoobishLayoutElement) =
        let positionX = float32 x
        let _positionY = float32 y

        let changeModel m =
            match m with
            | Slider s' ->
                let bounds = c.Content
                let relative = (positionX - bounds.X) / (bounds.Width)
                let newValue = s'.Min + (relative * s'.Max - s'.Min)
                let steppedNewValue = truncate(newValue / s'.Step) * s'.Step
                s'.OnValueChanged (clamp steppedNewValue s'.Min s'.Max)
                Slider{s' with Value = steppedNewValue}
            | Combobox _ -> m
            | Textbox _ -> m

        dispatch c.Id (ChangeModel changeModel)
    {ThemeId = "Slider"; Children = []; Attributes = attributes @ [sliderRange 0.0f 100.0f; (OnPressInternal handlePress)]}


let combobox children attributes =
    let mutable onChange: string -> unit = ignore
    for a in attributes do
        match a with
        | OnChange (onChange') ->
            onChange <- onChange'
        | _ -> ()


    let children' = children |> List.map(fun c' ->
        let mutable text = ""
        for a in c'.Attributes do
            match a with
            | Text (text') ->
                text <- text'
            | _ -> ()

        let onClick = OnClickInternal (
            fun dispatch c ->
                dispatch c.ParentId Hide
                onChange (text)
            )
        {c' with Attributes = onClick :: c'.Attributes}
    )

    let dropdown = panel children' [ hidden; ZIndex(10 * 255); Overlay; Margin(0,0,0,0);]

    let onClickInternal: NoobishAttribute = OnClickInternal(fun dispatch c ->
        for child in c.Children do
            dispatch child.Id ToggleVisibility
    )

    {ThemeId = "Combobox"; Children = [dropdown]; Attributes = Layout(NoobishLayout.OverlaySource) :: onClickInternal :: attributes}

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

    let createNoobishLayoutElementState (version: Guid) (c: NoobishLayoutElement) =
        {
            Id = c.Id
            ParentId = c.ParentId
            Visible = c.Visible
            Focused = false
            Toggled = false
            FocusedTime = TimeSpan.FromDays(-1)
            PressedTime = TimeSpan.FromDays(-1)
            ScrolledTime = TimeSpan.FromDays(-1)

            ScrollX = 0.0f
            ScrollY = 0.0f

            KeyboardShortcut = c.KeyboardShortcut
            Version = version
            Model = c.Model

            Children = c.Children |> Array.map(fun child -> child.Id)
        }

    let private createNoobishLayoutElement (styleSheet: NoobishStyleSheet) (measureText: string -> string -> int*int) (settings: NoobishSettings) (mutateState: string -> ComponentMessage -> unit) (zIndex: int) (parentId: string) (parentPath: string) (parentWidth: float32) (parentHeight: float32) (startX: float32) (startY: float32) (themeId: string) (attributes: list<NoobishAttribute>) =

        let cid = $"%s{parentPath}/%s{themeId}-(%g{startX},%g{startY})"
        let cstate = "default"

        let scale (v: int) = float32 v * settings.Scale
        let scaleTuple (left, right, top, bottom) =
            (scale left, scale right, scale top, scale bottom)

        let mutable name = ""
        let mutable enabled = true
        let mutable visible = true
        let mutable toggled = false
        let mutable zIndex = zIndex
        let mutable overlay = false


        let mutable cursorWidth = 2f

        let mutable textAlign = NoobishTextAlign.Left
        let mutable text = ""
        let mutable textWrap = false
        let mutable paddingLeft, paddingRight, paddingTop, paddingBottom = scaleTuple (styleSheet.GetPadding themeId cstate)
        let mutable marginLeft, marginRight, marginTop, marginBottom = scaleTuple (styleSheet.GetMargin themeId cstate)

        let mutable fillHorizontal = false
        let mutable fillVertical = false

        let mutable isBlock = false
        let mutable onClick: unit -> unit = ignore
        let mutable onClickInternal: (string -> ComponentMessage -> unit) -> NoobishLayoutElement -> unit = (fun _ _ -> ())
        let mutable consumedButtons = ResizeArray<NoobishMouseButtonId>()
        let mutable consumedKeys = ResizeArray<NoobishKeyId>()
        let mutable keyTypedEnabled = false

        let mutable onPress: struct(int*int) -> unit = ignore
        let mutable onPressInternal: (string -> ComponentMessage -> unit) -> (struct(int*int)) -> NoobishLayoutElement -> unit = (fun _ _ _ -> ())

        let mutable onChange: string -> unit = ignore

        let mutable texture = NoobishTextureId.None
        let mutable textureEffect = NoobishTextureEffect.None
        let mutable textureColor = styleSheet.GetColor cid cstate
        let mutable textureSize = NoobishTextureSize.BestFitMax
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
                paddingBottom <- scale bottom
            | Margin (left, right, top, bottom) ->
                marginLeft <- scale left
                marginRight <- scale right
                marginTop <- scale top
                marginBottom <- scale bottom
            | MarginLeft left ->
                marginLeft <- scale left
            | MarginRight right ->
                marginLeft <- scale right
            | MarginTop top ->
                marginTop <- scale top
            | MarginBottom bottom ->
                marginBottom <- scale bottom
            | MinSize (width, height) ->
                minWidth <- scale width
                minHeight <- scale height
            // Text
            | Text(value) -> text <- value
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
                consumedButtons.Add (NoobishMouseButtonId.Left)
            | OnClickInternal(v) ->
                onClickInternal <- v
                consumedButtons.Add (NoobishMouseButtonId.Left)
            | OnPress(v) ->
                onPress <- v
                consumedButtons.Add (NoobishMouseButtonId.Left)
            | OnPressInternal(v) ->
                onPressInternal <- v
                consumedButtons.Add (NoobishMouseButtonId.Left)
            | OnChange(v) ->
                onChange <- v
                consumedButtons.Add (NoobishMouseButtonId.Left)
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
            | Block -> isBlock <- true
            | Enabled (v) -> enabled <- v
            | Visible (v) -> visible <- v
            | Texture (t) ->
                texture <- t
            | TextureEffect (t) ->
                textureEffect <- t
            | TextureSize (s) ->
                textureSize <- s
            | TextureRotation (t) ->
                textureRotation <- t
            | Scroll ->
                scrollHorizontal <- true
                scrollVertical <- true
                // Scroll needs to fill the whole area.
                fillHorizontal <- true
                fillVertical <- true
            |ScrollHorizontal ->
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
                relativeX <- scale x
                relativeY <- scale y
            | KeyboardShortcut k ->
                keyboardShortcut <- k
                consumedKeys.Add k
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



        let color = styleSheet.GetColor cid cstate
        let textFont = styleSheet.GetFont cid cstate

        let maxWidth = parentWidth * float32 colspan

        model |> Option.iter (fun model' ->
            match model' with
            | Slider (_s) ->
                minWidth <- maxWidth - paddingLeft - paddingRight - marginLeft - marginRight
                minHeight <- minHeight + styleSheet.GetHeight "Slider" "default"
            | Combobox (_c) -> ()
            | Textbox (_t) -> ()
        )


        let mutable textLines = ""
        if not (String.IsNullOrWhiteSpace text) then
            let paddedWidth = maxWidth - marginLeft - marginRight - paddingLeft - paddingRight
            textLines <- if textWrap then splitLines (measureText textFont) paddedWidth text else text

            let (contentWidth, contentHeight) = measureText textFont textLines

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
        let text = if String.IsNullOrWhiteSpace textLines then [||] else textLines.Split '\n'

        {
            Id = cid
            ParentId = parentId
            Path = path
            ThemeId = themeId
            Enabled = enabled
            Visible = visible
            Toggled = toggled
            ZIndex = zIndex
            Overlay = overlay

            FillHorizontal = fillHorizontal
            FillVertical = fillVertical

            TextAlignment = textAlign

            Text = text
            TextWrap = textWrap

            Model = model

            KeyboardShortcut = keyboardShortcut

            Texture =
                if texture <> NoobishTextureId.None then
                    Some (
                        {
                            Texture = texture
                            TextureEffect = textureEffect
                            TextureColor = textureColor
                            TextureColorDisabled = Color.DimGray
                            TextureSize = textureSize
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

            OnClickInternal = (fun c ->
                onClickInternal(mutateState) c
                onClick())

            OnPressInternal = (fun mousePos c ->
                onPressInternal (mutateState) mousePos c
                onPress mousePos )

            OnChange = onChange
            ConsumedMouseButtons = consumedButtons |> Seq.distinct |> Seq.toArray
            ConsumedKeys = consumedKeys |> Seq.distinct |> Seq.toArray

            KeyTypedEnabled = keyTypedEnabled

            Layout = layout
            ColSpan = colspan
            RowSpan = rowspan

            Color = styleSheet.GetColor themeId "default"
            ColorDisabled = styleSheet.GetColor themeId "disabled"

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
        (measureText: string -> string -> int*int)
        (styleSheet: NoobishStyleSheet)
        (settings: NoobishSettings)
        (mutateState: string -> ComponentMessage -> unit)
        (zIndex: int)
        (parentId: string)
        (parentPath: string)
        (startX: float32)
        (startY: float32)
        (parentWidth: float32)
        (parentHeight: float32)
        (c: NoobishElement): NoobishLayoutElement  =

        let parentComponent = createNoobishLayoutElement styleSheet measureText settings mutateState zIndex parentId parentPath parentWidth parentHeight startX startY c.ThemeId c.Attributes

        let mutable offsetX = 0.0f
        let mutable offsetY = 0.0f

        let newChildren = ResizeArray<NoobishLayoutElement>()

        let calculateChildHeight () =
            let childHeight = newChildren |> Seq.fold (fun acc c -> acc + c.OuterHeight) 0.0f
            childHeight

        let calculateOverflowHeight () =
            let childHeight = newChildren |> Seq.fold (fun acc c -> acc + c.OverflowHeight) 0.0f
            childHeight

        let calculateChildWidth () =
            if Seq.isEmpty newChildren then
                0.0f
            else
                newChildren |> Seq.map (fun c -> c.OuterWidth) |> Seq.max

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
                let childComponent = layoutElement measureText styleSheet settings mutateState zIndex parentComponent.Id path childStartX childStartY childWidth childHeight child

                newChildren.Add(childComponent)

                let childEndX = (offsetX + childComponent.OuterWidth) // ceil fixes
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

            let height =
                if parentComponent.FillVertical then
                    availableHeight + parentComponent.PaddingVertical + parentComponent.MarginVertical
                else
                    let childHeight = calculateChildHeight() + parentComponent.PaddingVertical + parentComponent.MarginVertical
                    clamp childHeight 0f availableHeight

            let overflowHeight = calculateOverflowHeight()
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

            let notFinished () = row < rows

            let cellUsed = Array2D.create cols rows false

            for child in c.Children do
                let childStartX = floor (parentBounds.X + (float32 col) * (colWidth))
                let childStartY = floor (parentBounds.Y + (float32 row) * (rowHeight))
                let path = sprintf "%s:grid(%i,%i)" parentComponent.Path col row
                let childComponent = layoutElement measureText styleSheet settings mutateState (zIndex + 1) parentComponent.Id path childStartX childStartY colWidth rowHeight child

                newChildren.Add({
                    childComponent with
                        OuterWidth = (colWidth * float32 childComponent.ColSpan)
                        OuterHeight = (rowHeight * float32 childComponent.RowSpan)
                        })

                for c = col to col + childComponent.ColSpan - 1 do
                    for r = row to row + childComponent.RowSpan - 1 do
                        cellUsed.[c, r] <- true

                while notFinished() && cellUsed.[col, row] do
                    bump childComponent.ColSpan childComponent.RowSpan

            {parentComponent with
                Children = newChildren.ToArray()
                OverflowWidth = parentComponent.ContentWidth
                OverflowHeight = parentComponent.ContentHeight}
        | NoobishLayout.OverlaySource ->

            if c.Children.Length <> 1 then failwith "Can only pop open one at a time."

            let path = sprintf "%s:overlay" parentComponent.Path
            let childComponent = layoutElement measureText styleSheet settings mutateState  (zIndex + 1) parentComponent.Id path parentComponent.X parentComponent.Y 800f 600f c.Children.[0]

            {parentComponent with Children = [|childComponent|]}
        | NoobishLayout.Absolute ->
            let parentBounds = parentComponent.Content

            for i = 0 to c.Children.Length - 1 do
                let childStartX = parentBounds.X + parentBounds.Width / 2.0f
                let childStartY = parentBounds.Y + parentBounds.Height / 2.0f

                let childWidth = parentBounds.Width
                let childHeight = parentBounds.Height

                let path = sprintf "%s:absolute:%i" parentComponent.Path i
                let childComponent = layoutElement measureText styleSheet settings mutateState (zIndex + 1) parentComponent.Id path childStartX childStartY childWidth childHeight c.Children.[i]
                newChildren.Add({ childComponent with StartX = childComponent.StartX; StartY = childComponent.StartY })

            {parentComponent with Children = newChildren.ToArray()}
        | NoobishLayout.None ->
            parentComponent


    let layout (measureText: string -> string -> int*int) (styleSheet: NoobishStyleSheet) (settings: NoobishSettings) (mutateState: string -> ComponentMessage -> unit) (layer: int) (width: float32) (height: float32) (elements: list<NoobishElement>) =

        let path = sprintf "layer-%i" layer
        elements
            |> List.map(fun c ->
                layoutElement measureText styleSheet settings mutateState (layer * 128) "" path 0.0f 0.0f width height c
            ) |> List.toArray


