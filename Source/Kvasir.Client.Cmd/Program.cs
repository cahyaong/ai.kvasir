﻿// --------------------------------------------------------------------------------------------------------------------
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
using nGratis.Cop.Olympus.Contract;

public class Program
{
    private static readonly AppBootstrapper AppBootstrapper = new();

    private static void Main(string[] args)
    {
        var magicLogger = AppBootstrapper.CreateMagicLogger();
        magicLogger.Log(Verbosity.Info, "Executing...");

        Parser.Default
            .ParseArguments(args, typeof(ProcessingCardOption), typeof(PlayingGameOption))
            .WithParsed<ProcessingCardOption>(Program.ProcessCard)
            .WithParsed<PlayingGameOption>(Program.PlayGame);

        if (Debugger.IsAttached)
        {
            magicLogger.Log(Verbosity.Info, "Press <ENTER> key to continue...");
            Console.ReadLine();
        }
    }

    private static void ProcessCard(ProcessingCardOption option)
    {
        var processingJob = Program.AppBootstrapper.CreateJob<ProcessingCardJob>();

        var processingParameter = JobParameter.Builder
            .Create()
            .WithEntry("CardSet.Name", option.CardSetName)
            .Build();

        Task.Run(async () => await processingJob.PerformAsync(processingParameter))
            .Wait();
    }

    private static void PlayGame(PlayingGameOption option)
    {
        var playingJob = Program.AppBootstrapper.CreateJob<PlayingGameJob>();

        Task.Run(async () => await playingJob.PerformAsync(JobParameter.None))
            .Wait();
    }
}