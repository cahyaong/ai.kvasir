// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Effect.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, May 19, 2023 5:31:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public class Effect : IEffect
{
    public Effect()
    {
        this.Kind = EffectKind.Unknown;
        this.Parameter = Engine.Parameter.Unknown;
    }

    public static IEffect Unknown => UnknownEffect.Instance;

    public int Id => this.GetHashCode();

    public string Name => DefinedText.Unsupported;

    public EffectKind Kind { get; init; }

    public IParameter Parameter { get; init; }
}

internal sealed class UnknownEffect : IEffect
{
    private UnknownEffect()
    {
    }

    public static UnknownEffect Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public EffectKind Kind => EffectKind.Unknown;

    public IParameter Parameter => Engine.Parameter.Unknown;
}