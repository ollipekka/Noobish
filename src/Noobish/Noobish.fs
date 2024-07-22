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
open Internal






type Noobish(maxCount: int) =


    let rasterizerState = 
        let rasterizerState = new RasterizerState()
        rasterizerState.ScissorTestEnable <- true
        rasterizerState

    let mutable waitForLayout = true

    let drawQueue = PriorityQueue<int, int>()

    let toLayout = PriorityQueue<int, int>()

    let frames = Array.init 2 (fun _ -> NoobishComponents(maxCount))
    
    let mutable previousFrameIndex = 0
    let mutable frameIndex = 1

    member this.Components with get() = frames.[frameIndex]

    member val Cursor = 0 with get, set 

    member val FocusedElementId = UIComponentId.empty with get, set
    member val FocusedTime = TimeSpan.Zero with get, set

    member val ScreenWidth: float32 = 0f with get, set 
    member val ScreenHeight: float32 = 0f with get, set

    member val ToProcess = PriorityQueue<int, int>() with get
    member val PreviousMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState() with get, set
    member val PreviousKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState() with get, set

    member val Debug = false with get, set

    member this.IsActive(cid: int<UIComponentId>) =
        let index = cid |> UIComponentId.index

        if index <> -1 && this.Components.Id.[index] = cid then 
            index
        else 
            -1
    member this.GetIndex(cid: int<UIComponentId>) =
        let index = cid |> UIComponentId.index

        if index <> -1 && this.Components.Id.[index] = cid then 
            index
        else 
            -1

    member this.Create(themeId: string): int<UIComponentId> =

        let i = this.Components.Count
        let cid: int<UIComponentId> = UIComponentId.create (uint16 this.Components.Count) (uint16 this.Components.RunningId)
        this.Components.Id.[i] <- cid
        this.Components.ThemeId.[i] <- themeId
        this.Components.Enabled.[i] <- true 
        this.Components.Visible.[i] <- true 
        this.Components.Layer.[i] <- 1
        this.Components.Layout.[i] <- Layout.None
        this.Components.GridSpan.[i] <- {Colspan = 1; Rowspan = 1}
        this.Components.GridCellAlignment.[i] <- NoobishAlignment.None
        this.Components.MinSize.[i] <- {Width = 0f; Height = 0f}

        this.Components.LastPressTime[i] <- TimeSpan.Zero
        
        this.Components.Count <- this.Components.Count + 1
        this.Components.RunningId <- this.Components.RunningId + 1

        cid

    member this.Overlaypane(rcid: int<UIComponentId>) =
        let cid = this.Create "Overlay"
        this.Components.Layout.[cid |> UIComponentId.index] <- Layout.Relative rcid
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        
        cid

    member this.Window() =
        let cid = this.Create "Space"
        this.SetGridLayout (16, 9) cid |> ignore
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}

        let windowId = 
            this.PanelVertical()
                |> this.SetFill

        this.SetChildren [|
            this.Space() |> this.SetColspan 16 |> this.SetRowspan 1
            this.Space() |> this.SetColspan 4 |> this.SetRowspan 8
            windowId |> this.SetColspan 8 |> this.SetRowspan 7
        |] cid |> ignore

        windowId

    member this.WindowWithGrid (cols: int, rows: int) =
        let cid = this.Create "Space"
        this.SetGridLayout (16, 9) cid |> ignore
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}

        let windowId = 
            this.PanelWithGrid(cols, rows)
                |> this.SetColspan 14 |> this.SetRowspan 5
                |> this.SetFill

        this.SetChildren [|
            this.Space() |> this.SetColspan 16 |> this.SetRowspan 1
            this.Space() |> this.SetColspan 2 |> this.SetRowspan 7
            windowId 
        |] cid |> ignore

        windowId

    member this.LargeWindowWithGrid (cols: int, rows: int) (children: int<UIComponentId>[])=
        let cid = this.Create "Space"
        this.SetGridLayout (16, 9) cid |> ignore
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}

        let windowId = 
            this.PanelWithGrid(cols, rows)
                |> this.SetColspan 14 |> this.SetRowspan 7
                |> this.SetChildren children
                |> this.SetFill

        this.SetChildren [|
            this.Space() |> this.SetColspan 16 |> this.SetRowspan 1
            this.Space() |> this.SetColspan 1 |> this.SetRowspan 7
            windowId 
        |] cid |> ignore

        windowId

    member this.Header (t: string) = 
        let cid = this.Create "Header"
        this.Components.WantsText.[cid |> UIComponentId.index] <- true
        this.Components.Text.[cid |> UIComponentId.index] <- t
        this.Components.Block.[cid |> UIComponentId.index] <- true

        cid

    member this.Label (t: string) = 
        let cid = this.Create "Label"
        this.Components.Text.[cid |> UIComponentId.index] <- t

        cid

    member this.Paragraph (t: string) = 
        let cid = this.Create "Paragraph"
        this.Components.Text.[cid |> UIComponentId.index] <- t
        this.Components.Textwrap.[cid |> UIComponentId.index] <- true 
        this.Components.TextAlign.[cid |> UIComponentId.index] <- NoobishAlignment.TopLeft 
        this.Components.Block.[cid |> UIComponentId.index] <- true 
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = false}

        cid


    member this.Textbox (t: string) (onTextChanged: string -> unit) = 
        let cid = this.Create "TextBox"

        this.Components.WantsFocus.[cid |> UIComponentId.index] <- true 
        this.Components.OnFocus.[cid |> UIComponentId.index] <- (
            fun event focus -> ()
                
        )

        this.Components.WantsKeyTyped.[cid |> UIComponentId.index] <- true 
        this.Components.OnKeyTyped.[cid |> UIComponentId.index] <- (fun event typed ->
            let text = this.Components.Text.[cid |> UIComponentId.index]
            if int typed = 8 then // backspace
                if text.Length > 0 && this.Cursor > 0 then
                    this.Components.Text.[cid |> UIComponentId.index] <- text.Remove(this.Cursor - 1, 1)
                    this.Cursor <- this.Cursor - 1
            elif int typed = 13 || int typed = 10 then 
                this.FocusedElementId <- UIComponentId.empty
                onTextChanged (this.Components.Text[cid |> UIComponentId.index])
                this.Cursor <- 0
            elif int typed = 27 then
                this.FocusedElementId <- UIComponentId.empty
                this.Components.Text.[cid |> UIComponentId.index] <- t
                this.Cursor <- 0
            elif int typed = 127 then // deleted
                if text.Length > 0 && this.Cursor < text.Length then
                    this.Components.Text.[cid |> UIComponentId.index] <- text.Remove(this.Cursor, 1)
            else
                this.Components.Text.[cid |> UIComponentId.index] <- text.Insert(this.Cursor, typed.ToString())
                this.Cursor <- this.Cursor + 1
        )
        this.Components.WantsKeyPressed.[cid |> UIComponentId.index] <- true 
        this.Components.OnKeyPressed.[cid |> UIComponentId.index] <- (fun event key ->
            let text = this.Components.Text.[cid |> UIComponentId.index]
            if key = Microsoft.Xna.Framework.Input.Keys.Left then 
                let newCursorPos = this.Cursor - 1 
                this.Cursor <- max 0 newCursorPos
            elif key = Microsoft.Xna.Framework.Input.Keys.Right then 
                let newCursorPos = this.Cursor + 1 
                this.Cursor <- min text.Length newCursorPos
        )

        this.Components.WantsText.[cid |> UIComponentId.index] <- true
        this.Components.Text.[cid |> UIComponentId.index] <- t



        cid

    member this.Button (t: string) (onClick: int<UIComponentId> -> GameTime -> unit) = 
        let cid = this.Create "Button"

        this.Components.WantsText.[cid |> UIComponentId.index] <- true
        this.Components.Text.[cid |> UIComponentId.index] <- t
        this.Components.WantsOnClick.[cid |> UIComponentId.index] <- true
        this.Components.OnClick.[cid |> UIComponentId.index] <- (fun source _ gameTime -> onClick source gameTime)
        this.Components.WantsOnPress.[cid |> UIComponentId.index] <- true 
        cid

    member this.Space () = 
        let cid = this.Create "Space"
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Div () = 
        let cid = this.Create "Division"
        this.Components.Block.[cid |> UIComponentId.index] <- true
        this.Components.Layout.[cid |> UIComponentId.index] <- (Layout.LinearVertical) 

        cid

    member this.Grid (cols: int, rows: int) = 
        let cid = this.Create "Division"
        this.SetGridLayout (cols, rows) cid |> ignore
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        cid

    member this.Panel () = 
        let cid = this.Create "Panel"
        cid   

    member this.PanelWithGrid (cols: int, rows: int) = 
        let cid = this.Create "Panel"
        this.SetGridLayout (cols, rows) cid |> ignore
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.PanelHorizontal () = 
        let cid = this.Create "Panel"
        this.Components.Layout.[cid |> UIComponentId.index] <- (Layout.LinearHorizontal) 
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.PanelVertical () = 
        let cid = this.Create "Panel"
        this.Components.Layout.[cid |> UIComponentId.index] <- (Layout.LinearVertical) 
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.DivHorizontal () = 
        let cid = this.Create "Division"
        this.Components.Layout.[cid |> UIComponentId.index] <- (Layout.LinearHorizontal) 
        this.Components.Block.[cid |> UIComponentId.index] <- true 
        //this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.DivVertical () = 
        let cid = this.Create "Division"
        this.Components.Layout.[cid |> UIComponentId.index] <- (Layout.LinearVertical) 
        this.Components.Block.[cid |> UIComponentId.index] <- true 
        //this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.Canvas () = 
        let cid = this.Create "Division"
        this.Components.Layout.[cid |> UIComponentId.index] <- (Layout.Relative (cid)) 
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        cid   

    member this.HorizontalRule () = 
        let cid = this.Create "HorizontalRule"
        this.Components.Id.[cid |> UIComponentId.index] <- cid
        this.Components.Block.[cid |> UIComponentId.index] <- true
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = false} 
        cid


    member this.SetRelativePosition (p: NoobishPosition) (cid: int<UIComponentId>) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.RelativePosition.[index] <- (RelativePosition.Position {X = p.X; Y = p.Y})
        cid

    member this.SetRelativePositionFunc (f: int<UIComponentId> -> int<UIComponentId> -> float32 -> float32 -> NoobishPosition) (cid: int<UIComponentId>) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.RelativePosition.[index] <- (RelativePosition.Func f)
        cid

    member this.SetConstrainToParentBounds (v: bool) (cid: int<UIComponentId>) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ConstrainToParentBounds.[index] <- v
        cid

    member this.SetThemeId (themeId: string) (cid: int<UIComponentId>) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ThemeId.[index] <- themeId
        cid

    member this.SetSize (s: NoobishSize) (cid: int<UIComponentId>) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.MinSizeOverride.[index] <- true
            this.Components.MinSize.[index] <- s
        cid

    member this.SetLayer (layer: int) (cid: int<UIComponentId>) = 
        let index = this.GetIndex cid 
        if index <> -1 then 

            let children = this.Components.Children.[cid |> UIComponentId.index]
            if children.Count > 0 then 
                let previousLayer = this.Components.Layer.[index]

                for j = 0 to children.Count - 1 do 
                    let ccid = children.[j]
                    let deltaLayer = this.Components.Layer.[ccid |> UIComponentId.index] - previousLayer
                    this.SetLayer (layer + deltaLayer) children.[j] |> ignore

            this.Components.Layer.[index] <- layer
        cid

    member this.SetVisible (v: bool) (cid: int<UIComponentId>): int<UIComponentId> = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Visible.[index] <- v
            
            let children = this.Components.Children.[cid |> UIComponentId.index]
            for j = 0 to children.Count - 1 do 
                this.SetVisible v children.[j] |> ignore
        cid

    member this.SetEnabled (v: bool) (cid: int<UIComponentId>) = 
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Enabled.[index] <- v
        cid

    member this.SetPosition (p: Vector2) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Bounds.[index] <- {this.Components.Bounds.[index] with X = p.X; Y = p.Y}
        cid

    member this.SetVerticalLayout (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Layout.[index] <- Layout.LinearVertical
        cid 
    member this.SetHorizontalLayout (cid: int<UIComponentId>) =

        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Layout.[index] <- Layout.LinearHorizontal
        cid 

    member this.SetGridLayout (cols: int, rows: int) (cid: int<UIComponentId>): int<UIComponentId> =

        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Layout[cid |> UIComponentId.index] <- Layout.Grid (cols, rows)

            let gridCells = this.Components.GridCells.[index]
            gridCells.Clear()
        cid


    member this.SetGridCellAlignment (a: NoobishAlignment) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.GridCellAlignment.[index] <- a
        cid


    member this.SetRowspan (rowspan: int) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.GridSpan.[index] <- {this.Components.GridSpan.[index] with Rowspan = rowspan}
        cid

    member this.SetColspan (colspan: int) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.GridSpan.[index] <- {this.Components.GridSpan.[index] with Colspan = colspan}
        cid



    member this.SetMargin (margin: int) (cid: int<UIComponentId>) =
        let margin = float32 margin
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.MarginOverride.[index] <- true
            this.Components.Margin.[index] <- {Top = margin; Right = margin; Bottom = margin; Left = margin}
        cid

    member this.SetPadding (padding: int) (cid: int<UIComponentId>) =
        let padding = float32 padding
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.PaddingOverride.[index] <- true
            this.Components.Padding.[index] <- {Top = padding; Right = padding; Bottom = padding; Left = padding}
        cid


    member this.FillHorizontal (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Horizontal = true}
        cid

    member this.SetFillHorizontal (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Horizontal = true}
        cid

    member this.FillVertical (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {this.Components.Fill.[index] with Vertical = true}
        cid

    member this.SetFill (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Fill.[index] <- {Horizontal = true; Vertical = true}
        cid

    member this.SetScrollHorizontal (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            if this.Components.Layout.[index] = Layout.None then 
                failwith "Layout can't be none when scroll is set."
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Horizontal = true}
        cid

    member this.SetScrollVertical (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            if this.Components.Layout.[index] = Layout.None then 
                failwith "Layout can't be none when scroll is set."
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Vertical = true}
        cid


    member this.SetScroll (horizontal: bool) (vertical: bool) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            if this.Components.Layout.[index] = Layout.None then 
                failwith "Layout can't be none when scroll is set."
            this.Components.Scroll.[index] <- {this.Components.Scroll.[index] with Horizontal = horizontal; Vertical = vertical}
        cid

    member this.SetText (text: string) (cid: int<UIComponentId>) =
        let index: int = this.GetIndex cid 
        if index <> -1 then 
            this.Components.WantsText.[cid |> UIComponentId.index] <- true
            this.Components.Text.[index] <- text
        cid

    member this.SetTextAlign (align: NoobishAlignment) (cid: int<UIComponentId>) =
        let index: int = this.GetIndex cid 
        if index <> -1 then 
            this.Components.TextAlignOverride.[index] <- true 
            this.Components.TextAlign.[index] <- align
        cid

    member this.AlignText (textAlignment: NoobishAlignment) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.TextAlign.[index] <- textAlignment
        cid       

    member this.AlignTextTopLeft (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.TopLeft cid

    member this.AlignTextTop (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.TopCenter cid

    member this.AlignTextTopRight (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.TopRight cid

    member this.AlignTextLeft (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.Left cid

    member this.AlignTextCenter (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.Center cid

    member this.AlignTextRight (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.Right cid

    member this.AlignTextBottomLeft (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.BottomLeft cid

    member this.AlignTextBottomCenter (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.BottomCenter cid

    member this.AlignTextBottomRight (cid: int<UIComponentId>) = 
        this.AlignText NoobishAlignment.BottomRight cid

    member this.Image(): int<UIComponentId> = 
        let cid = this.Create "Image"
        this.Components.Layout.[cid |> UIComponentId.index] <- Layout.Relative cid
        cid 

    member this.SetImage (textureId: NoobishTextureId) (cid: int<UIComponentId>) =
        let index: int = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Image.[index] <- ValueSome textureId
        cid

    member this.SetImageSize (imageSize: NoobishImageSize) (cid: int<UIComponentId>) =
        let index: int = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ImageSize.[index] <- imageSize
        cid

    member this.SetImageColor (color: Color) (cid: int<UIComponentId>) =
        let index: int = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ImageColorOverride.[index] <- true
            this.Components.ImageColor.[index] <- color
        cid

    member this.SetImageTextureEffect (t: NoobishTextureEffect) (cid: int<UIComponentId>) =
        let index: int = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ImageTextureEffect.[index] <- t
        cid

    member this.SetImageAlign (align: NoobishAlignment) (cid: int<UIComponentId>) =
        let index: int = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ImageAlign.[index] <- align
        cid

    member this.SetImageRotation (phi: float32) (cid: int<UIComponentId>) =
        let index: int = this.GetIndex cid 
        if index <> -1 then 
            this.Components.ImageRotation.[index] <- phi
        cid
  
    member this.SetToggled (t: bool) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.Toggled.[index] <- t
        cid

    member this.SetOnPress (onPress: int<UIComponentId> -> NoobishPosition -> GameTime -> unit) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.WantsOnPress.[index] <- true
            this.Components.OnPress.[index] <- onPress
        cid

    member this.SetOnClick (onClick: int<UIComponentId> -> NoobishPosition -> GameTime -> unit) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index <> -1 then 
            this.Components.WantsOnClick.[index] <- true
            this.Components.OnClick.[index] <- onClick
        cid


    member this.BumpLayer2 (layer: int) (cid: int<UIComponentId>) = 
            let childLayer = this.Components.Layer.[cid |> UIComponentId.index] + layer
            this.Components.Layer.[cid |> UIComponentId.index] <- childLayer
            this.BumpLayer childLayer this.Components.Children.[cid |> UIComponentId.index]

    member this.BumpLayer (layer: int) (children: IReadOnlyList<int<UIComponentId>>) = 
        for i = 0 to children.Count - 1 do 
            let cid = children.[i]
            let childLayer = this.Components.Layer.[cid |> UIComponentId.index] + layer
            this.Components.Layer.[cid |> UIComponentId.index] <- childLayer
            this.BumpLayer childLayer this.Components.Children.[cid |> UIComponentId.index]

    member this.SetChildren (cs: int<UIComponentId>[]) (cid: int<UIComponentId>) =
        let index = this.GetIndex cid 
        if index > -1 then 
            for i = 0 to cs.Length - 1 do 
                let childId = cs[i]
                if this.Components.ParentId.[childId |> UIComponentId.index] <> UIComponentId.empty then failwith "what"
                this.Components.ParentId.[childId |> UIComponentId.index] <- cid
            let childrenIds = this.Components.Children.[index]
            childrenIds.AddRange cs

            let layer = this.Components.Layer.[index]
            this.BumpLayer layer (childrenIds :> IReadOnlyList<int<UIComponentId>>)
        cid

    member this.AddChild (childId: int<UIComponentId>) (cid: int<UIComponentId>) = 
        let index = this.GetIndex cid 
        if index > -1 then 

            if this.Components.ParentId.[childId |> UIComponentId.index] <> UIComponentId.empty then failwith "what"
            this.Components.ParentId.[childId |> UIComponentId.index] <- cid
            let childrenIds = this.Components.Children.[index]
            childrenIds.Add childId

        
            let layer = this.Components.Layer.[index]
            this.BumpLayer2 layer childId
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
            if this.Components.WantsFocus.[i] && this.FocusedElementId = this.Components.Id.[i] then 
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
            let layer = 1f - float32 (layer + 5) / 32768.0f

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

            let layer = (1f - float32 (layer + 1) / 32768.0f)

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


    member this.DrawImage (graphics: GraphicsDevice) (content: ContentManager) (spriteBatch: SpriteBatch) (textureId: NoobishTextureId) (boundsWithMarginAndPadding: NoobishRectangle) (gameTime: GameTime) (scrollX: float32) (scrollY: float32) (i) =
        let old = graphics.ScissorRectangle
        let rotatedBoundnds = DrawUI.toRotatedRectangle boundsWithMarginAndPadding this.Components.ImageRotation.[i]
        graphics.ScissorRectangle <- rotatedBoundnds
        spriteBatch.Begin(rasterizerState = rasterizerState, samplerState = SamplerState.PointClamp)    
        let layer = 1f - float32 this.Components.Layer.[i] / 32768.0f

        let textureEffect = DrawUI.getTextureEfffect this.Components.ImageTextureEffect.[i] 
        let imageColor = this.Components.ImageColor.[i]
        let imageAlign = this.Components.ImageAlign.[i]
        let imageSize = this.Components.ImageSize.[i]
        let imageRotation = this.Components.ImageRotation.[i]


        match textureId with
        | NoobishTextureId.Basic(textureId) ->
            let texture = content.Load<Texture2D> textureId

            let sourceRect = Rectangle(0, 0, texture.Width, texture.Height)
            let rect = DrawUI.calculateImageBounds imageSize imageAlign boundsWithMarginAndPadding texture.Width texture.Height scrollX scrollY

            let origin = Vector2(float32 sourceRect.Width / 2.0f, float32 sourceRect.Height / 2.0f)

            let startPos = Vector2(float32 rect.X + float32 rect.Width / 2.0f, float32 rect.Y + float32 rect.Height / 2.0f)
            let scale = Vector2(float32 rect.Width / float32 texture.Width, float32 rect.Height / float32 texture.Height)

            spriteBatch.Draw(
                texture,
                startPos,
                sourceRect,
                imageColor,
                imageRotation,
                origin,
                scale,
                textureEffect,
                layer)
        | NoobishTextureId.Atlas(aid, tid) ->

            let atlas = content.Load<NoobishTextureAtlas> aid
            let texture = atlas.[tid]

            let rect = DrawUI.calculateImageBounds imageSize imageAlign boundsWithMarginAndPadding texture.Width texture.Height scrollX scrollY

            let origin = Vector2(float32 texture.Width / 2.0f, float32 texture.Height / 2.0f)


            let startPos = Vector2(float32 rect.X + float32 rect.Width / 2.0f, float32 rect.Y + float32 rect.Height / 2.0f)
            let scale = Vector2(float32 rect.Width / float32 texture.Width, float32 rect.Height / float32 texture.Height)

            spriteBatch.Draw(
                texture.Atlas,
                startPos,
                texture.SourceRectangle,
                imageColor,
                imageRotation,
                origin,
                scale,
                textureEffect,
                layer)

        | NoobishTextureId.NinePatch (aid, tid) ->

            let atlas = content.Load<NoobishTextureAtlas> aid
            let texture = atlas.[tid]

            let rect = DrawUI.calculateImageBounds imageSize imageAlign boundsWithMarginAndPadding texture.Width texture.Height scrollX scrollY


            spriteBatch.DrawAtlasNinePatch(
                texture,
                Vector2(float32 rect.X, float32 rect.Y),
                (float32) rect.Width,
                (float32) rect.Height,
                imageColor,
                imageRotation,
                Vector2.One,
                textureEffect,
                layer)
        spriteBatch.End()
        graphics.ScissorRectangle <- old

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

                if this.FocusedElementId = this.Components.Id[i] then 
                    this.DrawCursor styleSheet content textureAtlas spriteBatch i gameTime 0f 0f

                this.DrawScrollBars styleSheet textureAtlas spriteBatch gameTime parentScrollX parentScrollY i 
                spriteBatch.End()

                match this.Components.Image.[i] with 
                | ValueSome (t) ->
                    this.DrawImage graphics content spriteBatch t boundsWithMarginAndPadding gameTime parentScrollX parentScrollY i 
                | ValueNone -> ()

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

                    match this.Components.Image.[i] with 
                    | ValueSome (t) ->
                        let b = DrawUI.toRotatedRectangle bounds this.Components.ImageRotation.[i]
                        DrawUI.drawRectangle spriteBatch pixel (Color.Multiply(Color.Purple, 0.5f)) (float32 b.X) (float32 b.Y) (float32 b.Width) (float32 b.Height)
                    | ValueNone -> ()

                spriteBatch.End()

                let scrollX = parentScrollX + this.Components.ScrollX.[i]
                let scrollY = parentScrollY + this.Components.ScrollY.[i]


                let children = this.Components.Children.[i]
                if children.Count > 0 then 
                    for j = 0 to children.Count - 1 do 
                        this.DrawComponent graphics content spriteBatch textBatch styleSheet boundsWithMargin scrollX scrollY (children.[j] |> UIComponentId.index) gameTime 
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
                   ((this.Components.ParentId.[j] |> UIComponentId.index) = (previousFrame.ParentId.[j] |> UIComponentId.index)) &&
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

                if not this.Components.TextAlignOverride.[i] then 
                    let textAlign = styleSheet.GetTextAlignment themeId "default"
                    this.Components.TextAlign.[i] <- textAlign

                if not this.Components.MarginOverride.[i] then
                    this.Components.Margin.[i] <- styleSheet.GetMargin themeId "default"

                if not this.Components.PaddingOverride.[i] then 
                    this.Components.Padding.[i] <- styleSheet.GetPadding themeId "default"


                if not this.Components.MinSizeOverride.[i] then 
                    this.Components.MinSize.[i] <- {
                        Width = styleSheet.GetWidth themeId "default"
                        Height = styleSheet.GetHeight  themeId "default"
                    }

                if not this.Components.ImageColorOverride.[i] then 
                    this.Components.ImageColor.[i] <- styleSheet.GetColor themeId "default"

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

    member this.Clear() =

        this.FocusedElementId <- UIComponentId.empty
       
        waitForLayout <- true

        previousFrameIndex <- frameIndex
        frameIndex <- (frameIndex + 1) % frames.Length
        this.Components.Clear()