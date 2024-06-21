namespace Noobish


open Microsoft.Xna.Framework

open Noobish

open Noobish.Internal

open System
open System.Collections.Generic

[<Flags>]
[<Struct>]
[<RequireQualifiedAccess>]
type NoobishElementState =
| Empty    =  0
| Selected =  1
| Toggled  =  2
| Hovered  =  4
| Pressed  =  8
| Disabled = 16
| Hidden   = 32
| Focused  = 64

module NoobishElementState =

    let inline isSet (self: NoobishElementState) (flag: NoobishElementState) =
        self &&& flag = flag

    let hide s =
        s ||| NoobishElementState.Hidden

    let show s =
        s &&& ~~~NoobishElementState.Hidden

    let focus s =
        s ||| NoobishElementState.Focused

    let unfocus s =
        s &&& ~~~NoobishElementState.Focused

    let select s =
        s ||| NoobishElementState.Selected

    let deselect s =
        s &&& ~~~NoobishElementState.Selected

    let enable s =
        s &&& ~~~NoobishElementState.Selected

    let disable s =
        s ||| NoobishElementState.Disabled

    let toggle s =
        s |||  NoobishElementState.Toggled

    let detoggle s =
        s &&& ~~~NoobishElementState.Toggled
