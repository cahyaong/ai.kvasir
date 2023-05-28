// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IZone.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, April 15, 2022 2:24:33 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Collections.Generic;

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