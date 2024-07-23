// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TabletopExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, July 6, 2021 11:06:28 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class JudicialAssistant : IJudicialAssistant
{
    private readonly IExecutionManager _executionManager;

    public JudicialAssistant(IExecutionManager executionManager)
    {
        this._executionManager = executionManager;
    }

    public static IJudicialAssistant Unknown => UnknownJudicialAssistant.Instance;

    public IEnumerable<IPermanent> FindPermanents(ITabletop tabletop, PlayerModifier playerModifier)
    {
        var player = JudicialAssistant.FindPlayer(tabletop, playerModifier);

        return tabletop
            .Battlefield
            .FindAll()
            .Where(permanent => permanent.ControllingPlayer == player)
            .ToImmutableArray();
    }

    public IEnumerable<Creature> FindCreatures(
        ITabletop tabletop,
        PlayerModifier playerModifier,
        CreatureModifier creatureModifier)
    {
        Guard
            .Require(creatureModifier, nameof(creatureModifier))
            .Is.Not.Default();

        var player = JudicialAssistant.FindPlayer(tabletop, playerModifier);

        var filteredCreatures = tabletop
            .Battlefield
            .FindAll()
            .Where(permanent => permanent.Card.Kind == CardKind.Creature)
            .Select(permanent => permanent.ToProxyCreature());

        filteredCreatures = creatureModifier switch
        {
            CreatureModifier.None => filteredCreatures,

            CreatureModifier.CanAttack => filteredCreatures
                .Where(creature => creature.Permanent.ControllingPlayer == player)
                .Where(creature => !creature.Permanent.IsTapped)
                .Where(creature => !creature.HasSummoningSickness),

            CreatureModifier.CanBlock => filteredCreatures
                .Where(creature => creature.Permanent.ControllingPlayer == player)
                .Where(creature => !creature.Permanent.IsTapped),

            _ => Enumerable.Empty<Creature>()
        };

        return filteredCreatures.ToImmutableArray();
    }

    public IEnumerable<Land> FindLands(ITabletop tabletop, PlayerModifier playerModifier)
    {
        var player = JudicialAssistant.FindPlayer(tabletop, playerModifier);

        return tabletop
            .Battlefield
            .FindAll()
            .Where(permanent => permanent.Card.Kind == CardKind.Land)
            .Where(permanent => permanent.ControllingPlayer == player)
            .Select(permanent => permanent.ToProxyLand())
            .ToImmutableArray();
    }

    public IEnumerable<IAction> FindLegalActions(ITabletop tabletop, PlayerModifier playerModifier)
    {
        if (playerModifier == PlayerModifier.NonActive)
        {
            return Enumerable.Empty<IAction>();
        }

        var legalActions = new List<IAction>();
        var player = JudicialAssistant.FindPlayer(tabletop, playerModifier);

        var cards = player
            .Hand
            .FindAll()
            .ToImmutableArray();

        var canPlayLand =
            tabletop.Stack.IsEmpty &&
            tabletop.PlayedLandCount <= 0;

        if (canPlayLand)
        {
            legalActions.AddRange(cards
                .Where(card => card.Kind == CardKind.Land)
                .Select(Action.PlayCard));
        }

        var potentialManaPool = this.CalculatePotentialManaPool(tabletop, playerModifier);

        var canPlayNonLand = tabletop
            .Stack
            .FindAll()
            .All(action => action.OwningPlayer != player);

        if (canPlayNonLand)
        {
            legalActions.AddRange(cards
                .Where(card => card.Kind != CardKind.Land)
                .Where(card => potentialManaPool.CanPay(card.Cost.Parameter.FindValue<IManaCost>(ParameterKey.Amount)))
                .Select(Action.PlayCard));
        }

        return legalActions;
    }

    public IManaPool CalculatePotentialManaPool(ITabletop tabletop, PlayerModifier playerModifier)
    {
        var player = JudicialAssistant.FindPlayer(tabletop, playerModifier);

        var manaBlobBuilder = ManaBlob.Builder
            .Create()
            .WithAmount(player.ManaPool);

        var target = new Target
        {
            Player = player
        };

        tabletop
            .Battlefield
            .FindAll()
            .Where(permanent => permanent.ControllingPlayer == player)
            .Where(permanent => !permanent.IsTapped)
            .Where(permanent => permanent.HasPart<CharacteristicPart>())
            .SelectMany(permanent => permanent
                .FindPart<CharacteristicPart>()
                .ActivatedAbilities)
            .Where(ability => ability.CanProduceMana)
            .Where(ability => this
                    ._executionManager
                    .FindCostHandler(ability.Cost)
                    .Validate(tabletop, ability.Cost, target) == ValidationResult.Successful)
            .ForEach(ability => ability
                .Effect
                .Unroll()
                .Where(effect => effect.Kind == EffectKind.ProducingMana)
                .Select(effect => effect.Parameter.FindValue<IManaPool>(ParameterKey.Amount))
                .ForEach(manaPool => manaBlobBuilder.WithAmount(manaPool)));

        return manaBlobBuilder.Build();
    }

    private static IPlayer FindPlayer(ITabletop tabletop, PlayerModifier playerModifier)
    {
        Guard
            .Require(playerModifier, nameof(playerModifier))
            .Is.Not.Default();

        return playerModifier == PlayerModifier.Active
            ? tabletop.ActivePlayer
            : tabletop.NonActivePlayer;
    }
}

internal sealed class UnknownJudicialAssistant : IJudicialAssistant
{
    private UnknownJudicialAssistant()
    {
    }

    public static UnknownJudicialAssistant Instance { get; } = new();

    public IEnumerable<IPermanent> FindPermanents(ITabletop _, PlayerModifier __) =>
        throw new NotSupportedException("Finding permanents is not allowed!");

    public IEnumerable<Creature> FindCreatures(ITabletop _, PlayerModifier __, CreatureModifier ___) =>
        throw new NotSupportedException("Finding creatures is not allowed!");

    public IEnumerable<Land> FindLands(ITabletop _, PlayerModifier __) =>
        throw new NotSupportedException("Finding lands is not allowed!");

    public IEnumerable<IAction> FindLegalActions(ITabletop _, PlayerModifier __) =>
        throw new NotSupportedException("Finding legal actions is not allowed!");

    public IManaPool CalculatePotentialManaPool(ITabletop _, PlayerModifier __) =>
        throw new NotSupportedException("Calculating potential mana pool is not allowed!");
}