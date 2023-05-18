// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICost.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, June 4, 2022 6:30:05 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public interface ICost : IDiagnostic
{
    CostKind Kind { get; }

    ICostTarget Target { get; set; }

    IParameter Parameter { get; }
}

internal sealed class UnknownCost : ICost
{
    private UnknownCost()
    {
    }

    internal static UnknownCost Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public CostKind Kind => CostKind.Unknown;

    public ICostTarget Target
    {
        get => CostTarget.Unknown;
        set => throw new NotSupportedException("Setting target is not allowed!");
    }

    public IParameter Parameter => Engine.Parameter.Unknown;
}

internal sealed class NoneCost : ICost
{
    private NoneCost()
    {
    }

    public static NoneCost Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.None;

    public CostKind Kind => CostKind.None;

    public ICostTarget Target
    {
        get => CostTarget.None;
        set => throw new NotSupportedException("Setting target is not allowed!");
    }

    public IParameter Parameter => Engine.Parameter.None;
}