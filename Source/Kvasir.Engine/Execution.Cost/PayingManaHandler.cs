// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayingManaHandler.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
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