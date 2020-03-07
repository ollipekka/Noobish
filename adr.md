# Architecture Decision Record

## Internal Datatypes

* Use ints for user defined positions, margins, paddings and sizes.
* Use floats in Noobish Object Model (NOM) to ensure that rounding errors caused by integer divisions do not produce rendering errors.

## Color datatype

* Hex decimal int used for color in the format of 0xRRGGBBAA.

## Qualified access

* Internal types should for attributes should be placed inside Component module. In case types are required outside, they should be prefixed with Noobish and `[<RequireQualifiedAccess>]` should be used to avoid name collisions between discriminated unions.
