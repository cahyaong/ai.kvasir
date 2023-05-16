// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICost.cs" company="nGratis">
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
// <creation_timestamp>Saturday, June 4, 2022 6:30:05 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public interface ICost : IDiagnostic
{
    CostKind Kind { get; }

    ICostTarget Target { get; set; }

    IParameter Parameter { get; }
}

public sealed class UnknownCost : ICost
{
    private UnknownCost()
    {
    }

    internal static UnknownCost Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.Unknown;

    public CostKind Kind => CostKind.Unknown;

    public ICostTarget Target
    {
        get => CostTarget.Unknown;
        set => throw new NotSupportedException("Setting target is not allowed!");
    }

    public IParameter Parameter => Engine.Parameter.Unknown;
}

public sealed class NoneCost : ICost
{
    private NoneCost()
    {
    }

    public static NoneCost Instance { get; } = new();

    public int Id => -42;

    public string Name => DefinedText.None;

    public CostKind Kind => CostKind.None;

    public ICostTarget Target
    {
        get => CostTarget.None;
        set => throw new NotSupportedException("Setting target is not allowed!");
    }

    public IParameter Parameter => Engine.Parameter.None;
}