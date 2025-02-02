// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.EntityFactory.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 28 January 2019 5:37:12 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq;

using System.Collections.Generic;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;
using nGratis.AI.Kvasir.Framework;

using Arg = nGratis.Cop.Olympus.Framework.Arg;
using MockBuilder = nGratis.AI.Kvasir.Framework.MockBuilder;

internal static partial class MockExtensions
{
    private static readonly IReadOnlyDictionary<string, DefinedBlob.Deck> DeckByCodeLookup = Enumerable
        .Empty<DefinedBlob.Deck>()
        .Append(MockBuilder.CreateDefinedElfDeck())
        .Append(MockBuilder.CreateDefinedGoblinDeck())
        .ToDictionary(deck => deck.Code);

    public static Mock<IEntityFactory> WithDefaultPlayer(this Mock<IEntityFactory> mockFactory)
    {
        mockFactory
            .Setup(mock => mock.CreatePlayer(Arg.IsAny<DefinedBlob.Player>()))
            .Returns<DefinedBlob.Player>(definedPlayer =>
            {
                if (!MockExtensions.DeckByCodeLookup.TryGetValue(definedPlayer.DeckCode, out var definedDeck))
                {
                    throw new KvasirTestingException(
                        "No lookup entry is defined for deck!",
                        ("Code", definedPlayer.DeckCode));
                }

                var mockStrategy = MockBuilder
                    .CreateMock<IStrategy>()
                    .WithDefault();

                return new Player
                {
                    Kind = PlayerKind.Testing,
                    Name = definedPlayer.Name,
                    Deck = new StubDeck(definedDeck),
                    Strategy = mockStrategy.Object
                };
            })
            .Verifiable();

        return mockFactory;
    }
}