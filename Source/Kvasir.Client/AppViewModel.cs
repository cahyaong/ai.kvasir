﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppViewModel.cs" company="nGratis">
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
// <creation_timestamp>Wednesday, 24 October 2018 7:49:06 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using Caliburn.Micro;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Wpf;

    public class AppViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public AppViewModel(IEnumerable<IScreen> screens)
        {
            Guard
                .Require(screens, nameof(screens))
                .Is.Not.Empty();

            var orderedScreens = screens
                .Where(screen => screen != null)
                .Select(screen =>
                {
                    var definitionAttribute = screen
                        .GetType()
                        .GetCustomAttribute<PageDefinitionAttribute>();

                    return new
                    {
                        Screen = screen,
                        DisplayText = definitionAttribute?.DisplayText ?? Text.Undefined,
                        Ordering = definitionAttribute?.Ordering ?? int.MaxValue,
                        TypeName = screen.GetType().Name
                    };
                })
                .Select(anon =>
                {
                    anon.Screen.DisplayName = anon
                        .DisplayText
                        .ToLowerInvariant()
                        .Replace(" ", "-");

                    return anon;
                })
                .OrderBy(anon => anon.Ordering)
                .ThenBy(anon => anon.DisplayText)
                .ThenBy(anon => anon.TypeName)
                .Select(anon => anon.Screen)
                .ToImmutableList();

            this.Items.AddRange(orderedScreens);
        }
    }
}