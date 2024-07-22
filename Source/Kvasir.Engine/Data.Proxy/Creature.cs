// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Creature.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, April 29, 2022 6:34:41 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.AI.Kvasir.Contract;

public class Creature
{
    private readonly Lazy<CreaturePart> _deferredCreaturePart;

    public Creature(IPermanent permanent)
    {
        this._deferredCreaturePart = new Lazy<CreaturePart>(
            permanent.FindPart<CreaturePart>,
            false);

        this.Permanent = permanent;
    }

    public IPermanent Permanent { get; }

    public string Name => this.Permanent.Name;

    public bool IsTapped
    {
        get => this.Permanent.IsTapped;
        set => this.Permanent.IsTapped = value;
    }

    public int Power => this._deferredCreaturePart.Value.Power;

    public int Toughness => this._deferredCreaturePart.Value.Toughness;

    public bool HasSummoningSickness
    {
        get => this._deferredCreaturePart.Value.HasSummoningSickness;
        internal set => this._deferredCreaturePart.Value.HasSummoningSickness = value;
    }

    public int Damage
    {
        get => this._deferredCreaturePart.Value.Damage;
        internal set => this._deferredCreaturePart.Value.Damage = value;
    }
}