// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Deck.cs" company="nGratis">
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
// <creation_timestamp>Friday, December 27, 2019 7:41:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract
{
    using System.Collections.Generic;
    using System.Linq;
    using nGratis.Cop.Olympus.Contract;

    public static partial class DefinedBlob
    {
        public record Deck
        {
            private readonly IDictionary<Card, ushort> _quantityByCardLookup;

            private Deck()
            {
                this._quantityByCardLookup = new Dictionary<Card, ushort>();
            }

            public ushort this[Card card]
            {
                get
                {
                    Guard
                        .Require(card, nameof(card))
                        .Is.Not.Null();

                    return this._quantityByCardLookup.TryGetValue(card, out var quantity)
                        ? quantity
                        : (ushort)0;
                }

                private set => this._quantityByCardLookup[card] = value;
            }

            public ushort this[string name, string cardSetCode, ushort number]
            {
                get => this[new Card(name, cardSetCode, number)];

                private set => this[new Card(name, cardSetCode, number)] = value;
            }

            public string Code { get; private set; }

            public string Name { get; private set; }

            public IEnumerable<Card> Cards => this._quantityByCardLookup.Keys;

            public ushort CardQuantity { get; private set; }

            public class Builder
            {
                private readonly Deck _deck;

                private Builder()
                {
                    this._deck = new Deck();
                }

                public static Builder Create() => new();

                public Builder WithCode(string code)
                {
                    Guard
                        .Require(code, nameof(code))
                        .Is.Not.Empty();

                    this._deck.Code = code;

                    return this;
                }

                public Builder WithName(string name)
                {
                    Guard
                        .Require(name, nameof(name))
                        .Is.Not.Empty();

                    this._deck.Name = name;

                    return this;
                }

                public Builder WithCardAndQuantity(Card card, ushort quantity)
                {
                    this._deck[card] = quantity;

                    return this;
                }

                public Builder WithCardAndQuantity(string name, string cardSetCode, ushort number, ushort quantity)
                {
                    this._deck[name, cardSetCode, number] = quantity;

                    return this;
                }

                public Deck Build()
                {
                    // TODO: Add mandatory properties validation!

                    this._deck.CardQuantity = (ushort)this
                        ._deck
                        ._quantityByCardLookup
                        .Values
                        .Aggregate(0, (total, quantity) => total += quantity);

                    return this._deck;
                }
            }

            public record Card
            {
                public Card(string name, string cardSetCode, ushort number)
                {
                    Guard
                        .Require(name, nameof(name))
                        .Is.Not.Empty();

                    Guard
                        .Require(cardSetCode, nameof(cardSetCode))
                        .Is.Not.Empty();

                    Guard
                        .Require(number, nameof(number))
                        .Is.GreaterThan(0);

                    this.Name = name;
                    this.CardSetCode = cardSetCode;
                    this.Number = number;
                }

                public string Name { get; }

                public string CardSetCode { get; }

                public ushort Number { get; }
            }
        }
    }
}