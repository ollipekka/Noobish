[<AutoOpen>]
module Noobish.List

open Noobish


type Noobish with 

    member this.List<'T> (items: 'T[]) (selectedIndex: int) (onValueChanged: 'T -> unit) =

        let cid = this.Create "Panel"
        this.Components.Fill.[cid |> UIComponentId.index] <- {Horizontal = true; Vertical = true}
        this.Components.Scroll.[cid |> UIComponentId.index] <- {Horizontal = false; Vertical = true}
        this.Components.Layout.[cid |> UIComponentId.index] <- Layout.LinearVertical

        cid |> this.SetChildren (
            items |> Array.mapi (
                fun i item -> 
                    this.Div()
                    |> this.SetThemeId (if i % 2 = 0 then "List-Division-Even" else "List-Division-Odd")
                    |> this.FillHorizontal
                    |> this.SetToggled (selectedIndex = i)
                    |> this.SetOnClick(fun _src _position _gameTime -> onValueChanged item)
                    |> this.SetChildren [|
                        this.Label (item.ToString())
                        |> this.SetThemeId ("List-Label")
                        |> this.FillHorizontal
                        |> this.SetToggled (selectedIndex = i)
                        |> this.SetOnClick (fun _src _position _gameTime -> onValueChanged item)
                    |]

            ))