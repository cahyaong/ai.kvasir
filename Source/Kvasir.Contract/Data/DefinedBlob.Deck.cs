﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Deck.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
    using nGratis.Cop.Core.Contract;

    public static partial class DefinedBlob
    {
        public class Deck
        {
            private readonly IDictionary<string, ushort> _quantityByNameLookup;

            public Deck(string name)
            {
                Guard
                    .Require(name, nameof(name))
                    .Is.Not.Empty();

                this._quantityByNameLookup = new Dictionary<string, ushort>();

                this.Name = name;
            }

            public ushort this[string cardName]
            {
                get
                {
                    Guard
                        .Require(cardName, nameof(cardName))
                        .Is.Not.Empty();

                    return this._quantityByNameLookup.TryGetValue(cardName, out var quantity)
                        ? quantity
                        : (ushort)0;
                }

                set
                {
                    Guard
                        .Require(cardName, nameof(cardName))
                        .Is.Not.Empty();

                    this._quantityByNameLookup[cardName] = value;
                }
            }

            public string Name { get; }

            public IEnumerable<string> CardNames => this._quantityByNameLookup.Keys;

            public ushort CardQuantity => (ushort)this
                ._quantityByNameLookup
                .Sum(pair => pair.Value);
        }
    }
}