// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HumanizerExtensions.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, December 8, 2021 6:45:54 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace System;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Humanizer;
using nGratis.AI.Kvasir.Contract;

// TODO (SHOULD): Move this extensions class to Cop.Olympus project!

public static class HumanizerExtensions
{
    private static readonly IDictionary<string, int> NumericalValueByOrdinalLookup = Enumerable
        .Range(0, 25)
        .ToImmutableDictionary(index => index.ToOrdinalWords());

    public static int ToNumericalValue(this string text)
    {
        if (!HumanizerExtensions.NumericalValueByOrdinalLookup.TryGetValue(text, out var value))
        {
            throw new KvasirException(
                "There is no matching ordinal value in lookup!",
                ("Text", text));
        }

        return value;
    }
}