namespace Noobish 

open System
open Serilog

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Content
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Noobish
open Noobish.Styles
open Noobish.TextureAtlas


[<Struct>]
[<CustomEquality; NoComparison>]
type UIComponentId = {
    Index: int
    Id: Guid
} with 
    override this.Equals other =
        match other with
        | :? UIComponentId as p -> (this :> IEquatable<UIComponentId>).Equals p
        | _ -> false   
        
    override this.GetHashCode () = this.Id.GetHashCode() 

    static member op_Equality(this : UIComponentId, other : UIComponentId) =
        this.Id.Equals other.Id
    
    interface IEquatable<UIComponentId> with 
        member this.Equals (other: UIComponentId) =
            this.Id.Equals other.Id

module UIComponentId =
    let empty: UIComponentId = { Index = -1; Id = Guid.Empty }


[<Struct>]
type TableSpan = {
    Rowspan: int
    Colspan: int
}

[<Struct>]
type Fill = {
    Horizontal: bool
    Vertical: bool
}

[<Struct>]
type Margin = {
    Top: float32
    Right: float32
    Bottom: float32
    Left: float32
}

[<Struct>]
type Padding = {
    Top: float32
    Right: float32
    Bottom: float32
    Left: float32
}

[<Struct>]
type Scroll = {
    Horizontal: bool
    Vertical: bool
}

[<Struct>]
type Size = {Width: float32; Height: float32}

[<Struct>]
type Position = {X: float32; Y: float32}


[<Struct>]
type NoobishRectangle = {
    X: float32
    Y: float32
    Width: float32
    Height: float32
} with
    member r.Left with get() = r.X
    member r.Right with get() = r.X + r.Width
    member r.Top with get() = r.Y
    member r.Bottom with get() = r.Y + r.Height


module DrawUI = 


    let createRectangle (x: float32) (y:float32) (width: float32) (height: float32) =
        Rectangle (int (x), int (y), int (width), int (height))

    let drawDrawable (textureAtlas: NoobishTextureAtlas) (spriteBatch: SpriteBatch)  (position: Vector2) (size: Vector2) (layer: float32) (color: Color) (drawables: NoobishDrawable[]) =
        for drawable in drawables do
            match drawable with
            | NoobishDrawable.Texture _ -> failwith "Texture not supported for cursor."
            | NoobishDrawable.NinePatch(tid) ->
                let texture = textureAtlas.[tid]

                spriteBatch.DrawAtlasNinePatch2(
                    texture,
                    Rectangle(int position.X, int position.Y, int size.X, int size.Y),
                    color,
                    layer)
            | NoobishDrawable.NinePatchWithColor(tid, color) ->
                let texture = textureAtlas.[tid]

                spriteBatch.DrawAtlasNinePatch2(
                    texture,
                    Rectangle(int position.X, int position.Y, int size.X, int size.Y),
                    color,
                    layer)





type OnClickEvent = {
    SourceId: UIComponentId
}

[<RequireQualifiedAccess>]
type Layout =
| LinearHorizontal 
| LinearVertical
| Grid of cols: int * rows: int
| Relative of UIComponentId
| None

type NoobishFrame(count) = 

    member val Count = 0 with get, set

    member val Id = Array.create count UIComponentId.empty

    member val ThemeId = Array.create count ""

    member val ParentId = Array.create count UIComponentId.empty

    member val Children = Array.init count (fun _ -> ResizeArray<UIComponentId>())

    member val Visible = Array.create count true 
    member val Enabled = Array.create count true 

    member val Block = Array.create count false

    member val Text = Array.create count ""
    member val TextAlign = Array.create count NoobishTextAlignment.Left
    member val Textwrap = Array.create count false

    member val Layer = Array.create count 0
    member val Bounds = Array.create<NoobishRectangle> count {X = 0f; Y = 0f; Width = 0f; Height = 0f}
    member val MinSize = Array.create count {Width = 0f; Height = 0f}
    member val ContentSize = Array.create count {Width = 0f; Height = 0f}
    member val RelativePosition = Array.create count {X = 0f; Y = 0f}

    member val Fill = Array.create<Fill> count ({Horizontal = false; Vertical = false})

    member val Padding = Array.create<Padding> count {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Margin = Array.create<Margin> count {Top = 0f; Right = 0f; Bottom = 0f; Left = 0f}
    member val Layout = Array.create count Layout.None

    member val GridSpan = Array.create count ({Rowspan = 1; Colspan = 1})

    member val WantsOnClick = Array.create count false
    member val OnClick = Array.init<OnClickEvent -> unit> count (fun _event -> ignore)

    member val LastPressTime = Array.create count TimeSpan.Zero

    member val LastHoverTime = Array.create count TimeSpan.Zero

    member val WantsKeyTyped = Array.create count false 
    member val OnKeyTyped = Array.create<OnClickEvent -> char -> unit> count (fun _ _ -> ()) 


    member val WantsKeyPressed = Array.create count false 
    member val OnKeyPressed = Array.create<OnClickEvent -> Keys -> unit> count (fun _ _ -> ()) 

    member val WantsFocus = Array.create count false 
    member val OnFocus = Array.create<OnClickEvent -> bool -> unit> count (fun _ _ -> ())

    member val Scroll = Array.create<Scroll> count {Horizontal = true; Vertical = true}

    member val Toggled = Array.create count false 
    member val Hovered = Array.create count false 
