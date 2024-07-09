// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundSimulator" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, July 23, 2020 5:36:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class RoundSimulator : IRoundSimulator
{
    private readonly IMagicEntityFactory _entityFactory;

    private readonly IRandomGenerator _randomGenerator;

    private readonly IRoundJudge _roundJudge;

    private ITabletop _tabletop;

    public RoundSimulator(IMagicEntityFactory entityFactory, IRandomGenerator randomGenerator, IRoundJudge roundJudge)
    {
        this._entityFactory = entityFactory;
        this._randomGenerator = randomGenerator;
        this._roundJudge = roundJudge;

        this._tabletop = Tabletop.Unknown;
    }

    public SimulationResult Simulate(SimulationConfig simulationConfig)
    {
        if (simulationConfig.DefinedPlayers.Count != 2)
        {
            throw new KvasirException("Currently supporting 1 vs. 1 match!");
        }

        var players = simulationConfig
            .DefinedPlayers
            .Select(this._entityFactory.CreatePlayer)
            .ToImmutableArray();

        this
            .SetupTabletop(players)
            .SetupPlayers(players)
            .SetupPlayerZones(this._tabletop.ActivePlayer)
            .SetupPlayerZones(this._tabletop.NonActivePlayer);

        var executionResults = new List<ExecutionResult>();
        var shouldContinue = true;

        while (shouldContinue)
        {
            var executionResult = this._roundJudge.ExecuteNextTurn(this._tabletop);
            executionResults.Add(executionResult);

            shouldContinue =
                !executionResult.IsTerminal &&
                this._tabletop.TurnId < simulationConfig.MaxTurnCount - 1;
        }

        return new SimulationResult(
            this._tabletop,
            executionResults
                .SelectMany(result => result.Messages)
                .ToImmutableArray());
    }

    private RoundSimulator SetupTabletop(IReadOnlyCollection<IPlayer> players)
    {
        this._tabletop = new Tabletop
        {
            Players = players,
            Phase = Phase.Setup
        };

        return this;
    }

    private RoundSimulator SetupPlayers(IReadOnlyCollection<IPlayer> players)
    {
        var firstPlayer = players.First();
        var secondPlayer = players.Last();

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
            this._tabletop.NonActivePlayer = secondPlayer;
        }
        else
        {
            this._tabletop.ActivePlayer = secondPlayer;
            this._tabletop.NonActivePlayer = firstPlayer;
        }

        this._tabletop.ActivePlayer.Life = 20;
        this._tabletop.NonActivePlayer.Life = 20;

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
            .ForEach(player.Library.AddToTop);

        Enumerable
            .Range(0, MagicConstant.Hand.MaxCardCount)
            .Select(_ => player.Library.FindFromTop())
            .ForEach(card => player.Library.MoveToZone(card, player.Hand));

        // TODO (SHOULD): Implement proper `mulligan` for sub-rule 103.4!

        return this;
    }
}