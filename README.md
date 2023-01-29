# Noobish

## Introduction

Noobish is an element tree suitable for F# Elmish applications. Current reference implementation is for MonoGame, but in theory other implementations could exist. Noobish supports multiple components and it can be extended with custom components (up to a point). Noobish is built with mobile devices in mind.

Since Noobish is designed for Elmish, the whole element tree is rebuilt on each *'view'* call. Noobish persists state of elements between renders to keep scroll position.

## Supported components

* **Header** represents a header text.
* **Label** is the basic visualization of short single-line text.
* **Paragraph** supports multiline text.
* **Scroll** enables scrolling of overflowing content.
* **TextBox** allows user to type text.
* **Button** let's user to click on a thing.
* **Combobox** provides a selection dropdown.
* **Horizontal rule** looks nice under a header.
* **Panel** let's you organize components.
* **Grid** provides grid-based layouts

## Scaling

Noobish supports scaling the UI components for larger screens. The library relies on spritefonts and text will not be scaled.

## Theme

Noobish has a default renderer. Only single theme is supported. Each component can be themed separately using NinePatches.

## Limitations

Noobish tracks identity of a component by its location. Noobish can't handle layouts where components disappear from the layout between view calls.

## Attributes

* ToDo

## Getting started

Preferred usage is to include source files directly into your project.

## Project Layout

* *NoobishTypes* the common types provided by the library usable independently of .
* *Noobish:* the element tree and layout modules.
* *Noobish.Test:* unit tests for the element tree and layout modules.
* *Noobish.MonoGame:* MonoGame related implementation.
* *Noobish.MonoGame.Demo:* Executable MonoGame related kitchen sink and demo.
* *Noobish.TextureAtlas:* The texture atlas library.
* *Noobish.TextureAtlas.PipelineExtension:* MonoGame related processort that packs textures and turns them into .xnb file.
* *Noobish.TextureAtlas.PipelineExtension:* MonoGame related test processort with test content.


## ToDo

* Memoize support
* Nine Patch support
* Keyboard input
* API stabilization
* Multiple theme support
* Split themes into smaller sections
