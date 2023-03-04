// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActionTarget.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, July 6, 2022 6:09:23 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;

public interface IActionTarget
{
    IPlayer Player { get; set; }

    IReadOnlyCollection<ICard> Cards { get; }

    // TODO (SHOULD): Implement ability as another kind of action source!
}

internal class UnknownActionTarget : IActionTarget
{
    private UnknownActionTarget()
    {
    }

    internal static UnknownActionTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => Engine.Player.Unknown;
        set => throw new NotSupportedException("Setting player is not supported!");
    }

    public IReadOnlyCollection<ICard> Cards => Array.Empty<ICard>();
}

internal class NoneActionTarget : IActionTarget
{
    private NoneActionTarget()
    {
    }

    internal static NoneActionTarget Instance { get; } = new();

    public IPlayer Player
    {
        get => Engine.Player.None;
        set => throw new NotSupportedException("Setting player is not supported!");
    }

    public IReadOnlyCollection<ICard> Cards => Array.Empty<ICard>();
}