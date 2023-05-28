// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Zone.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
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

internal sealed class UnknownZone<TEntity> : IZone<TEntity>
{
    private UnknownZone()
    {
    }

    internal static UnknownZone<TEntity> Instance { get; } = new();

    public ZoneKind Kind => ZoneKind.Unknown;

    public Visibility Visibility => Visibility.Unknown;

    public int Quantity =>
        throw new NotSupportedException("Getting quantity is not allowed!");

    public void AddToTop(TEntity _) =>
        throw new NotSupportedException("Adding entity to top is not allowed!");

    public TEntity FindFromTop() =>
        throw new NotSupportedException("Finding entity from top is not allowed!");

    public void RemoveFromTop() =>
        throw new NotSupportedException("Removing entity from top is not allowed!");

    public void RemoveManyFromTop(int _) =>
        throw new NotSupportedException("Removing many entities from top is not allowed!");

    public IEnumerable<TEntity> FindManyFromTop(int _) =>
        throw new NotSupportedException("Finding many entities from top is not allowed!");

    public IEnumerable<TEntity> FindAll() =>
        throw new NotSupportedException("Finding all entities is not allowed!");

    public void RemoveAll() =>
        throw new NotSupportedException("Removing all entities is not allowed!");

    public void MoveToZone<TOtherEntity>(TEntity _, IZone<TOtherEntity> __, Func<TEntity, TOtherEntity> ___) =>
        throw new NotSupportedException("Moving entity to zone is not allowed!");
}