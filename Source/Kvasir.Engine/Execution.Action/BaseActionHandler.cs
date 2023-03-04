// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseActionHandler.cs" company="nGratis">
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
// <creation_timestamp>Thursday, February 23, 2023 7:01:39 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Immutable;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine.Execution;

public abstract class BaseActionHandler : IActionHandler
{
    public abstract ActionKind ActionKind { get; }

    public virtual bool IsSpecialAction => false;

    public ValidationResult Validate(ITabletop tabletop, IAction action, IActionRequirement requirement)
    {
        if (action.Kind != this.ActionKind)
        {
            throw new KvasirException(
                "Handler is expecting a correct action kind!",
                ("Actual Kind", action.Kind),
                ("Expected Kind", this.ActionKind));
        }

        var reasons = this
            .ValidateCore(tabletop, action, requirement)
            .Reasons
            .ToImmutableList();

        return ValidationResult.Create(reasons);
    }

    public void Resolve(ITabletop tabletop, IAction action, IActionRequirement requirement)
    {
        if (action.Kind != this.ActionKind)
        {
            throw new KvasirException(
                "Handler is expecting a correct action kind!",
                ("Actual Kind", action.Kind),
                ("Expected Kind", this.ActionKind));
        }

        this.ResolveCore(tabletop, action, requirement);
    }

    protected virtual ValidationResult ValidateCore(ITabletop tabletop, IAction action, IActionRequirement requirement)
    {
        return ValidationResult.Successful;
    }

    protected abstract void ResolveCore(ITabletop tabletop, IAction action, IActionRequirement requirement);
}