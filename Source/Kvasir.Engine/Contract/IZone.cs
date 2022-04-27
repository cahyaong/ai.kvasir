// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IZone.cs" company="nGratis">
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
// <creation_timestamp>Friday, April 15, 2022 2:24:33 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public interface IZone
{
    ZoneKind Kind { get; init; }

    Visibility Visibility { get; init; }

    IEnumerable<Card> Cards { get; }

    void AddCardToTop(Card card);

    Card RemoveCardFromTop();

    void MoveCardToZone(Card card, IZone zone);
}

internal class UnknownZone : IZone
{
    private UnknownZone()
    {
    }

    internal static IZone Instance { get; } = new UnknownZone();

    public ZoneKind Kind
    {
        get => ZoneKind.Unknown;
        init => throw new NotSupportedException("Setting kind is not allowed!");
    }

    public Visibility Visibility
    {
        get => Visibility.Unknown;
        init => throw new NotSupportedException("Setting visibility is not allowed!");
    }

    public IEnumerable<Card> Cards => Array.Empty<Card>();

    public void AddCardToTop(Card card)
    {
        throw new NotSupportedException("Adding card to top is not allowed!");
    }

    public Card RemoveCardFromTop()
    {
        throw new NotSupportedException("Removing card from top is not allowed!");
    }

    public void MoveCardToZone(Card card, IZone zone)
    {
        throw new NotSupportedException("Moving card to zone is not allowed!");
    }
}