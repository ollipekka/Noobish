namespace Noobish

open Noobish
open Noobish.Internal
open Noobish.Styles

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
    member val ParentId = UIComponentIdV2.empty with get, set

    member this.Reset(frameId: int, page: string) =
        this.FrameId <- frameId
        this.Page <- page
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
    abstract Padding: NoobishPadding[] with get
    abstract Margin: NoobishMargin[] with get
    abstract MinSize: NoobishSize[] with get
    abstract Bounds: NoobishRectangle[] with get
    abstract Layer: int[] with get


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
    member val Padding = Array.create count {NoobishPadding.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Margin = Array.create count {NoobishMargin.Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val MinSize = Array.create count {Width = 0f; Height = 0f}
    member val Bounds = Array.create<NoobishRectangle> count {X = 0f; Y = 0f; Width = 0f; Height = 0f}
    member val Layer = Array.create count 0

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
        member this.Padding = this.Padding
        member this.Margin = this.Margin
        member this.MinSize = this.MinSize
        member this.Bounds = this.Bounds
        member this.Layer = this.Layer

