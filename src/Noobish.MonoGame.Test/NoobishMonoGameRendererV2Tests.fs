module Noobish.MonoGame.Test.NoobishMonoGameRendererV2Tests

open System
open System.Collections.Generic
open NUnit.Framework
open Noobish
open Noobish.Styles
open Noobish.TextureAtlas
open Noobish.MonoGame.Test.MockRenderContext

let private emptyThemeDict<'T>() : IReadOnlyDictionary<string, IReadOnlyDictionary<string, 'T>> =
    Dictionary<string, IReadOnlyDictionary<string, 'T>>() :> IReadOnlyDictionary<_, _>

let private toInner (entries: (string * 'T) list) =
    let dict = Dictionary<string, 'T>()
    for (key, value) in entries do
        dict.[key] <- value
    dict :> IReadOnlyDictionary<_, _>

let private toThemes (entries: (string * (string * 'T) list) list) =
    let dict = Dictionary<string, IReadOnlyDictionary<string, 'T>>()
    for (themeId, values) in entries do
        dict.[themeId] <- toInner values
    dict :> IReadOnlyDictionary<_, _>

let private createStyleSheet (drawables: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishDrawable[]>>) =
    { Name = "Test"
      TextureAtlasId = "Atlas"
      Widths = emptyThemeDict<float32>()
      Heights = emptyThemeDict<float32>()
      Paddings = emptyThemeDict<NoobishPadding>()
      Margins = emptyThemeDict<NoobishMargin>()
      Colors = emptyThemeDict<NoobishColor>()
      Fonts = emptyThemeDict<string>()
      FontSizes = emptyThemeDict<int>()
      FontColors = emptyThemeDict<NoobishColor>()
      TextAlignments = emptyThemeDict<NoobishAlignment>()
      Drawables = drawables }

let private createStyleSheetWithWidths
    (drawables: IReadOnlyDictionary<string, IReadOnlyDictionary<string, NoobishDrawable[]>>)
    (widths: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>) =
    { Name = "Test"
      TextureAtlasId = "Atlas"
      Widths = widths
      Heights = emptyThemeDict<float32>()
      Paddings = emptyThemeDict<NoobishPadding>()
      Margins = emptyThemeDict<NoobishMargin>()
      Colors = emptyThemeDict<NoobishColor>()
      Fonts = emptyThemeDict<string>()
      FontSizes = emptyThemeDict<int>()
      FontColors = emptyThemeDict<NoobishColor>()
      TextAlignments = emptyThemeDict<NoobishAlignment>()
      Drawables = drawables }

let private createAtlas () =
    { Name = "Atlas"
      Textures = Dictionary<string, NoobishTexture>() :> IReadOnlyDictionary<_, _> }

let private createGlyphWithAdvance c advance =
    {
        Unicode = c
        Advance = advance
        AtlasBounds = struct(1f, 1f, 0f, 0f)
        PlaneBounds = struct(1f, 1f, 0f, 0f)
        Kerning = Dictionary<char, float32>() :> IReadOnlyDictionary<_, _>
    }

let private createFont () =
    let glyphs = Dictionary<char, NoobishGlyph>()
    for c in [| 'x'; '*' |] do
        glyphs.[c] <- createGlyphWithAdvance c 1f
    for c in [| 'c'; 'a'; 't' |] do
        glyphs.[c] <- createGlyphWithAdvance c 5f
    {
      Atlas =
        { FontType = "Test"
          DistanceRange = 2f
          Size = 1f
          Width = 1
          Height = 1
          yOrigin = "top" }
      Metrics =
        { EmSize = 1
          LineHeight = 1f
          Ascender = 0f
          Descender = 0f
          UnderlineY = 0f
          UnderlineThickness = 0f }
      Glyphs = glyphs :> IReadOnlyDictionary<_, _>
      Kerning = Dictionary<char, IReadOnlyDictionary<char, float32>>() :> IReadOnlyDictionary<_, _> }

let private createFontMap () =
    let fonts = Dictionary<string, NoobishMonoGameFont>()
    fonts.["Font"] <- { Font = createFont(); Texture = Unchecked.defaultof<_> }
    fonts :> IReadOnlyDictionary<_, _>

[<Test>]
let ``renderer draws background for visible component`` () =
    let drawables =
        toThemes [ ("Button", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 20f; Height = 10f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Button"
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let commands = ctx.Commands |> Seq.toList
    let drawableCount =
        commands
        |> List.filter (function RenderCommand.Drawable _ -> true | _ -> false)
        |> List.length
    Assert.AreEqual(1, drawableCount)

[<Test>]
let ``renderer draws slider track and pin`` () =
    let drawables =
        toThemes
            [ ("Slider", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("SliderPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 20f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Slider"
    components.WantsSlider.[0] <- true
    components.SliderMin.[0] <- 0f
    components.SliderMax.[0] <- 10f
    components.SliderValue.[0] <- 5f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 200, 50, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let drawablesCount =
        ctx.Commands
        |> Seq.filter (function RenderCommand.Drawable _ -> true | _ -> false)
        |> Seq.length
    Assert.AreEqual(2, drawablesCount)

[<Test>]
let ``renderer draws progress segments and fills`` () =
    let drawables =
        toThemes
            [ ("ProgressBar", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ])
              ("ProgressBar-Dash", [ ("default", [| NoobishDrawable.NinePatch "dash" |]) ])
              ("ProgressBar-Progress", [ ("default", [| NoobishDrawable.NinePatch "fill" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 10f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "ProgressBar"
    components.WantsProgress.[0] <- true
    components.ProgressStyle.[0] <- NoobishProgressStyle.Bar
    components.ProgressSegments.[0] <- 2
    components.ProgressValue.[0] <- 0.6f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 200, 50, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let drawablesCount =
        ctx.Commands
        |> Seq.filter (function RenderCommand.Drawable _ -> true | _ -> false)
        |> Seq.length
    Assert.AreEqual(5, drawablesCount)

[<Test>]
let ``renderer draws progress fill when segments are not enabled`` () =
    let drawables =
        toThemes
            [ ("ProgressBar", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ])
              ("ProgressBar-Progress", [ ("default", [| NoobishDrawable.NinePatch "fill" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 10f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "ProgressBar"
    components.WantsProgress.[0] <- true
    components.ProgressStyle.[0] <- NoobishProgressStyle.Bar
    components.ProgressSegments.[0] <- 1
    components.ProgressValue.[0] <- 0.5f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 200, 50, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let drawablesCount =
        ctx.Commands
        |> Seq.filter (function RenderCommand.Drawable _ -> true | _ -> false)
        |> Seq.length
    Assert.AreEqual(2, drawablesCount)

[<Test>]
let ``renderer draws radial progress using triangles`` () =
    let drawables =
        toThemes
            [ ("ProgressBar", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 64f; Height = 64f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "ProgressBar"
    components.WantsProgress.[0] <- true
    components.ProgressStyle.[0] <- NoobishProgressStyle.Radial
    components.ProgressValue.[0] <- 0.5f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 128, 128, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let triangleCount =
        ctx.Commands
        |> Seq.filter (function RenderCommand.Triangle _ -> true | _ -> false)
        |> Seq.length
    Assert.Greater(triangleCount, 0)

[<Test>]
let ``renderer draws square radial progress using triangles`` () =
    let drawables =
        toThemes
            [ ("ProgressBar", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 64f; Height = 64f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "ProgressBar"
    components.WantsProgress.[0] <- true
    components.ProgressStyle.[0] <- NoobishProgressStyle.RadialSquare
    components.ProgressValue.[0] <- 0.5f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 128, 128, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let triangleCount =
        ctx.Commands
        |> Seq.filter (function RenderCommand.Triangle _ -> true | _ -> false)
        |> Seq.length
    Assert.Greater(triangleCount, 0)

[<Test>]
let ``renderer logs debug rectangle when debug enabled`` () =
    let drawables =
        toThemes [ ("Panel", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    renderer.Debug <- true
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Panel"
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let hasDebugRect =
        ctx.Commands
        |> Seq.exists (function RenderCommand.Rectangle _ -> true | _ -> false)
    Assert.IsTrue(hasDebugRect)

[<Test>]
let ``renderer draws children after parent`` () =
    let drawables =
        toThemes [ ("Panel", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(2)
    components.Count <- 2
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 20f; Height = 20f }
    components.Bounds.[1] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Visible.[1] <- true
    components.Enabled.[1] <- true
    components.ThemeId.[0] <- "Panel"
    components.ThemeId.[1] <- "Panel"
    components.Text.[0] <- ""
    components.Text.[1] <- ""
    let childId = UIComponentIdV2.create 1us 0us 1us 1us
    components.Id.[1] <- childId
    components.ParentId.[1] <- UIComponentIdV2.create 1us 0us 0us 0us
    components.Children.[0].Add childId
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let drawableCount =
        ctx.Commands
        |> Seq.filter (function RenderCommand.Drawable _ -> true | _ -> false)
        |> Seq.length
    Assert.AreEqual(2, drawableCount)

[<Test>]
let ``renderer draws text and caret`` () =
    let drawables =
        toThemes
            [ ("Label", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ])
              ("Cursor", [ ("default", [| NoobishDrawable.NinePatch "cursor" |]) ]) ]
    let styleSheet =
        { (createStyleSheet drawables) with
            Fonts = toThemes [ ("Label", [ ("default", "Font") ]) ]
            FontSizes = toThemes [ ("Label", [ ("default", 12) ]) ]
            FontColors = toThemes [ ("Label", [ ("default", NoobishColor.white) ]) ]
            TextAlignments = toThemes [ ("Label", [ ("default", NoobishAlignment.TopLeft) ]) ]
            Widths = toThemes [ ("Cursor", [ ("default", 2f) ]) ]
            Colors = toThemes [ ("Cursor", [ ("default", NoobishColor.white) ]) ] }
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 50f; Height = 20f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Label"
    components.Text.[0] <- "x"
    components.Textwrap.[0] <- false
    components.Focused.[0] <- true
    components.WantsTextChanged.[0] <- true
    components.CaretIndex.[0] <- 1
    components.Id.[0] <- UIComponentIdV2.create 1us 0us 0us 7us
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let hasText =
        ctx.Commands
        |> Seq.exists (function RenderCommand.TextSingle _ -> true | RenderCommand.TextMulti _ -> true | _ -> false)
    let hasCursorDrawable =
        ctx.Commands
        |> Seq.exists (function RenderCommand.Drawable (bounds, _, _, _) -> bounds.Width = 2f | _ -> false)
    Assert.IsTrue(hasText)
    Assert.IsTrue(hasCursorDrawable)

[<Test>]
let ``renderer draws masked text and positions caret by mask`` () =
    let drawables =
        toThemes
            [ ("TextBox", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ])
              ("Cursor", [ ("default", [| NoobishDrawable.NinePatch "cursor" |]) ]) ]
    let styleSheet =
        { (createStyleSheet drawables) with
            Fonts = toThemes [ ("TextBox", [ ("default", "Font") ]) ]
            FontSizes = toThemes [ ("TextBox", [ ("default", 12) ]) ]
            FontColors = toThemes [ ("TextBox", [ ("default", NoobishColor.white) ]) ]
            TextAlignments = toThemes [ ("TextBox", [ ("default", NoobishAlignment.TopLeft) ]) ]
            Widths = toThemes [ ("Cursor", [ ("default", 2f) ]) ]
            Colors = toThemes [ ("Cursor", [ ("default", NoobishColor.white) ]) ] }
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 20f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "TextBox"
    components.Text.[0] <- "cat"
    components.TextDisplayMode.[0] <- NoobishTextDisplayMode.Masked
    components.Textwrap.[0] <- false
    components.Focused.[0] <- true
    components.WantsTextChanged.[0] <- true
    components.CaretIndex.[0] <- 2
    components.Id.[0] <- UIComponentIdV2.create 1us 0us 0us 8us
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let textCommands =
        ctx.Commands
        |> Seq.choose (function RenderCommand.TextSingle text -> Some text | _ -> None)
        |> Seq.toList
    let cursorBounds =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (bounds, _, _, _) when bounds.Width = 2f -> Some bounds | _ -> None)
        |> Seq.head

    Assert.AreEqual([ "***" ], textCommands)
    Assert.AreEqual(32f, cursorBounds.X)

[<Test>]
let ``renderer skips invisible components`` () =
    let drawables =
        toThemes [ ("Panel", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Visible.[0] <- false
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Panel"
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    Assert.AreEqual(0, ctx.Commands.Count)

[<Test>]
let ``renderer skips components with no area`` () =
    let drawables =
        toThemes [ ("Panel", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 0f; Height = 10f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Panel"
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    Assert.AreEqual(0, ctx.Commands.Count)

[<Test>]
let ``renderer skips text when text clip has no area`` () =
    let drawables =
        toThemes [ ("Label", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet =
        { (createStyleSheet drawables) with
            Fonts = toThemes [ ("Label", [ ("default", "Font") ]) ]
            FontSizes = toThemes [ ("Label", [ ("default", 12) ]) ]
            FontColors = toThemes [ ("Label", [ ("default", NoobishColor.white) ]) ]
            TextAlignments = toThemes [ ("Label", [ ("default", NoobishAlignment.TopLeft) ]) ] }
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Padding.[0] <- { NoobishPadding.Top = 6f; Right = 6f; Bottom = 6f; Left = 6f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Label"
    components.Text.[0] <- "x"
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let hasText =
        ctx.Commands
        |> Seq.exists (function RenderCommand.TextSingle _ -> true | RenderCommand.TextMulti _ -> true | _ -> false)
    Assert.IsFalse(hasText)

[<Test>]
let ``renderer skips children when child clip has no area`` () =
    let drawables =
        toThemes [ ("Panel", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(2)
    components.Count <- 2
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Bounds.[1] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Padding.[0] <- { NoobishPadding.Top = 6f; Right = 6f; Bottom = 6f; Left = 6f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Visible.[1] <- true
    components.Enabled.[1] <- true
    components.ThemeId.[0] <- "Panel"
    components.ThemeId.[1] <- "Panel"
    components.Text.[0] <- ""
    components.Text.[1] <- ""
    let childId = UIComponentIdV2.create 1us 0us 1us 1us
    components.Id.[1] <- childId
    components.ParentId.[1] <- UIComponentIdV2.create 1us 0us 0us 0us
    components.Children.[0].Add childId
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let drawableCount =
        ctx.Commands
        |> Seq.filter (function RenderCommand.Drawable _ -> true | _ -> false)
        |> Seq.length
    Assert.AreEqual(1, drawableCount)

[<Test>]
let ``renderer draws scrollbars after scroll`` () =
    let drawables =
        toThemes
            [ ("ScrollBar", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("ScrollBarPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let widths =
        toThemes
            [ ("ScrollBar", [ ("default", 8f) ])
              ("ScrollBarPin", [ ("default", 8f) ]) ]
    let styleSheet = createStyleSheetWithWidths drawables widths
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 100f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Scroll.[0] <- { Scroll.Horizontal = false; Vertical = true }
    components.ContentSize.[0] <- { Width = 100f; Height = 300f }
    components.Text.[0] <- ""

    let ctxInitial = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctxInitial :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.0, TimeSpan.FromSeconds 0.0))

    components.ScrollY.[0] <- -50f
    let ctxAfterScroll = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctxAfterScroll :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.2, TimeSpan.FromSeconds 0.2))

    let scrollDrawablesCount =
        ctxAfterScroll.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function
                | NoobishDrawable.NinePatch "track"
                | NoobishDrawable.NinePatch "pin" -> true
                | _ -> false))
        |> Seq.length
    Assert.AreEqual(2, scrollDrawablesCount)

[<Test>]
let ``renderer hides scrollbars after timeout`` () =
    let drawables =
        toThemes
            [ ("ScrollBar", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("ScrollBarPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let widths =
        toThemes
            [ ("ScrollBar", [ ("default", 8f) ])
              ("ScrollBarPin", [ ("default", 8f) ]) ]
    let styleSheet = createStyleSheetWithWidths drawables widths
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 100f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Scroll.[0] <- { Scroll.Horizontal = false; Vertical = true }
    components.ContentSize.[0] <- { Width = 100f; Height = 300f }
    components.Text.[0] <- ""

    let ctxInitial = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctxInitial :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.0, TimeSpan.FromSeconds 0.0))

    components.ScrollY.[0] <- -50f
    let ctxScroll = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctxScroll :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.1, TimeSpan.FromSeconds 0.1))

    let ctxAfterTimeout = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctxAfterTimeout :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 2.0, TimeSpan.FromSeconds 2.0))

    let scrollDrawablesCount =
        ctxAfterTimeout.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function
                | NoobishDrawable.NinePatch "track"
                | NoobishDrawable.NinePatch "pin" -> true
                | _ -> false))
        |> Seq.length
    Assert.AreEqual(0, scrollDrawablesCount)

[<Test>]
let ``renderer tracks scroll activity for index-based components`` () =
    let drawables =
        toThemes
            [ ("ScrollBar", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("ScrollBarPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let widths =
        toThemes
            [ ("ScrollBar", [ ("default", 8f) ])
              ("ScrollBarPin", [ ("default", 8f) ]) ]
    let styleSheet = createStyleSheetWithWidths drawables widths
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 100f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Scroll.[0] <- { Scroll.Horizontal = false; Vertical = true }
    components.ContentSize.[0] <- { Width = 100f; Height = 300f }
    components.Id.[0] <- UIComponentIdV2.create 1us 0us 0us 0us
    components.Text.[0] <- ""

    let countScrollDrawables (ctx: MockRenderContext) =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function
                | NoobishDrawable.NinePatch "track"
                | NoobishDrawable.NinePatch "pin" -> true
                | _ -> false))
        |> Seq.length

    components.ScrollY.[0] <- -50f
    let ctxInitial = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctxInitial :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.0, TimeSpan.FromSeconds 0.0))
    Assert.AreEqual(2, countScrollDrawables ctxInitial)

    components.ScrollY.[0] <- -75f
    let ctxChanged = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctxChanged :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.2, TimeSpan.FromSeconds 0.2))
    Assert.AreEqual(2, countScrollDrawables ctxChanged)

    let ctxSame = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctxSame :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.3, TimeSpan.FromSeconds 0.3))
    Assert.AreEqual(2, countScrollDrawables ctxSame)

[<Test>]
let ``renderer draws horizontal scrollbars when enabled`` () =
    let drawables =
        toThemes
            [ ("ScrollBar", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("ScrollBarPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let widths =
        toThemes
            [ ("ScrollBar", [ ("default", 6f) ])
              ("ScrollBarPin", [ ("default", 6f) ]) ]
    let styleSheet = createStyleSheetWithWidths drawables widths
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 120f; Height = 80f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Scroll.[0] <- { Scroll.Horizontal = true; Vertical = false }
    components.ContentSize.[0] <- { Width = 240f; Height = 80f }
    components.Text.[0] <- ""

    components.ScrollX.[0] <- -40f
    let ctx = MockRenderContext(styleSheet, createAtlas(), 300, 200, createFontMap())
    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.2, TimeSpan.FromSeconds 0.2))

    let scrollDrawablesCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function
                | NoobishDrawable.NinePatch "track"
                | NoobishDrawable.NinePatch "pin" -> true
                | _ -> false))
        |> Seq.length
    Assert.AreEqual(2, scrollDrawablesCount)

[<Test>]
let ``renderer computes content extent from children`` () =
    let drawables =
        toThemes
            [ ("ScrollBar", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("ScrollBarPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let widths =
        toThemes
            [ ("ScrollBar", [ ("default", 8f) ])
              ("ScrollBarPin", [ ("default", 8f) ]) ]
    let styleSheet = createStyleSheetWithWidths drawables widths
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(2)
    components.Count <- 2
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 100f }
    components.Bounds.[1] <- { X = 0f; Y = 150f; Width = 80f; Height = 40f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Visible.[1] <- true
    components.Enabled.[1] <- true
    components.Scroll.[0] <- { Scroll.Horizontal = false; Vertical = true }
    components.ContentSize.[0] <- { Width = 0f; Height = 0f }
    components.Text.[0] <- ""
    components.Text.[1] <- ""
    let childId = UIComponentIdV2.create 1us 0us 1us 1us
    components.Id.[1] <- childId
    components.ParentId.[1] <- UIComponentIdV2.create 1us 0us 0us 0us
    components.Children.[0].Add childId
    components.ScrollY.[0] <- -25f

    let ctx = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())
    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.2, TimeSpan.FromSeconds 0.2))

    let scrollDrawablesCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function
                | NoobishDrawable.NinePatch "track"
                | NoobishDrawable.NinePatch "pin" -> true
                | _ -> false))
        |> Seq.length
    Assert.AreEqual(2, scrollDrawablesCount)

[<Test>]
let ``renderer skips background when bounds has no area`` () =
    let drawables =
        toThemes [ ("Panel", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 0f; Height = 10f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Panel"
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let backgroundCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function NoobishDrawable.NinePatch "bg" -> true | _ -> false))
        |> Seq.length
    Assert.AreEqual(0, backgroundCount)

[<Test>]
let ``renderer skips slider pin when pin bounds have no area`` () =
    let drawables =
        toThemes
            [ ("Slider", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("SliderPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let widths = toThemes [ ("SliderPin", [ ("default", 8f) ]) ]
    let styleSheet = createStyleSheetWithWidths drawables widths
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 0f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Slider"
    components.WantsSlider.[0] <- true
    components.SliderMin.[0] <- 0f
    components.SliderMax.[0] <- 10f
    components.SliderValue.[0] <- 5f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 200, 50, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let pinCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function NoobishDrawable.NinePatch "pin" -> true | _ -> false))
        |> Seq.length
    Assert.AreEqual(0, pinCount)

[<Test>]
let ``renderer skips slider track when track bounds have no area`` () =
    let drawables =
        toThemes [ ("Slider", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Padding.[0] <- { NoobishPadding.Top = 5f; Right = 5f; Bottom = 5f; Left = 5f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "Slider"
    components.WantsSlider.[0] <- true
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let trackCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function NoobishDrawable.NinePatch "track" -> true | _ -> false))
        |> Seq.length
    Assert.AreEqual(0, trackCount)

[<Test>]
let ``renderer skips progress fill when fill bounds have no area`` () =
    let drawables =
        toThemes
            [ ("ProgressBar", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ])
              ("ProgressBar-Progress", [ ("default", [| NoobishDrawable.NinePatch "fill" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Padding.[0] <- { NoobishPadding.Top = 6f; Right = 6f; Bottom = 6f; Left = 6f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "ProgressBar"
    components.WantsProgress.[0] <- true
    components.ProgressValue.[0] <- 0.5f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let fillCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function NoobishDrawable.NinePatch "fill" -> true | _ -> false))
        |> Seq.length
    Assert.AreEqual(0, fillCount)

[<Test>]
let ``renderer skips progress segments when content has no area`` () =
    let drawables =
        toThemes
            [ ("ProgressBar", [ ("default", [| NoobishDrawable.NinePatch "bg" |]) ])
              ("ProgressBar-Dash", [ ("default", [| NoobishDrawable.NinePatch "dash" |]) ])
              ("ProgressBar-Progress", [ ("default", [| NoobishDrawable.NinePatch "fill" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Padding.[0] <- { NoobishPadding.Top = 6f; Right = 6f; Bottom = 6f; Left = 6f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.ThemeId.[0] <- "ProgressBar"
    components.WantsProgress.[0] <- true
    components.ProgressSegments.[0] <- 3
    components.ProgressValue.[0] <- 0.5f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime())

    let segmentCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function
                | NoobishDrawable.NinePatch "dash"
                | NoobishDrawable.NinePatch "fill" -> true
                | _ -> false))
        |> Seq.length
    Assert.AreEqual(0, segmentCount)

[<Test>]
let ``renderer skips scrollbars when content bounds have no area`` () =
    let drawables =
        toThemes
            [ ("ScrollBar", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("ScrollBarPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let widths =
        toThemes
            [ ("ScrollBar", [ ("default", 8f) ])
              ("ScrollBarPin", [ ("default", 8f) ]) ]
    let styleSheet = createStyleSheetWithWidths drawables widths
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 10f; Height = 10f }
    components.Padding.[0] <- { NoobishPadding.Top = 6f; Right = 6f; Bottom = 6f; Left = 6f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Scroll.[0] <- { Scroll.Horizontal = false; Vertical = true }
    components.ContentSize.[0] <- { Width = 10f; Height = 100f }
    components.ScrollY.[0] <- -10f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 100, 100, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.2, TimeSpan.FromSeconds 0.2))

    let scrollDrawablesCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function
                | NoobishDrawable.NinePatch "track"
                | NoobishDrawable.NinePatch "pin" -> true
                | _ -> false))
        |> Seq.length
    Assert.AreEqual(0, scrollDrawablesCount)

[<Test>]
let ``renderer skips scrollbars when track thickness is zero`` () =
    let drawables =
        toThemes
            [ ("ScrollBar", [ ("default", [| NoobishDrawable.NinePatch "track" |]) ])
              ("ScrollBarPin", [ ("default", [| NoobishDrawable.NinePatch "pin" |]) ]) ]
    let styleSheet = createStyleSheet drawables
    let renderer = NoobishMonoGameRendererV2()
    let components = NoobishComponentsV2(1)
    components.Count <- 1
    components.Bounds.[0] <- { X = 0f; Y = 0f; Width = 100f; Height = 100f }
    components.Visible.[0] <- true
    components.Enabled.[0] <- true
    components.Scroll.[0] <- { Scroll.Horizontal = false; Vertical = true }
    components.ContentSize.[0] <- { Width = 100f; Height = 300f }
    components.ScrollY.[0] <- -10f
    components.Text.[0] <- ""
    let ctx = MockRenderContext(styleSheet, createAtlas(), 200, 200, createFontMap())

    renderer.DrawWithContext components (ctx :> INoobishMonoGameRenderContext) "Style" (Microsoft.Xna.Framework.GameTime(TimeSpan.FromSeconds 0.2, TimeSpan.FromSeconds 0.2))

    let scrollDrawablesCount =
        ctx.Commands
        |> Seq.choose (function RenderCommand.Drawable (_, _, _, drawables) -> Some drawables | _ -> None)
        |> Seq.filter (fun drawables ->
            drawables
            |> Seq.exists (function
                | NoobishDrawable.NinePatch "track"
                | NoobishDrawable.NinePatch "pin" -> true
                | _ -> false))
        |> Seq.length
    Assert.AreEqual(0, scrollDrawablesCount)
