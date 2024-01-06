module Noobish.Localization

open System.Collections.Generic

type NoobishLocalizationBundle = {
    Name: string
    Localizations: IReadOnlyDictionary<string, string>

} with
    member this.Item
        with get (tid: string) = this.Localizations.[tid]

    member this.GetLocalizedText (key: string) = 
        let mutable value = ""
        let success = this.Localizations.TryGetValue(key, &value)
        if success then 
            value 
        else 
            $"*{key}*"


