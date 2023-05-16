// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICard.cs" company="nGratis">
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
// <creation_timestamp>Saturday, June 4, 2022 6:27:26 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public interface ICard : IDiagnostic
{
    // TODO (SHOULD): Implement the concept of owner and controller!

    CardKind Kind { get; }

    CardSuperKind SuperKind { get; }

    IReadOnlyCollection<CardSubKind> SubKinds { get; }

    ICost Cost { get; }

    int Power { get; }

    int Toughness { get; }

    IReadOnlyCollection<IAbility> Abilities { get; }
}

internal sealed class UnknownCard : ICard
{
    private UnknownCard()
    {
    }

    internal static UnknownCard Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public CardKind Kind => CardKind.Unknown;

    public CardSuperKind SuperKind => CardSuperKind.Unknown;

    public IReadOnlyCollection<CardSubKind> SubKinds =>
        throw new NotSupportedException("Getting sub kinds is not allowed!");

    public ICost Cost => Engine.Cost.Unknown;

    public int Power =>
        throw new NotSupportedException("Getting power is not allowed!");

    public int Toughness =>
        throw new NotSupportedException("Getting toughness is not allowed!");

    public IReadOnlyCollection<IAbility> Abilities =>
        throw new NotSupportedException("Getting abilities is not allowed!");
}