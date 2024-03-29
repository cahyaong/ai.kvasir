﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LuceneExtensions.Serialization.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 14 November 2018 11:56:53 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Lucene.Net;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Humanizer;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using DefinedText = nGratis.AI.Kvasir.Contract.DefinedText;

internal static partial class LuceneExtensions
{
    private static readonly ConcurrentDictionary<Type, IndexableTypeInfo> TypeInfoByTypeLookup;

    static LuceneExtensions()
    {
        LuceneExtensions.TypeInfoByTypeLookup = new ConcurrentDictionary<Type, IndexableTypeInfo>();
    }

    public static Document ToLuceneDocument<TInstance>(this TInstance instance)
        where TInstance : class
    {
        var document = new Document();

        var typeInfo = LuceneExtensions.TypeInfoByTypeLookup.GetOrAdd(
            typeof(TInstance),
            type => new IndexableTypeInfo(type));

        var propertyInfos = typeInfo
            .SerializingPropertyInfos
            .Select(info => new
            {
                info.Name,
                Value = info.BackingInfo.GetValue(instance),
                info.IndexSerializer
            })
            .ToImmutableArray();

        var isValid = propertyInfos
            .All(info => info.Value is not null);

        if (!isValid)
        {
            var fieldNames = propertyInfos
                .Where(info => info.Value is null)
                .Select(info => info.Name)
                .OrderBy(name => name)
                .ToImmutableArray();

            throw new KvasirException(
                "Instance must have valid fields when converting to Lucene document!",
                ("Instance Type", instance.GetType().FullName ?? DefinedText.Unknown),
                ("Field Names", $"({fieldNames.ToPrettifiedText()})"));
        }

        propertyInfos
            .Select(info => info.IndexSerializer.Serialize(
                info.Name,
                info.Value ?? throw new KvasirException(
                    $"Field value is guaranteed not {DefinedText.Null}!",
                    ("Name", info.Name))))
            .ForEach(field => document.Add(field));

        return document;
    }

    public static TInstance ToInstance<TInstance>(this Document document)
        where TInstance : class, new()
    {
        var instance = new TInstance();

        var typeInfo = LuceneExtensions.TypeInfoByTypeLookup.GetOrAdd(
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
        private static readonly IReadOnlyDictionary<Type, IIndexSerializer> IndexSerializerByTypeLookup;

        static IndexablePropertyInfo()
        {
            IndexablePropertyInfo.IndexSerializerByTypeLookup = new Dictionary<Type, IIndexSerializer>
            {
                [typeof(string)] = StringSerializer.Instance,
                [typeof(short)] = ShortSerializer.Instance,
                [typeof(int)] = IntegerSerializer.Instance,
                [typeof(DateTime)] = DateTimeSerializer.Instance
            };
        }

        public IndexablePropertyInfo(PropertyInfo backingInfo)
        {
            var hasRegisteredSerializer = IndexablePropertyInfo
                .IndexSerializerByTypeLookup
                .TryGetValue(backingInfo.PropertyType, out var indexSerializer);

            if (!hasRegisteredSerializer)
            {
                throw new KvasirException(
                    "Property does not have index serializer!",
                    ("Type", backingInfo.PropertyType.FullName ?? DefinedText.Unknown));
            }

            this.BackingInfo = backingInfo;
            this.Name = backingInfo.Name.Kebaberize();

            this.IndexSerializer = indexSerializer ?? throw new KvasirException(
                "Property must have valid index serializer!",
                ("Type", backingInfo.PropertyType.FullName ?? DefinedText.Unknown));
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
                .Is.OfType(typeof(string));

            return new StringField(name, (string)value, Field.Store.YES);
        }

        public object Deserialize(IIndexableField indexableField)
        {
            return indexableField.GetStringValue() ?? DefinedText.Empty;
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
                .Is.OfType(typeof(short));

            return new Int32Field(name, (short)value, Field.Store.YES);
        }

        public object Deserialize(IIndexableField indexableField)
        {
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
                .Is.OfType(typeof(int));

            return new Int32Field(name, (int)value, Field.Store.YES);
        }

        public object Deserialize(IIndexableField indexableField)
        {
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
                .Is.OfType(typeof(DateTime));

            return new Int64Field(name, ((DateTime)value).Ticks, Field.Store.YES);
        }

        public object Deserialize(IIndexableField indexableField)
        {
            return new DateTime(indexableField.GetInt64Value() ?? 0, DateTimeKind.Utc);
        }
    }
}