namespace Noobish.PipelineExtension

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler
open Microsoft.Xna.Framework.Content.Pipeline.Graphics

[<ContentTypeWriter>]
type MSDFFontWriter () =
    inherit ContentTypeWriter<ExternalReference<TextureContent>*MSDFFont>()

    let writeAtlas (writer: ContentWriter) (a: MSDFAtlas) =
      writer.Write(a.``type``)
      writer.Write(a.distanceRange)
      writer.Write(a.size)
      writer.Write(a.width)
      writer.Write(a.height)
      writer.Write(a.yOrigin)


    let writeMetrics (writer: ContentWriter) (m: MSDFMetrics) =
       writer.Write(m.emSize)
       writer.Write(m.lineHeight)
       writer.Write(m.ascender)
       writer.Write(m.descender)
       writer.Write(m.underlineY)
       writer.Write(m.underlineThickness)

    let writeBounds (writer: ContentWriter) (b: MSDFBounds) =
      writer.Write(b.top)
      writer.Write(b.right)
      writer.Write(b.bottom)
      writer.Write(b.left)

    let writeGlyph (writer:ContentWriter) (g: MSDFGlyph) =

      writer.Write(g.unicode)
      writer.Write(g.advance)

      do
         let hasAtlasBounds = (g.atlasBounds |> box |> isNull |> not)
         if hasAtlasBounds then
            writeBounds writer g.atlasBounds
         else
            writeBounds writer {top = 0f; right = 0f; bottom = 0f; left = 0f}

      do
         let hasPlaneBounds = (g.planeBounds |> box |> isNull |> not )
         if hasPlaneBounds then
            writeBounds writer g.planeBounds
         else
            writeBounds writer {top = 0f; right = 0f; bottom = 0f; left = 0f}

    let writeKerning (writer:ContentWriter) (k: MSDFKerning) =
      writer.Write(k.unicode1)
      writer.Write(k.unicode2)
      writer.Write(k.advance)

    override s.Write(writer: ContentWriter, (atlas: ExternalReference<TextureContent>, input: MSDFFont)) =

      writer.WriteExternalReference atlas
      writeAtlas writer input.atlas
      writeMetrics writer input.metrics

      writer.Write(input.glyphs.Length)
      for g in input.glyphs do
         writeGlyph writer g
      writer.Write(input.kerning.Length)
      for k in input.kerning do
         writeKerning writer k



    override s.GetRuntimeReader(targetPlatform: TargetPlatform) = "Noobish.PipelineExtension.MSDFFontReader, Noobish"