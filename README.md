# Noobish

## Introduction

Noobish is an HTML like element-tree suitable for F# Elmish applications. Current reference implementation is for MonoGame, but in theory other implementations could exist. Noobish supports multiple components and it can be extended with custom components (up to a point). It is possible to use Noobish on mobile devices and the aim is to support mobile specific requirements such as the native keyboard.

## Supported components

* *Label* is the basic visualization of short text.
* *Paragraph* supports multiparagrah text.
* *Scroll* enables scrolling of overflowign content.
* *TextBox* accepts input typed by user.
* *Button* let's user to click on thing
* *Combobox* provides a selection dropdown.
* *Horizontal rule* looks nice under a header.
* *Panel* let's you organize components.
* *Grid* provides grid-based layouts

## Scaling

Noobish supports scaling the UI components for larger screens. Due to use of spritefonts, text will not be scaled.

## Theme

Noobish monogame implementation supports only a signle theme. Each component can be themed separately.

## Attributes

* ToDo

## Getting started

Preferred usage is to include source files directly into your project.

## ToDo

* Memoize support
* Nine Patch support
* Keyboard input
* API stabilization
* Multiple theme support
