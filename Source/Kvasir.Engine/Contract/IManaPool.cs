// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IManaPool.cs" company="nGratis">
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
// <creation_timestamp>Sunday, March 19, 2023 3:03:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public interface IManaPool
{
    int TotalAmount { get; }

    IEnumerable<Mana> AvailableManas { get; }

    int FindAmount(Mana mana);

    void AddAmount(Mana mana, int amount);

    void RemoveAmount(Mana mana, int amount);
}

internal sealed class UnknownManaPool : IManaPool
{
    private UnknownManaPool()
    {
    }

    public static UnknownManaPool Instance { get; } = new();

    public int TotalAmount =>
        throw new NotSupportedException("Getting total amount is not allowed!");

    public IEnumerable<Mana> AvailableManas =>
        throw new NotSupportedException("Getting available manas is not allowed!");

    public int FindAmount(Mana _) =>
        throw new NotSupportedException("Finding amount is not allowed!");

    public void AddAmount(Mana _, int __) =>
        throw new NotSupportedException("Adding amount is not allowed!");

    public void RemoveAmount(Mana _, int __) =>
        throw new NotSupportedException("Removing amount is not allowed!");
}