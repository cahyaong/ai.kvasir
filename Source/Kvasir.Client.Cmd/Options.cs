// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Options.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, July 28, 2020 6:47:18 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using CommandLine;

[Verb("processing-card")]
internal class ProcessingCardOption
{
    [Option("card-set-name", Required = true)]
    public string CardSetName { get; set; } = string.Empty;
}

[Verb("performing-experiment")]
internal class PerformingExperimentOption
{
}