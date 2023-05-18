// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessingCardExecution.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Thursday, April 2, 2020 5:49:47 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.AI.Kvasir.Core.Parser;
using nGratis.Cop.Olympus.Contract;

internal class ProcessingCardExecution : IExecution
{
    private readonly IUnprocessedMagicRepository _unprocessedRepository;

    private readonly IProcessedMagicRepository _processedRepository;

    private readonly IMagicCardProcessor _cardProcessor;

    private readonly ILogger _logger;

    public ProcessingCardExecution(
        IUnprocessedMagicRepository unprocessedRepository,
        IProcessedMagicRepository processedRepository,
        IMagicCardProcessor cardProcessor,
        ILogger logger)
    {
        this._unprocessedRepository = unprocessedRepository;
        this._processedRepository = processedRepository;
        this._cardProcessor = cardProcessor;
        this._logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(ExecutionParameter parameter)
    {
        var unparsedCardSet = await this._unprocessedRepository.GetCardSetAsync(parameter.GetValue("CardSet.Name"));
        var unparsedCards = await this._unprocessedRepository.GetCardsAsync(unparsedCardSet);

        var processingResults = unparsedCards
            .Select(unparsedCard => this._cardProcessor.Process(unparsedCard))
            .ToArray();

        processingResults
            .Where(result => result.IsValid)
            .Select(result => result.GetValue<DefinedBlob.Card>())
            .ForEachAsync(card => this._processedRepository.SaveCardAsync(card));

        this._logger.LogInfo("Saved valid cards...");

        using var summaryPrinter = SummaryPrinter.Create(2);

        summaryPrinter
            .Indent()
            .WithCardSet(
                unparsedCardSet.Name,
                ("Parsed Cards", processingResults.Length),
                ("Invalid Cards", processingResults.Count(result => !result.IsValid)));

        processingResults
            .Where(result => !result.IsValid)
            .OrderBy(result => result.GetValue<DefinedBlob.Card>().Number)
            .ForEach(result => summaryPrinter.WithInvalidCard(
                result.GetValue<DefinedBlob.Card>(),
                result.Messages.ToArray()));

        var summaryContent = summaryPrinter
            .Dedent()
            .Print();

        this._logger.LogWarning($"Found invalid cards!{Environment.NewLine}{summaryContent}");

        return ExecutionResult.Successful;
    }

    internal sealed class SummaryPrinter : IDisposable
    {
        private readonly StringWriter _rawWriter;

        private readonly IndentedTextWriter _contentWriter;

        private bool _isDisposed;

        private SummaryPrinter(int indentSize)
        {
            Guard
                .Require(indentSize, nameof(indentSize))
                .Is.GreaterThanOrEqualTo(0);

            this._rawWriter = new StringWriter();
            this._contentWriter = new IndentedTextWriter(this._rawWriter, new string(' ', indentSize));
        }

        ~SummaryPrinter()
        {
            this.Dispose(false);
        }

        public static SummaryPrinter Create(int indentSize)
        {
            return new SummaryPrinter(indentSize);
        }

        public SummaryPrinter Indent()
        {
            this._contentWriter.Indent++;

            return this;
        }

        public SummaryPrinter Dedent()
        {
            this._contentWriter.Indent = Math.Max(0, this._contentWriter.Indent - 1);

            return this;
        }

        public SummaryPrinter WithCardSet<TValue>(string name, params (string Key, TValue Value)[] statistics)
        {
            Guard
                .Require(name, nameof(name))
                .Is.Not.Empty();

            this._contentWriter.WriteLine();
            this._contentWriter.WriteLine($"Card Set: [{name}]");

            foreach (var (key, value) in statistics)
            {
                this._contentWriter.Write(!string.IsNullOrEmpty(key) ? key : DefinedText.Empty);
                this._contentWriter.WriteLine($": {value}");
            }

            this._contentWriter.WriteLine();

            return this;
        }

        public SummaryPrinter WithInvalidCard(DefinedBlob.Card card, params string[] parsingMessages)
        {
            this._contentWriter.WriteLine($"#{card.Number}");
            this._contentWriter.WriteLine($"Card: [{card.Name}]");

            if (parsingMessages.Any())
            {
                this._contentWriter.WriteLine(@"Messages:");
                this.Indent();

                parsingMessages
                    .Where(message => !string.IsNullOrEmpty(message))
                    .ForEach(message => this._contentWriter.WriteLine($"* {message}"));

                this.Dedent();
            }
            else
            {
                this._contentWriter.WriteLine($"Messages: {DefinedText.Empty}");
            }

            this._contentWriter.WriteLine();

            return this;
        }

        public string Print()
        {
            this._contentWriter.Flush();

            return this
                ._rawWriter
                .GetStringBuilder()
                .ToString()
                .TrimEnd();
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool isDisposing)
        {
            if (this._isDisposed)
            {
                return;
            }

            if (isDisposing)
            {
                this._contentWriter.Dispose();
                this._rawWriter.Dispose();
            }

            this._isDisposed = true;
        }
    }
}