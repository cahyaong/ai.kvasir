// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RegexExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 28 December 2018 7:58:57 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace System.Text.RegularExpressions;

using System.Collections.Generic;
using System.Linq;
using nGratis.Cop.Olympus.Contract;

public static class RegexExtensions
{
    public static IEnumerable<string> FindCaptureValues(this Match match, string name)
    {
        Guard
            .Require(name, nameof(name))
            .Is.Not.Empty();

        var matchedGroup = match.Groups[name];

        if (!matchedGroup.Success)
        {
            return Enumerable.Empty<string>();
        }

        return matchedGroup
            .Captures
            .Select(capture => capture.Value);
    }
}