// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IZone.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, April 15, 2022 2:24:33 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public interface IZone<TEntity>
{
    ZoneKind Kind { get; }

    Visibility Visibility { get; }

    int Quantity { get; }

    bool IsEmpty => this.Quantity <= 0;

    void AddToTop(TEntity entity);

    TEntity FindFromTop();

    void RemoveFromTop();

    void RemoveManyFromTop(int count);

    IEnumerable<TEntity> FindManyFromTop(int count);

    IEnumerable<TEntity> FindAll();

    void RemoveAll();

    void MoveToZone<TOtherEntity>(TEntity entity, IZone<TOtherEntity> zone, Func<TEntity, TOtherEntity> convert);

    void MoveToZone(TEntity entity, IZone<TEntity> zone)
    {
        this.MoveToZone(entity, zone, _ => entity);
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
        throw new NotImplementedException("Removing all entities is not allowed!");

    public void MoveToZone<TOtherEntity>(TEntity _, IZone<TOtherEntity> __, Func<TEntity, TOtherEntity> ___) =>
        throw new NotSupportedException("Moving entity to zone is not allowed!");
}