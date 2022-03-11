﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineExtensions.cs" company="nGratis">
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
// <creation_timestamp>Saturday, November 27, 2021 7:37:21 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace nGratis.AI.Kvasir.Engine
{
    using FluentAssertions;
    using nGratis.Cop.Olympus.Contract;

    internal static class EngineExtensions
    {
        public static void ExecuteCombatPhase(this TurnCoordinator turnCoordinator)
        {
            Guard
                .Require(turnCoordinator, nameof(turnCoordinator))
                .Is.Not.Null();

            turnCoordinator
                .ExecuteStep(0, Ticker.PhaseState.Combat, Ticker.StepState.DeclareAttackers)
                .HasError
                .Should().BeFalse("because declaring attackers should not fail");

            turnCoordinator
                .ExecuteStep(0, Ticker.PhaseState.Combat, Ticker.StepState.AssignBlockers)
                .HasError
                .Should().BeFalse("because assigning blockers should not fail");

            turnCoordinator
                .ExecuteStep(0, Ticker.PhaseState.Combat, Ticker.StepState.CombatDamage)
                .HasError
                .Should().BeFalse("because resolving combat damage should not fail");
        }
    }
}