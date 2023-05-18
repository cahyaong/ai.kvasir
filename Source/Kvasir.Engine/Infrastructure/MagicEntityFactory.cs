// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicEntityFactory.cs" company="nGratis">
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
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;

public class MagicEntityFactory : IMagicEntityFactory
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

    public MagicEntityFactory(IProcessedMagicRepository processedRepository, IRandomGenerator randomGenerator)
    {
        this._processedRepository = processedRepository;
        this._randomGenerator = randomGenerator;
    }

    public IPlayer CreatePlayer(DefinedBlob.Player definedPlayer)
    {
        if (!MagicEntityFactory.DefinedDeckByCodeLookup.TryGetValue(definedPlayer.DeckCode, out var definedDeck))
        {
            throw new KvasirException(
                "No deck is defined with given code!",
                ("Deck Code", definedPlayer.DeckCode));
        }

        return new Player
        {
            Name = definedPlayer.Name,
            Deck = this.CreateDeck(definedDeck),
            Strategy = new RandomStrategy(this._randomGenerator)
        };
    }

    private IDeck CreateDeck(DefinedBlob.Deck definedDeck)
    {
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
                .Select(_ => this.CreateCard(anon.DefinedCard)))
            .ToImmutableArray();

        return new Deck
        {
            Cards = cards
        };
    }

    private ICard CreateCard(DefinedBlob.Card definedCard)
    {
        return new Card
        {
            Name = definedCard.Name,
            Kind = definedCard.Kind,
            SuperKind = definedCard.SuperKind,
            SubKinds = definedCard.SubKinds,
            Power = definedCard.Power,
            Toughness = definedCard.Toughness,
            Cost = MagicEntityFactory.CreateCost(definedCard.Cost)
        };
    }

    private static ICost CreateCost(DefinedBlob.Cost definedCost)
    {
        switch (definedCost.Kind)
        {
            case CostKind.PayingMana:
                return MagicEntityFactory.ConvertCost((DefinedBlob.PayingManaCost)definedCost);

            default:
                return new Cost
                {
                    Kind = definedCost.Kind,
                    Parameter = Parameter.None
                };
        }
    }

    private static ICost ConvertCost(DefinedBlob.PayingManaCost definedCost)
    {
        var manaCost = (IManaCost)ManaBlob.Builder
            .Create()
            .WithDefinedCost(definedCost)
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
}