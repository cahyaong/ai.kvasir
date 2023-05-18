// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IKeyCalculator.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 21 December 2018 11:38:56 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core;

using System;
using nGratis.Cop.Olympus.Contract;

public interface IKeyCalculator
{
    DataSpec Calculate(Uri uri);
}