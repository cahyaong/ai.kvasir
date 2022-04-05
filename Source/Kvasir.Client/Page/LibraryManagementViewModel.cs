// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LibraryManagementViewModel.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
//
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
//
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
//
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// </copyright>
// <author>Cahya Ong - cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, 23 October 2018 11:37:00 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client;

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
using nGratis.Cop.Olympus.Wpf;
using nGratis.Cop.Olympus.Wpf.Glue;
using ReactiveUI;

[PageDefinition("Library", Ordering = 1)]
public sealed class LibraryManagementViewModel : ReactiveScreen, IDisposable
{
    private readonly IUnprocessedMagicRepository _unprocessedRepository;

    private int _cardSetCount;

    private int _cardCount;

    private IDisposableCollection<CardSetViewModel> _cardSetViewModels;

    private CardSetViewModel _selectedCardSetViewModel;

    private bool _isDisposed;

    public LibraryManagementViewModel(IUnprocessedMagicRepository unprocessedRepository)
    {
        Guard
            .Require(unprocessedRepository, nameof(unprocessedRepository))
            .Is.Not.Null();

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

    public IDisposableCollection<CardSetViewModel> CardSetViewModels
    {
        get => this._cardSetViewModels;
        private set => this.RaiseAndSetIfChanged(ref this._cardSetViewModels, value);
    }

    public CardSetViewModel SelectedCardSetViewModel
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
            Guard
                .Require(repository, nameof(repository))
                .Is.Not.Null();

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