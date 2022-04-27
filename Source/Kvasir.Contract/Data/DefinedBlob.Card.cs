// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Card.cs" company="nGratis">
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
// <creation_timestamp>Friday, December 27, 2019 7:22:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using nGratis.Cop.Olympus.Contract;

public static partial class DefinedBlob
{
    public record Card
    {
        public ushort Number { get; init; }

        public string SetCode { get; init; } = DefinedText.Unknown;

        public uint MultiverseId { get; init; }

        public string Name { get; init; } = DefinedText.Unknown;

        public CardKind Kind { get; init; } = CardKind.Unknown;

        public CardSuperKind SuperKind { get; init; } = CardSuperKind.Unknown;

        public CardSubKind[] SubKinds { get; init; } = Array.Empty<CardSubKind>();

        public bool IsTribal { get; init; }

        public Cost Cost { get; init; } = Cost.Unknown;

        public ushort Power { get; init; }

        public ushort Toughness { get; init; }

        public Ability[] Abilities { get; init; } = Array.Empty<Ability>();
    }
}