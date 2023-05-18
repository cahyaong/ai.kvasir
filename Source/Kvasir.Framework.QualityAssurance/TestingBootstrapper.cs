// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestingBootstrapper.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, April 5, 2022 3:44:55 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using FluentAssertions.Formatting;

public class TestingBootstrapper
{
    private TestingBootstrapper()
    {
    }

    public static TestingBootstrapper Create()
    {
        return new TestingBootstrapper();
    }

    public TestingBootstrapper WithFormatter()
    {
        Formatter.AddFormatter(new PermanentFormatter());
        Formatter.AddFormatter(new PlayerFormatter());

        return this;
    }
}