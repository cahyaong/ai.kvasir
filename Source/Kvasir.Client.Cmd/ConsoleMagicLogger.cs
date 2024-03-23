// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConsoleMagicLogger.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 23, 2024 2:35:21 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

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
    }

    public void Log(Verbosity verbosity, string message)
    {
        var content = new Markup(message, new Style().Foreground(Color.CornflowerBlue));

        this._layout[LayoutId.Notification]
            .Update(new Panel(content)
                .BorderColor(Color.CornflowerBlue)
                .Expand());

        this.Redraw();
    }

    private static Layout CreateLayout()
    {
        var layout = new Layout();

        layout.SplitRows(
            new Layout(LayoutId.Tabletop).Ratio(9),
            new Layout(LayoutId.Notification).Ratio(1));

        return layout;
    }

    private void Redraw()
    {
        AnsiConsole.Cursor.SetPosition(0, 0);
        AnsiConsole.Write(this._layout);
    }

    private static class LayoutId
    {
        public const string Tabletop = "<tabletop>";
        public const string Notification = "<notification>";
    }
}