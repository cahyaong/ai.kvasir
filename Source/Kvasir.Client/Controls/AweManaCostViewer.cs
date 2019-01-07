// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AweManaCostViewer.cs" company="nGratis">
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
// <creation_timestamp>Monday, 7 January 2019 8:12:55 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;
    using nGratis.Cop.Core.Wpf;

    [TemplatePart(Name = "PART_ContentPanel", Type = typeof(Panel))]
    internal class AweManaCostViewer : Control
    {
        public static readonly DependencyProperty ManaCostProperty = DependencyProperty.Register(
            nameof(AweManaCostViewer.ManaCost),
            typeof(ManaCost),
            typeof(AweManaCostViewer),
            new PropertyMetadata(null, AweManaCostViewer.OnManaCostChanged));

        private static readonly IEnumerable<Mana> ColorManas = Enum
            .GetValues(typeof(Mana))
            .Cast<Mana>()
            .Where(mana => mana != Mana.Unknown)
            .Where(mana => mana != Mana.Colorless)
            .ToArray();

        private readonly IThemeManager _themeManager;

        private Panel _contentPanel;

        public AweManaCostViewer()
            : this(ThemeManager.Instance)
        {
        }

        internal AweManaCostViewer(IThemeManager themeManager)
        {
            Guard
                .Require(themeManager, nameof(themeManager))
                .Is.Not.Null();

            this._themeManager = themeManager;
        }

        public ManaCost ManaCost
        {
            get => (ManaCost)this.GetValue(AweManaCostViewer.ManaCostProperty);
            set => this.SetValue(AweManaCostViewer.ManaCostProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this._contentPanel = (Panel)this.Template.FindName("PART_ContentPanel", this);

            this.AddColorlessShape("?");
        }

        private static void OnManaCostChanged(DependencyObject container, DependencyPropertyChangedEventArgs args)
        {
            if (!(container is AweManaCostViewer viewer))
            {
                return;
            }

            viewer._contentPanel.Children.Clear();

            if (!(args.NewValue is ManaCost manaCost))
            {
                viewer.AddColorlessShape("?");

                return;
            }

            viewer.AddColorlessShape(manaCost[Mana.Colorless].ToString());

            AweManaCostViewer
                .ColorManas
                .Select(mana => new
                {
                    Mana = mana,
                    Amount = manaCost[mana]
                })
                .Where(anon => anon.Amount > 0)
                .ForEach(anon => viewer.AddColorShapes(anon.Mana, anon.Amount));
        }

        private void AddColorlessShape(string value)
        {
            var primaryBrush = this._themeManager.FindBrush("Cop.Brush.Shade1");
            var secondaryBrush = this._themeManager.FindBrush("Cop.Brush.Shade6");

            var shape = new Grid
            {
                Children =
                {
                    new Ellipse
                    {
                        Width = 16,
                        Height = 16,
                        Fill = secondaryBrush,
                        Stroke = secondaryBrush
                    },
                    new TextBlock
                    {
                        Text = !string.IsNullOrEmpty(value) ? value : "?",
                        Foreground = primaryBrush,
                        FontSize = 12,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                },
                Margin = new Thickness(0, 0, 2, 0)
            };

            this._contentPanel.Children.Add(shape);
        }

        private void AddColorShapes(Mana mana, ushort count)
        {
            Guard
                .Require(mana, nameof(mana))
                .Is.Not.Default();

            var primaryBrush = this._themeManager.FindBrush($"AI.Brush.Mana.{mana}.Primary");
            var secondaryBrush = this._themeManager.FindBrush($"AI.Brush.Mana.{mana}.Secondary");

            Enumerable
                .Range(0, count)
                .Select(_ => new Ellipse
                {
                    Width = 16,
                    Height = 16,
                    Fill = primaryBrush,
                    Stroke = secondaryBrush,
                    StrokeThickness = 1,
                    Margin = new Thickness(0, 0, 2, 0)
                })
                .ForEach(shape => this._contentPanel.Children.Add(shape));
        }
    }
}