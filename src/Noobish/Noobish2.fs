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
open Internal






type Noobish2(maxCount: int) =


    let rasterizerState = 
        let rasterizerState = new RasterizerState()
        rasterizerState.ScissorTestEnable <- true
        rasterizerState

    let mutable previousMouseState: MouseState = Microsoft.Xna.Framework.Input.Mouse.GetState()

    let mutable previousKeyState: KeyboardState = Microsoft.Xna.Framework.Input.Keyboard.GetState()
    let mutable waitForLayout = true

    let drawQueue = PriorityQueue<int, int>()

    let toLayout = PriorityQueue<int, int>()

    let frames = Array.init 2 (fun _ -> NoobishComponents(256))
    
    let mutable previousFrameIndex = 0
    let mutable frameIndex = 1

    member this.Components with get() = frames.[frameIndex]

    member val Cursor = 0 with get, set 

    member val FocusedElementId = UIComponentId.empty with get, set
    member val FocusedTime = TimeSpan.Zero with get, set

    member val ScreenWidth: float32 = 0f with get, set 
    member val ScreenHeight: float32 = 0f with get, set

    member val Debug = false with get, set

    member this.IsActive(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && this.Components.Id.[index].Id.Equals cid.Id then 
            index
        else 
            -1
    member this.GetIndex(cid: UIComponentId) =
        let index = cid.Index

        if index <> -1 && this.Components.Id.[index].Id.Equals cid.Id then 
            index
        else 
            -1

    member this.Create(themeId: string): UIComponentId =

        let i = this.Components.Count
        let cid: UIComponentId = {Index = i; Id = Guid.NewGuid()}
        this.Components.Id.[i] <- cid
        this.Components.ThemeId.[i] <- themeId
        if this.Components.ParentId.[i] <> UIComponentId.empty then failwith "crap"
        this.Components.Enabled.[i] <- true 
        this.Components.Visible.[i] <- true 
        this.Components.Layer.[i] <- 1
        this.Components.Layout.[i] <- Layout.None
        this.Components.GridSpan.[i] <- {Colspan = 1; Rowspan = 1}
        this.Components.MinSize.[i] <- {Width = 0f; Height = 0f}

        this.Components.LastPressTime[i] <- TimeSpan.Zero
        
        this.Components.Count <- this.Components.Count + 1

        cid

    member this.Overlaypane(rcid: UIComponentId) =
        let cid = this.Create "Overlay"
        this.Components.Layout.[cid.Index] <- Layout.Relative rcid
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        
        cid

    member this.Window() =
        let cid = this.Create "Panel"
        this.Components.Layout.[cid.Index] <- Layout.LinearVertical
        cid

    member this.WindowWithGrid (cols: int, rows: int) =
        let cid = this.Create "Window"
        this.Components.Layout.[cid.Index] <- Layout.Grid(cols, rows)
        cid

    member this.Header (t: string) = 
        let cid = this.Create "Header1"
        this.Components.Text.[cid.Index] <- t
        this.Components.Block.[cid.Index] <- true

        cid

    member this.Label (t: string) = 
        let cid = this.Create "Label"
        this.Components.Text.[cid.Index] <- t

        cid

    member this.Paragraph (t: string) = 
        let cid = this.Create "Paragraph"
        this.Components.Text.[cid.Index] <- t
        this.Components.Textwrap.[cid.Index] <- true 
        this.Components.TextAlign.[cid.Index] <- NoobishTextAlignment.TopLeft 
        this.Components.Block.[cid.Index] <- true 
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = false}

        cid


    member this.Textbox (t: string) (onTextChanged: string -> unit) = 
        let cid = this.Create "TextBox"

        this.Components.WantsFocus.[cid.Index] <- true 
        this.Components.OnFocus.[cid.Index] <- (
            fun event focus -> ()
                
        )

        this.Components.WantsKeyTyped.[cid.Index] <- true 
        this.Components.OnKeyTyped.[cid.Index] <- (fun event typed ->
            let text = this.Components.Text.[cid.Index]
            if int typed = 8 then // backspace
                if text.Length > 0 && this.Cursor > 0 then
                    this.Components.Text.[cid.Index] <- text.Remove(this.Cursor - 1, 1)
                    this.Cursor <- this.Cursor - 1
            elif int typed = 13 || int typed = 10 then 
                this.FocusedElementId <- UIComponentId.empty
                onTextChanged (this.Components.Text[cid.Index])
                this.Cursor <- 0
            elif int typed = 27 then
                this.FocusedElementId <- UIComponentId.empty
                this.Components.Text.[cid.Index] <- t
                this.Cursor <- 0
            elif int typed = 127 then // deleted
                if text.Length > 0 && this.Cursor < text.Length then
                    this.Components.Text.[cid.Index] <- text.Remove(this.Cursor, 1)
            else
                this.Components.Text.[cid.Index] <- text.Insert(this.Cursor, typed.ToString())
                this.Cursor <- this.Cursor + 1
        )
        this.Components.WantsKeyPressed.[cid.Index] <- true 
        this.Components.OnKeyPressed.[cid.Index] <- (fun event key ->
            let text = this.Components.Text.[cid.Index]
            if key = Microsoft.Xna.Framework.Input.Keys.Left then 
                let newCursorPos = this.Cursor - 1 
                this.Cursor <- max 0 newCursorPos
            elif key = Microsoft.Xna.Framework.Input.Keys.Right then 
                let newCursorPos = this.Cursor + 1 
                this.Cursor <- min text.Length newCursorPos
        )

        this.Components.Text.[cid.Index] <- t



        cid

    member this.Button (t: string) (onClick: UIComponentId -> unit) = 
        let cid = this.Create "Button"
        this.Components.Text.[cid.Index] <- t
        this.Components.WantsOnClick.[cid.Index] <- true
        this.Components.OnClick.[cid.Index] <- (fun source _ -> onClick source)
        cid

    member this.Space () = 
        let cid = this.Create "Space"
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Div () = 
        let cid = this.Create "Division"
        this.Components.Block.[cid.Index] <- true
        this.SetLayout (Layout.LinearVertical) cid.Index

        cid

    member this.Grid (cols: int, rows: int) = 
        let cid = this.Create "Division"
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid

    member this.PanelWithGrid (cols: int, rows: int) = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.Grid(cols, rows)) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.PanelHorizontal () = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.LinearHorizontal) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.PanelVertical () = 
        let cid = this.Create "Panel"
        this.SetLayout (Layout.LinearVertical) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.DivHorizontal () = 
        let cid = this.Create "Division"
        this.SetLayout (Layout.LinearHorizontal) cid.Index
        this.Components.Block.[cid.Index] <- true 
        //this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.DivVertical () = 
        let cid = this.Create "Division"
        this.SetLayout (Layout.LinearVertical) cid.Index
        this.Components.Block.[cid.Index] <- true 
        //this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.Canvas () = 
        let cid = this.Create "Division"
        this.SetLayout (Layout.Relative (cid)) cid.Index
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.HorizontalRule () = 
        let cid = this.Create "HorizontalRule"
        this.Components.Id.[cid.Index] <- cid
        this.Components.Block.[cid.Index] <- true
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = false} 
        cid


    member this.SetRelativePosition (x: int, y: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.RelativePosition.[index] <- {X = float32 x; Y = float32 y}
        cid

    member this.SetRelativePositionFunc (f: UIComponentId -> UIComponentId -> Position) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.RelativePositionFunc.[index] <- f
        cid

    member this.SetConstrainToParentBounds (v: bool) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ConstrainToParentBounds.[index] <- v
        cid


    member this.SetThemeId (themeId: string) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ThemeId.[index] <- themeId
        cid

    member this.SetSize (width: int, height: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.MinSize.[index] <- {Width = float32 width; Height = float32 height}
        cid

    member this.SetLayer (layer: int) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 

            let children = this.Components.Children.[cid.Index]
            if children.Count > 0 then 
                let previousLayer = this.Components.Layer.[index]

                for j = 0 to children.Count - 1 do 
                    let ccid = children.[j]
                    let deltaLayer = this.Components.Layer.[ccid.Index] - previousLayer
                    this.SetLayer (layer + deltaLayer) children.[j] |> ignore

            this.Components.Layer.[index] <- layer
        cid


    member this.SetVisible (v: bool) (cid: UIComponentId): UIComponentId = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Visible.[index] <- v
            
            let children = this.Components.Children.[cid.Index]
            for j = 0 to children.Count - 1 do 
                this.SetVisible v children.[j] |> ignore
        cid

    member this.SetEnabled (v: bool) (cid: UIComponentId) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Enabled.[index] <- v
        cid

    member this.SetPosition (x: int, y: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Bounds.[index] <- {this.Components.Bounds.[index] with X = float32 x; Y = float32 y}
        cid

    member private this.SetLayout (layout: Layout) (index: int) =
        this.Components.Layout.[index] <- layout

    member this.SetRowspan (rowspan: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.GridSpan.[index] <- {this.Components.GridSpan.[index] with Rowspan = rowspan}
        cid

    member this.SetColspan (colspan: int) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.GridSpan.[index] <- {this.Components.GridSpan.[index] with Colspan = colspan}
        cid

    member this.SetMargin (margin: int) (cid: UIComponentId) =
        let margin = float32 margin
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.MarginOverride.[index] <- true
            this.Components.Margin.[index] <- {Top = margin; Right = margin; Bottom = margin; Left = margin}
        cid

    member this.SetPadding (padding: int) (cid: UIComponentId) =
        let padding = float32 padding
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.PaddingOverride.[index] <- true
            this.Components.Padding.[index] <- {Top = padding; Right = padding; Bottom = padding; Left = padding}
        cid


    member this.FillHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Horizontal = true}
        cid

    member this.SetFillHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Horizontal = true}
        cid

    member this.FillVertical (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Vertical = true}
        cid
    member this.SetFill (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        cid

    member this.SetScrollHorizontal (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            if this.Components.Layout.[index] = Layout.None then 
                failwith "Layout can't be none when scroll is set."
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Horizontal = true}
        cid

    member this.SetScrollVertical (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            if this.Components.Layout.[index] = Layout.None then 
                failwith "Layout can't be none when scroll is set."
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Vertical = true}
        cid


    member this.SetScroll (horizontal: bool) (vertical: bool) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            if this.Components.Layout.[index] = Layout.None then 
                failwith "Layout can't be none when scroll is set."
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Horizontal = horizontal; Vertical = vertical}
        cid

    member this.AlignText (textAlignment: NoobishTextAlignment) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.TextAlign.[index] <- textAlignment
        cid       

    member this.AlignTextTopLeft (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.TopLeft cid

    member this.AlignTextTop (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.TopCenter cid

    member this.AlignTextTopRight (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.TopRight cid


    member this.AlignTextLeft (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.Left cid

    member this.AlignTextCenter (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.Center cid

    member this.AlignTextRight (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.Right cid

    member this.AlignTextBottomLeft (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.BottomLeft cid

    member this.AlignTextBottomCenter (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.BottomCenter cid

    member this.AlignTextBottomRight (cid: UIComponentId) = 
        this.AlignText NoobishTextAlignment.BottomRight cid

  
    member this.SetToggled (t: bool) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Toggled.[index] <- t
        cid

    member this.SetOnPress (onPress: UIComponentId -> Position -> unit) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.WantsOnPress.[index] <- true
            this.Components.OnPress.[index] <- onPress
        cid
    member this.SetOnClick (onClick: UIComponentId -> Position -> unit) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.WantsOnClick.[index] <- true
            this.Components.OnClick.[index] <- onClick
        cid

    member this.BumpLayer (layer: int) (children: IReadOnlyList<UIComponentId>) = 
        for i = 0 to children.Count - 1 do 
            let cid = children.[i]
            let childLayer = this.Components.Layer.[cid.Index] + layer
            this.Components.Layer.[cid.Index] <- childLayer
            this.BumpLayer childLayer this.Components.Children.[cid.Index]


    member this.SetChildren (cs: UIComponentId[]) (cid: UIComponentId) =
        let index = this.GetIndex cid 
        if index > -1 then 
            for i = 0 to cs.Length - 1 do 
                let childId = cs[i]
                if this.Components.ParentId.[childId.Index] <> UIComponentId.empty then failwith "what"
                this.Components.ParentId.[childId.Index] <- cid
            let childrenIds = this.Components.Children.[index]
            childrenIds.AddRange cs

            let layer = this.Components.Layer.[index]
            this.BumpLayer layer (childrenIds :> IReadOnlyList<UIComponentId>)
        cid



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
        let themeId = this.Components.ThemeId.[i]
        let layer = 1f - float32 (this.Components.Layer.[i] + 32) / 255f


        let fontId = styleSheet.GetFont themeId "default"
        let font = content.Load<NoobishFont>  fontId
        let fontSize = styleSheet.GetFontSize themeId "default"

        let bounds = this.Components.Bounds.[i]
        let margin = this.Components.Margin.[i]
        let padding = this.Components.Padding.[i]
        let contentStartX = bounds.X + margin.Left + padding.Left
        let contentStartY = bounds.Y +  margin.Top + padding.Top
        let contentWidth = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom
        let bounds: Internal.NoobishRectangle = {X = contentStartX; Y = contentStartY; Width = contentWidth; Height = contentHeight}

        let text = this.Components.Text.[i]
        let textAlign = this.Components.TextAlign.[i]
        let textWrap = this.Components.Textwrap.[i]

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

    member this.DrawBackground (styleSheet: NoobishStyleSheet) (textureAtlas: NoobishTextureAtlas) (spriteBatch: SpriteBatch) (gameTime: GameTime) (scrollX: float32) (scrollY: float32) (i: int)=

        let cstate =
            if this.Components.WantsFocus.[i] && this.FocusedElementId.Id = this.Components.Id.[i].Id then 
                "focused"
            elif not this.Components.Enabled.[i] then
                "disabled"
            elif this.Components.Toggled.[i] then
                "toggled"
            else
                "default"
        let themeId = this.Components.ThemeId.[i]
        let bounds = this.Components.Bounds.[i]
        let margin = this.Components.Margin.[i]
        let layer = this.Components.Layer.[i]
        let contentStartX = bounds.X + margin.Left + scrollX
        let contentStartY = bounds.Y +  margin.Top + scrollY
        let contentWidth = bounds.Width - margin.Left - margin.Right
        let contentHeight = bounds.Height - margin.Top - margin.Bottom

        let color =
            if this.Components.WantsFocus.[i] && this.FocusedElementId = this.Components.Id.[i] then
                styleSheet.GetColor themeId "focused"
            elif not this.Components.Enabled.[i] then
                styleSheet.GetColor themeId "disabled"
            elif this.Components.Toggled.[i] then
                styleSheet.GetColor themeId "toggled"
            else
                let lastPressTime = this.Components.LastPressTime.[i]
                let progress = 1.0 - min ((gameTime.TotalGameTime - lastPressTime).TotalSeconds / 0.05) 1.0

                let color = styleSheet.GetColor themeId "default"
                let pressedColor = styleSheet.GetColor themeId "toggled"
                let finalColor = Color.Lerp(color, pressedColor, float32 progress)

                finalColor

        let drawables = styleSheet.GetDrawables themeId cstate

        let position = Vector2(contentStartX, contentStartY)
        let size = Vector2(contentWidth, contentHeight)

        let layer = 1f - (float32 layer / 255f)

        DrawUI.drawDrawable textureAtlas spriteBatch position size layer color drawables


    member this.DrawScrollBars
        (styleSheet: NoobishStyleSheet)
        (textureAtlas: NoobishTextureAtlas)
        (spriteBatch: SpriteBatch)
        (gameTime: GameTime)
        _scrollX
        _scrollY
        (i: int) =

        let scroll = this.Components.Scroll.[i]
        let lastScrollTime = this.Components.LastScrollTime.[i]
        let delta = float32 (min (gameTime.TotalGameTime - lastScrollTime).TotalSeconds 0.3)
        let progress = 1.0f - delta / 0.3f

        let scrollBarWidth = styleSheet.GetWidth "ScrollBar" "default"
        if scrollBarWidth = 0f then failwith "Missing width for styleSheet 'ScrollBar' mode 'default'."
        if scroll.Vertical && progress > 0.0f then

            let scrollBarColor = styleSheet.GetColor "ScrollBar" "default"
            let scrollbarDrawable = styleSheet.GetDrawables "ScrollBar" "default"

            let scrollbarPinColor = styleSheet.GetColor "ScrollBarPin" "default"
            let scrollbarPinDrawable = styleSheet.GetDrawables "ScrollBar" "default"

            let scrollY = this.Components.ScrollY.[i]
            let contentSize = this.Components.ContentSize.[i]
            let bounds = this.Components.Bounds.[i]
            let padding = this.Components.Padding.[i]
            let margin = this.Components.Margin.[i]
            let contentX = bounds.X + margin.Left + padding.Left
            let contentY = bounds.Y + margin.Top + padding.Top
            let contentWidth = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
            let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom
            let x = contentX + contentWidth - scrollBarWidth / 2f
            let color = Color.Multiply(scrollBarColor, progress)

            let layer = this.Components.Layer.[i]
            let layer = 1f - float32 (layer + 5) / 255.0f

            DrawUI.drawDrawable textureAtlas spriteBatch (Vector2(x, bounds.Y + 4f)) (Vector2(scrollBarWidth, bounds.Height - 8f)) layer color scrollbarDrawable

            let pinPosition =  - ( scrollY / contentSize.Height) * bounds.Height
            let pinHeight = ( contentHeight / contentSize.Height) * bounds.Height
            let color = Color.Multiply(scrollbarPinColor, progress)

            DrawUI.drawDrawable textureAtlas spriteBatch (Vector2(x, bounds.Y + pinPosition + 4f)) (Vector2(scrollBarWidth, pinHeight - 8f)) layer color scrollbarPinDrawable


    member this.DrawText (content: ContentManager) (styleSheet: NoobishStyleSheet) (textBatch: TextBatch) (scrollX: float32) (scrollY: float32) (i: int) =
        let text = this.Components.Text.[i]
        if text.Length > 0 then 
            let themeId = this.Components.ThemeId.[i]
            let layer = this.Components.Layer.[i]

            let layer = (1f - float32 (layer + 1) / 255.0f)

            let state = 
                if this.Components.WantsFocus.[i] && this.FocusedElementId = this.Components.Id.[i] then
                    "focused"
                elif not this.Components.Enabled.[i] then
                    "disabled"
                elif this.Components.Toggled.[i] then
                    "toggled"
                else
                    "default"


            let fontId = styleSheet.GetFont themeId state
            let font = content.Load<NoobishFont> fontId

            let fontSize = (styleSheet.GetFontSize themeId state)

            let bounds = this.Components.Bounds.[i]
            let margin = this.Components.Margin.[i]
            let padding = this.Components.Padding.[i]
            let contentStartX = scrollX + bounds.X + padding.Left + margin.Left
            let contentStartY = scrollY + bounds.Y + padding.Top + margin.Top
            let contentWidth = bounds.Width - padding.Left - padding.Right - margin.Left - margin.Right
            let contentHeight = bounds.Height - padding.Top - padding.Bottom - margin.Left - margin.Right
            let bounds: Internal.NoobishRectangle = {X = contentStartX; Y = contentStartY; Width = contentWidth; Height = contentHeight}
            let textWrap = this.Components.Textwrap.[i]
            let textAlign = this.Components.TextAlign.[i]
            let textBounds =
                    NoobishFont.calculateBounds font fontSize false bounds 0f 0f textAlign text

            let textColor = styleSheet.GetFontColor themeId state
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
        (parentBounds: NoobishRectangle)
        (parentScrollX: float32)
        (parentScrollY: float32)
        (i: int)
        (gameTime: GameTime) =

        let visible = this.Components.Visible.[i]
        if visible then 
            let parentBounds = 
                if this.Components.ConstrainToParentBounds.[i] then 
                    parentBounds
                else 
                    {X = 0f; Y = 0f; Width = this.ScreenWidth; Height = this.ScreenHeight}

            let textureAtlas = content.Load(styleSheet.TextureAtlasId)

            let bounds = this.Components.Bounds.[i]
            let margin = this.Components.Margin.[i]
            let padding = this.Components.Padding.[i]
            let contentStartX = parentScrollX + bounds.X + margin.Left
            let contentStartY = parentScrollY + bounds.Y + margin.Top
            let contentWidth = bounds.Width - margin.Left - margin.Right
            let contentHeight = bounds.Height - margin.Top - margin.Bottom

            let boundsWithMargin = ({
                    X = contentStartX
                    Y = contentStartY
                    Width = contentWidth
                    Height = contentHeight
            })
            let boundsWithMargin = boundsWithMargin.Clamp parentBounds


            if boundsWithMargin.Width > 0f && boundsWithMargin.Height > 0f then 
                let boundsWithMarginAndPadding = {

                        X = contentStartX + padding.Left
                        Y = contentStartY + padding.Top
                        Width = contentWidth - padding.Left - padding.Right
                        Height = contentHeight - padding.Top - padding.Bottom
                }
                let boundsWithMarginAndPadding = boundsWithMarginAndPadding.Clamp parentBounds

                let oldScissorRect = graphics.ScissorRectangle

                graphics.ScissorRectangle <- DrawUI.toRectangle boundsWithMargin 
                spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)

                this.DrawBackground styleSheet textureAtlas spriteBatch gameTime parentScrollX parentScrollY i

                if this.FocusedElementId = this.Components.Id.[i] then 
                    this.DrawCursor styleSheet content textureAtlas spriteBatch i gameTime 0f 0f

                this.DrawScrollBars styleSheet textureAtlas spriteBatch gameTime parentScrollX parentScrollY i 
                spriteBatch.End()

                graphics.ScissorRectangle <- DrawUI.toRectangle boundsWithMarginAndPadding

                this.DrawText content styleSheet textBatch parentScrollX parentScrollY i


                graphics.ScissorRectangle <- oldScissorRect

                spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)
                if this.Debug then
                    let pixel = content.Load<Texture2D> "Pixel"
                    let themeId = this.Components.ThemeId.[i]
                    let bounds = boundsWithMarginAndPadding

                    let debugColor =
                        if themeId = "Scroll" then Color.Multiply(Color.Red, 0.1f)
                        elif themeId = "Button" then Color.Multiply(Color.Green, 0.1f)
                        elif themeId = "Paragraph" then Color.Multiply(Color.Purple, 0.1f)
                        else Color.Multiply(Color.Yellow, 0.1f)

                    DrawUI.drawRectangle spriteBatch pixel debugColor (bounds.X + parentScrollX) (bounds.Y + parentScrollY) (bounds.Width) (bounds.Height)
                    if themeId = "Scroll" || themeId = "Slider" then
                        let r = {
                            X = float32 bounds.X
                            Y = float32 bounds.Y
                            Width =  float32 bounds.Width
                            Height = float32 bounds.Height}
                        DrawUI.debugDrawBorders spriteBatch pixel (Color.Multiply(Color.Red, 0.5f)) r

                    let text  = this.Components.Text.[i] 
                    let textWrap = this.Components.Textwrap.[i]
                    let textAlign = this.Components.TextAlign.[i]
                    if text <> "" then

                        let fontId = styleSheet.GetFont themeId "default"
                        let font = content.Load<NoobishFont>  fontId
                        let fontSize = styleSheet.GetFontSize themeId "default"


                        let textBounds = NoobishFont.calculateBounds font fontSize textWrap bounds parentScrollX parentScrollX textAlign text

                        DrawUI.drawRectangle spriteBatch pixel Color.Purple (textBounds.X) (textBounds.Y) (textBounds.Width) (textBounds.Height)

                spriteBatch.End()

                let scrollX = parentScrollX + this.Components.ScrollX.[i]
                let scrollY = parentScrollY + this.Components.ScrollY.[i]


                let children = this.Components.Children.[i]
                if children.Count > 0 then 
                    for j = 0 to children.Count - 1 do 
                        this.DrawComponent graphics content spriteBatch textBatch styleSheet boundsWithMargin scrollX scrollY children.[j].Index gameTime 
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
        if waitForLayout && this.Components.Count > 0 then 

            let previousFrame = frames.[previousFrameIndex]
            let mutable drift = (this.Components.Count <> previousFrame.Count)

            let mutable j = 0
            while not drift && j < this.Components.Count do 
                if this.Components.ThemeId.[j] = previousFrame.ThemeId.[j] &&
                   (this.Components.ParentId.[j].Index = previousFrame.ParentId.[j].Index) &&
                   (this.Components.Layer.[j] = previousFrame.Layer.[j]) &&
                   (this.Components.ContentSize.[j].Width - previousFrame.ContentSize.[j].Width) < Single.Epsilon &&
                   (this.Components.ContentSize.[j].Height - previousFrame.ContentSize.[j].Height) < Single.Epsilon && 
                   (this.Components.Children.[j].Count = previousFrame.Children[j].Count) && 
                   (this.Components.GridSpan.[j].Colspan = previousFrame.GridSpan.[j].Colspan) &&
                   (this.Components.GridSpan.[j].Rowspan = previousFrame.GridSpan.[j].Rowspan)
                   
                    then 

                    j <- j + 1
                else 
                    drift <- true

            if not drift then 
                for i = 0 to this.Components.Count do 
                    
                    this.Components.Hovered.[i] <- previousFrame.Hovered.[i]
                    this.Components.LastHoverTime.[i] <- previousFrame.LastHoverTime.[i]
                    this.Components.LastPressTime.[i] <- previousFrame.LastPressTime.[i]

                    this.Components.ScrollX.[i] <- previousFrame.ScrollX.[i]
                    this.Components.ScrollY.[i] <- previousFrame.ScrollY.[i]
                    this.Components.LastScrollTime.[i] <- previousFrame.LastScrollTime.[i]

            for i = 0 to this.Components.Count - 1 do 
                let themeId = this.Components.ThemeId.[i]

                if not this.Components.MarginOverride.[i] then 
                    let (top, right, bottom, left) = styleSheet.GetMargin themeId "default"
                    this.Components.Margin.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                if not this.Components.PaddingOverride.[i] then 
                    let (top, right, bottom, left) = styleSheet.GetPadding themeId "default"
                    this.Components.Padding.[i] <- {Top = float32 top; Left = float32 left;  Bottom = float32 bottom; Right = float32 right;}

                this.Components.MinSize.[i] <- {

                    Width = styleSheet.GetWidth themeId "default"
                    Height = styleSheet.GetHeight  themeId "default"
                }

                (* Run layout only for root components. *)
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, this.Components.Layer.[i])


            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.Components.CalculateContentSize content styleSheet screenWidth screenHeight i
                this.Components.LayoutComponent content styleSheet 0f 0f screenWidth screenHeight i

            waitForLayout <- false

        for i = 0 to this.Components.Count - 1 do 
            let layer = this.Components.Layer.[i]

            if this.Components.ParentId.[i] = UIComponentId.empty then 
                drawQueue.Enqueue(i, layer)

        let oldRasterizerState = graphics.RasterizerState
        graphics.RasterizerState <- rasterizerState

        let styleSheet = content.Load<Noobish.Styles.NoobishStyleSheet> styleSheetId
        while drawQueue.Count > 0 do 
            let i = drawQueue.Dequeue()
            this.DrawComponent graphics content spriteBatch textBatch styleSheet {X = 0f; Y = 0f; Width = this.ScreenWidth; Height = this.ScreenHeight} 0f 0f i gameTime


        graphics.RasterizerState <- oldRasterizerState


    member this.ComponentContains (x: float32) (y: float32) (scrollX: float32) (scrollY: float32) (i: int) = 
        let bounds = this.Components.Bounds.[i]
        let left = scrollX + bounds.X 
        let top = scrollY + bounds.Y 
        let right = left + bounds.Width
        let bottom = top + bounds.Height
        
        not (x < left || x > right || y < top || y > bottom)

    member this.Hover (x: float32) (y: float32) (gameTime: GameTime) (i: int)  =
        if this.Components.Visible.[i] && this.Components.Enabled.[i]&& this.ComponentContains x y 0f 0f i then 
            Log.Logger.Debug ("Mouse hover inside component {ComponentId}", i)

            let children = this.Components.Children.[i]
            if children.Count = 0 then 
                this.Components.LastHoverTime.[i] <- gameTime.TotalGameTime
            else 
                for j = 0 to children.Count - 1 do 
                    this.Hover x y gameTime children.[j].Index

    member this.Click (x: float32) (y: float32) (gameTime: GameTime) (parentScrollX: float32) (parentScrollY: float32) (i: int): bool  =

        if this.Components.Visible.[i] && this.Components.Enabled.[i]&& this.ComponentContains x y parentScrollX parentScrollY i then 
            Log.Logger.Debug ("Mouse click inside component {ComponentId}", i)

            let children = this.Components.Children.[i]

            let scrollX = parentScrollX + this.Components.ScrollX.[i]
            let scrollY = parentScrollY + this.Components.ScrollY.[i]
            let mutable found = false
            let mutable j = 0
            while not found && j < children.Count do 
                if this.Click x y gameTime scrollX scrollY  children.[j].Index then 
                    found <- true 
                j <- j + 1

            if not found && this.Components.WantsOnClick.[i] then 
                found <- true
                this.Components.OnClick.[i] (this.Components.Id.[i]) {X = x; Y = y}
            
            if not found  && this.Components.WantsFocus.[i] then
                found <- true 
                this.FocusedElementId <- this.Components.Id.[i]
                this.Cursor <- this.Components.Text.[i].Length
                this.FocusedTime <- gameTime.TotalGameTime
                this.Components.OnFocus.[i] ({SourceId = this.Components.Id.[i]}) true
            
            found

        else 
            false


    member this.KeyTyped (c: char) = 
        let focusedIndex = this.GetIndex this.FocusedElementId
        if focusedIndex <> -1 && this.Components.WantsKeyTyped.[focusedIndex] then 
            Log.Logger.Debug ("Key typed {Key} for component {Component}", c, focusedIndex)
            this.Components.OnKeyTyped.[focusedIndex] {SourceId = this.Components.Id.[focusedIndex]} c


    member this.Press (x: float32) (y: float32) (gameTime: GameTime) (parentScrollX: float32) (parentScrollY: float32) (i: int): bool =


        if this.Components.Visible.[i] && this.Components.Enabled.[i]&& this.ComponentContains x y parentScrollX parentScrollY i then 
            Log.Logger.Debug ("Mouse press inside component {ComponentId}", i)
            let children = this.Components.Children.[i]

            if this.Components.WantsOnPress.[i] || children.Count = 0 then 
                this.Components.LastPressTime.[i] <- gameTime.TotalGameTime
                this.Components.OnPress.[i] this.Components.Id.[i] {X = x; Y = y}
                true 
            else 
                let scrollX = parentScrollX + this.Components.ScrollX.[i]
                let scrollY = parentScrollY + this.Components.ScrollY.[i]
                let mutable handled = false
                let mutable j = 0

                while not handled && j < children.Count do
                    let cid = children.[j]

                    if this.Press x y gameTime scrollX scrollY cid.Index then 
                        handled <- true 

                    j <- j + 1
                handled
        else 
            false

    member this.Scroll
        (positionX: float32)
        (positionY: float32)
        (scale: float32)
        (time: TimeSpan)
        (scrollX: float32)
        (scrollY: float32)
        (i: int): bool =

        let scaleValue v = v * scale

        let mutable handled = false

        if this.Components.Enabled.[i] && this.Components.Visible.[i] && this.ComponentContains positionX positionY scrollX scrollY i then
            let handledByChild =
                let children = this.Components.Children.[i]
                let mutable handledByChild = false 
                for j = 0 to children.Count - 1 do 
                    if this.Scroll positionX positionY scale time scrollX scrollY children.[j].Index then 
                        handledByChild <- true 
                    else 
                        handledByChild <- false 
                handledByChild

            if not handledByChild then
                let contentSize = this.Components.ContentSize.[i]
                let bounds = this.Components.Bounds.[i]
                let scroll = this.Components.Scroll.[i]
                let padding = this.Components.Padding.[i]
                let margin = this.Components.Margin.[i]
                
                let contentWidth = bounds.Width - margin.Left - margin.Right - padding.Left - padding.Right
                let contentHeight = bounds.Height - margin.Top - margin.Bottom - padding.Top - padding.Bottom

                if scroll.Horizontal && contentSize.Width > contentWidth then
                    this.Components.ScrollX.[i] <- this.Components.ScrollX.[i] + scrollX
                    this.Components.LastScrollTime.[i] <- time
                    handled <- true
                if scroll.Vertical && contentSize.Height > contentHeight then
                    let scaledScroll = scaleValue scrollY
                    let nextScroll = this.Components.ScrollY.[i] + scaledScroll
                    let minScroll = contentHeight - contentSize.Height


                    this.Components.ScrollY.[i] <- clamp nextScroll minScroll 0.0f
                    this.Components.LastScrollTime.[i] <- time
                    handled <- true
            else
                handled <- true

        handled



    member this.ProcessMouse(gameTime: GameTime) =
        let mouseState =  Microsoft.Xna.Framework.Input.Mouse.GetState()

        let x = float32 mouseState.X
        let y = float32 mouseState.Y
        for i = 0 to this.Components.Count - 1 do 
            if this.Components.ParentId.[i] = UIComponentId.empty then 
                this.Hover x y gameTime i 

        if mouseState.LeftButton = ButtonState.Pressed then 
            for i = 0 to this.Components.Count - 1 do 
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, -this.Components.Layer.[i])

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.Press x y gameTime 0f 0f i |> ignore

        elif mouseState.LeftButton = ButtonState.Released && previousMouseState.LeftButton = ButtonState.Pressed then 
            this.FocusedElementId <- UIComponentId.empty
            for i = 0 to this.Components.Count - 1 do 
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    toLayout.Enqueue(i, -this.Components.Layer.[i])

            while toLayout.Count > 0 do 
                let i = toLayout.Dequeue()
                this.Click x y gameTime 0f 0f i |> ignore

        let scrollWheelValue = mouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue
        if scrollWheelValue <> 0 then
            let scroll = float32 scrollWheelValue / 2.0f
            let absScroll = abs scroll
            let sign = sign scroll |> float32
            let absScrollAmount = min absScroll (absScroll * float32 gameTime.ElapsedGameTime.TotalSeconds * 10.0f)

            for i = 0 to this.Components.Count - 1 do 
                if this.Components.ParentId.[i] = UIComponentId.empty then 
                    this.Scroll x y 1.0f gameTime.TotalGameTime 0.0f (- absScrollAmount * sign) i |> ignore

        previousMouseState <- mouseState

    member this.ProcessKeys (gameTime: GameTime) = 
        let keyState = Keyboard.GetState()

        let index = this.GetIndex(this.FocusedElementId)
        if index <> -1 && this.Components.WantsKeyPressed.[index] then 
            if previousKeyState.IsKeyDown(Keys.Left) && keyState.IsKeyUp (Keys.Left) then 
                this.Components.OnKeyPressed.[index] {SourceId = this.Components.Id.[index]} Keys.Left

            if previousKeyState.IsKeyDown(Keys.Right) && keyState.IsKeyUp (Keys.Right) then 
                this.Components.OnKeyPressed.[index] {SourceId = this.Components.Id.[index]} Keys.Right

        previousKeyState <- keyState

    member this.Update (gameTime: GameTime) = 
        this.ProcessMouse(gameTime)
        this.ProcessKeys(gameTime)

    member this.Clear() =

        this.FocusedElementId <- UIComponentId.empty
       
        waitForLayout <- true

        previousFrameIndex <- frameIndex
        frameIndex <- (frameIndex + 1) % frames.Length
        this.Components.Clear()