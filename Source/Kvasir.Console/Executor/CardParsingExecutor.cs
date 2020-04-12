// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CardParsingExecutor.cs" company="nGratis">
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
// <creation_timestamp>Thursday, April 2, 2020 5:49:47 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Console
{
    using System.Linq;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Core;
    using nGratis.AI.Kvasir.Core.Parser;
    using nGratis.Cop.Olympus.Contract;

    internal class CardParsingExecutor : IExecutor
    {
        private readonly IMagicRepository _repository;

        private readonly IMagicCardParser _cardParser;

        private readonly ILogger _logger;

        public CardParsingExecutor(IMagicRepository repository, IMagicCardParser cardParser, ILogger logger)
        {
            Guard
                .Require(repository, nameof(repository))
                .Is.Not.Null();

            Guard
                .Require(cardParser, nameof(cardParser))
                .Is.Not.Null();

            Guard
                .Require(logger, nameof(logger))
                .Is.Not.Null();

            this._repository = repository;
            this._cardParser = cardParser;
            this._logger = logger;
        }

        public async Task ExecuteAsync(ExecutionParameter parameter)
        {
            Guard
                .Require(parameter, nameof(parameter))
                .Is.Not.Null();

            var unparsedCardSet = await this._repository.GetCardSetAsync(parameter.GetValue("CardSet.Name"));
            var unparsedCards = await this._repository.GetCardsAsync(unparsedCardSet);

            var parsingResults = unparsedCards
                .Select(unparsedCard => this._cardParser.Parse(unparsedCard))
                .ToArray();

            this._logger.LogInfo($"Card set: [{unparsedCardSet.Name}]");
            this._logger.LogInfo($"Parsed cards: [{parsingResults.Length}]");
            this._logger.LogInfo($"Invalid cards: [{parsingResults.Count(result => !result.IsValid)}]");
        }
    }
}