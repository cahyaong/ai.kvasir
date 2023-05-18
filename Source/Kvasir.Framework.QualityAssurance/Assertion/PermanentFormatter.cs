// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PermanentFormatter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, March 29, 2022 2:47:23 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using System;
using System.Collections.Generic;
using FluentAssertions.Formatting;
using nGratis.AI.Kvasir.Engine;
using nGratis.Cop.Olympus.Contract;

public class PermanentFormatter : IValueFormatter
{
    public bool CanHandle(object value) => value is Permanent or IEnumerable<Permanent>;

    public void Format(object value, FormattedObjectGraph graph, FormattingContext context, FormatChild child)
    {
        var text = value switch
        {
            Permanent permanent => PermanentFormatter.Format(permanent),
            IEnumerable<Permanent> permanents => $"({permanents.ToPrettifiedText(PermanentFormatter.Format)})",
            _ => DefinedText.Unsupported
        };

        graph.AddFragment(text);
    }

    private static string Format(Permanent permanent)
    {
        return permanent?.Name ?? DefinedText.Unknown;
    }
}