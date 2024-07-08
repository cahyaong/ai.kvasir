// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICost.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, July 5, 2024 12:21:58 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public sealed class UnknownCost : ICost
{
    private UnknownCost()
    {
    }

    public static UnknownCost Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public CostKind Kind => CostKind.Unknown;

    public IParameter Parameter => UnknownParameter.Instance;
}

public sealed class NoneCost : ICost
{
    private NoneCost()
    {
    }

    public static NoneCost Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.None;

    public CostKind Kind => CostKind.MarkingNone;

    public IParameter Parameter => NoneParameter.Instance;
}