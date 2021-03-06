﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Zone.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 24 January 2019 9:55:01 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    [DebuggerDisplay("<Zone> {this.Kind}, {this._cards.Count} cards")]
    public class Zone
    {
        private readonly Stack<Card> _cards;

        public Zone(ZoneKind kind, Visibility visibility)
        {
            Guard
                .Require(kind, nameof(kind))
                .Is.Not.Default();

            Guard
                .Require(visibility, nameof(visibility))
                .Is.Not.Default();

            this._cards = new Stack<Card>();

            this.Kind = kind;
            this.Visibility = visibility;
        }

        public ZoneKind Kind { get; }

        public Visibility Visibility { get; }

        public IEnumerable<Card> Cards => this._cards;

        public void AddCard(Card card)
        {
            Guard
                .Require(card, nameof(card))
                .Is.Not.Null();

            if (this._cards.Contains(card))
            {
                throw new KvasirException(
                    $"Card instance [{card.GetHashCode()}] with name [{card.Name}] is found " +
                    $"in zone [{this.Kind}].");
            }

            this._cards.Push(card);
        }

        public Card RemoveCard()
        {
            if (this._cards.Count <= 0)
            {
                throw new KvasirException($"Zone [{this.Kind}] has no more card to remove.");
            }

            return this._cards.Pop();
        }
    }
}