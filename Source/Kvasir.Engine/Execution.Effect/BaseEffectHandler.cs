// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseEffectHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, March 19, 2024 3:21:18 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public abstract class BaseEffectHandler : IEffectHandler
{
    public abstract EffectKind EffectKind { get; }

    public void Resolve(ITabletop tabletop, IEffect effect, ITarget target)
    {
        if (effect.Kind != this.EffectKind)
        {
            throw new KvasirException(
                "Handler is expecting correct effect kind!",
                ("Actual Kind", effect.Kind),
                ("Expected Kind", this.EffectKind));
        }

        this.ResolveCore(tabletop, effect, target);
    }

    protected abstract void ResolveCore(ITabletop tabletop, IEffect effect, ITarget target);
}