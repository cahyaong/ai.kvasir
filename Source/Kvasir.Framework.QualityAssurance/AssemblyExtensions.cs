﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.IO;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public static class AssemblyExtensions
    {
        public static Stream FindSessionDataStream(this Assembly assembly, string name)
        {
            Guard
                .Require(assembly, nameof(assembly))
                .Is.Not.Null();

            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            return
                assembly.GetManifestResourceStream($"nGratis.AI.Kvasir.Framework.Data.{name}.ngksession") ??
                throw new KvasirTestingException($"Session [{name}] must be embedded!");
        }

        public static Stream FindDeckDataStream(this Assembly assembly, string name)
        {
            Guard
                .Require(assembly, nameof(assembly))
                .Is.Not.Null();

            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            return
                assembly.GetManifestResourceStream($"nGratis.AI.Kvasir.Framework.Data.{name}.ngkdeck") ??
                throw new KvasirTestingException($"Session [{name}] must be embedded!");
        }
    }
}