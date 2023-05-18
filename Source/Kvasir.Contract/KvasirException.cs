// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirException.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 25 October 2018 8:46:37 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System;
using System.Linq;

public class KvasirException : Exception
{
    public KvasirException(string message)
        : base(message)
    {
    }

    public KvasirException(string message, params string[] submessages)
        : base(submessages.Aggregate(message, (blob, submessage) => $"{blob} {submessage}"))
    {
    }

    public KvasirException(string message, params (string Key, object Value)[] details)
        : this(message, details.Select(detail => $"{detail.Key}: [{detail.Value}].").ToArray())
    {
    }
}