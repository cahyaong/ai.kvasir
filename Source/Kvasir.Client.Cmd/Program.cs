// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, March 28, 2020 6:00:41 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommandLine;
using nGratis.AI.Kvasir.Contract;

public class Program
{
    private static void Main(string[] args)
    {
        Parser.Default
            .ParseArguments(args, typeof(ProcessingCardOption), typeof(PlayingGameOption))
            .WithParsed<ProcessingCardOption>(Program.ProcessCard)
            .WithParsed<PlayingGameOption>(Program.PlayGame);

        if (Debugger.IsAttached)
        {
            Console.WriteLine();
            Console.WriteLine("Press <ANY> key to continue...");
            Console.ReadLine();
        }
    }

    private static void ProcessCard(ProcessingCardOption option)
    {
        using var appBootstrapper = new AppBootstrapper();
        var processingExecution = appBootstrapper.CreateExecution<ProcessingCardExecution>();

        var processingParameter = ExecutionParameter.Builder
            .Create()
            .WithEntry("CardSet.Name", option.CardSetName)
            .Build();

        Task.Run(async () => await processingExecution.ExecuteAsync(processingParameter))
            .Wait();
    }

    private static void PlayGame(PlayingGameOption option)
    {
        using var appBootstrapper = new AppBootstrapper();
        var playingExecution = appBootstrapper.CreateExecution<PlayingGameExecution>();

        Task.Run(async () => await playingExecution.ExecuteAsync(ExecutionParameter.None))
            .Wait();
    }
}