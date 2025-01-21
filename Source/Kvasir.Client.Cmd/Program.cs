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
using nGratis.Cop.Olympus.Contract;

public class Program
{
    private static readonly AppBootstrapper AppBootstrapper = new();

    private static void Main(string[] args)
    {
        // TODO (SHOULD): Consolidate steps to run experiment from empty cache!

        var magicLogger = AppBootstrapper.CreateMagicLogger();
        magicLogger.Log(Verbosity.Info, "Executing...");

        Parser.Default
            .ParseArguments<ProcessingCardOption, PerformingExperimentOption>(args)
            .WithParsed<ProcessingCardOption>(Program.ProcessCard)
            .WithParsed<PerformingExperimentOption>(Program.PerformExperiment);

        Program.AppBootstrapper.Dispose();

        if (Debugger.IsAttached)
        {
            magicLogger.Log(Verbosity.Info, "Press <ENTER> key to continue...");
            Console.ReadLine();
        }
    }

    private static void ProcessCard(ProcessingCardOption processingOption)
    {
        var processingJob = Program.AppBootstrapper.CreateJob<ProcessingCardJob>();

        var processingParameter = JobParameter.Builder
            .Create()
            .WithEntry("CardSet.Name", processingOption.CardSetName)
            .Build();

        Task.Run(async () => await processingJob.PerformAsync(processingParameter))
            .Wait();
    }

    private static void PerformExperiment(PerformingExperimentOption performingOption)
    {
        var performingJob = Program.AppBootstrapper.CreateJob<PerformingExperimentJob>();

        Task.Run(async () => await performingJob.PerformAsync(JobParameter.None))
            .Wait();
    }
}