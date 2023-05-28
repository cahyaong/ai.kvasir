// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DoingNothingHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, May 12, 2023 5:51:53 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public class DoingNothingHandler : ICostHandler
{
    public CostKind CostKind => CostKind.DoingNothing;

    public ValidationResult Validate(ITabletop _, ICost __, ITarget ___)
    {
        return ValidationResult.Successful;
    }

    public void Resolve(ITabletop _, ICost __, ITarget ___)
    {
    }
}