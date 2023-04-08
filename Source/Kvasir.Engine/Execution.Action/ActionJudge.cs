// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionJudge.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
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
    private static readonly IReadOnlyDictionary<ActionKind, IActionHandler> ActionHandlerByActionKindLookup;

    static ActionJudge()
    {
        // TODO (COULD): Inject handlers via DI container!

        ActionJudge.ActionHandlerByActionKindLookup = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(type => !type.IsInterface && !type.IsAbstract)
            .Where(type => type.IsAssignableTo(typeof(IActionHandler)))
            .Select(Activator.CreateInstance)
            .Cast<IActionHandler>()
            .ToImmutableDictionary(handler => handler.ActionKind);
    }

    public QueueingResult QueueAction(ITabletop tabletop, IAction action)
    {
        var actionHandler = ActionJudge.FindActionHandler(action);
        var validationResult = actionHandler.Validate(tabletop, action);

        if (validationResult.HasError)
        {
            var owner = action.Owner;

            action = Action.Pass();
            action.Owner = owner;
        }
        else if (actionHandler.IsSpecialAction)
        {
            actionHandler.Resolve(tabletop, action);

            return QueueingResult.CreateWhenSpecialActionPerformed(validationResult);
        }

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

    private static IActionHandler FindActionHandler(IAction action)
    {
        if (!ActionJudge.ActionHandlerByActionKindLookup.TryGetValue(action.Kind, out var actionHandler))
        {
            throw new KvasirException(
                "Action has no handler associated to it!",
                ("Action Kind", action.Kind));
        }

        return actionHandler;
    }
}