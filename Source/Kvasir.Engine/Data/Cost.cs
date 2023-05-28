// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Cost.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, June 4, 2022 6:43:33 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class Cost : ICost
{
    public Cost()
    {
        this.Kind = CostKind.Unknown;
        this.Parameter = Engine.Parameter.Unknown;
    }

    public static ICost Unknown => UnknownCost.Instance;

    public static ICost None => NoneCost.Instance;

    public int Id => this.GetHashCode();

    public string Name => DefinedText.Unsupported;

    public CostKind Kind { get; init; }

    public IParameter Parameter { get; init; }
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

    public CostKind Kind => CostKind.DoingNothing;

    public IParameter Parameter => Engine.Parameter.None;
}