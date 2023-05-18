// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 9 November 2018 10:55:58 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using nGratis.AI.Kvasir.Contract;

public static class KvasirExtensions
{
    public static bool IsDated(this DateTime timestamp)
    {
        return
            timestamp >= Constant.EpochTimestamp &&
            timestamp <= DateTime.UtcNow;
    }
}