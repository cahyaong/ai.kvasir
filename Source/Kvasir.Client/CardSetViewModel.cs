// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardSetViewModel.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
// <creation_timestamp>Saturday, 10 November 2018 5:28:38 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Olympus.Contract;
    using ReactiveUI;

    public class CardSetViewModel : ReactiveObject
    {
        private readonly IMagicRepository _repository;

        private IEnumerable<CardViewModel> _cardViewModels;

        private CardViewModel _selectedCardViewModel;

        private int _notParsedCardCount;

        private int _validCardCount;

        private int _invalidCardCount;

        public CardSetViewModel(UnparsedBlob.CardSet unparsedCardSet, IMagicRepository repository)
        {
            Guard
                .Require(unparsedCardSet, nameof(unparsedCardSet))
                .Is.Not.Null();

            Guard
                .Require(repository, nameof(repository))
                .Is.Not.Null();

            this._repository = repository;

            this.UnparsedCardSet = unparsedCardSet;
            this.CardViewModels = Enumerable.Empty<CardViewModel>();

            this.PopulateCardsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateCardsAsync());
            this.ParseCardsCommand = ReactiveCommand.CreateFromTask(async () => await this.ParseCardsAysnc());
        }

        public UnparsedBlob.CardSet UnparsedCardSet { get; }

        public IEnumerable<CardViewModel> CardViewModels
        {
            get => this._cardViewModels;
            private set => this.RaiseAndSetIfChanged(ref this._cardViewModels, value);
        }

        public CardViewModel SelectedCardViewModel
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
            var unparsedCards = await this._repository.GetCardsAsync(this.UnparsedCardSet);

            this.CardViewModels = unparsedCards
                .Select(unparsedCard => new CardViewModel(unparsedCard, this._repository))
                .ToArray();

            this.NotParsedCardCount = this.CardViewModels.Count();
            this.ValidCardCount = 0;
            this.InvalidCardCount = 0;

            this.CardViewModels
                .Select(vm => vm.WhenPropertyChanged())
                .Merge()
                .Where(pattern =>
                    pattern.EventArgs.PropertyName == nameof(CardViewModel.DefinedCard) ||
                    pattern.EventArgs.PropertyName == nameof(CardViewModel.ParsingMessages))
                .Throttle(TimeSpan.FromMilliseconds(250))
                .Subscribe(_ => this.UpdateParsingStatistics());
        }

        private async Task ParseCardsAysnc()
        {
            if (this.CardViewModels?.Any() != true)
            {
                return;
            }

            if (this.SelectedCardViewModel == null)
            {
                this.SelectedCardViewModel = this.CardViewModels.First();
            }

            await Task.Run(() =>
            {
                this.SelectedCardViewModel.ParseCardCommand.Execute(null);

                this.CardViewModels
                    .AsParallel()
                    .Where(vm => vm.ParseCardCommand.CanExecute(null))
                    .ForEach(vm => vm.ParseCardCommand.Execute(null));
            });
        }

        private void UpdateParsingStatistics()
        {
            var parsedCardViewModels = this
                .CardViewModels
                .Where(vm => vm.DefinedCard != null)
                .ToArray();

            this.NotParsedCardCount = this.CardViewModels.Count() - parsedCardViewModels.Length;
            this.ValidCardCount = parsedCardViewModels.Count(vm => !vm.ParsingMessages.Any());
            this.InvalidCardCount = parsedCardViewModels.Length - this.ValidCardCount;
        }
    }
}