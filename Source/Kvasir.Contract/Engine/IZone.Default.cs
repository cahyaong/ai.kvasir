// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IZone.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 1, 2024 12:26:25 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

public sealed class UnknownZone<TEntity> : IZone<TEntity>
{
    private UnknownZone()
    {
    }

    public static UnknownZone<TEntity> Instance { get; } = new();

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