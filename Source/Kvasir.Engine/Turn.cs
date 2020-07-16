// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Turn.cs" company="nGratis">
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
// <creation_timestamp>Saturday, July 11, 2020 7:22:23 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine
{
    using System;
    using nGratis.Cop.Olympus.Contract;
    using Stateless;

    internal class Turn
    {
        public enum PhaseState
        {
            Unknown = 0,

            Beginning,
            PrecombatMain,
            Combat,
            PostcombatMain,
            Ending
        }

        public enum StepState
        {
            Unknown = 0,

            Untap,
            Upkeep,
            Draw,
            None,
            BeginningOfCombat,
            DeclareAttackers,
            DeclareBlockers,
            CombatDamage,
            EndOfCombat,
            End,
            Cleanup
        }

        private enum Trigger
        {
            Unknown = 0,

            Next
        }

        private readonly StateMachine<PhaseState, Trigger> _phaseStateMachine;

        private readonly StateMachine<StepState, Trigger> _stepStateMachine;

        public Turn()
        {
            this._phaseStateMachine = new StateMachine<PhaseState, Trigger>(PhaseState.Unknown);
            this._stepStateMachine = new StateMachine<StepState, Trigger>(StepState.Unknown);

            this.ConfigurePhaseStateMachine();
            this.ConfigureStepStateMachine();
        }

        public event EventHandler<StateChangedEventArgs> StateChanged;

        public void ProcessNextPhase()
        {
            this._phaseStateMachine.Fire(Trigger.Next);
        }

        private void ConfigurePhaseStateMachine()
        {
            this._phaseStateMachine
                .Configure(PhaseState.Unknown)
                .Permit(Trigger.Next, PhaseState.Beginning);

            this._phaseStateMachine
                .Configure(PhaseState.Beginning)
                .OnEntry(this.OnPhaseEntered)
                .Permit(Trigger.Next, PhaseState.PrecombatMain);

            this._phaseStateMachine
                .Configure(PhaseState.PrecombatMain)
                .OnEntry(this.OnPhaseEntered)
                .Permit(Trigger.Next, PhaseState.Combat);

            this._phaseStateMachine
                .Configure(PhaseState.Combat)
                .OnEntry(this.OnPhaseEntered)
                .Permit(Trigger.Next, PhaseState.PostcombatMain);

            this._phaseStateMachine
                .Configure(PhaseState.PostcombatMain)
                .OnEntry(this.OnPhaseEntered)
                .Permit(Trigger.Next, PhaseState.Ending);

            this._phaseStateMachine
                .Configure(PhaseState.Ending)
                .OnEntry(this.OnPhaseEntered)
                .Permit(Trigger.Next, PhaseState.Unknown);
        }

        private void ConfigureStepStateMachine()
        {
            // Define steps for `Beginning` phase.

            this._stepStateMachine
                .Configure(StepState.Unknown)
                .PermitIf(
                    Trigger.Next,
                    StepState.Untap,
                    () => this._phaseStateMachine.IsInState(PhaseState.Beginning));

            this._stepStateMachine
                .Configure(StepState.Untap)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.Upkeep);

            this._stepStateMachine
                .Configure(StepState.Upkeep)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.Draw);

            this._stepStateMachine
                .Configure(StepState.Draw)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.Unknown);

            // Define steps for `Precombat Main` phase.

            this._stepStateMachine
                .Configure(StepState.Unknown)
                .PermitIf(
                    Trigger.Next,
                    StepState.None,
                    () => this._phaseStateMachine.IsInState(PhaseState.PrecombatMain));

            this._stepStateMachine
                .Configure(StepState.None)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.Unknown);

            // Define steps for `Combat` phase.

            this._stepStateMachine
                .Configure(StepState.Unknown)
                .PermitIf(
                    Trigger.Next,
                    StepState.BeginningOfCombat,
                    () => this._phaseStateMachine.IsInState(PhaseState.Combat));

            this._stepStateMachine
                .Configure(StepState.BeginningOfCombat)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.DeclareAttackers);

            this._stepStateMachine
                .Configure(StepState.DeclareAttackers)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.DeclareBlockers);

            this._stepStateMachine
                .Configure(StepState.DeclareBlockers)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.CombatDamage);

            this._stepStateMachine
                .Configure(StepState.CombatDamage)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.EndOfCombat);

            this._stepStateMachine
                .Configure(StepState.EndOfCombat)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.Unknown);

            // Define steps for `Postcombat Main` phase.

            this._stepStateMachine
                .Configure(StepState.Unknown)
                .PermitIf(
                    Trigger.Next,
                    StepState.None,
                    () => this._phaseStateMachine.IsInState(PhaseState.PostcombatMain));

            // Define steps for `Ending` phase.

            this._stepStateMachine
                .Configure(StepState.Unknown)
                .PermitIf(
                    Trigger.Next,
                    StepState.End,
                    () => this._phaseStateMachine.IsInState(PhaseState.Ending));

            this._stepStateMachine
                .Configure(StepState.End)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.Cleanup);

            this._stepStateMachine
                .Configure(StepState.Cleanup)
                .OnEntry(this.OnStepEntered)
                .Permit(Trigger.Next, StepState.Unknown);
        }

        private void OnPhaseEntered()
        {
            do
            {
                this._stepStateMachine.Fire(Trigger.Next);
            }
            while (!this._stepStateMachine.IsInState(StepState.Unknown));
        }

        private void OnStepEntered()
        {
            this.StateChanged?.Invoke(
                this,
                new StateChangedEventArgs(this._phaseStateMachine.State, this._stepStateMachine.State));
        }

        public class StateChangedEventArgs : EventArgs
        {
            public StateChangedEventArgs(PhaseState phaseState, StepState stepState)
            {
                Guard
                    .Require(phaseState, nameof(phaseState))
                    .Is.Not.Default();

                Guard
                    .Require(stepState, nameof(stepState))
                    .Is.Not.Default();

                this.PhaseState = phaseState;
                this.StepState = stepState;
            }

            public PhaseState PhaseState { get; }

            public StepState StepState { get; }
        }
    }
}