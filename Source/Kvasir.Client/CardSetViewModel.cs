// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardSetViewModel.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 10 November 2018 5:28:38 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Contract.Magic;
    using nGratis.Cop.Core.Contract;
    using ReactiveUI;

    public class CardSetViewModel : ReactiveObject
    {
        private readonly IMagicRepository _magicRepository;

        private IEnumerable<Card> _cards;

        public CardSetViewModel(CardSet cardSet, IMagicRepository magicRepository)
        {
            Guard
                .Require(cardSet, nameof(cardSet))
                .Is.Not.Null();

            Guard
                .Require(magicRepository, nameof(magicRepository))
                .Is.Not.Null();

            this._magicRepository = magicRepository;

            this.CardSet = cardSet;
            this.Cards = Enumerable.Empty<Card>();

            this.PopulateCardsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateCardsAsync());
        }

        public CardSet CardSet { get; }

        public IEnumerable<Card> Cards
        {
            get => this._cards;
            private set => this.RaiseAndSetIfChanged(ref this._cards, value);
        }

        public ICommand PopulateCardsCommand { get; }

        private async Task PopulateCardsAsync()
        {
            this.Cards = await this._magicRepository.GetCardsAsync(this.CardSet);
        }
    }
}