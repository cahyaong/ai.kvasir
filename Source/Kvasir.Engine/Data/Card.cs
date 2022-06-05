﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Card.cs" company="nGratis">
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
// <creation_timestamp>Saturday, June 4, 2022 6:27:52 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

[DebuggerDisplay("<Card> {this.Name} ({this.Id})")]
public class Card : ICard
{
    public Card()
    {
        this.Name = DefinedText.Unknown;
        this.Kind = CardKind.Unknown;
        this.SuperKind = CardSuperKind.Unknown;
        this.SubKinds = Array.Empty<CardSubKind>();
        this.Cost = UnknownCost.Instance;
        this.Power = -42;
        this.Toughness = -42;
        this.Abilities = Array.Empty<IAbility>();
    }

    public static ICard Unknown => UnknownCard.Instance;

    public int Id => this.GetHashCode();

    public string Name { get; init; }

    public CardKind Kind { get; init; }

    public CardSuperKind SuperKind { get; init; }

    public IReadOnlyCollection<CardSubKind> SubKinds { get; init; }

    public ICost Cost { get; init; }

    public int Power { get; init; }

    public int Toughness { get; init; }

    public IReadOnlyCollection<IAbility> Abilities { get; init; }
}