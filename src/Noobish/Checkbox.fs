[<AutoOpen>]
module Noobish.Checkbox

open Noobish


type Noobish2 with 

    member this.Checkbox (text: string) (toggled: bool) (onValueChanged: bool -> unit) =

        let divId = this.DivHorizontal()

        let check = 
            this.Button "" (fun _ -> onValueChanged (not toggled ))
            |> this.SetThemeId "CheckBox"
            |> this.SetToggled toggled 

        divId |> this.SetChildren [|
            check 
            this.Label text
        |] 