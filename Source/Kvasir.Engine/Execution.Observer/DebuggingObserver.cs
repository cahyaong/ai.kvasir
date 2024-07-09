// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DebuggingObserver.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, March 24, 2024 2:53:00 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class DebuggingObserver : IObserver
{
    private readonly IMagicLogger _magicLogger;

    private bool _shouldExecuteUntilNextPhase;
    private bool _shouldExecuteUntilNextTurn;
    private bool _shouldExecuteUntilNextRound;

    public DebuggingObserver(IMagicLogger magicLogger)
    {
        this._magicLogger = magicLogger;
        this._shouldExecuteUntilNextPhase = true;
    }

    public void OnPhaseAndStepChanged(ITabletop tabletop)
    {
        this.HandleTabletopLogging(tabletop);
        this.HandleUserInput(tabletop);
    }

    private void HandleTabletopLogging(ITabletop tabletop)
    {
        this._magicLogger.Log(tabletop);
    }

    private void HandleUserInput(ITabletop tabletop)
    {
        if (this._shouldExecuteUntilNextRound)
        {
            return;
        }

        var shouldPause =
            (this._shouldExecuteUntilNextPhase) ||
            (this._shouldExecuteUntilNextTurn && tabletop.Phase == Phase.Ending);

        if (!shouldPause)
        {
            this._magicLogger.Log(Verbosity.Info, "Executing...");
            return;
        }

        this._magicLogger.Log(
            Verbosity.Warning,
            "Pausing... Press <Q>, <W> or <E> to execute next phase, next turn or end of round!");

        this._shouldExecuteUntilNextTurn = false;
        this._shouldExecuteUntilNextPhase = false;

        var isHandled = false;

        while (!isHandled)
        {
            var keyInfo = Console.ReadKey(true);
            isHandled = true;

            switch (char.ToUpperInvariant(keyInfo.KeyChar))
            {
                case 'Q':
                    this._shouldExecuteUntilNextPhase = true;
                    break;

                case 'W':
                    this._shouldExecuteUntilNextTurn = true;
                    break;

                case 'E':
                    this._shouldExecuteUntilNextRound = true;
                    break;

                default:
                    isHandled = false;
                    break;
            }
        }
    }
}