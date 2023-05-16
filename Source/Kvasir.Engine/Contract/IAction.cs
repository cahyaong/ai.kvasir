// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAction.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, June 1, 2022 12:30:36 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.Cop.Olympus.Contract;

public interface IAction : IDiagnostic
{
    ActionKind Kind { get; }

    IPlayer Owner { get; set; }

    ICost Cost { get; }

    IActionTarget Target { get; }

    IParameter Parameter { get; set; }
}

internal class UnknownAction : IAction
{
    private UnknownAction()
    {
    }

    internal static UnknownAction Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public ActionKind Kind => ActionKind.Unknown;

    public IPlayer Owner
    {
        get => Player.Unknown;
        set => throw new NotSupportedException("Setting owner is not allowed!");
    }

    public ICost Cost => Engine.Cost.Unknown;

    public IActionTarget Target => ActionTarget.Unknown;

    public IParameter Parameter
    {
        get => Engine.Parameter.Unknown;
        set => throw new NotSupportedException("Setting parameter is not allowed!");
    }
}