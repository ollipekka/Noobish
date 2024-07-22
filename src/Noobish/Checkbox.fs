[<AutoOpen>]
module Noobish.Checkbox

open Noobish


type Noobish with 

    member this.Checkbox (text: string) (toggled: bool) (onValueChanged: bool -> unit) =

        let divId = this.DivHorizontal()

        let check = 
            this.Div()
            |> this.SetOnClick (fun _ _ _ -> onValueChanged (not toggled ))
            |> this.SetThemeId "CheckBox"
            |> this.SetToggled toggled 
            
        divId 
            |> this.AddChild check 
            |> this.AddChild (this.Label text)