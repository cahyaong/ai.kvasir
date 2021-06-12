// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicSimulation" company="nGratis">
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

namespace nGratis.AI.Kvasir.Engine
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public class MagicSimulation
    {
        private readonly IMagicEntityFactory _entityFactory;

        private readonly IRandomGenerator _randomGenerator;

        private readonly ILogger _logger;

        private Ticker _ticker;

        private Tabletop _tabletop;

        public MagicSimulation(
            IMagicEntityFactory entityFactory,
            IRandomGenerator randomGenerator,
            ILogger logger)
        {
            Guard
                .Require(entityFactory, nameof(entityFactory))
                .Is.Not.Null();

            Guard
                .Require(randomGenerator, nameof(randomGenerator))
                .Is.Not.Null();

            Guard
                .Require(logger, nameof(logger))
                .Is.Not.Null();

            this._entityFactory = entityFactory;
            this._randomGenerator = randomGenerator;
            this._logger = logger;
        }

        public SimulationResult Simulate(SimulationConfig config)
        {
            Guard
                .Require(config, nameof(config))
                .Is.Not.Null();

            this
                .SetupTabletop()
                .SetupPlayers(config.DefinedPlayers.ToImmutableArray())
                .SetupPlayerZones(this._tabletop.ActivePlayer)
                .SetupPlayerZones(this._tabletop.NonactivePlayer)
                .SetupSharedZones();

            while (this._ticker.TurnId < config.MaxTurnCount)
            {
                this._ticker.ProcessUntilEndOfTurn();
            }

            return new SimulationResult
            {
                Tabletop = this._tabletop
            };
        }

        private MagicSimulation SetupTabletop()
        {
            if (this._ticker != null)
            {
                this._ticker.StateChanged -= this.OnTickerStateChanged;
            }

            this._ticker = new Ticker();
            this._ticker.StateChanged += this.OnTickerStateChanged;

            this._tabletop = new Tabletop();

            return this;
        }

        private MagicSimulation SetupPlayers(ImmutableArray<DefinedBlob.Player> definedPlayers)
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

            this._tabletop.ActivePlayer.Opponent = this._tabletop.NonactivePlayer;
            this._tabletop.NonactivePlayer.Opponent = this._tabletop.ActivePlayer;

            return this;
        }

        private MagicSimulation SetupPlayerZones(Player player)
        {
            if (player.Deck == null)
            {
                throw new KvasirException($"Player [{player.Name}] does NOT have valid deck!");
            }

            player.Library = new Zone(ZoneKind.Library, Visibility.Hidden);
            player.Hand = new Zone(ZoneKind.Hand, Visibility.Hidden);
            player.Graveyard = new Zone(ZoneKind.Graveyard, Visibility.Public);

            this
                ._randomGenerator
                .GenerateShufflingIndexes((ushort)player.Deck.Cards.Count)
                .Select(index => player
                    .Deck.Cards
                    .Skip(index)
                    .Take(1)
                    .Single())
                .ForEach(card => player.Library.AddCard(card));

            Enumerable
                .Range(0, GameConstant.Hand.MaximumCardCount)
                .Select(_ => player.Library.RemoveCard())
                .ForEach(card => player.Hand.AddCard(card));

            // TODO: Implement proper `mulligan` for Rx-103.4 sub-rule.

            return this;
        }

        private MagicSimulation SetupSharedZones()
        {
            this._tabletop.Battlefield = new Zone(ZoneKind.Battlefield, Visibility.Public);
            this._tabletop.Stack = new Zone(ZoneKind.Stack, Visibility.Public);
            this._tabletop.Exile = new Zone(ZoneKind.Exile, Visibility.Public);
            this._tabletop.Command = new Zone(ZoneKind.Command, Visibility.Public);
            this._tabletop.Ante = new Zone(ZoneKind.Ante, Visibility.Public);

            return this;
        }

        private void OnTickerStateChanged(object sender, Ticker.StateChangedEventArgs args)
        {
            this._logger.LogInfoWithDetails(
                "Processing turn and step...",
                ("ID", $"{args.TurnId:D4}-{args.PhaseState}-{args.StepState}"),
                ("Active Player", this._tabletop.ActivePlayer.Name));

            if (args.PhaseState == Ticker.PhaseState.Ending && args.StepState == Ticker.StepState.Cleanup)
            {
                var swappedPlayer = this._tabletop.ActivePlayer;

                this._tabletop.ActivePlayer = this._tabletop.NonactivePlayer;
                this._tabletop.NonactivePlayer = swappedPlayer;
            }
        }
    }
}