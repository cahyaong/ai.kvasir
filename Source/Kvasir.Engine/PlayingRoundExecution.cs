// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayingRoundExecution.cs" company="nGratis">
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
// <creation_timestamp>Thursday, July 23, 2020 5:36:22 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public class PlayingRoundExecution : IExecution
    {
        private readonly DefinedBlob.Player[] _definedPlayers;

        private readonly IMagicEntityFactory _entityFactory;

        private readonly IRandomGenerator _randomGenerator;

        private readonly Tabletop _tabletop;

        public PlayingRoundExecution(
            IReadOnlyCollection<DefinedBlob.Player> definedPlayers,
            IMagicEntityFactory entityFactory,
            IRandomGenerator randomGenerator)
        {
            Guard
                .Require(definedPlayers, nameof(definedPlayers))
                .Is.Not.Null();

            Guard
                .Require(entityFactory, nameof(entityFactory))
                .Is.Not.Null();

            Guard
                .Require(randomGenerator, nameof(randomGenerator))
                .Is.Not.Null();

            this._definedPlayers = definedPlayers.ToArray();
            this._entityFactory = entityFactory;
            this._randomGenerator = randomGenerator;
            this._tabletop = new Tabletop();
        }

        public async Task<ExecutionResult> ExecuteAsync(ExecutionParameter parameter)
        {
            return await Task.Run(() => this.Execute(parameter));
        }

        private ExecutionResult Execute(ExecutionParameter parameter)
        {
            Guard
                .Require(parameter, nameof(parameter))
                .Is.Not.Null();

            this
                .SetupPlayers()
                .SetupPlayerZones(this._tabletop.ActivePlayer)
                .SetupPlayerZones(this._tabletop.NonactivePlayer)
                .SetupSharedZones();

            return Result.CreateSuccessful(this._tabletop);
        }

        private PlayingRoundExecution SetupPlayers()
        {
            if (this._definedPlayers.Length != 2)
            {
                throw new KvasirException("Currently supporting 1 vs. 1 match!");
            }

            var firstPlayer = this._entityFactory.CreatePlayer(this._definedPlayers[0]);
            var secondPlayer = this._entityFactory.CreatePlayer(this._definedPlayers[1]);

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

        private PlayingRoundExecution SetupPlayerZones(Player player)
        {
            if (player.Deck == null)
            {
                throw new KvasirException($"Player [{player.Name}] does NOT have valid deck!");
            }

            var cards = player
                .Deck.Cards
                .ToArray();

            player.Library = new Zone(ZoneKind.Library, Visibility.Hidden);
            player.Hand = new Zone(ZoneKind.Hand, Visibility.Hidden);
            player.Graveyard = new Zone(ZoneKind.Graveyard, Visibility.Public);

            this
                ._randomGenerator
                .GenerateShufflingIndexes((ushort)cards.Length)
                .Select(index => cards[index])
                .ForEach(card => player.Library.AddCard(card));

            Enumerable
                .Range(0, GameConstant.Hand.MaximumCardCount)
                .Select(_ => player.Library.RemoveCard())
                .ForEach(card => player.Hand.AddCard(card));

            // TODO: Implement proper `mulligan` for Rx-103.4 sub-rule.

            return this;
        }

        private PlayingRoundExecution SetupSharedZones()
        {
            this._tabletop.Battlefield = new Zone(ZoneKind.Battlefield, Visibility.Public);
            this._tabletop.Stack = new Zone(ZoneKind.Stack, Visibility.Public);
            this._tabletop.Exile = new Zone(ZoneKind.Exile, Visibility.Public);
            this._tabletop.Command = new Zone(ZoneKind.Command, Visibility.Public);
            this._tabletop.Ante = new Zone(ZoneKind.Ante, Visibility.Public);

            return this;
        }

        public class Result : ExecutionResult
        {
            private Result(Tabletop tabletop, params string[] messages)
                : base(messages)
            {
                Guard
                    .Require(tabletop, nameof(tabletop))
                    .Is.Not.Null();

                this.Tabletop = tabletop;
            }

            public Tabletop Tabletop { get; }

            public static Result CreateSuccessful(Tabletop tabletop)
            {
                return new Result(tabletop);
            }

            public static Result CreateFailure(Tabletop tabletop, string[] messages)
            {
                Guard
                    .Require(messages, nameof(messages))
                    .Is.Not.Null();

                messages = messages
                    .Where(message => !string.IsNullOrEmpty(message))
                    .ToArray();

                if (!messages.Any())
                {
                    throw new KvasirException("Failure result must contain at least 1 message!");
                }

                return new Result(tabletop, messages);
            }
        }
    }
}