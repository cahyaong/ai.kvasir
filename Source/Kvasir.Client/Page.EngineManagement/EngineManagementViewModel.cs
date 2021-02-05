// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EngineManagementViewModel.cs" company="nGratis">
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
// <creation_timestamp>Tuesday, 1 January 2019 6:04:11 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Wpf;
    using nGratis.Cop.Olympus.Wpf.Glue;
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

        private IEnumerable<CardSetViewModel> _cardSetViewModels;

        private CardSetViewModel _selectedCardSetViewModel;

        public EngineManagementViewModel(IUnprocessedMagicRepository unprocessedRepository)
        {
            Guard
                .Require(unprocessedRepository, nameof(unprocessedRepository))
                .Is.Not.Null();

            this._unprocessedRepository = unprocessedRepository;
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
                this.SelectedCardSetViewModel?.PopulateCardsCommand.Execute(null);
            }
        }

        protected override async Task ActivateCoreAsync(CancellationToken cancellationToken)
        {
            await this.PopulateCardSetsAsync();

            this.SelectedCardSetViewModel = this.CardSetViewModels.FirstOrDefault();
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
}