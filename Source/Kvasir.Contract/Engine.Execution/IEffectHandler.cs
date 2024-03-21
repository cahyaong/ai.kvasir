// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEffectHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, March 19, 2024 3:20:51 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IEffectHandler
{
    EffectKind EffectKind { get; }

    void Resolve(ITabletop tabletop, IEffect effect, ITarget target);
}