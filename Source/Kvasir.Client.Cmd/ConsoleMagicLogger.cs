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
using System.Linq;
using System.Text;
using Humanizer;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using Spectre.Console;
using Spectre.Console.Rendering;

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
        this._layout[LayoutId.Status]
            .Update(new Panel(ConsoleMagicLogger.CreateStatusRendering(tabletop))
                .Header("< STATUS >")
                .SquareBorder()
                .Expand());

        var firstPlayer = tabletop.Players.First();
        var secondPlayer = tabletop.Players.Last();

        this._layout[LayoutId.PlayerOne]
            .Update(new Panel(ConsoleMagicLogger.CreatePlayerRendering(tabletop, firstPlayer))
                .Header($"< PLAYER 1 — {firstPlayer.Name} >")
                .SquareBorder()
                .Expand());

        this._layout[LayoutId.PlayerTwo]
            .Update(new Panel(ConsoleMagicLogger.CreatePlayerRendering(tabletop, secondPlayer))
                .Header($"< PLAYER 2 — {secondPlayer.Name} >")
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

    private void Redraw()
    {
        AnsiConsole.Cursor.SetPosition(0, 0);
        AnsiConsole.Write(this._layout);
    }

    private static Layout CreateLayout()
    {
        var statusLayout = new Layout(LayoutId.Status);

        var tabletopLayout = new Layout(LayoutId.Tabletop).SplitColumns(
            new Layout(LayoutId.PlayerOne).Ratio(1),
            new Layout(LayoutId.PlayerTwo).Ratio(1),
            new Layout(LayoutId.Stack).Ratio(1));

        var notificationLayout = new Layout(LayoutId.Notification);

        var mainLayout = new Layout().SplitRows(
            statusLayout.Ratio(2),
            tabletopLayout.Ratio(7),
            notificationLayout.Ratio(1));

        return mainLayout;
    }

    private static IRenderable CreateStatusRendering(ITabletop tabletop)
    {
        return ConsoleMagicLogger
            .CreateSimpleTable()
            .AddRow("Turn ID", tabletop.TurnId.ToString())
            .AddRow("Phase", tabletop.Phase.Humanize(LetterCasing.Title));
    }

    private static IRenderable CreatePlayerRendering(ITabletop tabletop, IPlayer player)
    {
        return ConsoleMagicLogger
            .CreateSimpleTable()
            .Expand()
            .AddRow("Life", player.Life.ToString())
            .AddRow("Status", tabletop.ActivePlayer == player ? "Active" : "Non-Active")
            .AddRow(
                "Library",
                new StringBuilder()
                    .AppendLine(player.Library.Quantity.ToString())
                    .Append(player.Library.IsEmpty 
                        ? Contract.DefinedText.Empty
                        : player.Library.FindFromTop().Name)
                    .ToString())
            .AddRow("Hand", player.Hand.Quantity.ToString())
            .AddRow(
                "Battlefield",
                tabletop.Battlefield.FindAll().Count(permanent => permanent.ControllingPlayer == player).ToString())
            .AddRow("Graveyard", player.Graveyard.Quantity.ToString());
    }

    private static Table CreateSimpleTable()
    {
        return new Table()
            .HideHeaders()
            .SimpleBorder()
            .ShowRowSeparators()
            .BorderColor(Color.Grey)
            .AddColumn(
                string.Empty,
                column => column.NoWrap().RightAligned().Width(12))
            .AddColumn(
                string.Empty,
                column => column.NoWrap().LeftAligned().Width(18));
    }

    private static class LayoutId
    {
        public const string Tabletop = "<tabletop>";
        public const string PlayerOne = "<tabletop.player-one>";
        public const string PlayerTwo = "<tabletop.player-two>";
        public const string Stack = "<tabletop.stack>";

        public const string Status = "<status>";
        public const string Notification = "<notification>";
    }
}