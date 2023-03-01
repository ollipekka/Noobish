# Noobish

## Introduction

![Noobish](screenshots/hello.png "Hello Noobish!")

Noobish is an experimental element tree / ui library for MonoGame written with F#. Noobish supports multiple basic components and limited customization through themes and custom components. Noobish uses non-complex (one stretch) Nine Patches for component images. Noobish can be used with mobile devices.

Since Noobish is designed for Elmish, the element tree is rebuilt on each *'view'* call. Noobish persists state of elements between view cycles to keep state stuch as scroll position.

## Supported components

* **h1**, **h2**, **h3** represent headers of varying sizes.
* **label** is the basic visualization of short single-line text.
* **p** renders wrapped multiline text.
* **scroll** enables scrolling of the overflowing content.
* **textbox** captures user key input.
* **button** provides something for the users to click on.
* **combobox** allows selecting a value from a dropdown.
* **checkbox** is a toggleable selection with a text.
* **hr** is a nice line to divide content.
* **div** and **panel** provide a simple block layout.
* **grid** creates a complex layout.

## Attributes

* ToDo

## Markup

Noobish views are built as code with a DSL. Each view is a list of NoobishElements.

```fsharp

panel
    [
        h2 [text "Welcome to Noobish"; textCenter]
        hr []
        grid 6 4
            [
                checkbox [text "FSharp"; toggled true; colspan 2; rowspan 1]
                checkbox [text "MonoGame"; toggled true; colspan 2; rowspan 1]
                checkbox [text "Elmish"; toggled true; colspan 2; rowspan 1]
                label [text "Coolness:"; colspan 1; rowspan 1; textLeft]
                slider [sliderValue 80f; fillHorizontal; colspan 5; rowspan 1]
                label [text "Features:"; colspan 1; rowspan 1; textLeft]
                textbox [ text "extendable, functional, net6.0 and cross-platform"; textLeft; colspan 5; rowspan 1]
                button [text "Report a bug"; colspan 2]
                button [text "Contribute"; colspan 2]
                button [text "Fork"; colspan 2]
            ]
            [

            ]
    ]


## Style Sheets

Noobish style sheets are json files. The styles cascade towards *'default'*.

```json
{
    "TextureAtlas": "Dark/DarkAtlas.json",
    "Styles": {
        "Default": {
            "default": {
                "font": "Metropolis-Regular",
                "fontSize": 12,
                "fontColor": "bbbbbbFF",
                "padding": [4, 4, 4, 4],
                "margin": [4, 4, 4, 4],
                "color": "39404dff"
            }
        },
        "Button": {
            "default": {
                "textAlign": "center",
                "drawables": [
                    ["button-default.9"]
                ]
            },
            "toggled": {
                "color": "4b692fff"
            }
        }
    }
}
```

## Scaling

Noobish does not support scaling due to issues with texture bleeding and nine patches. The recommended approach is to render to an offscreen buffer (MonoGame RenderTarget) and scale it to desired scale.

## Project setup

From your main project, add reference to Noobish.dll.

In your content pipeline, add reference to

* Noobish.PipelineExtension.

Best way to start is to copy over a theme and a font from the demo project and start from there.

## Generating Fonts

Fonts are generated using msdf-atlas-gen.

## Project Layout

* *Noobish:* The library project consumed by the user.
* *Noobish.Test:* The tests of the library.
* *Noobish.Demo:* Executable demo.
* *Noobish.Demo.Content:* Content project for the Noobish.Demo.
* *Noobish.PipelineExtension:* MonoGame Content Pipeline Extension that creates TextureAtlases and StyleSheet.

## Limitations

Noobish tracks identity of a component by its location. Noobish doesn't handle properly layouts where components disappear from the layout between view calls.

## ToDo

* Memoize support.
* API stabilization.
* Study utilizing generic models in components.
  * ```combobox<SwordSelectionModel>```
  * ```list<ListItem>```
