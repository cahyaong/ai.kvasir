// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefinedBlob.Card.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2019 Cahya Ong
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
// <creation_timestamp>Friday, December 27, 2019 7:22:02 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract
{
    public static partial class DefinedBlob
    {
        public class Card
        {
            public Card()
            {
                this.SubKinds = Default.SubKinds;
                this.Cost = Cost.Unknown;
                this.Abilities = Default.Abilities;
            }

            public uint MultiverseId { get; set; }

            public string Name { get; set; }

            public CardKind Kind { get; set; }

            public CardSuperKind SuperKind { get; set; }

            public CardSubKind[] SubKinds { get; set; }

            public bool IsTribal { get; set; }

            public Cost Cost { get; set; }

            public ushort Power { get; set; }

            public ushort Toughness { get; set; }

            public Ability[] Abilities { get; set; }

            public static class Default
            {
                public static readonly CardSubKind[] SubKinds = new CardSubKind[0];

                public static readonly Ability[] Abilities = new Ability[0];
            }
        }
    }
}