namespace Noobish

module NoobishLayoutV2 =
    let private max0 value =
        if value < 0f then 0f else value

    let private computeBounds
        (components: NoobishComponentsV2)
        (startX: float32)
        (startY: float32)
        (availableWidth: float32)
        (availableHeight: float32)
        (index: int) =
        let minSize = components.MinSize.[index]
        let fill = components.Fill.[index]
        let margin = components.Margin.[index]

        let width =
            if fill.Horizontal then
                max minSize.Width (availableWidth - margin.Left - margin.Right)
            else
                minSize.Width

        let height =
            if fill.Vertical then
                max minSize.Height (availableHeight - margin.Top - margin.Bottom)
            else
                minSize.Height

        let bounds: Internal.NoobishRectangle = {
            X = startX + margin.Left
            Y = startY + margin.Top
            Width = max0 width
            Height = max0 height
        }

        components.Bounds.[index] <- bounds
        bounds

    let rec private layoutComponent
        (components: NoobishComponentsV2)
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
                let minHeight = components.MinSize.[childIndex].Height
                let outerMin = minHeight + margin.Top + margin.Bottom
                if components.Fill.[childIndex].Vertical then
                    fillCount <- fillCount + 1
                else
                    fixedHeight <- fixedHeight + outerMin

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
                let minHeight = components.MinSize.[childIndex].Height
                let outerMin = minHeight + margin.Top + margin.Bottom
                let outerHeight =
                    if components.Fill.[childIndex].Vertical then
                        max outerMin fillShare
                    else
                        outerMin
                let childWidth = max0 (contentWidth - margin.Left - margin.Right)
                let childHeight = max0 (outerHeight - margin.Top - margin.Bottom)
                layoutComponent components (contentX + margin.Left) (cursorY + margin.Top) childWidth childHeight childIndex
                cursorY <- cursorY + outerHeight

        | LayoutV2.LinearHorizontal ->
            let children = components.Children.[index]
            let mutable fixedWidth = 0f
            let mutable fillCount = 0
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let margin = components.Margin.[childIndex]
                let minWidth = components.MinSize.[childIndex].Width
                let outerMin = minWidth + margin.Left + margin.Right
                if components.Fill.[childIndex].Horizontal then
                    fillCount <- fillCount + 1
                else
                    fixedWidth <- fixedWidth + outerMin

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
                let minWidth = components.MinSize.[childIndex].Width
                let outerMin = minWidth + margin.Left + margin.Right
                let outerWidth =
                    if components.Fill.[childIndex].Horizontal then
                        max outerMin fillShare
                    else
                        outerMin
                let childWidth = max0 (outerWidth - margin.Left - margin.Right)
                let childHeight = max0 (contentHeight - margin.Top - margin.Bottom)
                layoutComponent components (cursorX + margin.Left) (contentY + margin.Top) childWidth childHeight childIndex
                cursorX <- cursorX + outerWidth

        | LayoutV2.Grid(cols, rows) ->
            let children = components.Children.[index]
            let cellWidth = if cols > 0 then contentWidth / float32 cols else 0f
            let cellHeight = if rows > 0 then contentHeight / float32 rows else 0f

            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let row = if cols > 0 then i / cols else 0
                let col = if cols > 0 then i % cols else 0
                let span = components.GridSpan.[childIndex]
                let margin = components.Margin.[childIndex]
                let childStartX = contentX + float32 col * cellWidth
                let childStartY = contentY + float32 row * cellHeight
                let childWidth = cellWidth * float32 span.Colspan
                let childHeight = cellHeight * float32 span.Rowspan
                let width = max0 (childWidth - margin.Left - margin.Right)
                let height = max0 (childHeight - margin.Top - margin.Bottom)
                layoutComponent components (childStartX + margin.Left) (childStartY + margin.Top) width height childIndex

        | LayoutV2.Relative _ ->
            let children = components.Children.[index]
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let margin = components.Margin.[childIndex]
                let childWidth = max0 (contentWidth - margin.Left - margin.Right)
                let childHeight = max0 (contentHeight - margin.Top - margin.Bottom)
                layoutComponent components (contentX + margin.Left) (contentY + margin.Top) childWidth childHeight childIndex

        | LayoutV2.Stack ->
            let children = components.Children.[index]
            for i = 0 to children.Count - 1 do
                let childIndex = int children.[i].Index
                let margin = components.Margin.[childIndex]
                let childWidth = max0 (contentWidth - margin.Left - margin.Right)
                let childHeight = max0 (contentHeight - margin.Top - margin.Bottom)
                layoutComponent components (contentX + margin.Left) (contentY + margin.Top) childWidth childHeight childIndex

        | LayoutV2.None ->
            ()

    let layoutFrame (components: NoobishComponentsV2) (rootWidth: float32) (rootHeight: float32) =
        for i = 0 to components.Count - 1 do
            if components.ParentId.[i] = UIComponentIdV2.empty then
                layoutComponent components 0f 0f rootWidth rootHeight i
