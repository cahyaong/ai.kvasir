// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TickerTests.cs" company="nGratis">
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
// <creation_timestamp>Sunday, July 12, 2020 1:08:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.Test
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using FluentAssertions;
    using FluentAssertions.Events;
    using FluentAssertions.Execution;
    using Xunit;

    public class TickerTests
    {
        public class ProcessUntilEndOfTurnMethod
        {
            [Fact]
            public void WhenInvokedOnFirstTurn_ShouldNotGenerateDrawStep()
            {
                // Arrange.

                var ticker = new Ticker();

                using var monitor = ticker.Monitor();

                // Act.

                ticker.ProcessUntilEndOfTurn();

                // Assert.

                monitor
                    .OccurredEvents
                    .ToTurnAssertionTexts()
                    .Should().HaveCount(11)
                    .And.ContainInOrder(Expected.TurnAssertionTexts.Except("Beginning_Draw"));
            }

            [Fact]
            public void WhenInvokedAfterFirstTurn_ShouldGenerateDrawStep()
            {
                // Arrange.

                var ticker = new Ticker();

                using var monitor = ticker.Monitor();

                // Act.

                ticker.ProcessUntilEndOfTurn();
                ticker.ProcessUntilEndOfTurn();

                // Assert.

                monitor
                    .OccurredEvents
                    .ToTurnAssertionTexts()
                    .Should().HaveCount(23)
                    .And.ContainInOrder(Expected
                        .TurnAssertionTexts
                        .Except("Beginning_Draw")
                        .Append(Expected.TurnAssertionTexts));
            }

            [Fact]
            public void WhenInvokedOnFirstTurn_ShouldUpdateRelevantIds()
            {
                // Arrange.

                var ticker = new Ticker();

                // Act.

                ticker.ProcessUntilEndOfTurn();

                // Assert.

                using (new AssertionScope())
                {
                    ticker
                        .TurnId
                        .Should().Be(0);

                    ticker
                        .PhaseId
                        .Should().Be(4);

                    ticker
                        .StepId
                        .Should().Be(10);
                }
            }

            [Fact]
            public void WhenInvokedAfterFirstTurn_ShouldUpdateRelevantIds()
            {
                // Arrange.

                var ticker = new Ticker();

                // Act.

                ticker.ProcessUntilEndOfTurn();
                ticker.ProcessUntilEndOfTurn();

                // Assert.

                using (new AssertionScope())
                {
                    ticker
                        .TurnId
                        .Should().Be(1);

                    ticker
                        .PhaseId
                        .Should().Be(4);

                    ticker
                        .StepId
                        .Should().Be(11);
                }
            }
        }

        private static class Expected
        {
            public static readonly ImmutableArray<string> TurnAssertionTexts = ImmutableArray.Create(
                "Beginning_Untap",
                "Beginning_Upkeep",
                "Beginning_Draw",
                "PrecombatMain_None",
                "Combat_BeginningOfCombat",
                "Combat_DeclareAttackers",
                "Combat_DeclareBlockers",
                "Combat_CombatDamage",
                "Combat_EndOfCombat",
                "PostcombatMain_None",
                "Ending_End",
                "Ending_Cleanup"
            );
        }
    }

    internal static class OccurredEventExtensions
    {
        public static IEnumerable<string> ToTurnAssertionTexts(this IEnumerable<OccurredEvent> occurredEvents)
        {
            if (occurredEvents == null)
            {
                return Enumerable.Empty<string>();
            }

            return occurredEvents
                .Where(occurredEvent =>
                    occurredEvent.EventName == nameof(Ticker.StateChanged) &&
                    occurredEvent.Parameters?.Length == 2)
                .Select(occurredEvent => occurredEvent.Parameters[1] as Ticker.StateChangedEventArgs)
                .Where(eventArgs => eventArgs != null)
                .Select(eventArgs => $"{eventArgs.PhaseState}_{eventArgs.StepState}")
                .ToImmutableList();
        }
    }
}