// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleViewModel.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, January 18, 2020 7:21:21 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using nGratis.AI.Kvasir.Contract;
using ReactiveUI;

public class RuleViewModel : ReactiveObject
{
    public RuleViewModel(UnparsedBlob.Rule unparsedRule)
    {
        this.UnparsedRule = unparsedRule;
    }

    public UnparsedBlob.Rule UnparsedRule { get; }
}