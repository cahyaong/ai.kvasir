// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 30 January 2019 11:56:11 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using nGratis.AI.Kvasir.Contract;

public static class KvasirAssertions
{
    public static ZoneAssertion<TEntity> Must<TEntity>(this IZone<TEntity> zone)
        where TEntity : IDiagnostic
    {
        return new ZoneAssertion<TEntity>(zone);
    }

    public static TabletopAssertion Must(this ITabletop tabletop)
    {
        return new TabletopAssertion(tabletop);
    }

    public static ExecutionResultAssertion Must(this ExecutionResult executionResult)
    {
        return new ExecutionResultAssertion(executionResult);
    }
}