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
using System.Collections.Immutable;
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

    public static void Pay(this IManaPool manaPool, IManaCost manaCost)
    {
        if (manaCost == ManaCost.Free)
        {
            return;
        }

        var canPay = manaPool.TotalAmount >= manaCost.TotalAmount;

        if (!canPay)
        {
            throw new KvasirException(
                "Mana pool must have enough amount to pay mana cost!",
                ("Mana Pool", manaCost.PrintDiagnostic()),
                ("Mana Cost", manaCost.PrintDiagnostic()));
        }

        var coloredManas = manaCost
            .RequiredManas
            .Where(mana => mana != Mana.Colorless)
            .ToImmutableArray();

        var plannedManaPool = ManaBlob.Builder
            .Create()
            .WithAmount(manaPool)
            .Build();

        var costAmount = 0;
        var poolAmount = 0;
        var paidAmount = 0;

        foreach (var coloredMana in coloredManas)
        {
            costAmount = manaCost.FindAmount(coloredMana);
            poolAmount = plannedManaPool.FindAmount(coloredMana);
            paidAmount = Math.Min(costAmount, poolAmount);

            if (paidAmount > 0)
            {
                plannedManaPool.RemoveAmount(coloredMana, paidAmount);
            }
        }

        costAmount = manaCost.FindAmount(Mana.Colorless);
        poolAmount = plannedManaPool.FindAmount(Mana.Colorless);
        paidAmount = Math.Min(costAmount, poolAmount);

        if (paidAmount > 0)
        {
            plannedManaPool.RemoveAmount(Mana.Colorless, paidAmount);
        }

        if (paidAmount < costAmount)
        {
            plannedManaPool
                .AvailableManas
                .ForEach(mana =>
                {
                    costAmount -= paidAmount;

                    poolAmount = plannedManaPool.FindAmount(mana);
                    paidAmount = Math.Min(costAmount, poolAmount);

                    if (paidAmount > 0)
                    {
                        plannedManaPool.RemoveAmount(mana, paidAmount);
                    }
                });
        }

        canPay = plannedManaPool.TotalAmount <= manaPool.TotalAmount - manaCost.TotalAmount;

        if (!canPay)
        {
            throw new KvasirException(
                "Mana pool must have enough amount to pay mana cost!",
                ("Mana Pool", manaCost.PrintDiagnostic()),
                ("Mana Cost", manaCost.PrintDiagnostic()));
        }

        foreach (var mana in manaPool.AvailableManas)
        {
            if (plannedManaPool.AvailableManas.Contains(mana))
            {
                manaPool.UpdateAmount(mana, plannedManaPool.FindAmount(mana));
            }
            else
            {
                manaPool.RemoveMana(mana);
            }
        }
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
            .Select(manaCost.PrintDiagnostic)
            .Where(token => !string.IsNullOrWhiteSpace(token));

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