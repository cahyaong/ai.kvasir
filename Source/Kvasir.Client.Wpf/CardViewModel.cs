// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardViewModel.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 21 December 2018 11:46:40 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.AI.Kvasir.Core.Parser;
using ReactiveUI;

public class CardViewModel : ReactiveObject
{
    private static readonly IMagicCardProcessor CardProcessor = new MagicCardProcessor();

    private readonly IUnprocessedMagicRepository _unprocessedRepository;

    private readonly IMagicCardProcessor _cardProcessor;

    private ImageSource? _originalImage;

    private DefinedBlob.Card? _definedCard;

    private IEnumerable _combinedCardKinds;

    private IEnumerable<string> _processingMessages;

    public CardViewModel(UnparsedBlob.Card unparsedCard, IUnprocessedMagicRepository unprocessedRepository)
        : this(unparsedCard, unprocessedRepository, CardViewModel.CardProcessor)
    {
    }

    private CardViewModel(
        UnparsedBlob.Card unparsedCard,
        IUnprocessedMagicRepository unprocessedRepository,
        IMagicCardProcessor cardProcessor)
    {
        this._unprocessedRepository = unprocessedRepository;
        this._cardProcessor = cardProcessor;
        this._combinedCardKinds = Array.Empty<object>();
        this._processingMessages = Array.Empty<string>();

        this.UnparsedCard = unparsedCard;

        this.PopulateDetailsCommand = ReactiveCommand.CreateFromTask(async () => await this.PopulateDetailAsync());
        this.ParseCardCommand = ReactiveCommand.CreateFromTask(async () => await this.ParseCardAsync());
    }

    public UnparsedBlob.Card UnparsedCard { get; }

    public ImageSource? OriginalImage
    {
        get => this._originalImage;
        private set => this.RaiseAndSetIfChanged(ref this._originalImage, value);
    }

    public DefinedBlob.Card? DefinedCard
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
        var cardImage = await this._unprocessedRepository.GetCardImageAsync(this.UnparsedCard);

        // TODO: Need to handle larger image size, e.g. Planechase card!

        this.OriginalImage = cardImage.ToImageSource();
    }

    private async Task ParseCardAsync()
    {
        this.DefinedCard = default;
        this.CombinedCardKinds = Enumerable.Empty<object>();
        this.ProcessingMessages = Enumerable.Empty<string>();

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