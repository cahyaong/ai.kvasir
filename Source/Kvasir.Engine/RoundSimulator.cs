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
using nGratis.Cop.Olympus.Contract;

public class RoundSimulator
{
    private readonly IMagicEntityFactory _entityFactory;

    private readonly IRandomGenerator _randomGenerator;

    private readonly ILogger _logger;

    private RoundJudge _roundJudge;

    private ITabletop _tabletop;

    public RoundSimulator(IMagicEntityFactory entityFactory, IRandomGenerator randomGenerator, ILogger logger)
    {
        this._entityFactory = entityFactory;
        this._randomGenerator = randomGenerator;
        this._logger = logger;

        this._roundJudge = RoundJudge.Unknown;
        this._tabletop = Tabletop.Unknown;
    }

    public SimulationResult Simulate(SimulationConfig simulationConfig)
    {
        this
            .SetupTabletop()
            .SetupPlayers(simulationConfig.DefinedPlayers.ToImmutableArray())
            .SetupPlayerZones(this._tabletop.ActivePlayer)
            .SetupPlayerZones(this._tabletop.NonActivePlayer);

        var executionResults = new List<ExecutionResult>();

        while (this._tabletop.TurnId < simulationConfig.MaxTurnCount - 1)
        {
            executionResults.Add(this._roundJudge.ExecuteNextTurn(this._tabletop));
        }

        return SimulationResult.Create(
            this._tabletop,
            executionResults
                .SelectMany(result => result.Messages)
                .ToImmutableArray()
        );
    }

    private RoundSimulator SetupTabletop()
    {
        this._roundJudge = new RoundJudge(this._logger);

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
            .ForEach(card => player.Library.AddToTop(card));

        Enumerable
            .Range(0, MagicConstant.Hand.MaxCardCount)
            .Select(_ => player.Library.FindFromTop())
            .ForEach(card => player.Library.MoveToZone(card, player.Hand));

        // TODO (SHOULD): Implement proper `mulligan` for sub-rule 103.4!

        return this;
    }
}