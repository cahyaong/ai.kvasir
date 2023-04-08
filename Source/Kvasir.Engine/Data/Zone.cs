// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Zone.cs" company="nGratis">
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
// <creation_timestamp>Thursday, 24 January 2019 9:55:01 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

[DebuggerDisplay("<Zone> {this.Kind}, {this.Quantity} entities")]
public class Zone<TEntity> : IZone<TEntity>
    where TEntity : IDiagnostic
{
    private readonly List<TEntity> _entities;

    public Zone()
    {
        this._entities = new List<TEntity>();

        this.Kind = ZoneKind.Unknown;
        this.Visibility = Visibility.Unknown;
    }

    public static IZone<TEntity> Unknown => UnknownZone<TEntity>.Instance;

    public ZoneKind Kind { get; init; }

    public Visibility Visibility { get; init; }

    public int Quantity => this._entities.Count;

    public void AddToTop(TEntity entity)
    {
        if (this._entities.Contains(entity))
        {
            throw new KvasirException(
                "Zone has existing entity!",
                ("Zone Kind", this.Kind),
                ("Entity ID", entity.Id),
                ("Entity Name", entity.Name));
        }

        this._entities.Add(entity);
    }

    public TEntity FindFromTop()
    {
        if (this.Quantity <= 0)
        {
            throw new KvasirException(
                "Zone has no more entity to find!",
                ("Zone Kind", this.Kind));
        }

        return this._entities.Last();
    }

    public IEnumerable<TEntity> FindManyFromTop(int count)
    {
        Guard
            .Require(count, nameof(count))
            .Is.GreaterThan(0);

        if (count > this.Quantity)
        {
            throw new KvasirException(
                "Zone has not enough entities to find many!",
                ("Zone Kind", this.Kind),
                ("Quantity", this.Quantity),
                ("Finding Count", count));
        }

        return this
            ._entities
            .Skip(this.Quantity - count)
            .Reverse();
    }

    public IEnumerable<TEntity> FindAll()
    {
        return this
            ._entities
            .AsEnumerable()
            .Reverse();
    }

    public void RemoveFromTop()
    {
        if (this.Quantity <= 0)
        {
            throw new KvasirException(
                "Zone has no more entity to remove!",
                ("Zone Kind", this.Kind));
        }

        this._entities.RemoveAt(this.Quantity - 1);
    }

    public void RemoveManyFromTop(int count)
    {
        Guard
            .Require(count, nameof(count))
            .Is.GreaterThan(0);

        if (count > this.Quantity)
        {
            throw new KvasirException(
                "Zone has not enough entities to remove many!",
                ("Zone Kind", this.Kind),
                ("Quantity", this.Quantity),
                ("Removing Count", count));
        }

        this._entities.RemoveRange(this.Quantity - count, count);
    }

    public void RemoveAll()
    {
        this._entities.Clear();
    }

    public void MoveToZone<TOtherEntity>(TEntity entity, IZone<TOtherEntity> zone, Func<TEntity, TOtherEntity> convert)
    {
        var matchedIndex = this._entities.IndexOf(entity);

        if (matchedIndex < 0)
        {
            throw new KvasirException(
                "Zone does not contain entity to move to different zone!",
                ("Source Kind", this.Kind),
                ("Target Kind", zone.Kind),
                ("Entity ID", entity.Id),
                ("Entity Name", entity.Name));
        }

        zone.AddToTop(convert(entity));
        this._entities.RemoveAt(matchedIndex);
    }
}