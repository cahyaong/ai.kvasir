// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JobResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Monday, July 8, 2024 12:16:16 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using nGratis.AI.Kvasir.Contract;

public class JobResult : KvasirResult
{
    public static JobResult Successful { get; } = new();

    public static JobResult Create(KvasirResult kvasirResult)
    {
        return new JobResult
        {
            Messages = kvasirResult.Messages
        };
    }
}