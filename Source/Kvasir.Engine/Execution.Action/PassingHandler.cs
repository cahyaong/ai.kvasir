// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PassingHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, September 18, 2022 6:09:11 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public class PassingHandler : BaseActionHandler
{
    public override ActionKind ActionKind => ActionKind.Passing;

    protected override void ResolveCore(ITabletop _, IAction __)
    {
    }
}