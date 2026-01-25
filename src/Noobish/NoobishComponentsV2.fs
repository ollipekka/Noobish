namespace Noobish

open System
open Noobish
open Noobish.Internal

[<RequireQualifiedAccess>]
type LayoutV2 =
| LinearHorizontal
| LinearVertical
| Stack
| Grid of cols: int * rows: int
| Relative of UIComponentIdV2
| None

type ComponentContextV2(components: INoobishComponents2) =
    member val Components = components with get
    member val FrameId = 0 with get, set
    member val Page = "" with get, set
    member val NamespaceId = 0us with get, set
    member val ComponentId = UIComponentIdV2.empty with get, set
    member val ParentId = UIComponentIdV2.empty with get, set

    member this.Reset(frameId: int, page: string, namespaceId: uint16) =
        this.FrameId <- frameId
        this.Page <- page
        this.NamespaceId <- namespaceId
        this.ParentId <- UIComponentIdV2.empty

and INoobishComponents2 =
    abstract AcquireContext: unit -> ComponentContextV2
    abstract ReleaseContext: ComponentContextV2 -> unit
    abstract Count: int with get, set
    abstract RunningId: int with get, set
    abstract Id: UIComponentIdV2[] with get
    abstract ThemeId: string[] with get
    abstract ParentId: UIComponentIdV2[] with get
    abstract Children: ResizeArray<UIComponentIdV2>[] with get
    abstract Visible: bool[] with get
    abstract Enabled: bool[] with get
    abstract Block: bool[] with get
    abstract Layout: LayoutV2[] with get
    abstract GridSpan: TableSpan[] with get
    abstract GridCellAlignment: NoobishAlignment[] with get
    abstract Fill: Fill[] with get
    abstract Scroll: Scroll[] with get
    abstract ScrollX: float32[] with get
    abstract ScrollY: float32[] with get
    abstract Toggled: bool[] with get
    abstract Hovered: bool[] with get
    abstract WantsToggle: bool[] with get
    abstract ContentSize: NoobishSize[] with get
    abstract Padding: NoobishPadding[] with get
    abstract PaddingOverride: bool[] with get
    abstract Margin: NoobishMargin[] with get
    abstract MarginOverride: bool[] with get
    abstract MinSize: NoobishSize[] with get
    abstract Bounds: NoobishRectangle[] with get
    abstract Layer: int[] with get
    abstract WantsText: bool[] with get
    abstract Text: string[] with get
    abstract Textwrap: bool[] with get
    abstract TextAlign: NoobishAlignment[] with get
    abstract Focused: bool[] with get
    abstract CaretIndex: int[] with get
    abstract WantsOnClick: bool[] with get
    abstract WantsOnPress: bool[] with get
    abstract WantsTextChanged: bool[] with get
    abstract WantsSlider: bool[] with get
    abstract SliderMin: float32[] with get
    abstract SliderMax: float32[] with get
    abstract SliderStep: float32[] with get
    abstract SliderValue: float32[] with get
    abstract WantsProgress: bool[] with get
    abstract ProgressValue: float32[] with get
    abstract ProgressSegments: int[] with get


/// ECS-style storage for UI components (V2).
type NoobishComponentsV2(count: int) =
    let contextPool = ResizeArray<ComponentContextV2>()

    member val Count = 0 with get, set
    member val RunningId = 0 with get, set
    member val Id = Array.create count UIComponentIdV2.empty
    member val ThemeId = Array.create count ""
    member val ParentId = Array.create count UIComponentIdV2.empty
    member val Children = Array.init count (fun _ -> ResizeArray<UIComponentIdV2>())
    member val Visible = Array.create count true
    member val Enabled = Array.create count true
    member val Block = Array.create count false
    member val Layout = Array.create count LayoutV2.None
    member val GridSpan = Array.create count ({Rowspan = 1; Colspan = 1})
    member val GridCellAlignment = Array.create count NoobishAlignment.None
    member val Fill = Array.create count {Fill.Horizontal = false; Vertical = false}
    member val Scroll = Array.create count {Scroll.Horizontal = false; Vertical = false}
    member val ScrollX = Array.create count 0f
    member val ScrollY = Array.create count 0f
    member val Toggled = Array.create count false
    member val Hovered = Array.create count false
    member val WantsToggle = Array.create count false
    member val ContentSize = Array.create count {Width = 0f; Height = 0f}
    member val Padding = Array.create count {NoobishPadding.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val PaddingOverride = Array.create count false
    member val Margin = Array.create count {NoobishMargin.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val MarginOverride = Array.create count false
    member val MinSize = Array.create count {Width = 0f; Height = 0f}
    member val Bounds = Array.create<NoobishRectangle> count {X = 0f; Y = 0f; Width = 0f; Height = 0f}
    member val Layer = Array.create count 0
    member val WantsText = Array.create count false
    member val Text = Array.create count ""
    member val Textwrap = Array.create count false
    member val TextAlign = Array.create count NoobishAlignment.None
    member val Focused = Array.create count false
    member val CaretIndex = Array.create count 0
    member val CaretBlinkStart = Array.create count TimeSpan.Zero
    member val CaretBlinkReset = Array.create count false
    member val WantsOnClick = Array.create count false
    member val WantsOnPress = Array.create count false
    member val WantsTextChanged = Array.create count false
    member val WantsSlider = Array.create count false
    member val SliderMin = Array.create count 0f
    member val SliderMax = Array.create count 0f
    member val SliderStep = Array.create count 0f
    member val SliderValue = Array.create count 0f
    member val WantsProgress = Array.create count false
    member val ProgressValue = Array.create count 0f
    member val ProgressSegments = Array.create count 1

    member private this.CreateContext() =
        ComponentContextV2(this)

    member this.AcquireContext() =
        if contextPool.Count > 0 then
            let index = contextPool.Count - 1
            let ctx = contextPool.[index]
            contextPool.RemoveAt(index)
            ctx
        else
            this.CreateContext()

    member this.ReleaseContext(ctx: ComponentContextV2) =
        contextPool.Add(ctx)

    member this.Clear() =
        for i = 0 to this.Count - 1 do
            this.Id.[i] <- UIComponentIdV2.empty
            this.ThemeId.[i] <- ""
            this.ParentId.[i] <- UIComponentIdV2.empty
            this.Children.[i].Clear()
            this.Visible.[i] <- true
            this.Enabled.[i] <- true
            this.Block.[i] <- false
            this.Layout.[i] <- LayoutV2.None
            this.GridSpan.[i] <- {Rowspan = 1; Colspan = 1}
            this.GridCellAlignment.[i] <- NoobishAlignment.None
            this.Fill.[i] <- {Horizontal = false; Vertical = false}
            this.Scroll.[i] <- {Horizontal = false; Vertical = false}
            this.Toggled.[i] <- false
            this.Hovered.[i] <- false
            this.WantsToggle.[i] <- false
            this.ContentSize.[i] <- {Width = 0f; Height = 0f}
            this.Padding.[i] <- {NoobishPadding.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
            this.PaddingOverride.[i] <- false
            this.Margin.[i] <- {NoobishMargin.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
            this.MarginOverride.[i] <- false
            this.MinSize.[i] <- {Width = 0f; Height = 0f}
            this.Layer.[i] <- 0
            this.WantsText.[i] <- false
            this.Text.[i] <- ""
            this.Textwrap.[i] <- false
            this.TextAlign.[i] <- NoobishAlignment.None
            this.Focused.[i] <- false
            this.CaretIndex.[i] <- 0
            this.CaretBlinkStart.[i] <- TimeSpan.Zero
            this.CaretBlinkReset.[i] <- false
            this.WantsOnClick.[i] <- false
            this.WantsOnPress.[i] <- false
            this.WantsTextChanged.[i] <- false
            this.WantsSlider.[i] <- false
            this.SliderMin.[i] <- 0f
            this.SliderMax.[i] <- 0f
            this.SliderStep.[i] <- 0f
            this.SliderValue.[i] <- 0f
            this.WantsProgress.[i] <- false
            this.ProgressValue.[i] <- 0f
            this.ProgressSegments.[i] <- 1
        this.Count <- 0

    interface INoobishComponents2 with
        member this.AcquireContext() = this.AcquireContext()
        member this.ReleaseContext(ctx) = this.ReleaseContext(ctx)
        member this.Count with get() = this.Count and set value = this.Count <- value
        member this.RunningId with get() = this.RunningId and set value = this.RunningId <- value
        member this.Id = this.Id
        member this.ThemeId = this.ThemeId
        member this.ParentId = this.ParentId
        member this.Children = this.Children
        member this.Visible = this.Visible
        member this.Enabled = this.Enabled
        member this.Block = this.Block
        member this.Layout = this.Layout
        member this.GridSpan = this.GridSpan
        member this.GridCellAlignment = this.GridCellAlignment
        member this.Fill = this.Fill
        member this.Scroll = this.Scroll
        member this.ScrollX = this.ScrollX
        member this.ScrollY = this.ScrollY
        member this.Toggled = this.Toggled
        member this.Hovered = this.Hovered
        member this.WantsToggle = this.WantsToggle
        member this.ContentSize = this.ContentSize
        member this.Padding = this.Padding
        member this.PaddingOverride = this.PaddingOverride
        member this.Margin = this.Margin
        member this.MarginOverride = this.MarginOverride
        member this.MinSize = this.MinSize
        member this.Bounds = this.Bounds
        member this.Layer = this.Layer
        member this.WantsText = this.WantsText
        member this.Text = this.Text
        member this.Textwrap = this.Textwrap
        member this.TextAlign = this.TextAlign
        member this.Focused = this.Focused
        member this.CaretIndex = this.CaretIndex
        member this.WantsOnClick = this.WantsOnClick
        member this.WantsOnPress = this.WantsOnPress
        member this.WantsTextChanged = this.WantsTextChanged
        member this.WantsSlider = this.WantsSlider
        member this.SliderMin = this.SliderMin
        member this.SliderMax = this.SliderMax
        member this.SliderStep = this.SliderStep
        member this.SliderValue = this.SliderValue
        member this.WantsProgress = this.WantsProgress
        member this.ProgressValue = this.ProgressValue
        member this.ProgressSegments = this.ProgressSegments

module NoobishComponentsV2 =
    let isClickable (components: NoobishComponentsV2) index =
        components.Visible.[index]
        && components.Enabled.[index]
        && components.WantsOnClick.[index]

    let isPressable (components: NoobishComponentsV2) index =
        components.Visible.[index]
        && components.Enabled.[index]
        && components.WantsOnPress.[index]

    let wantsMouse (components: NoobishComponentsV2) index =
        components.WantsToggle.[index]
        || components.WantsOnClick.[index]
        || components.WantsOnPress.[index]
        || components.WantsSlider.[index]
        || components.WantsProgress.[index]

    let wantsKeyboard (components: NoobishComponentsV2) index =
        components.WantsTextChanged.[index]
