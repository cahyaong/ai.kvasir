// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Permanent.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 24 January 2019 9:57:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

[DebuggerDisplay("<Permanent> {this.Name} ({this.Id})")]
public class Permanent : IPermanent
{
    private readonly IDictionary<Type, IPart> _partByTypeLookup;

    internal Permanent()
    {
        this._partByTypeLookup = new Dictionary<Type, IPart>();

        this.Card = Engine.Card.Unknown;
        this.OwningPlayer = Player.Unknown;
        this.ControllingPlayer = Player.Unknown;
        this.IsTapped = false;
    }

    public static IPermanent Unknown => UnknownPermanent.Instance;

    public int Id => this.GetHashCode();

    public string Name => this.Card.Name;

    public ICard Card { get; init; }

    public IPlayer OwningPlayer { get; set; }

    public IPlayer ControllingPlayer { get; set; }

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
                ("Existing Type(s)", $"({existingTypes.ToPrettifiedText(type => type.Name)})"));
        }

        partByTypeLookup.ForEach(this._partByTypeLookup.Add);
    }

    public void RemoveParts()
    {
        this._partByTypeLookup.Clear();
    }

    public bool HasPart<TPart>()
        where TPart : IPart
    {
        return this._partByTypeLookup.ContainsKey(typeof(TPart));
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