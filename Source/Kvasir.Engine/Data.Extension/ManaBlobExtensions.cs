// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManaBlobExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 15, 2023 4:57:58 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public static class ManaBlobExtensions
{
    private static readonly IReadOnlyDictionary<Mana, string> SymbolByManaLookup = new Dictionary<Mana, string>
    {
        [Mana.White] = "W",
        [Mana.Blue] = "U",
        [Mana.Black] = "B",
        [Mana.Red] = "R",
        [Mana.Green] = "G"
    };

    public static bool CanPay(this IManaPool manaPool, IManaCost manaCost)
    {
        if (manaCost == ManaCost.Free)
        {
            return true;
        }

        if (manaPool.TotalAmount < manaCost.TotalAmount)
        {
            return false;
        }

        return manaCost
            .RequiredManas
            .Where(mana => mana != Mana.Colorless)
            .All(mana => manaPool.FindAmount(mana) >= manaCost.FindAmount(mana));
    }

    public static string PrintDiagnostic(this IManaCost manaCost)
    {
        if (manaCost == ManaCost.Free)
        {
            return "[/]";
        }

        var tokens = Enum
            .GetValues<Mana>()
            .Where(mana => mana != Mana.Unknown)
            .Select(manaCost.PrintDiagnostic);

        return string.Join(", ", tokens);
    }

    public static string PrintDiagnostic(this IManaCost manaCost, Mana mana)
    {
        Guard
            .Require(mana, nameof(mana))
            .Is.Not.Default();

        var quantity = manaCost.FindAmount(mana);

        if (quantity <= 0)
        {
            return mana == Mana.Colorless ? "[0]" : string.Empty;
        }

        if (mana == Mana.Colorless)
        {
            return $"[{quantity}]";
        }

        if (!ManaBlobExtensions.SymbolByManaLookup.TryGetValue(mana, out var symbol))
        {
            throw new KvasirException("Mana must have a symbol associated to it!", ("Mana", mana));
        }

        return $"[{quantity}.{symbol}]";
    }
}