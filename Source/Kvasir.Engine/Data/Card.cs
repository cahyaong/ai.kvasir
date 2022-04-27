// --------------------------------------------------------------------------------------------------------------------
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
// <creation_timestamp>Thursday, 24 January 2019 9:57:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Diagnostics;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

// TODO: Introduce concept of design- and run-time card with sharing basic properties!

[DebuggerDisplay("<Card> {this.Name} ({this.GetHashCode()})")]
public abstract class Card
{
    // FIXME: Remove card inheritance!

    protected Card(string name, CardKind kind)
    {
        this.Kind = kind;

        this.Name = !string.IsNullOrEmpty(name)
            ? name
            : throw new KvasirException($"Name must not be {DefinedText.Empty}!");

        this.Owner = Player.Unknown;
        this.Controller = Player.Unknown;
    }

    public CardKind Kind { get; init; }

    public string Name { get; init; }

    public IPlayer Owner { get; internal set; }

    public IPlayer Controller { get; internal set; }
}