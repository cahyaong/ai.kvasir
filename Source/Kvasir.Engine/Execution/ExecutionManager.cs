// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionManager.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, May 21, 2023 6:40:14 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public class ExecutionManager : IExecutionManager
{
    private readonly IDictionary<CostKind, ICostHandler> _costHandlerByCostKindLookup;

    private readonly IDictionary<ActionKind, IActionHandler> _actionHandlerByActionKindLookup;

    public ExecutionManager()
    {
        this._costHandlerByCostKindLookup = new Dictionary<CostKind, ICostHandler>();
        this._actionHandlerByActionKindLookup = new Dictionary<ActionKind, IActionHandler>();
    }

    public void RegisterCostHandler(params ICostHandler[] costHandlers)
    {
        costHandlers
            .ForEach(handler => this._costHandlerByCostKindLookup[handler.CostKind] = handler);
    }

    public void RegisterActionHandler(params IActionHandler[] actionHandlers)
    {
        actionHandlers
            .ForEach(handler => this._actionHandlerByActionKindLookup[handler.ActionKind] = handler);
    }

    public ICostHandler FindCostHandler(ICost cost)
    {
        if (!this._costHandlerByCostKindLookup.TryGetValue(cost.Kind, out var costHandler))
        {
            throw new KvasirException("Cost must have a handler associated to it!", ("Cost Kind", cost.Kind));
        }

        return costHandler;
    }

    public IActionHandler FindActionHandler(IAction action)
    {
        if (!this._actionHandlerByActionKindLookup.TryGetValue(action.Kind, out var actionHandler))
        {
            throw new KvasirException("Action must have a handler associated to it!", ("Action Kind", action.Kind));
        }

        return actionHandler;
    }
}