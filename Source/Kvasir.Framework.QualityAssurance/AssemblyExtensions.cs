// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyExtensions.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, May 5, 2020 6:00:27 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace System.Reflection
{
    using System.Collections.Generic;
    using System.IO;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Engine;
    using nGratis.AI.Kvasir.Framework;
    using nGratis.Cop.Olympus.Contract;
    using YamlDotNet.Serialization;

    public static class AssemblyExtensions
    {
        public static Stream FindSessionDataStream(this object _, string name)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            return Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream($"nGratis.AI.Kvasir.Framework.Data.{name}.ngksession") ??
                throw new KvasirTestingException(@"Session data must be embedded!", $"Name: [{name}].");
        }

        public static void LoadCreatureData(this Zone zone, string name)
        {
            Guard
                .Require(zone, nameof(zone))
                .Is.Not.Null();

            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            using var dataStream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream($"nGratis.AI.Kvasir.Framework.Data.{name}.ngkcard") ??
                throw new KvasirTestingException(@"Creatures data must be embedded!", $"Name: [{name}].");

            dataStream
                .ReadText()
                .DeserializeFromYaml<List<StubCreature>>()
                .ForEach(zone.AddCard);
        }
    }
}