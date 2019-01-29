// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameContext.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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

namespace nGratis.AI.Kvasir.Core
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;
    using Stateless;

    public class GameContext
    {
        public enum Phase
        {
            Unknown = 0,

            Beginning,
            Playing,
            Ending
        }

        private enum Action
        {
            [SuppressMessage("ReSharper", "UnusedMember.Local")]
            Unknown = 0,

            Begin,
            PlayTurn,
            End
        }

        private readonly AgentDefinition[] _agentDefinitions;

        private readonly IMagicObjectFactory _objectFactory;

        private readonly IRandomGenerator _randomGenerator;

        private readonly StateMachine<Phase, Action> _stateMachine;

        public GameContext(
            IReadOnlyCollection<AgentDefinition> agentDefinitions,
            IMagicObjectFactory objectFactory,
            IRandomGenerator randomGenerator)
        {
            Guard
                .Require(agentDefinitions, nameof(agentDefinitions))
                .Is.Not.Null();

            Guard
                .Require(objectFactory, nameof(objectFactory))
                .Is.Not.Null();

            Guard
                .Require(randomGenerator, nameof(randomGenerator))
                .Is.Not.Null();

            this._agentDefinitions = agentDefinitions.ToArray();
            this._objectFactory = objectFactory;
            this._randomGenerator = randomGenerator;
            this._stateMachine = new StateMachine<Phase, Action>(Phase.Unknown);

            this._stateMachine
                .Configure(Phase.Unknown)
                .Permit(Action.Begin, Phase.Beginning);

            this._stateMachine
                .Configure(Phase.Beginning)
                .OnActivate(this.OnBeginningActivated)
                .Permit(Action.PlayTurn, Phase.Playing);

            this._stateMachine
                .Configure(Phase.Playing)
                .OnActivate(this.OnPlayingActivated)
                .Permit(Action.End, Phase.Ending);

            this._stateMachine
                .Configure(Phase.Ending)
                .OnActivate(this.OnEndingActivated);

            this._stateMachine.Fire(Action.Begin);
        }

        public Phase CurrentPhase => this._stateMachine.State;

        public Agent ActiveAgent { get; private set; }

        public Agent PassiveAgent { get; private set; }

        private void OnBeginningActivated()
        {
            this.SetupAgent();
        }

        private void OnPlayingActivated()
        {
        }

        private void OnEndingActivated()
        {
        }

        private void SetupAgent()
        {
            if (this._agentDefinitions.Length != 2)
            {
                throw new KvasirException("Currently supporting 1 vs. 1 match!");
            }

            var firstAgent = this._objectFactory.CreateAgent(this._agentDefinitions[0]);
            var secondAgent = this._objectFactory.CreateAgent(this._agentDefinitions[1]);

            var firstValue = 0;
            var secondValue = 0;

            while (firstValue == secondValue)
            {
                firstValue = this._randomGenerator.RollDice(20);
                secondValue = this._randomGenerator.RollDice(20);
            }

            if (firstValue > secondValue)
            {
                this.ActiveAgent = firstAgent;
                this.PassiveAgent = secondAgent;
            }
            else
            {
                this.ActiveAgent = secondAgent;
                this.PassiveAgent = firstAgent;
            }

            this.ActiveAgent.Life = 20;
            this.PassiveAgent.Life = 20;
        }
    }
}