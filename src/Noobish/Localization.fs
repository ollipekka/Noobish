module Noobish.Localization

open System.Collections.Generic

type NoobishLocalizationBundle = {
    Name: string
    Localizations: IReadOnlyDictionary<string, string>

} with
    member this.Item
        with get (tid: string) = this.Localizations.[tid]


