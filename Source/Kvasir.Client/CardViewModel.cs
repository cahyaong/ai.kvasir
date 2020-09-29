// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardViewModel.cs" company="nGratis">
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
    using nGratis.AI.Kvasir.Core.Parser;
    using nGratis.Cop.Olympus.Contract;
    using ReactiveUI;

    public class CardViewModel : ReactiveObject
    {
        // TODO: Need to make <DefinedCard> class as immutable as possible!

        private readonly IMagicRepository _repository;

        private readonly IMagicCardProcessor _cardProcessor;

        private ImageSource _originalImage;

        private DefinedBlob.Card _definedCard;

        private IEnumerable _combinedCardKinds;

        private IEnumerable<string> _processingMessages;

        public CardViewModel(UnparsedBlob.Card unparsedCard, IMagicRepository repository)
            : this(unparsedCard, repository, MagicCardProcessor.Instance)
        {
        }

        internal CardViewModel(UnparsedBlob.Card unparsedCard, IMagicRepository repository, IMagicCardProcessor cardProcessor)
        {
            Guard
                .Require(unparsedCard, nameof(unparsedCard))
                .Is.Not.Null();

            Guard
                .Require(repository, nameof(repository))
                .Is.Not.Null();

            Guard
                .Require(cardProcessor, nameof(cardProcessor))
                .Is.Not.Null();

            this._repository = repository;
            this._cardProcessor = cardProcessor;

            this.UnparsedCard = unparsedCard;

            this.PopulateDetailsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateDetailAsync());
            this.ParseCardCommand = ReactiveCommand.CreateFromTask(async () => await this.ParseCardAsync());
        }

        public UnparsedBlob.Card UnparsedCard { get; }

        public ImageSource OriginalImage
        {
            get => this._originalImage;
            private set => this.RaiseAndSetIfChanged(ref this._originalImage, value);
        }

        public DefinedBlob.Card DefinedCard
        {
            get => this._definedCard;
            private set => this.RaiseAndSetIfChanged(ref this._definedCard, value);
        }

        public IEnumerable CombinedCardKinds
        {
            get => this._combinedCardKinds;
            private set => this.RaiseAndSetIfChanged(ref this._combinedCardKinds, value);
        }

        public IEnumerable<string> ProcessingMessages
        {
            get => this._processingMessages;
            private set => this.RaiseAndSetIfChanged(ref this._processingMessages, value);
        }

        public ICommand PopulateDetailsCommand { get; }

        public ICommand ParseCardCommand { get; }

        private async Task PopulateDetailAsync()
        {
            var cardImage = await this._repository.GetCardImageAsync(this.UnparsedCard);

            // TODO: Need to handle larger image size, e.g. Planechase card!

            this.OriginalImage = cardImage.ToImageSource();
        }

        private async Task ParseCardAsync()
        {
            this.DefinedCard = default;
            this.CombinedCardKinds = Enumerable.Empty<object>();
            this.ProcessingMessages = Enumerable.Empty<string>();

            if (this.UnparsedCard != null)
            {
                var processingResult = await Task.Run(() => this._cardProcessor.Process(this.UnparsedCard));

                if (processingResult.IsValid)
                {
                    this.DefinedCard = processingResult.GetValue<DefinedBlob.Card>();

                    var combinedKinds = new List<object>();

                    if (this.DefinedCard.IsTribal)
                    {
                        combinedKinds.Add("Tribal");
                    }

                    if (this.DefinedCard.SuperKind != CardSuperKind.None)
                    {
                        combinedKinds.Add(this.DefinedCard.SuperKind);
                    }

                    combinedKinds.Add(this.DefinedCard.Kind);
                    combinedKinds.AddRange(this.DefinedCard.SubKinds.Cast<object>());

                    this.CombinedCardKinds = combinedKinds;
                }
                else
                {
                    // FIXME: Need to distinguish between not-parsed and invalid cards!
                    this.ProcessingMessages = processingResult.Messages;
                }
            }
        }
    }
}