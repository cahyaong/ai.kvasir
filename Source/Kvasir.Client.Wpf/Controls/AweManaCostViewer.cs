// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AwePayingManaCostViewer.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, 7 January 2019 8:12:55 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.UI.Wpf;

[TemplatePart(Name = "PART_ContentPanel", Type = typeof(Panel))]
internal class AwePayingManaCostViewer : Control
{
    public static readonly DependencyProperty PayingManaCostProperty = DependencyProperty.Register(
        nameof(AwePayingManaCostViewer.PayingManaCost),
        typeof(DefinedBlob.PayingManaCost),
        typeof(AwePayingManaCostViewer),
        new PropertyMetadata(null, AwePayingManaCostViewer.OnPayingManaCostChanged));

    private static readonly IEnumerable<Mana> ColorManas = Enum
        .GetValues(typeof(Mana))
        .Cast<Mana>()
        .Where(mana => mana != Mana.Unknown)
        .Where(mana => mana != Mana.Colorless)
        .ToArray();

    private readonly IThemeManager _themeManager;

    private Panel? _contentPanel;

    public AwePayingManaCostViewer()
        : this(ThemeManager.Instance)
    {
    }

    private AwePayingManaCostViewer(IThemeManager themeManager)
    {
        this._themeManager = themeManager;
    }

    public DefinedBlob.PayingManaCost PayingManaCost
    {
        get => (DefinedBlob.PayingManaCost)this.GetValue(AwePayingManaCostViewer.PayingManaCostProperty);
        set => this.SetValue(AwePayingManaCostViewer.PayingManaCostProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        this._contentPanel = (Panel)this.Template.FindName("PART_ContentPanel", this);

        this.AddColorlessShape("?");
    }

    private static void OnPayingManaCostChanged(DependencyObject container, DependencyPropertyChangedEventArgs args)
    {
        if (container is not AwePayingManaCostViewer viewer)
        {
            return;
        }

        if (viewer._contentPanel == null)
        {
            throw new KvasirException("Content panel should have been initialized on applying template?!");
        }

        viewer._contentPanel.Children.Clear();

        if (args.NewValue is not DefinedBlob.PayingManaCost payingManaCost)
        {
            viewer.AddColorlessShape("?");

            return;
        }

        viewer.AddColorlessShape(payingManaCost[Mana.Colorless].ToString());

        AwePayingManaCostViewer
            .ColorManas
            .Select(mana => new
            {
                Mana = mana,
                Amount = payingManaCost[mana]
            })
            .Where(anon => anon.Amount > 0)
            .ForEach(anon => viewer.AddColorShapes(anon.Mana, anon.Amount));
    }

    private void AddColorlessShape(string value)
    {
        if (this._contentPanel == null)
        {
            throw new KvasirException("Content panel should have been initialized on applying template?!");
        }

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

        if (this._contentPanel == null)
        {
            throw new KvasirException("Content panel should have been initialized on applying template?!");
        }

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