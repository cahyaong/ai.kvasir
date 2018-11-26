// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardLibraryViewModel.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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

namespace nGratis.AI.Kvasir.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using JetBrains.Annotations;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Core.Contract;
    using ReactiveUI;

    [UsedImplicitly]
    public class CardLibraryViewModel : ReactiveObject
    {
        private readonly IMagicRepository _magicRepository;

        private int cardCount;

        private IEnumerable<CardSetViewModel> _cardSetViewModels;

        private CardSetViewModel _selectedCardSetViewModel;

        public CardLibraryViewModel(IMagicRepository magicRepository)
        {
            Guard
                .Require(magicRepository, nameof(magicRepository))
                .Is.Not.Null();

            this._magicRepository = magicRepository;

            this.CardSetViewModels = Enumerable.Empty<CardSetViewModel>();
            this.PopulateCardSetsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateCardSetsAsync());
        }

        public int CardCount
        {
            get => this.cardCount;
            private set => this.RaiseAndSetIfChanged(ref this.cardCount, value);
        }

        public IEnumerable<CardSetViewModel> CardSetViewModels
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
                this.SelectedCardSetViewModel.PopulateCardsCommand.Execute(null);
            }
        }

        public ICommand PopulateCardSetsCommand { get; }

        private async Task PopulateCardSetsAsync()
        {
            await Task.Run(async () =>
            {
                var cardSets = await this._magicRepository.GetCardSetsAsync();

                // TODO: Implement custom sorter in <DataGrid> instead of here!
                // TODO: Implement pagination on <DataGrid> to improve rendering performance!

                this.CardSetViewModels = cardSets
                    .OrderByDescending(cardSet => cardSet.ReleasedTimestamp.IsDated()
                        ? cardSet.ReleasedTimestamp
                        : DateTime.MinValue)
                    .ThenBy(cardSet => cardSet.Name)
                    .Select(cardSet => new CardSetViewModel(cardSet, this._magicRepository))
                    .ToArray();

                this.CardCount = await this._magicRepository.GetCardCountAsync();
            });

            this.CardSetViewModels
                .Select(viewModel => viewModel.Changed)
                .Merge()
                .Throttle(TimeSpan.FromMilliseconds(500))
                .Subscribe(async _ => this.CardCount = await this._magicRepository.GetCardCountAsync());
        }
    }
}