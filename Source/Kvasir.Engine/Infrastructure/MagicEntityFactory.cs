// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicEntityFactory.cs" company="nGratis">
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
// <creation_timestamp>Monday, 28 January 2019 5:04:00 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;
using BuildParts = System.Func<Contract.DefinedBlob.Card, System.Collections.Generic.IEnumerable<IPart>>;

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

    private static readonly IReadOnlyDictionary<CardKind, BuildParts> PartsBuilderByCardKindLookup =
        new Dictionary<CardKind, BuildParts>
        {
            [CardKind.Land] = MagicEntityFactory.CreateLandParts,
            [CardKind.Creature] = MagicEntityFactory.CreateCreatureParts
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

    public ICard CreateCard(DefinedBlob.Card definedCard)
    {
        var card = new Card
        {
            Kind = CardKind.Land,
            Name = definedCard.Name
        };

        if (!MagicEntityFactory.PartsBuilderByCardKindLookup.TryGetValue(definedCard.Kind, out var buildParts))
        {
            throw new KvasirException(
                "No parts builder is defined for given card kind! " +
                ("Card Kind", definedCard.Kind));
        }

        var parts = buildParts(definedCard).ToArray();
        card.AddParts(parts);

        return card;
    }

    private Deck CreateDeck(DefinedBlob.Deck definedDeck)
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

    private static IEnumerable<IPart> CreateLandParts(DefinedBlob.Card definedCard)
    {
        return Array.Empty<IPart>();
    }

    private static IEnumerable<IPart> CreateCreatureParts(DefinedBlob.Card definedCard)
    {
        yield return new CreaturePart
        {
            Power = definedCard.Power,
            Toughness = definedCard.Toughness,
            HasSummoningSickness = false,
            Damage = 0
        };
    }
}