// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AbilityDefinition.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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
// <creation_timestamp>Friday, 11 January 2019 11:42:38 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract
{
    // TODO: Remove Base- prefix from all abstract classes!

    public class AbilityDefinition
    {
        public static AbilityDefinition NotSupported { get; } = NotSupportedAbilityDefinition.Instance;

        public virtual AbilityKind Kind { get; set; }

        public virtual CostDefinition[] CostDefinitions { get; set; } = Default.CostDefinitions;

        public virtual EffectDefinition[] EffectDefinitions { get; set; } = Default.EffectDefinitions;

        protected static class Default
        {
            public static readonly CostDefinition[] CostDefinitions = new CostDefinition[0];

            public static readonly EffectDefinition[] EffectDefinitions = new EffectDefinition[0];
        }
    }
}