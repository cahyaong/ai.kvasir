// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParameter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, March 19, 2023 12:14:20 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace nGratis.AI.Kvasir.Engine;

public interface IParameter
{
    TValue FindValue<TValue>(ParameterKey key);
}

internal sealed class UnknownParameter : IParameter
{
    private UnknownParameter()
    {
    }

    public static UnknownParameter Instance { get; } = new();

    public TValue FindValue<TValue>(ParameterKey _) =>
        throw new NotSupportedException("Finding value is not allowed!");
}

internal sealed class NoneParameter : IParameter
{
    private NoneParameter()
    {
    }

    public static NoneParameter Instance { get; } = new();

    public TValue FindValue<TValue>(ParameterKey _) => default!;
}