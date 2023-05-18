﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionJudge.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 8, 2023 12:56:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using nGratis.AI.Kvasir.Contract;

public class ActionJudge : IActionJudge
{
    private static readonly ICostHandler DoingNothingHandler;

    private static readonly IReadOnlyDictionary<CostKind, ICostHandler> CostHandlerByCostKindLookup;

    private static readonly IReadOnlyDictionary<ActionKind, IActionHandler> ActionHandlerByActionKindLookup;

    static ActionJudge()
    {
        // TODO (COULD): Inject handlers via DI container!

        var processedTypes = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => !type.IsInterface && !type.IsAbstract)
            .ToImmutableArray();

        ActionJudge.CostHandlerByCostKindLookup = processedTypes
            .Where(type => type.IsAssignableTo(typeof(ICostHandler)))
            .Select(Activator.CreateInstance)
            .Cast<ICostHandler>()
            .ToImmutableDictionary(handler => handler.CostKind);

        ActionJudge.DoingNothingHandler = ActionJudge.CostHandlerByCostKindLookup[CostKind.None];

        ActionJudge.ActionHandlerByActionKindLookup = processedTypes
            .Where(type => type.IsAssignableTo(typeof(IActionHandler)))
            .Select(Activator.CreateInstance)
            .Cast<IActionHandler>()
            .ToImmutableDictionary(handler => handler.ActionKind);
    }

    public QueueingResult QueueAction(ITabletop tabletop, IAction action)
    {
        var costHandler = ActionJudge.FindCostHandler(action.Cost);
        var actionHandler = ActionJudge.FindActionHandler(action);

        var validationResult = ValidationResult.Create(
            costHandler.Validate(tabletop, action.Cost),
            actionHandler.Validate(tabletop, action));

        if (validationResult.HasError)
        {
            var owner = action.Owner;

            action = Action.Pass();
            action.Owner = owner;
            costHandler = ActionJudge.DoingNothingHandler;
        }
        else if (actionHandler.IsSpecialAction)
        {
            costHandler.Resolve(tabletop, action.Cost);
            actionHandler.Resolve(tabletop, action);

            return QueueingResult.CreateWhenSpecialActionPerformed(validationResult);
        }

        costHandler.Resolve(tabletop, action.Cost);
        tabletop.Stack.AddToTop(action);

        var isActionPerformed = tabletop
            .Stack
            .FindAll()
            .Any(queuedAction => queuedAction.Kind != ActionKind.Passing);

        if (!ActionJudge.ShouldResolveStack(tabletop))
        {
            return QueueingResult.CreateWhenStackUnresolved(isActionPerformed, validationResult);
        }

        ActionJudge.ResolveStack(tabletop);

        return QueueingResult.CreateWhenStackResolved(isActionPerformed, validationResult);
    }

    public ExecutionResult ExecuteAction(ITabletop tabletop, IAction action)
    {
        var actionHandler = ActionJudge.FindActionHandler(action);
        var validationResult = actionHandler.Validate(tabletop, action);

        return ExecutionResult.Create(validationResult.Messages);
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

    private static void ResolveStack(ITabletop tabletop)
    {
        tabletop.Stack.RemoveManyFromTop(2);

        while (!tabletop.Stack.IsEmpty)
        {
            var action = tabletop.Stack.FindFromTop();

            ActionJudge
                .FindActionHandler(action)
                .Resolve(tabletop, action);

            tabletop.Stack.RemoveFromTop();
        }
    }

    private static ICostHandler FindCostHandler(ICost cost)
    {
        if (!ActionJudge.CostHandlerByCostKindLookup.TryGetValue(cost.Kind, out var costHandler))
        {
            throw new KvasirException("Cost must have a handler associated to it!", ("Cost Kind", cost.Kind));
        }

        return costHandler;
    }

    private static IActionHandler FindActionHandler(IAction action)
    {
        if (!ActionJudge.ActionHandlerByActionKindLookup.TryGetValue(action.Kind, out var actionHandler))
        {
            throw new KvasirException("Action must have a handler associated to it!", ("Action Kind", action.Kind));
        }

        return actionHandler;
    }
}