﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Zone.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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
// <creation_timestamp>Thursday, 24 January 2019 9:55:01 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System.Collections.Generic;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    public abstract class Zone
    {
        private readonly HashSet<Card> _cards;

        protected Zone()
        {
            this._cards = new HashSet<Card>();
        }

        public abstract ZoneKind Kind { get; }

        public IEnumerable<Card> Cards => this._cards;

        public void AddCard(Card card)
        {
            Guard
                .Require(card, nameof(card))
                .Is.Not.Null();

            if (this._cards.Contains(card))
            {
                throw new KvasirException(
                    $"Card instance [{card.GetHashCode()}] with name [{card.Name}] is in found " +
                    $"in zone [{this.Kind}].");
            }

            this._cards.Add(card);
        }
    }
}