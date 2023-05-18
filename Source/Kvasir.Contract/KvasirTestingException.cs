// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirTestingException.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, 8 November 2018 10:34:13 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public class KvasirTestingException : KvasirException
{
    public KvasirTestingException(string message)
        : base(message)
    {
    }

    public KvasirTestingException(string message, params string[] submessages)
        : base(message, submessages)
    {
    }

    public KvasirTestingException(string message, params (string Key, object Value)[] details)
        : base(message, details)
    {
    }
}