namespace Noobish


type NoobishLocalization = {
    Localizations: Dictionary<string, string>
} with
    member this.Item
        with get (lid: string) = this.Localizations.[lid]

type NoobishLocalizationReader () =
    inherit ContentTypeReader<NoobishFont>()

    override s.Read(reader: ContentReader, input: NoobishFont) =
        let name = reader.ReadString()

        let localizations = Dictionary<string, string>()

        let count = reader.ReadInt32()
        for i = 0 to count - 1 do
            let lid = reader.ReadString()
            let text = reader.ReadString()
            localizations.Add lid text

        {Name = name; Localizations = localizations}