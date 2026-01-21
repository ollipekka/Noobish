namespace Noobish

open System
open System.Buffers
open System.Diagnostics

module NoobishLayoutV2 =
    let internal computeAvailableContent (availableWidth: float32) (availableHeight: float32) (margin: NoobishMargin) (padding: NoobishPadding) =
        let width = Internal.max0 (availableWidth - margin.Left - margin.Right - padding.Left - padding.Right)
        let height = Internal.max0 (availableHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom)
        struct(width, height)

    let internal resolveContentSize
        (minSize: NoobishSize)
        (contentSize: NoobishSize)
        (fill: Fill)
        (scroll: Scroll)
        (availableContentWidth: float32)
        (availableContentHeight: float32) =
        let widthContent =
            if fill.Horizontal || (scroll.Horizontal && contentSize.Width > availableContentWidth) then
                max minSize.Width availableContentWidth
            else
                max minSize.Width contentSize.Width
        let heightContent =
            if fill.Vertical || (scroll.Vertical && contentSize.Height > availableContentHeight) then
                max minSize.Height availableContentHeight
            else
                max minSize.Height contentSize.Height
        struct(widthContent, heightContent)

    let internal computeOuterSizeVertical
        (minSize: NoobishSize)
        (contentSize: NoobishSize)
        (padding: NoobishPadding)
        (margin: NoobishMargin) =
        let childContentHeight = max minSize.Height contentSize.Height
        let childContentHeightWithPadding = childContentHeight + padding.Top + padding.Bottom
        let minHeightWithPadding = minSize.Height + padding.Top + padding.Bottom
        let outerContent = childContentHeightWithPadding + margin.Top + margin.Bottom
        let outerMin = minHeightWithPadding + margin.Top + margin.Bottom
        struct(outerContent, outerMin)

    let internal computeOuterSizeHorizontal
        (minSize: NoobishSize)
        (contentSize: NoobishSize)
        (padding: NoobishPadding)
        (margin: NoobishMargin) =
        let childContentWidth = max minSize.Width contentSize.Width
        let childContentWidthWithPadding = childContentWidth + padding.Left + padding.Right
        let minWidthWithPadding = minSize.Width + padding.Left + padding.Right
        let outerContent = childContentWidthWithPadding + margin.Left + margin.Right
        let outerMin = minWidthWithPadding + margin.Left + margin.Right
        struct(outerContent, outerMin)

    let internal shouldFill (fillFlag: bool) (scrollFlag: bool) (outerContent: float32) (availableContent: float32) =
        fillFlag || (scrollFlag && outerContent > availableContent)

    let internal computeFillShare (remaining: float32) (fillCount: int) =
        if fillCount > 0 then
            remaining / float32 fillCount
        else
            0f

    let private computeBounds
        (components: INoobishComponents2)
        (startX: float32)
        (startY: float32)
        (availableWidth: float32)
        (availableHeight: float32)
        (index: int) =
        let minSize = components.MinSize.[index]
        let contentSize = components.ContentSize.[index]
        let fill = components.Fill.[index]
        let scroll = components.Scroll.[index]
        let margin = components.Margin.[index]
        let padding = components.Padding.[index]

        let struct(availableContentWidth, availableContentHeight) = computeAvailableContent availableWidth availableHeight margin padding
        let struct(widthContent, heightContent) = resolveContentSize minSize contentSize fill scroll availableContentWidth availableContentHeight
        let width = widthContent + padding.Left + padding.Right
        let height = heightContent + padding.Top + padding.Bottom

        let bounds: NoobishRectangle = {
            X = startX + margin.Left
            Y = startY + margin.Top
            Width = Internal.max0 width
            Height = Internal.max0 height
        }

        components.Bounds.[index] <- bounds
        bounds

    let rec internal layoutComponent
        (components: INoobishComponents2)
        (startX: float32)
        (startY: float32)
        (availableWidth: float32)
        (availableHeight: float32)
        (index: int) =
        let bounds = computeBounds components startX startY availableWidth availableHeight index
        let padding = components.Padding.[index]
        let contentX = bounds.X + padding.Left
        let contentY = bounds.Y + padding.Top
        let contentWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let contentHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)

        match components.Layout.[index] with
        | LayoutV2.LinearVertical ->
            layoutLinearVerticalWithContent components contentX contentY contentWidth contentHeight index
        | LayoutV2.LinearHorizontal ->
            layoutLinearHorizontalWithContent components contentX contentY contentWidth contentHeight index
        | LayoutV2.Grid _ ->
            layoutGridWithContent components contentX contentY contentWidth contentHeight index
        | LayoutV2.Relative _ ->
            layoutRelativeWithContent components contentX contentY contentWidth contentHeight index
        | LayoutV2.Stack ->
            layoutStackWithContent components contentX contentY contentWidth contentHeight index
        | LayoutV2.None ->
            ()

    and private layoutLinearVerticalWithContent
        (components: INoobishComponents2)
        (contentX: float32)
        (contentY: float32)
        (contentWidth: float32)
        (contentHeight: float32)
        (index: int) =
        let children = components.Children.[index]
        let mutable fixedHeight = 0f
        let mutable fillCount = 0
        for i = 0 to children.Count - 1 do
            let childIndex = int children.[i].Index
            let margin = components.Margin.[childIndex]
            let minSize = components.MinSize.[childIndex]
            let contentSize = components.ContentSize.[childIndex]
            let padding = components.Padding.[childIndex]
            let struct(outerContent, _outerMin) = computeOuterSizeVertical minSize contentSize padding margin
            let scroll = components.Scroll.[childIndex]
            let fillVertical = shouldFill components.Fill.[childIndex].Vertical scroll.Vertical outerContent contentHeight
            if fillVertical then
                fillCount <- fillCount + 1
            else
                fixedHeight <- fixedHeight + outerContent

        let remaining = Internal.max0 (contentHeight - fixedHeight)
        let fillShare = computeFillShare remaining fillCount

        let mutable cursorY = contentY
        for i = 0 to children.Count - 1 do
            let childIndex = int children.[i].Index
            let margin = components.Margin.[childIndex]
            let minSize = components.MinSize.[childIndex]
            let contentSize = components.ContentSize.[childIndex]
            let padding = components.Padding.[childIndex]
            let struct(outerContent, outerMin) = computeOuterSizeVertical minSize contentSize padding margin
            let scroll = components.Scroll.[childIndex]
            let outerHeight =
                if shouldFill components.Fill.[childIndex].Vertical scroll.Vertical outerContent contentHeight then
                    max outerMin fillShare
                else
                    outerContent
            let childWidth = contentWidth
            let childHeight = outerHeight
            layoutComponent components contentX cursorY childWidth childHeight childIndex
            cursorY <- cursorY + outerHeight

    and private layoutLinearHorizontalWithContent
        (components: INoobishComponents2)
        (contentX: float32)
        (contentY: float32)
        (contentWidth: float32)
        (contentHeight: float32)
        (index: int) =
        let children = components.Children.[index]
        let mutable fixedWidth = 0f
        let mutable fillCount = 0
        for i = 0 to children.Count - 1 do
            let childIndex = int children.[i].Index
            let margin = components.Margin.[childIndex]
            let minSize = components.MinSize.[childIndex]
            let contentSize = components.ContentSize.[childIndex]
            let padding = components.Padding.[childIndex]
            let struct(outerContent, _outerMin) = computeOuterSizeHorizontal minSize contentSize padding margin
            let scroll = components.Scroll.[childIndex]
            let fillHorizontal = shouldFill components.Fill.[childIndex].Horizontal scroll.Horizontal outerContent contentWidth
            if fillHorizontal then
                fillCount <- fillCount + 1
            else
                fixedWidth <- fixedWidth + outerContent

        let remaining = Internal.max0 (contentWidth - fixedWidth)
        let fillShare = computeFillShare remaining fillCount

        let mutable cursorX = contentX
        for i = 0 to children.Count - 1 do
            let childIndex = int children.[i].Index
            let margin = components.Margin.[childIndex]
            let minSize = components.MinSize.[childIndex]
            let contentSize = components.ContentSize.[childIndex]
            let padding = components.Padding.[childIndex]
            let struct(outerContent, outerMin) = computeOuterSizeHorizontal minSize contentSize padding margin
            let scroll = components.Scroll.[childIndex]
            let outerWidth =
                if shouldFill components.Fill.[childIndex].Horizontal scroll.Horizontal outerContent contentWidth then
                    max outerMin fillShare
                else
                    outerContent
            let childWidth = outerWidth
            let childHeight = contentHeight
            layoutComponent components cursorX contentY childWidth childHeight childIndex
            cursorX <- cursorX + outerWidth

    and private layoutGridWithContent
        (components: INoobishComponents2)
        (contentX: float32)
        (contentY: float32)
        (contentWidth: float32)
        (contentHeight: float32)
        (index: int) =
        let cols, rows =
            match components.Layout.[index] with
            | LayoutV2.Grid(cols, rows) -> cols, rows
            | _ -> invalidArg "index" "Grid layout requires Grid layout type."
        if cols <= 0 || rows <= 0 then
            invalidArg "cols" "Grid layout requires positive columns and rows."
        let children = components.Children.[index]
        let cellWidth = contentWidth / float32 cols
        let cellHeight = contentHeight / float32 rows
        let cellCount = cols * rows
        let occupancy = ArrayPool<bool>.Shared.Rent cellCount
        try
            Array.Clear(occupancy, 0, cellCount)

            let canPlace row col colspan rowspan =
                if row < 0 || col < 0 || row + rowspan > rows || col + colspan > cols then
                    false
                else
                    let mutable ok = true
                    let mutable r = row
                    while ok && r < row + rowspan do
                        let mutable c = col
                        while ok && c < col + colspan do
                            if occupancy.[r * cols + c] then
                                ok <- false
                            c <- c + 1
                        r <- r + 1
                    ok

            let mark row col colspan rowspan =
                let mutable r = row
                while r < row + rowspan do
                    let mutable c = col
                    while c < col + colspan do
                        occupancy.[r * cols + c] <- true
                        c <- c + 1
                    r <- r + 1

            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let span = components.GridSpan.[childIndex]
                Debug.Assert(components.Fill.[childIndex].Horizontal && components.Fill.[childIndex].Vertical, "Grid children must fill horizontally and vertically.")
                let colspan = max 1 span.Colspan
                let rowspan = max 1 span.Rowspan
                let colspan = min colspan cols
                let rowspan = min rowspan rows
                let mutable placed = false
                let mutable row = 0
                let mutable col = 0
                let mutable r = 0
                while not placed && r < rows do
                    let mutable c = 0
                    while not placed && c < cols do
                        if canPlace r c colspan rowspan then
                            row <- r
                            col <- c
                            mark r c colspan rowspan
                            placed <- true
                        c <- c + 1
                    r <- r + 1
                if not placed then
                    row <- i / cols
                    col <- i % cols
                let childStartX = contentX + float32 col * cellWidth
                let childStartY = contentY + float32 row * cellHeight
                let childWidth = cellWidth * float32 colspan
                let childHeight = cellHeight * float32 rowspan
                layoutComponent components childStartX childStartY childWidth childHeight childIndex
        finally
            ArrayPool<bool>.Shared.Return occupancy

    and private layoutRelativeWithContent
        (components: INoobishComponents2)
        (contentX: float32)
        (contentY: float32)
        (contentWidth: float32)
        (contentHeight: float32)
        (index: int) =
        let children = components.Children.[index]
        for i = 0 to children.Count - 1 do
            let childIndex = int children.[i].Index
            layoutComponent components contentX contentY contentWidth contentHeight childIndex

    and private layoutStackWithContent
        (components: INoobishComponents2)
        (contentX: float32)
        (contentY: float32)
        (contentWidth: float32)
        (contentHeight: float32)
        (index: int) =
        let children = components.Children.[index]
        for i = 0 to children.Count - 1 do
            let childIndex = int children.[i].Index
            layoutComponent components contentX contentY contentWidth contentHeight childIndex

    let internal layoutLinearVertical
        (components: INoobishComponents2)
        (startX: float32)
        (startY: float32)
        (availableWidth: float32)
        (availableHeight: float32)
        (index: int) =
        let bounds = computeBounds components startX startY availableWidth availableHeight index
        let padding = components.Padding.[index]
        let contentX = bounds.X + padding.Left
        let contentY = bounds.Y + padding.Top
        let contentWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let contentHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        layoutLinearVerticalWithContent components contentX contentY contentWidth contentHeight index

    let internal layoutLinearHorizontal
        (components: INoobishComponents2)
        (startX: float32)
        (startY: float32)
        (availableWidth: float32)
        (availableHeight: float32)
        (index: int) =
        let bounds = computeBounds components startX startY availableWidth availableHeight index
        let padding = components.Padding.[index]
        let contentX = bounds.X + padding.Left
        let contentY = bounds.Y + padding.Top
        let contentWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let contentHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        layoutLinearHorizontalWithContent components contentX contentY contentWidth contentHeight index

    let internal layoutGrid
        (components: INoobishComponents2)
        (startX: float32)
        (startY: float32)
        (availableWidth: float32)
        (availableHeight: float32)
        (index: int) =
        let bounds = computeBounds components startX startY availableWidth availableHeight index
        let padding = components.Padding.[index]
        let contentX = bounds.X + padding.Left
        let contentY = bounds.Y + padding.Top
        let contentWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let contentHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        layoutGridWithContent components contentX contentY contentWidth contentHeight index

    let internal layoutRelative
        (components: INoobishComponents2)
        (startX: float32)
        (startY: float32)
        (availableWidth: float32)
        (availableHeight: float32)
        (index: int) =
        let bounds = computeBounds components startX startY availableWidth availableHeight index
        let padding = components.Padding.[index]
        let contentX = bounds.X + padding.Left
        let contentY = bounds.Y + padding.Top
        let contentWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let contentHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        layoutRelativeWithContent components contentX contentY contentWidth contentHeight index

    let internal layoutStack
        (components: INoobishComponents2)
        (startX: float32)
        (startY: float32)
        (availableWidth: float32)
        (availableHeight: float32)
        (index: int) =
        let bounds = computeBounds components startX startY availableWidth availableHeight index
        let padding = components.Padding.[index]
        let contentX = bounds.X + padding.Left
        let contentY = bounds.Y + padding.Top
        let contentWidth = Internal.max0 (bounds.Width - padding.Left - padding.Right)
        let contentHeight = Internal.max0 (bounds.Height - padding.Top - padding.Bottom)
        layoutStackWithContent components contentX contentY contentWidth contentHeight index

    let layoutFrame (components: INoobishComponents2) (rootWidth: float32) (rootHeight: float32) =
        for i = 0 to components.Count - 1 do
            if components.ParentId.[i] = UIComponentIdV2.empty then
                layoutComponent components 0f 0f rootWidth rootHeight i
