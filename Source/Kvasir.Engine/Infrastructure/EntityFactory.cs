﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityFactory.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 28 January 2019 5:04:00 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;

public class EntityFactory : IEntityFactory
{
    private static readonly IReadOnlyDictionary<string, DefinedBlob.Deck> DefinedDeckByCodeLookup =
        new Dictionary<string, DefinedBlob.Deck>
        {
            ["RED_01"] = DefinedBlob.Deck.Builder
                .Create()
                .WithCode("RED_01")
                .WithName("Basic Red Creature")
                .WithCardAndQuantity("Goblin Bully", "POR", 131, 4)
                .WithCardAndQuantity("Highland Giant", "POR", 132, 4)
                .WithCardAndQuantity("Hill Giant", "POR", 133, 4)
                .WithCardAndQuantity("Mountain", "POR", 208, 8)
                .Build(),
            ["WHITE_01"] = DefinedBlob.Deck.Builder
                .Create()
                .WithCode("WHITE_01")
                .WithName("Basic White Creature")
                .WithCardAndQuantity("Devoted Hero", "POR", 13, 4)
                .WithCardAndQuantity("Knight Errant", "POR", 20, 4)
                .WithCardAndQuantity("Border Guard", "POR", 9, 4)
                .WithCardAndQuantity("Plains", "POR", 196, 8)
                .Build()
        };

    private readonly IProcessedMagicRepository _processedRepository;

    private readonly IRandomGenerator _randomGenerator;

    private readonly IJudicialAssistant _judicialAssistant;

    public EntityFactory(
        IProcessedMagicRepository processedRepository,
        IRandomGenerator randomGenerator,
        IJudicialAssistant judicialAssistant)
    {
        this._processedRepository = processedRepository;
        this._randomGenerator = randomGenerator;
        this._judicialAssistant = judicialAssistant;
    }

    public IRandomGenerator CreateRandomGenerator(string id, int seed) => new RandomGenerator(id, seed);

    public IPlayer CreatePlayer(DefinedBlob.Player definedPlayer)
    {
        if (!EntityFactory.DefinedDeckByCodeLookup.TryGetValue(definedPlayer.DeckCode, out var definedDeck))
        {
            throw new KvasirException(
                "No deck is defined with given code!",
                ("Deck Code", definedPlayer.DeckCode));
        }

        return new Player
        {
            Name = definedPlayer.Name,
            Deck = this.CreateDeck(definedDeck),
            Strategy = new RandomStrategy(this._randomGenerator, this._judicialAssistant)
        };
    }

    private IDeck CreateDeck(DefinedBlob.Deck definedDeck)
    {
        // TODO (SHOULD): Share instance for card with the same name and set!

        var cards = definedDeck
            .Entries
            .Select(definedEntry => new
            {
                DefinedCard = this
                    ._processedRepository
                    .LoadCardAsync(definedEntry.SetCode, definedEntry.Number)
                    .RunSynchronously<DefinedBlob.Card>(),
                Quantity = definedDeck[definedEntry]
            })
            .SelectMany(anon => Enumerable
                .Range(0, anon.Quantity)
                .Select(_ => EntityFactory.CreateCard(anon.DefinedCard)))
            .ToImmutableArray();

        return new Deck
        {
            Cards = cards
        };
    }

    private static ICard CreateCard(DefinedBlob.Card definedCard)
    {
        var abilities = ImmutableArray<IAbility>.Empty;

        if (definedCard.Abilities.Any())
        {
            abilities = definedCard
                .Abilities
                .Select(EntityFactory.CreateAbility)
                .ToImmutableArray();
        }

        return new Card
        {
            Name = definedCard.Name,
            Kind = definedCard.Kind,
            SuperKind = definedCard.SuperKind,
            SubKinds = definedCard.SubKinds,
            Power = definedCard.Power,
            Toughness = definedCard.Toughness,
            Cost = EntityFactory.CreateCost(definedCard.Cost),
            Abilities = abilities
        };
    }

    private static ICost CreateCost(DefinedBlob.Cost definedCost)
    {
        return definedCost.Kind switch
        {
            CostKind.PayingMana => EntityFactory.ConvertCost((DefinedBlob.PayingManaCost)definedCost),

            _ => new Cost
            {
                Kind = definedCost.Kind,
                Parameter = Parameter.None
            }
        };
    }

    private static ICost ConvertCost(DefinedBlob.PayingManaCost definedCost)
    {
        var manaCost = (IManaCost)ManaBlob.Builder
            .Create()
            .WithAmount(definedCost)
            .Build();

        if (manaCost.TotalAmount <= 0)
        {
            manaCost = ManaCost.Free;
        }

        return new Cost
        {
            Kind = CostKind.PayingMana,
            Parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, manaCost)
                .Build()
        };
    }

    private static IAbility CreateAbility(DefinedBlob.Ability definedAbility)
    {
        var costs = definedAbility
            .Costs
            .Select(EntityFactory.CreateCost)
            .ToImmutableArray();

        var effects = definedAbility
            .Effects
            .Select(EntityFactory.CreateEffect)
            .ToImmutableArray();

        var canProduceMana = effects
            .Any(effect => effect.Kind == EffectKind.ProducingMana);

        return new Ability
        {
            CanProduceMana = canProduceMana,
            Cost = costs.Roll(),
            Effect = effects.Roll()
        };
    }

    private static IEffect CreateEffect(DefinedBlob.Effect definedEffect)
    {
        return definedEffect.Kind switch
        {
            EffectKind.ProducingMana => EntityFactory.ConvertEffect(
                (DefinedBlob.ProducingManaEffect)definedEffect),

            _ => throw new KvasirException(
                "Creating effect must be implemented explicitly!",
                ("Kind", definedEffect.Kind))
        };
    }

    private static IEffect ConvertEffect(DefinedBlob.ProducingManaEffect definedEffect)
    {
        var manaPool = (IManaPool)ManaBlob.Builder
            .Create()
            .WithAmount(definedEffect)
            .Build();

        return new Effect
        {
            Kind = EffectKind.ProducingMana,
            Parameter = Parameter.Builder
                .Create()
                .WithValue(ParameterKey.Amount, manaPool)
                .Build()
        };
    }
}