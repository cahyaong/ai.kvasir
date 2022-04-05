// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuceneExtensions.Serialization.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 14 November 2018 11:56:53 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Lucene.Net;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

internal static partial class LuceneExtensions
{
    private static readonly ConcurrentDictionary<Type, IndexableTypeInfo> TypeInfoLookup;

    static LuceneExtensions()
    {
        LuceneExtensions.TypeInfoLookup = new ConcurrentDictionary<Type, IndexableTypeInfo>();
    }

    public static Document ToLuceneDocument<TInstance>(this TInstance instance)
        where TInstance : class
    {
        Guard
            .Require(instance, nameof(instance))
            .Is.Not.Null();

        var document = new Document();

        var typeInfo = LuceneExtensions.TypeInfoLookup.GetOrAdd(
            typeof(TInstance),
            type => new IndexableTypeInfo(type));

        typeInfo
            .SerializingPropertyInfos
            .Select(info => info.IndexSerializer.Serialize(info.Name, info.BackingInfo.GetValue(instance)))
            .ForEach(field => document.Add(field));

        return document;
    }

    public static TInstance ToInstance<TInstance>(this Document document)
        where TInstance : class, new()
    {
        Guard
            .Require(document, nameof(document))
            .Is.Not.Null();

        var instance = new TInstance();

        var typeInfo = LuceneExtensions.TypeInfoLookup.GetOrAdd(
            typeof(TInstance),
            type => new IndexableTypeInfo(type));

        typeInfo
            .DeserializingPropertyInfos
            .ForEach(info => info.BackingInfo.SetValue(
                instance,
                info.IndexSerializer.Deserialize(document.GetField(info.Name))));

        return instance;
    }

    private sealed class IndexableTypeInfo
    {
        public IndexableTypeInfo(Type type)
        {
            Guard
                .Require(type, nameof(type))
                .Is.Not.Null();

            var propertyInfos = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(info => new IndexablePropertyInfo(info))
                .ToArray();

            this.SerializingPropertyInfos = propertyInfos
                .Where(info => info.CanSerialize)
                .ToArray();

            this.DeserializingPropertyInfos = propertyInfos
                .Where(info => info.CanDeserialize)
                .ToArray();
        }

        public IReadOnlyCollection<IndexablePropertyInfo> SerializingPropertyInfos { get; }

        public IReadOnlyCollection<IndexablePropertyInfo> DeserializingPropertyInfos { get; }
    }

    private sealed class IndexablePropertyInfo
    {
        private static readonly IReadOnlyDictionary<Type, IIndexSerializer> SerializerLookup;

        static IndexablePropertyInfo()
        {
            IndexablePropertyInfo.SerializerLookup = new Dictionary<Type, IIndexSerializer>
            {
                [typeof(string)] = StringSerializer.Instance,
                [typeof(short)] = ShortSerializer.Instance,
                [typeof(int)] = IntegerSerializer.Instance,
                [typeof(DateTime)] = DateTimeSerializer.Instance
            };
        }

        public IndexablePropertyInfo(PropertyInfo backingInfo)
        {
            Guard
                .Require(backingInfo, nameof(backingInfo))
                .Is.Not.Null();

            var hasRegisteredSerializer = IndexablePropertyInfo
                .SerializerLookup
                .TryGetValue(backingInfo.PropertyType, out var indexSerializer);

            if (!hasRegisteredSerializer)
            {
                throw new KvasirException($"Type [{backingInfo.PropertyType.FullName}] does not have serializer!");
            }

            this.BackingInfo = backingInfo;
            this.IndexSerializer = indexSerializer;
            this.Name = backingInfo.Name.Kebaberize();
        }

        public string Name { get; }

        public bool CanSerialize => this.BackingInfo.CanRead;

        public bool CanDeserialize => this.BackingInfo.CanWrite;

        public IIndexSerializer IndexSerializer { get; }

        internal PropertyInfo BackingInfo { get; }
    }

    private interface IIndexSerializer
    {
        IIndexableField Serialize(string name, object value);

        object Deserialize(IIndexableField indexableField);
    }

    private sealed class StringSerializer : IIndexSerializer
    {
        private StringSerializer()
        {
        }

        public static StringSerializer Instance { get; } = new();

        public IIndexableField Serialize(string name, object value)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            Guard
                .Require(value, nameof(value))
                .Is.Not.Null()
                .Is.OfType(typeof(string));

            return new StringField(name, (string)value, Field.Store.YES);
        }

        public object Deserialize(IIndexableField indexableField)
        {
            Guard
                .Require(indexableField, nameof(indexableField))
                .Is.Not.Null();

            return indexableField.GetStringValue() ?? Text.Empty;
        }
    }

    private sealed class ShortSerializer : IIndexSerializer
    {
        private ShortSerializer()
        {
        }

        public static ShortSerializer Instance { get; } = new();

        public IIndexableField Serialize(string name, object value)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            Guard
                .Require(value, nameof(value))
                .Is.Not.Null()
                .Is.OfType(typeof(short));

            return new Int32Field(name, (short)value, Field.Store.YES);
        }

        public object Deserialize(IIndexableField indexableField)
        {
            Guard
                .Require(indexableField, nameof(indexableField))
                .Is.Not.Null();

            return (short)(indexableField.GetInt32Value() ?? 0);
        }
    }

    private sealed class IntegerSerializer : IIndexSerializer
    {
        private IntegerSerializer()
        {
        }

        public static IntegerSerializer Instance { get; } = new();

        public IIndexableField Serialize(string name, object value)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            Guard
                .Require(value, nameof(value))
                .Is.Not.Null()
                .Is.OfType(typeof(int));

            return new Int32Field(name, (int)value, Field.Store.YES);
        }

        public object Deserialize(IIndexableField indexableField)
        {
            Guard
                .Require(indexableField, nameof(indexableField))
                .Is.Not.Null();

            return indexableField.GetInt32Value() ?? 0;
        }
    }

    private sealed class DateTimeSerializer : IIndexSerializer
    {
        private DateTimeSerializer()
        {
        }

        public static DateTimeSerializer Instance { get; } = new();

        public IIndexableField Serialize(string name, object value)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            Guard
                .Require(value, nameof(value))
                .Is.Not.Null()
                .Is.OfType(typeof(DateTime));

            return new Int64Field(name, ((DateTime)value).Ticks, Field.Store.YES);
        }

        public object Deserialize(IIndexableField indexableField)
        {
            Guard
                .Require(indexableField, nameof(indexableField))
                .Is.Not.Null();

            return new DateTime(indexableField.GetInt64Value() ?? 0, DateTimeKind.Utc);
        }
    }
}