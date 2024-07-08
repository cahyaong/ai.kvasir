// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parameter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 18, 2023 11:57:24 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

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
            if (!this._parameter._valueByKeyLookup.TryAdd(key, value))
            {
                throw new KvasirException(
                    "Parameter value has been defined!",
                    ("Key", key));
            }

            return this;
        }

        public Parameter Build()
        {
            return this._parameter;
        }
    }
}