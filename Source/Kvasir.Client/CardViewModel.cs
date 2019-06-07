// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardViewModel.cs" company="nGratis">
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
// <creation_timestamp>Friday, 21 December 2018 11:46:40 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Core.Contract;
    using ReactiveUI;

    public class CardViewModel : ReactiveObject
    {
        // TODO: Need to make <CardDefinition> class as immutable as possible!

        private static readonly CardDefinition InvalidCardDefinition = new CardDefinition();

        private readonly IMagicRepository _repository;

        private readonly IMagicCardParser _cardParser;

        private ImageSource _originalImage;

        private CardDefinition _cardDefinition;

        private IEnumerable _combinedCardKinds;

        private IEnumerable<string> _parsingMessages;

        public CardViewModel(RawCard rawCard, IMagicRepository repository)
            : this(rawCard, repository, MagicCardParser.Instance)
        {
        }

        internal CardViewModel(RawCard rawCard, IMagicRepository repository, IMagicCardParser cardParser)
        {
            Guard
                .Require(rawCard, nameof(rawCard))
                .Is.Not.Null();

            Guard
                .Require(repository, nameof(repository))
                .Is.Not.Null();

            Guard
                .Require(cardParser, nameof(cardParser))
                .Is.Not.Null();

            this._repository = repository;
            this._cardParser = cardParser;

            this.RawCard = rawCard;

            this.PopulateDetailsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateDetailAsync());
            this.ParseCardCommand = ReactiveCommand.CreateFromTask(async () => await this.ParseCardAsync());
        }

        public RawCard RawCard { get; }

        public ImageSource OriginalImage
        {
            get => this._originalImage;
            private set => this.RaiseAndSetIfChanged(ref this._originalImage, value);
        }

        public CardDefinition CardDefinition
        {
            get => this._cardDefinition;
            private set => this.RaiseAndSetIfChanged(ref this._cardDefinition, value);
        }

        public IEnumerable CombinedCardKinds
        {
            get => this._combinedCardKinds;
            private set => this.RaiseAndSetIfChanged(ref this._combinedCardKinds, value);
        }

        public IEnumerable<string> ParsingMessages
        {
            get => this._parsingMessages;
            private set => this.RaiseAndSetIfChanged(ref this._parsingMessages, value);
        }

        public ICommand PopulateDetailsCommand { get; }

        public ICommand ParseCardCommand { get; }

        private async Task PopulateDetailAsync()
        {
            var cardImage = await this._repository.GetCardImageAsync(this.RawCard);

            // TODO: Need to handle larger image size, e.g. Planechase card!

            this.OriginalImage = cardImage.ToBitmapSource();
        }

        private async Task ParseCardAsync()
        {
            if (this.RawCard == null)
            {
                this.CardDefinition = null;
            }
            else
            {
                var parsingResult = await Task.Run(() => this._cardParser.Parse(this.RawCard));

                if (parsingResult.IsValid)
                {
                    this.CardDefinition = parsingResult.GetValue<CardDefinition>();

                    var combinedKinds = new List<object>();

                    if (this.CardDefinition.IsTribal)
                    {
                        combinedKinds.Add("Tribal");
                    }

                    if (this.CardDefinition.SuperKind != CardSuperKind.None)
                    {
                        combinedKinds.Add(this.CardDefinition.SuperKind);
                    }

                    combinedKinds.Add(this.CardDefinition.Kind);
                    combinedKinds.AddRange(this.CardDefinition.SubKinds.Cast<object>());

                    this.CombinedCardKinds = combinedKinds;
                    this.ParsingMessages = Enumerable.Empty<string>();
                }
                else
                {
                    this.CardDefinition = CardViewModel.InvalidCardDefinition;
                    this.CombinedCardKinds = Enumerable.Empty<object>();
                    this.ParsingMessages = parsingResult.Messages;
                }
            }
        }
    }
}