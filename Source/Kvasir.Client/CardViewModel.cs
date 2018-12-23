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
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Contract.Magic;
    using nGratis.Cop.Core.Contract;
    using ReactiveUI;

    public class CardViewModel : ReactiveObject
    {
        private static readonly Int32Rect CroppingBound = new Int32Rect(3, 4, 217, 303);

        private readonly IMagicRepository _magicRepository;

        private ImageSource _originalImage;

        public CardViewModel(Card card, IMagicRepository magicRepository)
        {
            Guard
                .Require(card, nameof(card))
                .Is.Not.Null();

            Guard
                .Require(magicRepository, nameof(magicRepository))
                .Is.Not.Null();

            this._magicRepository = magicRepository;

            this.Card = card;

            this.PopulateDetailsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateDetailAsync());
        }

        public Card Card { get; }

        public ICommand PopulateDetailsCommand { get; }

        public ImageSource OriginalImage
        {
            get => this._originalImage;
            private set => this.RaiseAndSetIfChanged(ref this._originalImage, value);
        }

        private async Task PopulateDetailAsync()
        {
            var cardImage = await this._magicRepository.GetCardImageAsync(this.Card);

            this.OriginalImage = new CroppedBitmap(cardImage.ToBitmapSource(), CardViewModel.CroppingBound);
        }
    }
}