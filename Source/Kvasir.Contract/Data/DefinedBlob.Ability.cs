// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Ability.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, December 27, 2019 7:35:52 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;

public static partial class DefinedBlob
{
    public record Ability
    {
        public static Ability NotSupported => NotSupportedAbilityDefinition.Instance;

        public virtual AbilityKind Kind { get; init; } = AbilityKind.Unknown;

        public virtual Cost[] Costs { get; init; } = Array.Empty<Cost>();

        public virtual Effect[] Effects { get; init; } = Array.Empty<Effect>();
    }

    private sealed record NotSupportedAbilityDefinition : Ability
    {
        private NotSupportedAbilityDefinition()
        {
        }

        internal static NotSupportedAbilityDefinition Instance { get; } = new();

        public override AbilityKind Kind => AbilityKind.NotSupported;

        public override Cost[] Costs => Array.Empty<Cost>();

        public override Effect[] Effects => Array.Empty<Effect>();
    }
}