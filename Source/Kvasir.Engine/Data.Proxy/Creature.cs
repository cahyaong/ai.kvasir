// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Creature.cs" company="nGratis">
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
// <creation_timestamp>Friday, April 29, 2022 6:34:41 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;

public class Creature
{
    private readonly Lazy<PermanentPart> _deferredPermanentPart;

    private readonly Lazy<CreaturePart> _deferredCreaturePart;

    public Creature(ICard card)
    {
        this._deferredPermanentPart = new Lazy<PermanentPart>(card.FindPart<PermanentPart>, false);
        this._deferredCreaturePart = new Lazy<CreaturePart>(card.FindPart<CreaturePart>, false);

        this.Card = card;
    }

    public ICard Card { get; }

    public int Power => this._deferredCreaturePart.Value.Power;

    public int Toughness => this._deferredCreaturePart.Value.Toughness;

    public bool HasSummoningSickness
    {
        get => this._deferredCreaturePart.Value.HasSummoningSickness;
        internal set => this._deferredCreaturePart.Value.HasSummoningSickness = value;
    }

    public bool IsTapped
    {
        get => this._deferredPermanentPart.Value.IsTapped;
        internal set => this._deferredPermanentPart.Value.IsTapped = value;
    }

    public int Damage
    {
        get => this._deferredCreaturePart.Value.Damage;
        internal set => this._deferredCreaturePart.Value.Damage = value;
    }
}