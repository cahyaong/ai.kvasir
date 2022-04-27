// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundSimulator" company="nGratis">
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
// <creation_timestamp>Thursday, July 23, 2020 5:36:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class RoundSimulator
{
    private readonly IMagicEntityFactory _entityFactory;

    private readonly IRandomGenerator _randomGenerator;

    private readonly ILogger _logger;

    private Judge _judge;

    private Tabletop _tabletop;

    public RoundSimulator(IMagicEntityFactory entityFactory, IRandomGenerator randomGenerator, ILogger logger)
    {
        this._entityFactory = entityFactory;
        this._randomGenerator = randomGenerator;
        this._logger = logger;

        this._judge = Judge.Unknown;
        this._tabletop = Tabletop.Unknown;
    }

    public SimulationResult Simulate(SimulationConfig simulationConfig)
    {
        this
            .SetupTabletop()
            .SetupPlayers(simulationConfig.DefinedPlayers.ToImmutableArray())
            .SetupPlayerZones(this._tabletop.ActivePlayer)
            .SetupPlayerZones(this._tabletop.NonactivePlayer);

        while (this._tabletop.TurnId < simulationConfig.MaxTurnCount)
        {
            this._judge.ExecuteNextTurn(this._tabletop);
        }

        return new SimulationResult
        {
            Tabletop = this._tabletop
        };
    }

    private RoundSimulator SetupTabletop()
    {
        this._judge = new Judge(this._logger);

        this._tabletop = new Tabletop
        {
            Phase = Phase.Setup
        };

        return this;
    }

    private RoundSimulator SetupPlayers(ImmutableArray<DefinedBlob.Player> definedPlayers)
    {
        if (definedPlayers.Length != 2)
        {
            throw new KvasirException("Currently supporting 1 vs. 1 match!");
        }

        var firstPlayer = this._entityFactory.CreatePlayer(definedPlayers[0]);
        var secondPlayer = this._entityFactory.CreatePlayer(definedPlayers[1]);

        var firstValue = 0;
        var secondValue = 0;

        while (firstValue == secondValue)
        {
            firstValue = this._randomGenerator.RollDice(20);
            secondValue = this._randomGenerator.RollDice(20);
        }

        if (firstValue > secondValue)
        {
            this._tabletop.ActivePlayer = firstPlayer;
            this._tabletop.NonactivePlayer = secondPlayer;
        }
        else
        {
            this._tabletop.ActivePlayer = secondPlayer;
            this._tabletop.NonactivePlayer = firstPlayer;
        }

        this._tabletop.ActivePlayer.Life = 20;
        this._tabletop.NonactivePlayer.Life = 20;

        return this;
    }

    private RoundSimulator SetupPlayerZones(IPlayer player)
    {
        this._randomGenerator
            .GenerateShufflingIndexes((ushort)player.Deck.Cards.Count)
            .Select(index => player
                .Deck.Cards
                .Skip(index)
                .Take(1)
                .Single())
            .ForEach(card => player.Library.AddCardToTop(card));

        Enumerable
            .Range(0, MagicConstant.Hand.MaxCardCount)
            .Select(_ => player.Library.RemoveCardFromTop())
            .ForEach(card => player.Hand.AddCardToTop(card));

        // TODO: Implement proper `mulligan` for Rx-103.4 sub-rule.

        return this;
    }
}