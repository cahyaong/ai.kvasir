// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IEffect.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, May 19, 2023 5:27:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IEffect : IDiagnostic
{
    EffectKind Kind { get; }

    IParameter Parameter { get; }
}