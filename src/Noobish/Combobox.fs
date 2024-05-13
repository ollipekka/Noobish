[<AutoOpen>]
module Noobish.Combobox

open Noobish


type Noobish2 with 

    member this.Combobox<'T> (items: 'T[]) (selectedIndex: int) (onValueChanged: OnClickEvent -> 'T -> unit)= 
        let cid = this.Create "Combobox"
        this.Components.Text.[cid.Index] <- items.[selectedIndex].ToString()

        let overlayPaneId =
            this.Overlaypane cid 
            |> this.SetOnClick (fun event ->  
                this.SetVisible false event.SourceId |> ignore)

        let overlayWindowId =
            this.Window()
            |> this.SetChildren (
                items |> Array.map (
                    fun i -> 
                        this.Button (i.ToString()) (
                            fun (event) -> 
                                this.Components.Text.[cid.Index] <- i.ToString(); onValueChanged ({SourceId=cid}) i
                        ) |> this.SetFillHorizontal
                )
            )
            
        overlayPaneId 
        |> this.SetChildren [| overlayWindowId |]
        |> this.SetLayer 225
        |> this.SetVisible false 
        |> ignore


        this.Components.WantsOnClick.[cid.Index] <- true
        this.Components.OnClick.[cid.Index] <- (fun event -> 
            this.SetVisible true overlayPaneId |> ignore
        )

        cid