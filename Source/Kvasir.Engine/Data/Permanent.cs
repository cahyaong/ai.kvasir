// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Permanent.cs" company="nGratis">
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

[DebuggerDisplay("<Permanent> {this.Name} ({this.Id})")]
public class Permanent : IPermanent
{
    private readonly IDictionary<Type, IPart> _partByTypeLookup;

    internal Permanent()
    {
        this._partByTypeLookup = new Dictionary<Type, IPart>();

        this.Card = Engine.Card.Unknown;
        this.Owner = Player.Unknown;
        this.Controller = Player.Unknown;
        this.IsTapped = false;
    }

    public static IPermanent Unknown => UnknownPermanent.Instance;

    public int Id => this.GetHashCode();

    public string Name => this.Card.Name;

    public ICard Card { get; init; }

    public IPlayer Owner { get; set; }

    public IPlayer Controller { get; set; }

    public bool IsTapped { get; set; }

    public void AddPart(params IPart[] parts)
    {
        var partByTypeLookup = parts.ToImmutableDictionary(part => part.GetType());

        var existingTypes = this
            ._partByTypeLookup.Keys
            .Intersect(partByTypeLookup.Keys)
            .ToImmutableArray();

        if (existingTypes.Any())
        {
            throw new KvasirException(
                "Component with same type must be defined once!",
                ("Existing Type(s)", existingTypes.ToPrettifiedText(type => type.Name)));
        }

        partByTypeLookup.ForEach(this._partByTypeLookup.Add);
    }

    public void RemoveParts()
    {
        this._partByTypeLookup.Clear();
    }

    public TPart FindPart<TPart>()
        where TPart : IPart
    {
        if (!this._partByTypeLookup.TryGetValue(typeof(TPart), out var part))
        {
            throw new KvasirException(
                "Permanent does not have target part!",
                ("Card Name", this.Card.Name),
                ("Card Kind", this.Card.Kind),
                ("Part Type", typeof(TPart).FullName ?? DefinedText.Unknown));
        }

        return (TPart)part;
    }
}