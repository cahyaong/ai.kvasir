// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionParameter.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
// <creation_timestamp>Tuesday, April 7, 2020 6:39:59 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Console
{
    using System;
    using System.Collections.Generic;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    internal class ExecutionParameter
    {
        private readonly Dictionary<string, object> _entries;

        private ExecutionParameter()
        {
            this._entries = new Dictionary<string, object>();
        }

        public string GetValue(string name)
        {
            return this.GetValue<string>(name);
        }

        public T GetValue<T>(string name)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            if (!this._entries.TryGetValue(name, out var value))
            {
                throw new KvasirException(
                    @"Failed to find entry! " +
                    $"Name: [{name}].");
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public class Builder
        {
            private readonly ExecutionParameter _executionParameter;

            private Builder()
            {
                this._executionParameter = new ExecutionParameter();
            }

            public static Builder Create()
            {
                return new Builder();
            }

            public Builder WithEntry<T>(string name, T value)
            {
                Guard
                    .Require(name, nameof(name))
                    .Is.Not.Empty();

                Guard
                    .Require(value, nameof(value))
                    .Is.Not.Default();

                this._executionParameter._entries[name] = value;

                return this;
            }

            public ExecutionParameter Build()
            {
                return this._executionParameter;
            }
        }
    }
}