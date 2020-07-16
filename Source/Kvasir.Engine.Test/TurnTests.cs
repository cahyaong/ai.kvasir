// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TurnTests.cs" company="nGratis">
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
// <creation_timestamp>Sunday, July 12, 2020 1:08:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class TurnTests
    {
        public class ProcessNextPhaseMethod
        {
            [Fact]
            public void WhenInvokedForCompleteTurn_ShouldFireStateChangedInCorrectOrder()
            {
                // Arrange.

                var turn = new Turn();

                using (var monitor = turn.Monitor())
                {
                    // Act.

                    Enumerable
                        .Range(0, 5)
                        .ForEach(_ => turn.ProcessNextPhase());

                    // Assert.

                    monitor
                        .OccurredEvents
                        .Where(occurredEvent =>
                            occurredEvent.EventName == nameof(Turn.StateChanged) &&
                            occurredEvent.Parameters?.Length == 2)
                        .Select(occurredEvent => occurredEvent.Parameters[1] as Turn.StateChangedEventArgs)
                        .Where(eventArgs => eventArgs != null)
                        .Select(eventArgs => $"{eventArgs.PhaseState}_{eventArgs.StepState}")
                        .ToArray()
                        .Should().HaveCount(12)
                        .And.ContainInOrder(
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
                            "Ending_Cleanup");
                }
            }
        }
    }
}