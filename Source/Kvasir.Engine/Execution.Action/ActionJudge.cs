// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionJudge.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 8, 2023 12:56:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Linq;
using nGratis.AI.Kvasir.Contract;

public class ActionJudge : IActionJudge
{
    private readonly IExecutionManager _executionManager;

    public ActionJudge(IExecutionManager executionManager)
    {
        this._executionManager = executionManager;
    }

    public static IActionJudge Unknown => UnknownActionJudge.Instance;

    public QueueingResult QueueAction(ITabletop tabletop, IAction action)
    {
        var costHandler = this._executionManager.FindCostHandler(action.Cost);
        var actionHandler = this._executionManager.FindActionHandler(action);

        var validationResult = ValidationResult.Create(
            costHandler.Validate(tabletop, action.Cost, action.Target),
            actionHandler.Validate(tabletop, action));

        if (validationResult.HasError)
        {
            var owningPlayer = action.OwningPlayer;

            action = Action.Pass();
            action.OwningPlayer = owningPlayer;
            costHandler = this._executionManager.FindCostHandler(action.Cost);
        }
        else if (actionHandler.IsSpecialAction)
        {
            costHandler.Resolve(tabletop, action.Cost, action.Target);
            actionHandler.Resolve(tabletop, action);

            return QueueingResult.CreateWhenSpecialActionPerformed(validationResult);
        }

        costHandler.Resolve(tabletop, action.Cost, action.Target);
        tabletop.Stack.AddToTop(action);

        var isActionPerformed = tabletop
            .Stack
            .FindAll()
            .Any(queuedAction => queuedAction.Kind != ActionKind.Passing);

        if (!ActionJudge.ShouldResolveStack(tabletop))
        {
            return QueueingResult.CreateWhenStackUnresolved(isActionPerformed, validationResult);
        }

        this.ResolveStack(tabletop);

        return QueueingResult.CreateWhenStackResolved(isActionPerformed, validationResult);
    }

    public ExecutionResult ExecuteAction(ITabletop tabletop, IAction action)
    {
        var costHandler = this._executionManager.FindCostHandler(action.Cost);
        var actionHandler = this._executionManager.FindActionHandler(action);

        var validationResult = ValidationResult.Create(
            costHandler.Validate(tabletop, action.Cost, action.Target),
            actionHandler.Validate(tabletop, action));

        if (validationResult.HasError)
        {
            return ExecutionResult.Create(validationResult.Messages);
        }

        costHandler.Resolve(tabletop, action.Cost, action.Target);
        actionHandler.Resolve(tabletop, action);

        return ExecutionResult.SuccessfulWithoutWinner;
    }

    private static bool ShouldResolveStack(ITabletop tabletop)
    {
        if (tabletop.Stack.Quantity < 2)
        {
            return false;
        }

        // RX-117.4 — If all players pass in succession (that is, if all players pass without taking any actions in
        // between passing), the spell or ability on top of the stack resolves...

        // RX-405.5 — When all players pass in succession, the top (last-added) spell or ability on the stack
        // resolves...

        return tabletop
            .Stack
            .FindManyFromTop(2)
            .All(action => action.Kind == ActionKind.Passing);
    }

    private void ResolveStack(ITabletop tabletop)
    {
        tabletop.Stack.RemoveManyFromTop(2);

        while (!tabletop.Stack.IsEmpty)
        {
            var action = tabletop.Stack.FindFromTop();

            this._executionManager
                .FindActionHandler(action)
                .Resolve(tabletop, action);

            tabletop.Stack.RemoveFromTop();
        }
    }
}

internal sealed class UnknownActionJudge : IActionJudge
{
    private UnknownActionJudge()
    {
    }

    public static UnknownActionJudge Instance { get; } = new();

    public QueueingResult QueueAction(ITabletop _, IAction __) =>
        throw new NotSupportedException("Queueing action is not allowed!");

    public ExecutionResult ExecuteAction(ITabletop _, IAction __) =>
        throw new NotSupportedException("Executing action is not allowed!");
}