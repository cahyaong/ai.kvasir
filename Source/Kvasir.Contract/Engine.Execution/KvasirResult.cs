// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KvasirResult.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, July 23, 2020 5:44:12 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

using System.Collections.Generic;
using System.Linq;

public abstract class KvasirResult
{
    protected KvasirResult()
    {
        this.Messages = Enumerable.Empty<string>();
    }

    protected KvasirResult(IEnumerable<string> messages)
    {
        this.Messages = messages;
    }

    public bool HasError =>
        this.Messages.Any() ||
        this.HasErrorCore();

    // TODO (SHOULD): Add `required` modifier!
    public IEnumerable<string> Messages { get; protected init; }

    protected virtual bool HasErrorCore() => false;
}