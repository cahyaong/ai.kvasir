// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PayingManaHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 15, 2023 4:39:17 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class PayingManaHandler : BaseCostHandler
{
    private readonly IJudicialAssistant _judicialAssistant;

    private readonly IActionJudge _actionJudge;

    public PayingManaHandler(IJudicialAssistant judicialAssistant, IActionJudge actionJudge)
    {
        this._judicialAssistant = judicialAssistant;
        this._actionJudge = actionJudge;
    }

    public override CostKind CostKind => CostKind.PayingMana;

    protected override ValidationResult ValidateCore(ITabletop tabletop, ICost cost, ITarget target)
    {
        var amount = cost.Parameter.FindValue<IManaCost>(ParameterKey.Amount);

        // RX-202.1a — The mana cost of an object represents what a player must spend from their mana pool to cast
        // that card. Unless an object’s mana cost includes Phyrexian mana symbols(see RX-107.4f),
        // paying that mana cost requires matching the type of any colored or colorless mana symbols as
        // well as paying the generic mana indicated in the cost.

        var canPay =
            amount == ManaCost.Free ||
            target.Player.ManaPool.CanPay(amount);

        if (canPay)
        {
            return ValidationResult.Successful;
        }

        // TODO (MUST): Move the decision for tapping permanents for mana to strategy!

        var potentialManaPool = this._judicialAssistant.CalculatePotentialManaPool(
            tabletop,
            target.Player == tabletop.ActivePlayer ? PlayerModifier.Active : PlayerModifier.NonActive);

        canPay = potentialManaPool.CanPay(amount);

        if (canPay)
        {
            return ValidationResult.Successful;
        }

        var reasons = new List<ValidationReason>
        {
            ValidationReason.Create(
                "Target player has not enough mana to pay the cost!",
                new[] { "mtg-202.1a" },
                cost)
        };

        return ValidationResult.Create(reasons);
    }

    protected override void ResolveCore(ITabletop tabletop, ICost cost, ITarget target)
    {
        var amount = cost.Parameter.FindValue<IManaCost>(ParameterKey.Amount);

        tabletop
            .Battlefield
            .FindAll()
            .Where(permanent => permanent.Controller == target.Player)
            .Where(permanent => !permanent.IsTapped)
            .Where(permanent => permanent.HasPart<CharacteristicPart>())
            .Select(permanent => new
            {
                Permanent = permanent,
                ManaAbility = permanent
                    .FindPart<CharacteristicPart>()
                    .ActivatedAbilities
                    .Single(ability => ability.CanProduceMana)
            })
            .Take(amount.TotalAmount)
            .Select(anon => Action.ActivateManaAbility(anon.Permanent, anon.ManaAbility))
            .ForEach(action => this._actionJudge.ExecuteAction(tabletop, action));

        target.Player.ManaPool.Pay(amount);
    }
}