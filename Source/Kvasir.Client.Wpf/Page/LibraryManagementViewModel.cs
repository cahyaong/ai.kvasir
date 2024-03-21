// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LibraryManagementViewModel.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, 23 October 2018 11:37:00 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.UI.Wpf;
using nGratis.Cop.Olympus.UI.Wpf.Glue;
using ReactiveUI;

[PageDefinition("Library", Ordering = 1)]
public sealed class LibraryManagementViewModel : ReactiveScreen, IDisposable
{
    private readonly IUnprocessedMagicRepository _unprocessedRepository;

    private int _cardSetCount;

    private int _cardCount;

    private IDisposableCollection<CardSetViewModel>? _cardSetViewModels;

    private CardSetViewModel? _selectedCardSetViewModel;

    private bool _isDisposed;

    public LibraryManagementViewModel(IUnprocessedMagicRepository unprocessedRepository)
    {
        this._unprocessedRepository = unprocessedRepository;
    }

    ~LibraryManagementViewModel()
    {
        this.Dispose(false);
    }

    public int CardSetCount
    {
        get => this._cardSetCount;
        private set => this.RaiseAndSetIfChanged(ref this._cardSetCount, value);
    }

    public int CardCount
    {
        get => this._cardCount;
        private set => this.RaiseAndSetIfChanged(ref this._cardCount, value);
    }

    public IDisposableCollection<CardSetViewModel>? CardSetViewModels
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

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override async Task ActivateCoreAsync(CancellationToken cancellationToken)
    {
        this.PopulateCardSets();

        await Task.CompletedTask;
    }

    private void PopulateCardSets()
    {
        var virtualizingProvider = new CardSetViewModelProvider(this._unprocessedRepository);

        this.CardSetViewModels?.Dispose();
        this.CardSetViewModels = new AsyncVirtualizingCollection<CardSetViewModel>(virtualizingProvider);

        Observable
            .FromEventPattern<NotifyCollectionChangedEventArgs>(this.CardSetViewModels, "CollectionChanged")
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Subscribe(async _ =>
            {
                this.CardSetCount = this.CardSetViewModels.Count;
                this.CardCount = await this._unprocessedRepository.GetCardCountAsync();
            });

        Observable
            .FromEventPattern<EventArgs>(this._unprocessedRepository, "CardIndexed")
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(async _ => this.CardCount = await this._unprocessedRepository.GetCardCountAsync());
    }

    private void Dispose(bool isDisposing)
    {
        if (this._isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            this._cardSetViewModels?.Dispose();
        }

        this._isDisposed = true;
    }

    private sealed class CardSetViewModelProvider : IPagingDataProvider<CardSetViewModel>
    {
        private readonly IUnprocessedMagicRepository _repository;

        public CardSetViewModelProvider(IUnprocessedMagicRepository repository)
        {
            this._repository = repository;
        }

        public CardSetViewModel DefaultItem
        {
            get
            {
                var unparsedCardSet = new UnparsedBlob.CardSet
                {
                    Code = "---",
                    Name = "Loading...",
                    ReleasedTimestamp = DateTime.MinValue
                };

                return new CardSetViewModel(unparsedCardSet, this._repository);
            }
        }

        public async Task<int> GetCountAsync()
        {
            return await this._repository.GetCardSetCountAsync();
        }

        public async Task<IReadOnlyCollection<CardSetViewModel>> GetItemsAsync(int pagingIndex, int itemCount)
        {
            var unparsedCardSets = await this._repository.GetCardSetsAsync(pagingIndex, itemCount);

            return unparsedCardSets
                .Select(unparsedCardSet => new CardSetViewModel(unparsedCardSet, this._repository))
                .ToArray();
        }
    }
}