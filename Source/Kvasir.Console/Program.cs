// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
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
// <creation_timestamp>Saturday, March 28, 2020 6:00:41 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Console;

using System;
using System.Threading.Tasks;
using CommandLine;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;

public class Program
{
    private static void Main(string[] args)
    {
        Parser.Default
            .ParseArguments(args, typeof(ProcessingCardOption), typeof(PlayingGameOption))
            .WithParsed<ProcessingCardOption>(Program.ProcessCard)
            .WithParsed<PlayingGameOption>(Program.PlayGame);

        Console.WriteLine();
        Console.WriteLine("Press <ANY> key to continue...");
        Console.ReadLine();
    }

    private static void ProcessCard(ProcessingCardOption option)
    {
        Guard
            .Require(option, nameof(option))
            .Is.Not.Null();

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
        Guard
            .Require(option, nameof(option))
            .Is.Not.Null();

        using var appBootstrapper = new AppBootstrapper();

        var playingExecution = appBootstrapper.CreateExecution<PlayingGameExecution>();

        Task.Run(async () => await playingExecution.ExecuteAsync(ExecutionParameter.None))
            .Wait();
    }
}