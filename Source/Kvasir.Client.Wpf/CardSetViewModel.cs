﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardSetViewModel.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 10 November 2018 5:28:38 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using ReactiveUI;

public class CardSetViewModel : ReactiveObject
{
    private readonly IUnprocessedMagicRepository _unprocessedRepository;

    private IEnumerable<CardViewModel>? _cardViewModels;

    private CardViewModel? _selectedCardViewModel;

    private int _notParsedCardCount;

    private int _validCardCount;

    private int _invalidCardCount;

    public CardSetViewModel(UnparsedBlob.CardSet unparsedCardSet, IUnprocessedMagicRepository unprocessedRepository)
    {
        this._unprocessedRepository = unprocessedRepository;

        this.UnparsedCardSet = unparsedCardSet;
        this.CardViewModels = Enumerable.Empty<CardViewModel>();

        this.PopulateCardsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateCardsAsync());
        this.ParseCardsCommand = ReactiveCommand.CreateFromTask(async () => await this.ParseCardsAsync());
    }

    public UnparsedBlob.CardSet UnparsedCardSet { get; }

    public IEnumerable<CardViewModel>? CardViewModels
    {
        get => this._cardViewModels;
        private set => this.RaiseAndSetIfChanged(ref this._cardViewModels, value);
    }

    public CardViewModel? SelectedCardViewModel
    {
        get => this._selectedCardViewModel;

        set
        {
            this.RaiseAndSetIfChanged(ref this._selectedCardViewModel, value);
            this.SelectedCardViewModel?.PopulateDetailsCommand.Execute(null);
        }
    }

    public int NotParsedCardCount
    {
        get => this._notParsedCardCount;
        private set => this.RaiseAndSetIfChanged(ref this._notParsedCardCount, value);
    }

    public int ValidCardCount
    {
        get => this._validCardCount;
        private set => this.RaiseAndSetIfChanged(ref this._validCardCount, value);
    }

    public int InvalidCardCount
    {
        get => this._invalidCardCount;
        private set => this.RaiseAndSetIfChanged(ref this._invalidCardCount, value);
    }

    public ICommand PopulateCardsCommand { get; }

    public ICommand ParseCardsCommand { get; }

    private async Task PopulateCardsAsync()
    {
        var unparsedCards = await this._unprocessedRepository.GetCardsAsync(this.UnparsedCardSet);

        this.CardViewModels = unparsedCards
            .Select(unparsedCard => new CardViewModel(unparsedCard, this._unprocessedRepository))
            .ToArray();

        this.NotParsedCardCount = this.CardViewModels.Count();
        this.ValidCardCount = 0;
        this.InvalidCardCount = 0;

        this.CardViewModels
            .Select(vm => vm.WhenPropertyChanged())
            .Merge()
            .Where(pattern =>
                pattern.EventArgs.PropertyName == nameof(CardViewModel.DefinedCard) ||
                pattern.EventArgs.PropertyName == nameof(CardViewModel.ProcessingMessages))
            .Throttle(TimeSpan.FromMilliseconds(250))
            .Subscribe(_ => this.UpdateParsingStatistics());

        this.SelectedCardViewModel = this
            .CardViewModels
            .FirstOrDefault();
    }

    private async Task ParseCardsAsync()
    {
        if (this.CardViewModels?.Any() != true)
        {
            return;
        }

        this.SelectedCardViewModel ??= this.CardViewModels.First();

        await Task.Run(() =>
        {
            this.CardViewModels
                .AsParallel()
                .WithDegreeOfParallelism(8)
                .Where(vm => vm.ParseCardCommand.CanExecute(null))
                .ForEach(vm => vm.ParseCardCommand.Execute(null));
        });
    }

    private void UpdateParsingStatistics()
    {
        if (this.CardViewModels != null)
        {
            var parsedCardViewModels = this
                .CardViewModels
                .Where(vm => vm.DefinedCard != null)
                .ToImmutableArray();

            this.NotParsedCardCount = this.CardViewModels.Count() - parsedCardViewModels.Length;
            this.ValidCardCount = parsedCardViewModels.Count(vm => !vm.ProcessingMessages.Any());
            this.InvalidCardCount = parsedCardViewModels.Length - this.ValidCardCount;
        }
        else
        {
            this.NotParsedCardCount = 0;
            this.ValidCardCount = 0;
            this.InvalidCardCount = 0;
        }
    }
}