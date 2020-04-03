// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Arg.cs" company="nGratis">
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
// <creation_timestamp>Friday, 23 November 2018 9:26:00 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable CheckNamespace

namespace Moq.AI.Kvasir
{
    using Moq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public partial class Arg : Moq.Arg
    {
        public class UnparsedCardSet
        {
            public static UnparsedBlob.CardSet Is(string code)
            {
                Guard
                    .Require(code, nameof(code))
                    .Is.Not.Empty();

                return Match.Create<UnparsedBlob.CardSet>(cardSet => cardSet.Code == code);
            }
        }

        public class DefinedPlayer
        {
            public static DefinedBlob.Player Is(string name)
            {
                Guard
                    .Require(name, nameof(name))
                    .Is.Not.Empty();

                return Match.Create<DefinedBlob.Player>(agent => agent.Name == name);
            }
        }
    }
}