// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, September 24, 2020 6:18:00 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Newtonsoft.Json.Linq;

using nGratis.Cop.Olympus.Contract;

public static class JsonExtensions
{
    public static string ReadValue(this JToken token, string key)
    {
        Guard
            .Require(key, nameof(key))
            .Is.Not.Empty();

        return
            token[key]?.Value<string>() ??
            string.Empty;
    }

    public static T? ReadValue<T>(this JToken token, string key)
    {
        Guard
            .Require(key, nameof(key))
            .Is.Not.Empty();

        var fieldToken = token[key];

        return fieldToken != null
            ? fieldToken.Value<T>()
            : default;
    }
}