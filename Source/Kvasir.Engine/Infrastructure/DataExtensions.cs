// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataExtensions.cs" company="nGratis">
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
// <creation_timestamp>Thursday, November 11, 2021 5:15:47 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public static class DataExtensions
{
    private static readonly IReadOnlyDictionary<Phase, Phase> NextPhaseByCurrentPhaseLookup =
        new Dictionary<Phase, Phase>
        {
            [Phase.Setup] = Phase.Beginning,
            [Phase.Beginning] = Phase.PrecombatMain,
            [Phase.PrecombatMain] = Phase.Combat,
            [Phase.Combat] = Phase.PostcombatMain,
            [Phase.PostcombatMain] = Phase.Ending,
            [Phase.Ending] = Phase.Beginning
        };

    public static Phase Next(this Phase currentPhase)
    {
        Guard
            .Require(currentPhase, nameof(currentPhase))
            .Is.Not.Default();

        if (!DataExtensions.NextPhaseByCurrentPhaseLookup.TryGetValue(currentPhase, out var nextPhase))
        {
            throw new KvasirException(
                "No lookup entry is defined for next phase!",
                ("Current Phase", currentPhase));
        }

        return nextPhase;
    }
}