// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ManaCost.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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
// <creation_timestamp>Saturday, 29 December 2018 10:38:28 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    public class ManaCost : Cost
    {
        private static readonly IReadOnlyDictionary<string, Mana> ManaLookup = new Dictionary<string, Mana>
        {
            ["W"] = Mana.White,
            ["U"] = Mana.Blue,
            ["B"] = Mana.Black,
            ["R"] = Mana.Red,
            ["G"] = Mana.Green
        };

        private readonly IDictionary<Mana, ushort> _amountLookup;

        protected ManaCost()
        {
            this._amountLookup = new Dictionary<Mana, ushort>();
        }

        public static ManaCost Empty { get; } = EmptyManaCost.Instance;

        public static ManaCost Free { get; } = FreeManaCost.Instance;

        public virtual ushort this[Mana mana]
        {
            get
            {
                Guard
                    .Require(mana, nameof(mana))
                    .Is.Not.Default();

                return this._amountLookup.TryGetValue(mana, out var amount)
                    ? amount
                    : (ushort)0;
            }
        }

        public static ManaCost Parse(string value)
        {
            Guard
                .Require(value, nameof(value))
                .Is.Not.Empty();

            var match = Pattern.UnparsedValue.Match(value);

            if (!match.Success)
            {
                throw new ArgumentException($"Value [{value}] should contain colorless and/or color mana symbol.");
            }

            var manaCost = new ManaCost();

            var colorlessValue = match
                .FindCaptureValues("colorless")
                .SingleOrDefault();

            if (!string.IsNullOrEmpty(colorlessValue))
            {
                manaCost._amountLookup[Mana.Colorless] = ushort.Parse(colorlessValue);
            }

            match
                .FindCaptureValues("color")
                .Select(symbol => ManaCost.ManaLookup[symbol])
                .GroupBy(mana => mana)
                .ForEach(grouping => manaCost._amountLookup[grouping.Key] = (ushort)grouping.Count());

            return manaCost;
        }

        private static class Pattern
        {
            public static readonly Regex UnparsedValue = new Regex(
                @"^(?:{(?:(?<colorless>\d+)|(?<color>[WUBRG]))})+$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}