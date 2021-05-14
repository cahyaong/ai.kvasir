// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockExtensions.MagicEntityFactory.cs" company="nGratis">
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
// <creation_timestamp>Monday, 28 January 2019 5:37:12 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Moq.AI.Kvasir
{
    using System.Collections.Generic;
    using System.Linq;
    using Moq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Engine;
    using nGratis.AI.Kvasir.Engine.Test;
    using nGratis.Cop.Olympus.Contract;

    internal static partial class MockExtensions
    {
        private static readonly IReadOnlyDictionary<string, DefinedBlob.Deck> DeckByCodeLookup = Enumerable
            .Empty<DefinedBlob.Deck>()
            .Append(MockBuilder.CreateDefinedElfDeck())
            .Append(MockBuilder.CreateDefinedGoblinDeck())
            .ToDictionary(deck => deck.Code);

        public static Mock<IMagicEntityFactory> WithDefaultPlayer(this Mock<IMagicEntityFactory> mockFactory)
        {
            Guard
                .Require(mockFactory, nameof(mockFactory))
                .Is.Not.Null();

            mockFactory
                .Setup(mock => mock.CreatePlayer(Arg.IsAny<DefinedBlob.Player>()))
                .Returns<DefinedBlob.Player>(definedPlayer =>
                {
                    if (!MockExtensions.DeckByCodeLookup.TryGetValue(definedPlayer.DeckCode, out var definedDeck))
                    {
                        throw new KvasirTestingException($"Deck code [{definedPlayer.DeckCode}] is not defined!");
                    }

                    return new Player
                    {
                        Kind = PlayerKind.Testing,
                        Name = definedPlayer.Name,
                        Deck = new StubDeck(definedDeck)
                    };
                })
                .Verifiable();

            return mockFactory;
        }
    }
}