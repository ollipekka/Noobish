module Microsoft.Xna.Framework.Graphics

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

open Noobish.TextureAtlas

type SpriteBatch with
    member spriteBatch.DrawAtlasTexture (
            texture: Texture,
            position: Vector2,
            color: Color,
            rotation: float32,
            scale: Vector2,
            effect:SpriteEffects,
            layerDepth: float32) =

        match texture.TextureType with
        | TextureType.Texture ->
            spriteBatch.Draw(
                texture.Atlas,
                position,
                texture.SourceRectangle,
                color,
                rotation,
                texture.Origin,
                scale,
                effect,
                layerDepth)
        | TextureType.NinePatch(_) -> failwith "This is for drawing textures."

    member spriteBatch.DrawAtlasNinePatch (
            texture: Texture,
            position: Vector2,
            width: float32,
            height: float32,
            color: Color,
            rotation: float32,
            scale: Vector2,
            effect:SpriteEffects,
            layerDepth: float32) =

        match texture.TextureType with
        | TextureType.Texture -> failwith "This is meant for ninepatch"
        | TextureType.NinePatch(top, right, bottom, left) ->
            let sourceRect = texture.SourceRectangle


            let topf = float32 top
            let rightf = float32 right
            let bottomf = float32 bottom
            let leftf = float32 left

            let middleWidth = texture.SourceRectangle.Width - left - right
            let middleWidthf = float32 middleWidth
            let stretchedMiddleWidth =
                width - leftf * scale.X - rightf * scale.Y

            let middleHeight = texture.SourceRectangle.Height - top - bottom
            let middleHeightf = float32 middleHeight
            let stretchedMiddleHeight =
                height - topf * scale.X - bottomf * scale.Y

            let scaleX = stretchedMiddleWidth / middleWidthf / scale.X
            let scaleY = stretchedMiddleHeight / middleHeightf / scale.Y

            do
                let topLeft = Rectangle(sourceRect.Left, sourceRect.Top, left + 1, top + 1)

                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2(leftf / 2f , topf / 2f) * scale,
                    topLeft,
                    color,
                    rotation,
                    Vector2(leftf / 2f, topf / 2f),
                    scale,
                    effect,
                    layerDepth )

            do
                let topRight = Rectangle(sourceRect.Right - right + 1, sourceRect.Top, right + 1, top + 1)

                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2(leftf * scale.X + stretchedMiddleWidth + rightf / 2f * scale.X, topf / 2f * scale.Y),
                    topRight,
                    color,
                    rotation,
                    Vector2(rightf / 2f, topf / 2f),
                    scale,
                    effect,
                    layerDepth )

            do
                let topMiddle = Rectangle(sourceRect.Left + left + 1, sourceRect.Top, sourceRect.Width - left - right, top + 1)

                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2(leftf * scale.X + (stretchedMiddleWidth)/ 2f , topf / 2f * scale.Y),
                    topMiddle,
                    color,
                    rotation,
                    Vector2(middleWidthf / 2f, topf / 2f),
                    scale * Vector2(scaleX, 1f),
                    effect,
                    layerDepth )

            do
                let middleLeft = Rectangle(sourceRect.Left, sourceRect.Top + top + 1, left + 1, sourceRect.Height - top - bottom)

                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2(leftf / 2f * scale.X , topf * scale.Y + stretchedMiddleHeight / 2f),
                    middleLeft,
                    color,
                    rotation,
                    Vector2(leftf / 2f, middleHeightf / 2f),
                    scale * Vector2(1f, scaleY),
                    effect,
                    layerDepth )


            do
                let center = Rectangle(
                        sourceRect.Left + left + 1,
                        sourceRect.Top + top + 1,
                        sourceRect.Width - left - right,
                        sourceRect.Height - top - bottom)


                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2(leftf * scale.X + stretchedMiddleWidth / 2f, topf * scale.Y + stretchedMiddleHeight / 2f),
                    center,
                    color,
                    rotation,
                    Vector2(middleWidthf / 2f, middleHeightf / 2f),
                    scale * Vector2(scaleX, scaleY),
                    effect,
                    layerDepth )

            do
                let middleRight = Rectangle(sourceRect.Right - right + 1, sourceRect.Top + top + 1, right + 1, sourceRect.Height - top - bottom)

                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2((leftf + rightf / 2f) * scale.X + stretchedMiddleWidth , topf * scale.Y + stretchedMiddleHeight / 2f),
                    middleRight,
                    color,
                    rotation,
                    Vector2(rightf / 2f, middleHeightf / 2f),
                    scale * Vector2(1f, scaleY),
                    effect,
                    layerDepth )

            do
                let bottomLeft = Rectangle(sourceRect.Left, sourceRect.Top + sourceRect.Height - bottom + 1, left + 1, bottom)

                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2(leftf / 2f * scale.X, topf * scale.Y + stretchedMiddleHeight + bottomf / 2f * scale.Y),
                    bottomLeft,
                    color,
                    rotation,
                    Vector2(leftf / 2f, bottomf / 2f),
                    scale,
                    effect,
                    layerDepth )

            do

                let bottomMiddle = Rectangle(sourceRect.Left + left + 1, sourceRect.Top + sourceRect.Height - bottom + 1, sourceRect.Width - left - right, bottom)

                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2(leftf * scale.X + stretchedMiddleWidth / 2f, topf * scale.Y + stretchedMiddleHeight + bottomf / 2f * scale.Y),
                    bottomMiddle,
                    color,
                    rotation,
                    Vector2(middleWidthf / 2f, bottomf / 2f),
                    scale * Vector2(scaleX, 1f),
                    effect,
                    layerDepth )

            do
                let bottomRight = Rectangle(sourceRect.Right - right + 1, sourceRect.Top + sourceRect.Height - bottom + 1, right, bottom)

                spriteBatch.Draw(
                    texture.Atlas,
                    position + Vector2(leftf * scale.X + stretchedMiddleWidth + rightf / 2f * scale.X, topf * scale.Y + stretchedMiddleHeight + bottomf / 2f * scale.Y),
                    bottomRight,
                    color,
                    rotation,
                    Vector2(rightf / 2f, leftf / 2f),
                    scale,
                    effect,
                    layerDepth )