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
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Core.Contract;
    using ReactiveUI;

    public class CardViewModel : ReactiveObject
    {
        private static readonly Int32Rect CroppingBound = new Int32Rect(3, 4, 217, 303);

        // TODO: Need to make <CardInfo> class as immutable as possible!

        private static readonly CardInfo InvalidCardInfo = new CardInfo();

        private readonly IMagicRepository _magicRepository;

        private readonly IMagicParser _magicParser;

        private ImageSource _originalImage;

        private CardInfo _cardInfo;

        private IEnumerable _combinedKinds;

        private IEnumerable<string> _parsingMessages;

        public CardViewModel(RawCard card, IMagicRepository magicRepository)
            : this(card, magicRepository, MagicParser.Instance)
        {
        }

        internal CardViewModel(RawCard card, IMagicRepository magicRepository, IMagicParser magicParser)
        {
            Guard
                .Require(card, nameof(card))
                .Is.Not.Null();

            Guard
                .Require(magicRepository, nameof(magicRepository))
                .Is.Not.Null();

            Guard
                .Require(magicParser, nameof(magicParser))
                .Is.Not.Null();

            this._magicRepository = magicRepository;
            this._magicParser = magicParser;

            this.Card = card;

            this.PopulateDetailsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateDetailAsync());
            this.ParseCardCommand = ReactiveCommand.CreateFromTask(async () => await this.ParseCardAsync());
        }

        public RawCard Card { get; }

        public ImageSource OriginalImage
        {
            get => this._originalImage;
            private set => this.RaiseAndSetIfChanged(ref this._originalImage, value);
        }

        public CardInfo CardInfo
        {
            get => this._cardInfo;
            private set => this.RaiseAndSetIfChanged(ref this._cardInfo, value);
        }

        public IEnumerable CombinedKinds
        {
            get => this._combinedKinds;
            private set => this.RaiseAndSetIfChanged(ref this._combinedKinds, value);
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
            var cardImage = await this._magicRepository.GetCardImageAsync(this.Card);

            // TODO: Need to handle larger image size, e.g. Planechase card!

            this.OriginalImage = new CroppedBitmap(cardImage.ToBitmapSource(), CardViewModel.CroppingBound);
        }

        private async Task ParseCardAsync()
        {
            if (this.Card == null)
            {
                this.CardInfo = null;
            }
            else
            {
                var parsingResult = await Task.Run(() => this._magicParser.ParseRawCard(this.Card));

                if (parsingResult.IsValid)
                {
                    this.CardInfo = parsingResult.GetValue<CardInfo>();

                    var combinedKinds = new List<object>();

                    if (this.CardInfo.IsTribal)
                    {
                        combinedKinds.Add("Tribal");
                    }

                    if (this.CardInfo.SuperKind != CardSuperKind.None)
                    {
                        combinedKinds.Add(this.CardInfo.SuperKind);
                    }

                    combinedKinds.Add(this.CardInfo.Kind);
                    combinedKinds.AddRange(this.CardInfo.SubKinds.Cast<object>());

                    this.CombinedKinds = combinedKinds;
                    this.ParsingMessages = Enumerable.Empty<string>();
                }
                else
                {
                    this.CardInfo = CardViewModel.InvalidCardInfo;
                    this.CombinedKinds = Enumerable.Empty<object>();
                    this.ParsingMessages = parsingResult.Messages;
                }
            }
        }
    }
}