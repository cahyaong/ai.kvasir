// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParameter.Default.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, July 5, 2024 12:23:25 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;

public sealed class UnknownParameter : IParameter
{
    private UnknownParameter()
    {
    }

    public static UnknownParameter Instance { get; } = new();

    public TValue FindValue<TValue>(ParameterKey _) =>
        throw new NotSupportedException("Finding value is not allowed!");
}

public sealed class NoneParameter : IParameter
{
    private NoneParameter()
    {
    }

    public static NoneParameter Instance { get; } = new();

    public TValue FindValue<TValue>(ParameterKey _) => default!;
}