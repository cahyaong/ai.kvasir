// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobParameter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, April 7, 2020 6:39:59 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class JobParameter
{
    private readonly Dictionary<string, object> _entries;

    private JobParameter()
    {
        this._entries = new Dictionary<string, object>();
    }

    public static JobParameter None { get; } = new();

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
            throw new KvasirException("Entry is not defined!", ("Name", name));
        }

        return (T)Convert.ChangeType(value, typeof(T));
    }

    public class Builder
    {
        private readonly JobParameter _jobParameter;

        private Builder()
        {
            this._jobParameter = new JobParameter();
        }

        public static Builder Create()
        {
            return new Builder();
        }

        public Builder WithEntry<T>(string name, T value)
            where T : notnull
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            this._jobParameter._entries[name] = value;

            return this;
        }

        public JobParameter Build()
        {
            return this._jobParameter;
        }
    }
}