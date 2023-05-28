// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExecutionManager.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 21, 2023 6:39:51 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public interface IExecutionManager
{
    ICostHandler FindCostHandler(ICost cost);

    IActionHandler FindActionHandler(IAction action);
}