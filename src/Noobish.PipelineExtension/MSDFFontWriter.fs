namespace Noobish.PipelineExtension

open Microsoft.Xna.Framework.Content.Pipeline
open Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler

open System.Collections.Generic

[<ContentTypeWriter>]
type MSDFFontWriter () =
    inherit ContentTypeWriter<string*MSDFFont>()


    override s.Write(writer: ContentWriter, (name: string, input: MSDFFont)) =
      writer.Write(name)
      writer.Write(input.atlas.``type``)
      writer.Write(input.atlas.distanceRange)
      writer.Write(input.atlas.size)
      writer.Write(input.atlas.width)
      writer.Write(input.atlas.height)
      writer.Write(input.atlas.yOrigin)

      writer.Write(input.metrics.emSize)
      writer.Write(input.metrics.lineHeight)
      writer.Write(input.metrics.ascender)
      writer.Write(input.metrics.descender)
      writer.Write(input.metrics.underlineY)
      writer.Write(input.metrics.underlineThickness)

      writer.Write(input.glyphs.Length)
      for f in input.glyphs do



    override s.GetRuntimeReader(targetPlatform: TargetPlatform) = "Noobish.PipelineExtension.SDFontReader, Noobish"