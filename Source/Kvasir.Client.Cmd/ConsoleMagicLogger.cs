// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleMagicLogger.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 23, 2024 2:35:21 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System.Collections.Generic;
using System.Text;
using Humanizer;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using Spectre.Console;

public class ConsoleMagicLogger : IMagicLogger
{
    private static readonly IReadOnlyDictionary<Verbosity, Color> ColorByVerbosityLookup =
        new Dictionary<Verbosity, Color>
        {
            [Verbosity.Error] = Color.IndianRed,
            [Verbosity.Warning] = Color.DarkGoldenrod,
            [Verbosity.Info] = Color.CornflowerBlue
        };

    private readonly Layout _layout;

    public ConsoleMagicLogger()
    {
        AnsiConsole.Cursor.Show(false);

        this._layout = ConsoleMagicLogger.CreateLayout();
        this.Redraw();
    }

    public void Log(ITabletop tabletop)
    {
        var statusRendering = new Markup(new StringBuilder()
            .AppendLine($"Turn ID: {tabletop.TurnId}")
            .AppendLine($"Phase: {tabletop.Phase.Humanize(LetterCasing.Title)}")
            .AppendLine($"Active Player: {tabletop.ActivePlayer.Name}")
            .ToString());

        this._layout[LayoutId.Status]
            .Update(new Panel(statusRendering)
                .Header("< STATUS >")
                .SquareBorder()
                .Expand());

        this._layout[LayoutId.PlayerOne]
            .Update(new Panel(new Markup("..."))
                .Header("< PLAYER 1 >")
                .SquareBorder()
                .Expand());

        this._layout[LayoutId.PlayerTwo]
            .Update(new Panel(new Markup("..."))
                .Header("< PLAYER 2 >")
                .SquareBorder()
                .Expand());

        this.Redraw();
    }

    public void Log(Verbosity verbosity, string message)
    {
        if (!ConsoleMagicLogger.ColorByVerbosityLookup.TryGetValue(verbosity, out var color))
        {
            color = Color.HotPink;
        }

        var notificationRendering = new Markup(
            message,
            new Style().Foreground(color));

        this._layout[LayoutId.Notification]
            .Update(new Panel(notificationRendering)
                .BorderColor(color)
                .Expand());

        this.Redraw();
    }

    private static Layout CreateLayout()
    {
        var statusLayout = new Layout(LayoutId.Status);

        var playerLayout = new Layout().SplitColumns(
            new Layout(LayoutId.PlayerOne),
            new Layout(LayoutId.PlayerTwo));

        var tabletopLayout = new Layout(LayoutId.Tabletop).SplitRows(
            statusLayout.Ratio(2),
            playerLayout.Ratio(8));

        var notificationLayout = new Layout(LayoutId.Notification);

        var mainLayout = new Layout().SplitRows(
            tabletopLayout.Ratio(9),
            notificationLayout.Ratio(1));

        return mainLayout;
    }

    private void Redraw()
    {
        AnsiConsole.Cursor.SetPosition(0, 0);
        AnsiConsole.Write(this._layout);
    }

    private static class LayoutId
    {
        public const string Tabletop = "<tabletop>";
        public const string Status = "<tabletop.status>";
        public const string PlayerOne = "<tabletop.player-one>";
        public const string PlayerTwo = "<tabletop.player-two>";

        public const string Notification = "<notification>";
    }
}