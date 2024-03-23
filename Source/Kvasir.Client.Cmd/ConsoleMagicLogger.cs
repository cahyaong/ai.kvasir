// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleMagicLogger.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 23, 2024 2:35:21 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System.Text;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using Spectre.Console;

public class ConsoleMagicLogger : IMagicLogger
{
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
            .AppendLine($"Phase: {tabletop.Phase}")
            .AppendLine($"Active Player: {tabletop.ActivePlayer.Name}")
            .ToString());

        this._layout[LayoutId.Status]
            .Update(statusRendering);

        this.Redraw();
    }

    public void Log(Verbosity verbosity, string message)
    {
        var notificationRendering = new Markup(message, new Style().Foreground(Color.CornflowerBlue));

        this._layout[LayoutId.Notification]
            .Update(new Panel(notificationRendering)
                .BorderColor(Color.CornflowerBlue)
                .Expand());

        this.Redraw();
    }

    private static Layout CreateLayout()
    {
        var tabletopLayout = new Layout(LayoutId.Tabletop)
            .Ratio(9)
            .SplitColumns(
                new Layout(LayoutId.Status).Ratio(3),
                new Layout(LayoutId.Battlefield).Ratio(7));

        var notificationLayout = new Layout(LayoutId.Notification)
            .Ratio(1);

        var mainLayout = new Layout()
            .SplitRows(tabletopLayout, notificationLayout);

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
        public const string Battlefield = "<tabletop.battlefield>";

        public const string Notification = "<notification>";
    }
}