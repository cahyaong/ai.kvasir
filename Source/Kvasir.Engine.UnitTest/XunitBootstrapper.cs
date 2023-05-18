// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XunitBootstrapper.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, February 11, 2020 7:39:38 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

[assembly: Xunit.TestFramework(
    "nGratis.AI.Kvasir.Engine.UnitTest.XunitBootstrapper",
    "nGratis.AI.Kvasir.Engine.UnitTest")]

namespace nGratis.AI.Kvasir.Engine.UnitTest;

using nGratis.AI.Kvasir.Framework;
using Xunit.Abstractions;
using Xunit.Sdk;

public class XunitBootstrapper : XunitTestFramework
{
    public XunitBootstrapper(IMessageSink messageSink)
        : base(messageSink)
    {
        TestingBootstrapper
            .Create()
            .WithFormatter();
    }
}