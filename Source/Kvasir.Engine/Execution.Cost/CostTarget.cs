// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CostTarget.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, April 15, 2023 4:49:02 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

public class CostTarget : ICostTarget
{
    public CostTarget()
    {
        this.Player = Engine.Player.Unknown;
    }

    public static ICostTarget Unknown => UnknownCostTarget.Instance;

    public static ICostTarget None => NoneCostTarget.Instance;

    public IPlayer Player { get; set; }
}