// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parameter.cs" company="nGratis">
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
// <creation_timestamp>Saturday, March 18, 2023 11:57:24 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

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