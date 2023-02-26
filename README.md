# Noobish

## Introduction

![Noobish](screenshots/hello.png "Hello Noobish!")

Noobish is an experimental element tree / ui library for MonoGame written with F#. Noobish supports multiple basic components and supports limited customization. Noobish can be used with mobile devices.

Since Noobish is designed for Elmish, the whole element tree is rebuilt on each *'view'* call. Noobish persists state of elements between view cycles to keep scroll position.

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

### Project Setup

From your main project, add reference to:

* Noobish.dll and Noobish.Types.dll

In your content pipeline, add reference to

* Noobish.PipelineExtension.

## Project Layout

* *Noobish:* The library project consumed by the user.
* *Noobish.Test:* The tests of the library.
* *Noobish.Demo:* Executable demo.
* *Noobish.Demo.Content:* Content project for the Noobish.Demo.
* *Noobish.PipelineExtension:* MonoGame Content Pipeline Extension that creates TextureAtlases and StyleSheet.

## ToDo

* Memoize support
* Nine Patch support
* Keyboard input
* API stabilization
* Multiple theme support
* Split themes into smaller sections
