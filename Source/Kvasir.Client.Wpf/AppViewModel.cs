// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppViewModel.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Wednesday, 24 October 2018 7:49:06 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

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
                    DisplayText = definitionAttribute?.DisplayText ?? DefinedText.Unknown,
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