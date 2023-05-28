// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParameter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Sunday, March 19, 2023 12:14:20 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Contract;

public interface IParameter
{
    TValue FindValue<TValue>(ParameterKey key);
}