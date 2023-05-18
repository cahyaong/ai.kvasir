// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HumanizerExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, December 8, 2021 6:45:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace System;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Humanizer;
using nGratis.AI.Kvasir.Contract;

// TODO (SHOULD): Move this extensions class to Cop.Olympus project!

public static class HumanizerExtensions
{
    private static readonly IDictionary<string, int> NumericalValueByOrdinalLookup = Enumerable
        .Range(0, 25)
        .ToImmutableDictionary(index => index.ToOrdinalWords());

    public static int ToNumericalValue(this string text)
    {
        if (!HumanizerExtensions.NumericalValueByOrdinalLookup.TryGetValue(text, out var value))
        {
            throw new KvasirException(
                "There is no matching ordinal value in lookup!",
                ("Text", text));
        }

        return value;
    }

    public static string ToCorrectPluralization(this string text, int quantity)
    {
        return quantity <= 1
            ? text.Singularize(false, true)
            : text.Pluralize(false);
    }
}