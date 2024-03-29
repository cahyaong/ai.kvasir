﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parameter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 18, 2023 11:57:24 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

namespace nGratis.AI.Kvasir.Engine;

public class Parameter : IParameter
{
    private readonly IDictionary<ParameterKey, object> _valueByKeyLookup;

    private Parameter()
    {
        this._valueByKeyLookup = new Dictionary<ParameterKey, object>();
    }

    public static IParameter Unknown => UnknownParameter.Instance;

    public static IParameter None => NoneParameter.Instance;

    public TValue FindValue<TValue>(ParameterKey key)
    {
        if (!this._valueByKeyLookup.TryGetValue(key, out var value))
        {
            throw new KvasirException(
                "Parameter value is not defined!",
                ("Key", key));
        }

        return (TValue)value;
    }

    internal class Builder
    {
        private readonly Parameter _parameter;

        private Builder()
        {
            this._parameter = new Parameter();
        }

        public static Builder Create()
        {
            return new Builder();
        }

        internal Builder WithValue(ParameterKey key, object value)
        {
            if (this._parameter._valueByKeyLookup.ContainsKey(key))
            {
                throw new KvasirException(
                    "Parameter value has been defined!",
                    ("Key", key));
            }

            this._parameter._valueByKeyLookup.Add(key, value);

            return this;
        }

        public Parameter Build()
        {
            return this._parameter;
        }
    }
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