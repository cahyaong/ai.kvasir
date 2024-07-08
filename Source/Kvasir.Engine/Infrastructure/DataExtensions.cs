// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, November 11, 2021 5:15:47 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

using BuildParts = System.Func<Contract.ICard, System.Collections.Generic.IEnumerable<Contract.IPart>>;

public static class DataExtensions
{
    private static readonly IReadOnlyDictionary<Phase, Phase> NextPhaseByCurrentPhaseLookup =
        new Dictionary<Phase, Phase>
        {
            [Phase.Setup] = Phase.Beginning,
            [Phase.Beginning] = Phase.PrecombatMain,
            [Phase.PrecombatMain] = Phase.Combat,
            [Phase.Combat] = Phase.PostcombatMain,
            [Phase.PostcombatMain] = Phase.Ending,
            [Phase.Ending] = Phase.Beginning
        };

    private static readonly IReadOnlyDictionary<CardKind, BuildParts> PartsBuilderByCardKindLookup =
        new Dictionary<CardKind, BuildParts>
        {
            [CardKind.Land] = DataExtensions.CreateLandParts,
            [CardKind.Creature] = DataExtensions.CreateCreatureParts
        };

    public static Phase Next(this Phase currentPhase)
    {
        Guard
            .Require(currentPhase, nameof(currentPhase))
            .Is.Not.Default();

        if (!DataExtensions.NextPhaseByCurrentPhaseLookup.TryGetValue(currentPhase, out var nextPhase))
        {
            throw new KvasirException(
                "No lookup entry is defined for next phase!",
                ("Current Phase", currentPhase));
        }

        return nextPhase;
    }

    public static IPermanent AsPermanent(this ICard card, IPlayer player)
    {
        var permanent = card.AsPermanent();
        permanent.OwningPlayer = player;
        permanent.ControllingPlayer = player;

        return permanent;
    }

    public static IPermanent AsPermanent(this ICard card)
    {
        var permanent = new Permanent
        {
            Card = card,
            IsTapped = false
        };

        if (!DataExtensions.PartsBuilderByCardKindLookup.TryGetValue(card.Kind, out var buildParts))
        {
            throw new KvasirException(
                "No parts builder is defined for given card kind! " +
                ("Card Kind", card.Kind));
        }

        var parts = buildParts(card).ToArray();
        permanent.AddPart(parts);

        return permanent;
    }

    private static IEnumerable<IPart> CreateLandParts(ICard card)
    {
        if (card.Abilities.Any())
        {
            // TODO (SHOULD): Filter by ability kind!

            yield return new CharacteristicPart
            {
                ActivatedAbilities = card.Abilities
            };
        }
    }

    private static IEnumerable<IPart> CreateCreatureParts(ICard card)
    {
        yield return new CreaturePart
        {
            Power = card.Power,
            Toughness = card.Toughness,
            HasSummoningSickness = false,
            Damage = 0
        };
    }
}