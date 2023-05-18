// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayingManaHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 15, 2023 4:39:17 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class PayingManaHandler : BaseCostHandler
{
    public override CostKind CostKind => CostKind.PayingMana;

    protected override ValidationResult ValidateCore(ITabletop tabletop, ICost cost)
    {
        var amount = cost.Parameter.FindValue<IManaCost>(ParameterKey.Amount);

        var canPay =
            amount == ManaCost.Free ||
            cost.Target.Player.ManaPool.CanPay(amount);

        if (canPay)
        {
            return ValidationResult.Successful;
        }

        // RX-202.1a — The mana cost of an object represents what a player must spend from their mana pool to cast
        // that card. Unless an object’s mana cost includes Phyrexian mana symbols(see RX-107.4f),
        // paying that mana cost requires matching the type of any colored or colorless mana symbols as
        // well as paying the generic mana indicated in the cost.

        var reasons = new List<ValidationReason>
        {
            ValidationReason.Create(
                "Target player has not enough mana to pay the cost!",
                new[] { "mtg-202.1a" },
                cost)
        };

        return ValidationResult.Create(reasons);
    }

    protected override void PayCore(ITabletop tabletop, ICost cost)
    {
        var amount = cost.Parameter.FindValue<IManaCost>(ParameterKey.Amount);

        if (amount == ManaCost.Free)
        {
            return;
        }

        throw new NotImplementedException();
    }
}