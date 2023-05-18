// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleKeyCalculator.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 21 December 2018 11:40:37 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using System.IO;
using System.Linq;
using nGratis.Cop.Olympus.Contract;

internal class SimpleKeyCalculator : IKeyCalculator
{
    private SimpleKeyCalculator()
    {
    }

    public static SimpleKeyCalculator Instance { get; } = new();

    public DataSpec Calculate(Uri uri)
    {
        var key = uri.Segments.LastOrDefault();

        if (string.IsNullOrEmpty(key))
        {
            return new DataSpec(Default.Name, Mime.Unknown);
        }

        var name = Path.GetFileNameWithoutExtension(key);
        var mime = Mime.ParseByExtension(Path.GetExtension(key));

        return new DataSpec(name, mime);
    }

    public static class Default
    {
        public const string Name = "_unknown";
    }
}