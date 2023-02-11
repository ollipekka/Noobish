namespace Noobish

open System.Collections.Generic
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework

type NoobishFontAtlas = {
    FontType: string
    DistanceRange: float32
    Size: float32
    Width: int
    Height: int
    yOrigin: string
}

type NoobishFontMetrics = {
    EmSize: int32
    LineHeight: float32
    Ascender: float32
    Descender: float32
    UnderlineY: float32
    UnderlineThickness: float32
}

type NoobishGlyph = {
    Unicode: int64
    Advance: float32
    AtlasBounds: struct(float32*float32*float32*float32)
    PlaneBounds: struct(float32*float32*float32*float32)
}

type NoobishFont = {
    Atlas: NoobishFontAtlas
    Metrics: NoobishFontMetrics
    Glyphs: IReadOnlyDictionary<int64, NoobishGlyph>
    Kerning: IReadOnlyDictionary<int64, IReadOnlyDictionary<int64, float32>>
    Texture: Texture2D
}


type TextBatch (graphics: GraphicsDevice, batchSize: int) =

    let mutable vertexCount = 0
    let vertices = Array.create batchSize (VertexPositionColorTexture())
    let indices = Array.create (batchSize * 6) (int16 0)

    let effect = new BasicEffect(graphics)

    let addVertex (v:Vector3) (c:Color) (t: Vector2) =
        vertices.[vertexCount] <- VertexPositionColorTexture(v, c, t)
        vertexCount <- vertexCount + 1

    let addDegenerate () =
        let v = vertices.[vertexCount - 1]
        addVertex v.Position Color.Pink v.TextureCoordinate

    member val World = Matrix.Identity
    member val View = Matrix.Identity

    member val Projection =
        let vp = graphics.Viewport
        #if PSM || DIRECTX
        Matrix.CreateOrthographicOffCenter(0.0f, float32 vp.Width, float32 vp.Height, 0.0f, -1.0f, 0.0f)
        #else
        Matrix.CreateOrthographicOffCenter(0.0f, float32 vp.Width, float32 vp.Height, 0.0f, 0.0f, 1.0f)
        #endif

    member s.Flush () =
        if vertexCount > 0 then
            effect.VertexColorEnabled <- true
            effect.TextureEnabled <- true
            for pass in effect.CurrentTechnique.Passes do
                pass.Apply()
                graphics.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, vertexCount - 2)

            vertexCount <- 0

    member s.Glyph (font: NoobishFont) (center: Vector2) (halfSize: Vector2) (color: Color) (glyph: NoobishGlyph) =

        if vertexCount + 4 > vertices.Length then
            s.Flush()

        let halfWidth = halfSize.X
        let halfHeight = halfSize.Y

        let textureWidth = float32 font.Texture.Width
        let textureHeight = float32 font.Texture.Height

        let struct(top, right, bottom, left) = glyph.AtlasBounds
        let u = left / textureWidth
        let u2 = right / textureWidth
        let v = 1f - top / textureHeight
        let v2 = v  + (top - bottom) / textureHeight

        let t1 = Vector2(u, v)
        let p1 = center + Vector2(-halfWidth, -halfHeight)
        let t2 = Vector2(u2, v)
        let p2 = center + Vector2(halfWidth, -halfHeight)
        let t3 = Vector2(u, v2)
        let p3 = center + Vector2(-halfWidth, halfHeight)
        let t4 = Vector2(u2, v2)
        let p4 = center + Vector2(halfWidth, halfHeight)

        let layer = 0f

        if(vertexCount > 0) then
            addVertex (Vector3(p1.X, p1.Y, layer)) Color.Pink (Vector2(u2, v2))


        addVertex (Vector3(p1.X, p1.Y, layer)) color t1
        addVertex (Vector3(p2.X, p2.Y, layer)) color t2
        addVertex (Vector3(p3.X, p3.Y, layer)) color t3
        addVertex (Vector3(p4.X, p4.Y, layer)) color t4


        addDegenerate()



    member s.Draw (text:string) (font: NoobishFont) (position: Vector2) =
        effect.World <- s.World
        effect.View <- s.View
        effect.Projection <- s.Projection
        effect.Texture <- font.Texture

        let mutable nextPosX = position.X
        for c in text do
            let glyph = font.Glyphs.[int64(c)]

            let sourceRect =
                let struct(top, right, bottom, left) = glyph.AtlasBounds
                let width = int (right - left)
                let height = int (top - bottom)
                Rectangle(
                    int left,
                    font.Atlas.Height - height - int bottom,
                    int width,
                    int height
                )
            let struct(oTop, oRight, oBottom, oLeft) = glyph.PlaneBounds

            let x = oLeft + nextPosX

            let xOffset = oLeft * font.Atlas.Size
            let yOffset = oBottom * font.Atlas.Size
            let y = position.Y + (font.Metrics.LineHeight * font.Atlas.Size - float32 sourceRect.Height) - yOffset

            let size = (Vector2(float32 sourceRect.Width / 2f, float32 sourceRect.Height / 2f))
            let position = Vector2(x, y) + size

            s.Glyph font position size Color.White glyph

            let advance = glyph.Advance * font.Atlas.Size / float32 font.Metrics.EmSize
            nextPosX <- x + advance

        s.Flush()


    interface System.IDisposable with
        member s.Dispose() =
            effect.Dispose()