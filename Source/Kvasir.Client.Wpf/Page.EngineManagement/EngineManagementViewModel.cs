// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineManagementViewModel.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, 1 January 2019 6:04:11 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.UI.Wpf;
using nGratis.Cop.Olympus.UI.Wpf.Glue;
using ReactiveUI;

[PageDefinition("Engine", Ordering = 2)]
public class EngineManagementViewModel : ReactiveScreen
{
    // TODO: Consider adding searching filter on UI instead!

    private static readonly IEnumerable<string> TargetCardSetNames = new[]
    {
        "Duel Decks: Elves vs. Goblins",
        "Lorwyn",
        "Morningtide",
        "Portal"
    };

    private readonly IUnprocessedMagicRepository _unprocessedRepository;

    private IEnumerable<CardSetViewModel>? _cardSetViewModels;

    private CardSetViewModel? _selectedCardSetViewModel;

    public EngineManagementViewModel(IUnprocessedMagicRepository unprocessedRepository)
    {
        this._unprocessedRepository = unprocessedRepository;
    }

    public IEnumerable<CardSetViewModel>? CardSetViewModels
    {
        get => this._cardSetViewModels;
        private set => this.RaiseAndSetIfChanged(ref this._cardSetViewModels, value);
    }

    public CardSetViewModel? SelectedCardSetViewModel
    {
        get => this._selectedCardSetViewModel;

        set
        {
            this.RaiseAndSetIfChanged(ref this._selectedCardSetViewModel, value);
            this.SelectedCardSetViewModel?.PopulateCardsCommand.Execute(null);
        }
    }

    protected override async Task ActivateCoreAsync(CancellationToken cancellationToken)
    {
        await this.PopulateCardSetsAsync();

        this.SelectedCardSetViewModel = this.CardSetViewModels?.FirstOrDefault();
    }

    private async Task PopulateCardSetsAsync()
    {
        var unparsedCardSets = await this._unprocessedRepository.GetCardSetsAsync();

        this.CardSetViewModels = unparsedCardSets
            .Where(unparsedCardSet => EngineManagementViewModel.TargetCardSetNames.Contains(unparsedCardSet.Name))
            .OrderBy(unparsedCardSet => unparsedCardSet.Name)
            .Select(unparsedCardSet => new CardSetViewModel(unparsedCardSet, this._unprocessedRepository))
            .ToArray();
    }
}