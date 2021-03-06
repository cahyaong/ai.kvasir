﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonExtensions.cs" company="nGratis">
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
// <creation_timestamp>Thursday, September 24, 2020 6:18:00 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Newtonsoft.Json.Linq
{
    using nGratis.Cop.Olympus.Contract;

    public static class JsonExtensions
    {
        public static string ReadValue(this JToken token, string key)
        {
            Guard
                .Require(token, nameof(token))
                .Is.Not.Null();

            Guard
                .Require(key, nameof(key))
                .Is.Not.Empty();

            return
                token[key]?.Value<string>() ??
                string.Empty;
        }

        public static T ReadValue<T>(this JToken token, string key)
        {
            Guard
                .Require(token, nameof(token))
                .Is.Not.Null();

            Guard
                .Require(key, nameof(key))
                .Is.Not.Empty();

            var fieldToken = token[key];

            return fieldToken != null
                ? fieldToken.Value<T>()
                : default;
        }
    }
}