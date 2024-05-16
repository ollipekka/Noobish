[<AutoOpen>]
module Noobish.List

open Noobish


type Noobish2 with 

    member this.List<'T> (items: 'T[]) (selectedIndex: int) (onValueChanged: 'T -> unit) =

        let cid = this.Create "Panel"
        this.Components.Fill.[cid.Index] <- {Horizontal = true; Vertical = true}
        this.Components.Scroll.[cid.Index] <- {Horizontal = false; Vertical = true}
        this.Components.Layout.[cid.Index] <- Layout.LinearVertical

        cid |> this.SetChildren (
            items |> Array.mapi (
                fun i item -> 
                    this.Div()
                    |> this.SetThemeId (if i % 2 = 0 then "List-Division-Even" else "List-Division-Odd")
                    |> this.FillHorizontal
                    |> this.SetToggled (selectedIndex = i)
                    |> this.SetOnClick(fun src position -> onValueChanged item)
                    |> this.SetChildren [|
                        this.Label (item.ToString())
                        |> this.SetThemeId ("List-Label")
                        |> this.FillHorizontal
                        |> this.SetToggled (selectedIndex = i)
                        |> this.SetOnClick (fun src position -> onValueChanged item)
                    |]

            ))