namespace Noobish.PipelineExtension

open System.Collections.Generic

open Microsoft.Xna.Framework;
open Microsoft.Xna.Framework.Content;
open Microsoft.Xna.Framework.Graphics;

open Noobish.Localization

type LocalizationBundleReader () =
    inherit ContentTypeReader<NoobishLocalizationBundle>()

    override s.Read(reader: ContentReader, input: NoobishLocalizationBundle) =

        let result = Dictionary<string, string>()

        let name = reader.ReadString()
        let count = reader.ReadInt32()

        for i = 0 to count - 1 do
            let key = reader.ReadString()
            let value = reader.ReadString()
            result.[key] <- value

        {
            Name = name
            Localizations = result :> IReadOnlyDictionary<string, string>
        }