// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayerFormatter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>

// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, February 11, 2020 7:10:43 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Framework;

using FluentAssertions.Formatting;
using nGratis.AI.Kvasir.Engine;
using nGratis.Cop.Olympus.Contract;

public class PlayerFormatter : IValueFormatter
{
    public bool CanHandle(object value) => value is Player;

    public void Format(object value, FormattedObjectGraph graph, FormattingContext context, FormatChild child)
    {
        var text = value is Player player
            ? player.Name
            : DefinedText.Unsupported;

        graph.AddFragment(text);
    }
}