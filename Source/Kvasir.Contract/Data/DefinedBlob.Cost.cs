// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Cost.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
// <creation_timestamp>Friday, December 27, 2019 7:27:11 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract
{
    using System;
    using nGratis.Cop.Olympus.Contract;

    public static partial class DefinedBlob
    {
        public class Cost
        {
            public static Cost Unknown { get; } = UnknownCostDefinition.Instance;

            public static Cost Free { get; } = FreeCostDefinition.Instance;

            public virtual CostKind Kind { get; set; }

            public virtual string Amount { get; set; }
        }

        internal sealed class UnknownCostDefinition : Cost
        {
            private UnknownCostDefinition()
            {
            }

            internal static UnknownCostDefinition Instance { get; } = new UnknownCostDefinition();

            public override CostKind Kind
            {
                get => CostKind.Unknown;
                set => throw new NotSupportedException("Setting kind is not allowed!");
            }

            public override string Amount
            {
                get => Text.Unknown;
                set => throw new NotSupportedException("Setting amount is not allowed!");
            }
        }

        internal sealed class FreeCostDefinition : Cost
        {
            private FreeCostDefinition()
            {
            }

            internal static FreeCostDefinition Instance { get; } = new FreeCostDefinition();

            public override CostKind Kind
            {
                get => CostKind.Free;
                set => throw new NotSupportedException("Setting kind is not allowed!");
            }

            public override string Amount
            {
                get => string.Empty;
                set => throw new NotSupportedException("Setting amount is not allowed!");
            }
        }
    }
}