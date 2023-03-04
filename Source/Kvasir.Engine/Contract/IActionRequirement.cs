// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActionRequirement.cs" company="nGratis">
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
// <creation_timestamp>Sunday, February 19, 2023 6:23:24 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

using nGratis.AI.Kvasir.Engine.Execution;

namespace nGratis.AI.Kvasir.Engine;

using System;

public interface IActionRequirement
{
    ActionKind ActionKind { get; }

    TValue GetParameterValue<TValue>(ActionParameter actionParameter)
        where TValue : struct;
}

public partial class ActionRequirement
{
    public static IActionRequirement Unknown => UnknownActionRequirement.Instance;

    public static IActionRequirement None => NoneActionRequirement.Instance;
}

public class UnknownActionRequirement : IActionRequirement
{
    private UnknownActionRequirement()
    {
    }

    public static UnknownActionRequirement Instance { get; } = new();

    public ActionKind ActionKind => ActionKind.Unknown;

    public TValue GetParameterValue<TValue>(ActionParameter actionParameter)
        where TValue : struct
    {
        throw new NotSupportedException("Getting parameter value is not supported!");
    }
}

public class NoneActionRequirement : IActionRequirement
{
    private NoneActionRequirement()
    {
    }

    public static NoneActionRequirement Instance { get; } = new();

    public ActionKind ActionKind => ActionKind.Unknown;

    public TValue GetParameterValue<TValue>(ActionParameter actionParameter)
        where TValue : struct
    {
        return default;
    }
}