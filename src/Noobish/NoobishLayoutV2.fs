namespace Noobish

open System.Diagnostics

module NoobishLayoutV2 =
    let private max0 value =
        if value < 0f then 0f else value

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

        let minWidth = minSize.Width + padding.Left + padding.Right
        let minHeight = minSize.Height + padding.Top + padding.Bottom
        let availableContentWidth = max0 (availableWidth - margin.Left - margin.Right - padding.Left - padding.Right)
        let availableContentHeight = max0 (availableHeight - margin.Top - margin.Bottom - padding.Top - padding.Bottom)

        let widthContent =
            if fill.Horizontal || (scroll.Horizontal && contentSize.Width > availableContentWidth) then
                max minSize.Width availableContentWidth
            else
                max minSize.Width contentSize.Width

        let width = widthContent + padding.Left + padding.Right

        let heightContent =
            if fill.Vertical || (scroll.Vertical && contentSize.Height > availableContentHeight) then
                max minSize.Height availableContentHeight
            else
                max minSize.Height contentSize.Height

        let height = heightContent + padding.Top + padding.Bottom

        let bounds: NoobishRectangle = {
            X = startX + margin.Left
            Y = startY + margin.Top
            Width = max0 width
            Height = max0 height
        }

        components.Bounds.[index] <- bounds
        bounds

    let rec private layoutComponent
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
        let contentWidth = max0 (bounds.Width - padding.Left - padding.Right)
        let contentHeight = max0 (bounds.Height - padding.Top - padding.Bottom)

        match components.Layout.[index] with
        | LayoutV2.LinearVertical ->
            let children = components.Children.[index]
            let mutable fixedHeight = 0f
            let mutable fillCount = 0
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let margin = components.Margin.[childIndex]
                let minSize = components.MinSize.[childIndex]
                let contentSize = components.ContentSize.[childIndex]
                let padding = components.Padding.[childIndex]
                let childContentHeight = max minSize.Height contentSize.Height
                let childContentHeightWithPadding = childContentHeight + padding.Top + padding.Bottom
                let minHeightWithPadding = minSize.Height + padding.Top + padding.Bottom
                let outerContent = childContentHeightWithPadding + margin.Top + margin.Bottom
                let outerMin = minHeightWithPadding + margin.Top + margin.Bottom
                let scroll = components.Scroll.[childIndex]
                let scrollOverflow = scroll.Vertical && outerContent > contentHeight
                if components.Fill.[childIndex].Vertical || scrollOverflow then
                    fillCount <- fillCount + 1
                else
                    fixedHeight <- fixedHeight + outerContent

            let remaining = max0 (contentHeight - fixedHeight)
            let fillShare =
                if fillCount > 0 then
                    remaining / float32 fillCount
                else
                    0f

            let mutable cursorY = contentY
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let margin = components.Margin.[childIndex]
                let minSize = components.MinSize.[childIndex]
                let contentSize = components.ContentSize.[childIndex]
                let padding = components.Padding.[childIndex]
                let childContentHeight = max minSize.Height contentSize.Height
                let childContentHeightWithPadding = childContentHeight + padding.Top + padding.Bottom
                let minHeightWithPadding = minSize.Height + padding.Top + padding.Bottom
                let outerContent = childContentHeightWithPadding + margin.Top + margin.Bottom
                let outerMin = minHeightWithPadding + margin.Top + margin.Bottom
                let scroll = components.Scroll.[childIndex]
                let scrollOverflow = scroll.Vertical && outerContent > contentHeight
                let outerHeight =
                    if components.Fill.[childIndex].Vertical || scrollOverflow then
                        max outerMin fillShare
                    else
                        outerContent
                let childWidth = contentWidth
                let childHeight = outerHeight
                layoutComponent components contentX cursorY childWidth childHeight childIndex
                cursorY <- cursorY + outerHeight

        | LayoutV2.LinearHorizontal ->
            let children = components.Children.[index]
            let mutable fixedWidth = 0f
            let mutable fillCount = 0
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let margin = components.Margin.[childIndex]
                let minSize = components.MinSize.[childIndex]
                let contentSize = components.ContentSize.[childIndex]
                let padding = components.Padding.[childIndex]
                let childContentWidth = max minSize.Width contentSize.Width
                let childContentWidthWithPadding = childContentWidth + padding.Left + padding.Right
                let minWidthWithPadding = minSize.Width + padding.Left + padding.Right
                let outerContent = childContentWidthWithPadding + margin.Left + margin.Right
                let outerMin = minWidthWithPadding + margin.Left + margin.Right
                let scroll = components.Scroll.[childIndex]
                let scrollOverflow = scroll.Horizontal && outerContent > contentWidth
                if components.Fill.[childIndex].Horizontal || scrollOverflow then
                    fillCount <- fillCount + 1
                else
                    fixedWidth <- fixedWidth + outerContent

            let remaining = max0 (contentWidth - fixedWidth)
            let fillShare =
                if fillCount > 0 then
                    remaining / float32 fillCount
                else
                    0f

            let mutable cursorX = contentX
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let margin = components.Margin.[childIndex]
                let minSize = components.MinSize.[childIndex]
                let contentSize = components.ContentSize.[childIndex]
                let padding = components.Padding.[childIndex]
                let childContentWidth = max minSize.Width contentSize.Width
                let childContentWidthWithPadding = childContentWidth + padding.Left + padding.Right
                let minWidthWithPadding = minSize.Width + padding.Left + padding.Right
                let outerContent = childContentWidthWithPadding + margin.Left + margin.Right
                let outerMin = minWidthWithPadding + margin.Left + margin.Right
                let scroll = components.Scroll.[childIndex]
                let scrollOverflow = scroll.Horizontal && outerContent > contentWidth
                let outerWidth =
                    if components.Fill.[childIndex].Horizontal || scrollOverflow then
                        max outerMin fillShare
                    else
                        outerContent
                let childWidth = outerWidth
                let childHeight = contentHeight
                layoutComponent components cursorX contentY childWidth childHeight childIndex
                cursorX <- cursorX + outerWidth

        | LayoutV2.Grid(cols, rows) ->
            let children = components.Children.[index]
            let cellWidth = if cols > 0 then contentWidth / float32 cols else 0f
            let cellHeight = if rows > 0 then contentHeight / float32 rows else 0f
            let occupancy = Array.create (max 0 (cols * rows)) false

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
                let colspan = if cols > 0 then min colspan cols else colspan
                let rowspan = if rows > 0 then min rowspan rows else rowspan
                let margin = components.Margin.[childIndex]
                let mutable placed = false
                let mutable row = 0
                let mutable col = 0
                if cols > 0 && rows > 0 then
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
                    row <- if cols > 0 then i / cols else 0
                    col <- if cols > 0 then i % cols else 0
                let childStartX = contentX + float32 col * cellWidth
                let childStartY = contentY + float32 row * cellHeight
                let childWidth = cellWidth * float32 colspan
                let childHeight = cellHeight * float32 rowspan
                layoutComponent components childStartX childStartY childWidth childHeight childIndex

        | LayoutV2.Relative _ ->
            let children = components.Children.[index]
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                layoutComponent components contentX contentY contentWidth contentHeight childIndex

        | LayoutV2.Stack ->
            let children = components.Children.[index]
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                layoutComponent components contentX contentY contentWidth contentHeight childIndex

        | LayoutV2.None ->
            ()

    let layoutFrame (components: INoobishComponents2) (rootWidth: float32) (rootHeight: float32) =
        for i = 0 to components.Count - 1 do
            if components.ParentId.[i] = UIComponentIdV2.empty then
                layoutComponent components 0f 0f rootWidth rootHeight i
