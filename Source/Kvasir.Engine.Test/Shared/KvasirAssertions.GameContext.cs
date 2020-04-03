// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirAssertions.GameContext.cs" company="nGratis">
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
// <creation_timestamp>Thursday, February 20, 2020 7:18:36 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.Test
{
    using FluentAssertions;
    using FluentAssertions.Execution;
    using FluentAssertions.Primitives;
    using nGratis.Cop.Olympus.Contract;

    internal class GameContextAssertions : ReferenceTypeAssertions<GameContext, GameContextAssertions>
    {
        public GameContextAssertions(GameContext gameContext)
        {
            Guard
                .Require(gameContext, nameof(gameContext))
                .Is.Not.Null();

            this.Subject = gameContext;
        }

        protected override string Identifier { get; } = "game context";

        public AndConstraint<GameContextAssertions> HavePlayers()
        {
            using (new AssertionScope())
            {
                this
                    .Subject.ActivePlayer
                    .Should().NotBeNull($"{this.Identifier} should have active player");

                this
                    .Subject.NonactivePlayer
                    .Should().NotBeNull($"{this.Identifier} should have nonactive player");
            }

            return new AndConstraint<GameContextAssertions>(this);
        }
    }
}