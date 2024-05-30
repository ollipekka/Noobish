[<AutoOpen>]
module Noobish.Combobox

open Noobish

open Microsoft.Xna.Framework


type Noobish with 

    member this.Combobox<'T> (items: 'T[]) (selectedIndex: int) (onValueChanged: 'T -> unit) = 
        let cid = this.Create "Combobox"
        this.Components.Text.[cid.Index] <- items.[selectedIndex].ToString()

        let overlayPaneId =
            this.Overlaypane cid 
            |> this.SetOnClick (fun sourceId (_position: Position) (_gameTime: GameTime) ->  
                this.SetVisible false sourceId |> ignore)

        let overlayWindowId =
            this.Panel()
            |> this.SetVerticalLayout
            |> this.SetChildren (
                items |> Array.map (
                    fun i -> 
                        this.Button (i.ToString()) (
                            fun (event) (gameTime: GameTime) -> 
                                this.Components.Text.[cid.Index] <- i.ToString(); onValueChanged i
                        ) |> this.SetFillHorizontal
                )
            )
            
        overlayPaneId 
        |> this.SetChildren [| overlayWindowId |]
        |> this.SetLayer 225
        |> this.SetVisible false 
        |> ignore


        this.Components.WantsOnClick.[cid.Index] <- true
        this.Components.OnClick.[cid.Index] <- (fun (event: UIComponentId) (position: Position) (gameTime: GameTime) -> 
            this.SetVisible true overlayPaneId |> ignore
        )

        cid