﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IJob.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, April 2, 2020 5:48:24 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System.Threading.Tasks;

public interface IJob
{
    Task<JobResult> PerformAsync(JobParameter parameter);
}