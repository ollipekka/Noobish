namespace Noobish

open Microsoft.Xna.Framework

module NoobishColorMonoGame =
    let toColor (color: NoobishColor) =
        Color(color.R, color.G, color.B, color.A)

    
    let ofColor (color: Color): NoobishColor =
        {R = color.R; G= color.G; B= color.B; A= color.A}

