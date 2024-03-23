// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoundJudge.LoggingDecorator.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 23, 2024 2:42:28 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public partial class RoundJudge
{
    public class LoggingDecorator : IRoundJudge
    {
        private readonly IRoundJudge _roundJudge;
        private readonly IMagicLogger _magicLogger;

        public LoggingDecorator(IRoundJudge roundJudge, IMagicLogger magicLogger)
        {
            this._roundJudge = roundJudge;
            this._magicLogger = magicLogger;
        }

        public ExecutionResult ExecuteNextTurn(ITabletop tabletop)
        {
            var executionResult = this._roundJudge.ExecuteNextTurn(tabletop);

            this._magicLogger.Log(tabletop);

            return executionResult;
        }

        public ExecutionResult ExecuteNextPhase(ITabletop tabletop)
        {
            var executionResult = this._roundJudge.ExecuteNextPhase(tabletop);

            this._magicLogger.Log(tabletop);

            return executionResult;
        }
    }
}