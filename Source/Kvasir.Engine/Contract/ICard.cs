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
// <creation_timestamp>Thursday, April 28, 2022 4:03:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public interface ICard
{
    int Id { get; }

    CardKind Kind { get; }

    string Name { get; }

    IPlayer Owner { get; set; }

    IPlayer Controller { get; set; }

    void AddParts(params IPart[] parts);

    void RemoveParts();

    TPart FindPart<TPart>() where TPart : IPart;
}

public class UnknownCard : ICard
{
    private UnknownCard()
    {
    }

    internal static ICard Instance { get; } = new UnknownCard();

    public int Id => 0;

    public CardKind Kind => CardKind.Unknown;

    public string Name => DefinedText.Unknown;

    public IPlayer Owner
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting owner is not allowed!");
    }

    public IPlayer Controller
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting controller is not allowed!");
    }

    public void AddParts(params IPart[] parts)
    {
    }

    public void RemoveParts()
    {
    }

    public TPart FindPart<TPart>()
        where TPart : IPart
    {
        throw new NotSupportedException("Finding part is not allowed!");
    }
}