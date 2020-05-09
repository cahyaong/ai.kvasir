// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XunitBootstrapper.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, February 11, 2020 7:39:38 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

[assembly: Xunit.TestFramework(
    "nGratis.AI.Kvasir.Engine.Test.XunitBootstrapper",
    "nGratis.AI.Kvasir.Engine.Test")]

namespace nGratis.AI.Kvasir.Engine.Test
{
    using FluentAssertions.Formatting;
    using nGratis.AI.Kvasir.Framework;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public class XunitBootstrapper : XunitTestFramework
    {
        public XunitBootstrapper(IMessageSink messageSink) : base(messageSink)
        {
            Formatter.AddFormatter(new PlayerValueFormatter());
        }
    }
}