module Noobish.Test.StyleSheetTests

open System.Collections.Generic
open NUnit.Framework
open Noobish
open Noobish.Styles

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

let private createStyleSheet (widths: IReadOnlyDictionary<string, IReadOnlyDictionary<string, float32>>) =
    { Name = "Test"
      TextureAtlasId = "None"
      Widths = widths
      Heights = emptyThemeDict<float32>()
      Paddings = emptyThemeDict<NoobishPadding>()
      Margins = emptyThemeDict<NoobishMargin>()
      Colors = emptyThemeDict<NoobishColor>()
      Fonts = emptyThemeDict<string>()
      FontSizes = emptyThemeDict<int>()
      FontColors = emptyThemeDict<NoobishColor>()
      TextAlignments = emptyThemeDict<NoobishAlignment>()
      Drawables = emptyThemeDict<NoobishDrawable[]>() }

[<Test>]
let ``StyleSheet GetValue returns explicit theme state`` () =
    let widths = toThemes [ ("Button", [ ("hover", 12f) ]) ]
    let styleSheet = createStyleSheet widths

    let value = styleSheet.GetWidth "Button" "hover"

    Assert.AreEqual(12f, value)

[<Test>]
let ``StyleSheet GetValue falls back to theme default`` () =
    let widths = toThemes [ ("Button", [ ("default", 8f) ]) ]
    let styleSheet = createStyleSheet widths

    let value = styleSheet.GetWidth "Button" "hover"

    Assert.AreEqual(8f, value)

[<Test>]
let ``StyleSheet GetValue falls back to Default theme state`` () =
    let widths = toThemes [ ("Default", [ ("hover", 14f) ]) ]
    let styleSheet = createStyleSheet widths

    let value = styleSheet.GetWidth "Button" "hover"

    Assert.AreEqual(14f, value)

[<Test>]
let ``StyleSheet GetDefault falls back to Default default`` () =
    let widths = toThemes [ ("Button", [ ("hover", 12f) ]); ("Default", [ ("default", 6f) ]) ]
    let styleSheet = createStyleSheet widths

    let value = styleSheet.GetWidth "Button" "pressed"

    Assert.AreEqual(6f, value)

[<Test>]
let ``StyleSheet GetDefault returns fallback when Default missing`` () =
    let widths = toThemes [ ("Button", [ ("hover", 12f) ]) ]
    let styleSheet = createStyleSheet widths

    let value = styleSheet.GetWidth "Missing" "pressed"

    Assert.AreEqual(0f, value)

[<Test>]
let ``StyleSheet GetDefault returns fallback when theme and Default defaults missing`` () =
    let widths = toThemes [ ("Button", [ ("hover", 12f) ]) ]
    let styleSheet = createStyleSheet widths

    let value = styleSheet.GetWidth "Button" "pressed"

    Assert.AreEqual(0f, value)
