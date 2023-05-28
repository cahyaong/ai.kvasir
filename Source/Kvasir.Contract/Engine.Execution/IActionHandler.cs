// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IActionHandler.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, September 18, 2022 12:28:27 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IActionHandler
{
    ActionKind ActionKind { get; }

    bool IsSpecialAction { get; }

    ICost FindCost(IAction action);

    ValidationResult Validate(ITabletop tabletop, IAction action);

    void Resolve(ITabletop tabletop, IAction action);
}