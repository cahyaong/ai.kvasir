// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Ability.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, December 27, 2019 7:35:52 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;

public static partial class DefinedBlob
{
    public record Ability
    {
        public static Ability NotSupported => NotSupportedAbilityDefinition.Instance;

        public virtual AbilityKind Kind { get; init; }

        public virtual Cost[] Costs { get; init; } = Default.Costs;

        public virtual Effect[] Effects { get; init; } = Default.Effects;

        protected static class Default
        {
            public static readonly Cost[] Costs = Array.Empty<Cost>();

            public static readonly Effect[] Effects = Array.Empty<Effect>();
        }
    }

    internal sealed record NotSupportedAbilityDefinition : Ability
    {
        private NotSupportedAbilityDefinition()
        {
        }

        internal static NotSupportedAbilityDefinition Instance { get; } = new();

        public override AbilityKind Kind => AbilityKind.NotSupported;

        public override Cost[] Costs => Default.Costs;

        public override Effect[] Effects => Default.Effects;
    }
}