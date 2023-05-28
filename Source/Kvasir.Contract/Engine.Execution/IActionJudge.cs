// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActionJudge.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 8, 2023 1:05:12 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IActionJudge
{
    QueueingResult QueueAction(ITabletop tabletop, IAction action);

    ExecutionResult ExecuteAction(ITabletop tabletop, IAction action);
}