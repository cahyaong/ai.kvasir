// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameContext.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 23 January 2019 10:45:26 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;
    using Stateless;

    public class GameContext
    {
        public enum Status
        {
            Unknown = 0,

            Starting,
            Playing,
            Ending
        }

        private enum Action
        {
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            Unknown = 0,

            Start,
            PlayTurn,
            End
        }

        private readonly DefinedBlob.Player[] _definedPlayers;

        private readonly IMagicEntityFactory _entityFactory;

        private readonly IRandomGenerator _randomGenerator;

        private readonly StateMachine<Status, Action> _stateMachine;

        public GameContext(
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
            this._stateMachine = new StateMachine<Status, Action>(Status.Unknown);

            this._stateMachine
                .Configure(Status.Unknown)
                .Permit(Action.Start, Status.Starting);

            this._stateMachine
                .Configure(Status.Starting)
                .OnActivate(this.OnStartingActivated)
                .Permit(Action.PlayTurn, Status.Playing);

            this._stateMachine
                .Configure(Status.Playing)
                .OnActivate(this.OnPlayingActivated)
                .Permit(Action.End, Status.Ending);

            this._stateMachine
                .Configure(Status.Ending)
                .OnActivate(this.OnEndingActivated);

            this._stateMachine.Fire(Action.Start);
        }

        public Status CurrentStatus => this._stateMachine.State;

        public Player ActivePlayer { get; private set; }

        public Player NonactivePlayer { get; private set; }

        private void OnStartingActivated()
        {
            this
                .SetupPlayers()
                .SetupZones(this.ActivePlayer)
                .SetupZones(this.NonactivePlayer);
        }

        private void OnPlayingActivated()
        {
        }

        private void OnEndingActivated()
        {
        }

        private GameContext SetupPlayers()
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
                this.ActivePlayer = firstPlayer;
                this.NonactivePlayer = secondPlayer;
            }
            else
            {
                this.ActivePlayer = secondPlayer;
                this.NonactivePlayer = firstPlayer;
            }

            this.ActivePlayer.Life = 20;
            this.NonactivePlayer.Life = 20;

            this.ActivePlayer.Opponent = this.NonactivePlayer;
            this.NonactivePlayer.Opponent = this.ActivePlayer;

            return this;
        }

        private GameContext SetupZones(Player player)
        {
            if (player.Deck == null)
            {
                throw new KvasirException($"Player [{player.Name}] does NOT have valid deck!");
            }

            var cards = player
                .Deck.Cards
                .ToArray();

            player.Library = new Zone(ZoneKind.Library);

            this
                ._randomGenerator
                .GenerateShufflingIndexes((ushort)cards.Length)
                .Select(index => cards[index])
                .ForEach(card => player.Library.AddCard(card));

            player.Hand = new Zone(ZoneKind.Hand);

            Enumerable
                .Range(0, GameConstant.Hand.MaximumCardCount)
                .Select(_ => player.Library.RemoveCard())
                .ForEach(card => player.Hand.AddCard(card));

            // TODO: Implement proper `mulligan` for Rx-103.4 sub-rule.

            return this;
        }
    }
}