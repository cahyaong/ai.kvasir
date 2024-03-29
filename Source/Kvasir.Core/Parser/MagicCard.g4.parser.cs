﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicCardAbility.g4.parser.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, 12 January 2019 10:06:41 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Parser;

using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;
using nGratis.AI.Kvasir.Contract;
using nGratis.Cop.Olympus.Contract;
using AbilityParsingResult = ParsingResult<Contract.DefinedBlob.Ability>;
using CostParsingResult = ParsingResult<Contract.DefinedBlob.Cost>;
using EffectParsingResult = ParsingResult<Contract.DefinedBlob.Effect>;

public partial class MagicCardParser
{
    private static readonly IReadOnlyDictionary<string, Mana> ManaBySymbolLookup = new Dictionary<string, Mana>
    {
        ["{W}"] = Mana.White,
        ["{U}"] = Mana.Blue,
        ["{B}"] = Mana.Black,
        ["{R}"] = Mana.Red,
        ["{G}"] = Mana.Green
    };

    public static AbilityParsingResult ParseAbility(string unparsedAbility)
    {
        Guard
            .Require(unparsedAbility, nameof(unparsedAbility))
            .Is.Not.Empty();

        try
        {
            var parser = MagicCardParser.Create(unparsedAbility);

            return AbilityVisitor.Instance.VisitAbility_Bootstrapper(parser.ability_Bootstrapper());
        }
        catch (KvasirGrammarException exception)
        {
            return AbilityParsingResult.CreateFailure(
                $"Ability [{unparsedAbility}] parsing could not continue " +
                $"after processing '{unparsedAbility[exception.LetterIndex]}' " +
                $"at [{exception.LineIndex}:{exception.LetterIndex}]!");
        }
    }

    public static CostParsingResult ParseCost(string unparsedCost)
    {
        Guard
            .Require(unparsedCost, nameof(unparsedCost))
            .Is.Not.Empty();

        try
        {
            var parser = MagicCardParser.Create(unparsedCost);

            return CostVisitor.Instance.VisitCost_Bootstrapper(parser.cost_Bootstrapper());
        }
        catch (KvasirGrammarException exception)
        {
            return CostParsingResult.CreateFailure(
                $"Cost [{unparsedCost}] parsing could not continue " +
                $"after processing '{unparsedCost[exception.LetterIndex]}' " +
                $"at [{exception.LineIndex}:{exception.LetterIndex}]!");
        }
    }

    private static MagicCardParser Create(string unparsedValue)
    {
        Guard
            .Require(unparsedValue, nameof(unparsedValue))
            .Is.Not.Empty();

        var stream = new AntlrInputStream(unparsedValue);

        var lexer = new MagicCardLexer(stream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(ThrowingExceptionListener.Instance);

        var tokens = new CommonTokenStream(lexer);

        var parser = new MagicCardParser(tokens);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(ThrowingExceptionListener.Instance);

        return parser;
    }

    private sealed class AbilityVisitor : MagicCardBaseVisitor<AbilityParsingResult>
    {
        private AbilityVisitor()
        {
        }

        public static AbilityVisitor Instance { get; } = new();

        public override AbilityParsingResult VisitAbility_Bootstrapper(Ability_BootstrapperContext context)
        {
            // TODO: Add support for multiple abilities!

            return this.Visit(context.GetChild(0));
        }

        public override AbilityParsingResult VisitAbility_Activated(Ability_ActivatedContext context)
        {
            var costParsingResult = CostVisitor.Instance.VisitCost(context.cost());
            var effectParsingResult = EffectVisitor.Instance.VisitEffect(context.effect());

            var hasInvalidResult =
                costParsingResult.HasError ||
                effectParsingResult.HasError;

            if (hasInvalidResult)
            {
                return AbilityParsingResult.CreateFailure(costParsingResult, effectParsingResult);
            }

            var hasUnexpectedValue =
                costParsingResult.Value == null ||
                effectParsingResult.Value == null;

            if (hasUnexpectedValue)
            {
                throw new KvasirException("Found no error during parsing, but parsed value is not valid?");
            }

            var ability = new DefinedBlob.Ability
            {
                Kind = AbilityKind.Activated,
                Costs = new[]
                {
                    costParsingResult.Value ?? DefinedBlob.Cost.Unknown
                },
                Effects = new[]
                {
                    effectParsingResult.Value ?? DefinedBlob.Effect.Unknown
                }
            };

            return AbilityParsingResult.CreateSuccessful(ability);
        }
    }

    private sealed class CostVisitor : MagicCardBaseVisitor<CostParsingResult>
    {
        private CostVisitor()
        {
        }

        public static CostVisitor Instance { get; } = new();

        public override CostParsingResult VisitCost_Bootstrapper(Cost_BootstrapperContext context)
        {
            return this.Visit(context.GetChild(0));
        }

        public override CostParsingResult VisitCost_Tapping(Cost_TappingContext context)
        {
            return CostParsingResult.CreateSuccessful(DefinedBlob.TappingCost.Instance);
        }

        public override CostParsingResult VisitCost_PayingMana(Cost_PayingManaContext context)
        {
            var costBuilder = DefinedBlob.PayingManaCost.Builder.Create();

            if (context.SYMBOL_MANA_COLORLESS() != null)
            {
                var colorlessAmount = ushort.Parse(context
                    .SYMBOL_MANA_COLORLESS()
                    .GetText()
                    .Replace("{", string.Empty)
                    .Replace("}", string.Empty));

                costBuilder.WithAmount(Mana.Colorless, colorlessAmount);
            }

            if (context.SYMBOL_MANA_COLOR()?.Any() == true)
            {
                var invalidSymbols = context
                    .SYMBOL_MANA_COLOR()
                    .Select(node => node.GetText())
                    .Distinct()
                    .Where(symbol => !MagicCardParser.ManaBySymbolLookup.ContainsKey(symbol))
                    .ToArray();

                if (invalidSymbols.Any())
                {
                    return CostParsingResult.CreateFailure<Cost_PayingManaContext>(
                        $"No mapping for value ({invalidSymbols.ToPrettifiedText()}).");
                }

                context
                    .SYMBOL_MANA_COLOR()
                    .Select(node => MagicCardParser.ManaBySymbolLookup[node.GetText()])
                    .GroupBy(mana => mana)
                    .ForEach(grouping => costBuilder.WithAmount(grouping.Key, (ushort)grouping.Count()));
            }

            return CostParsingResult.CreateSuccessful(costBuilder.Build());
        }
    }

    private sealed class EffectVisitor : MagicCardBaseVisitor<EffectParsingResult>
    {
        private EffectVisitor()
        {
        }

        public static EffectVisitor Instance { get; } = new();

        public override EffectParsingResult VisitEffect_ProducingMana(Effect_ProducingManaContext context)
        {
            if (!MagicCardParser.ManaBySymbolLookup.TryGetValue(context.SYMBOL_MANA_COLOR().GetText(), out var mana))
            {
                return EffectParsingResult.CreateFailure<Effect_ProducingManaContext>(
                    $"No mapping for value [{context.SYMBOL_MANA_COLOR().GetText()}].");
            }

            var effect = DefinedBlob.ProducingManaEffect.Builder
                .Create()
                .WithAmount(mana, 1)
                .Build();

            return EffectParsingResult.CreateSuccessful(effect);
        }
    }

    private sealed class ThrowingExceptionListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
    {
        private ThrowingExceptionListener()
        {
        }

        public static ThrowingExceptionListener Instance { get; } = new();

        public void SyntaxError(
            IRecognizer recognizer,
            int token,
            int lineNumber,
            int letterIndex,
            string message,
            RecognitionException exception)
        {
            throw new KvasirGrammarException(lineNumber - 1, letterIndex);
        }

        public void SyntaxError(
            IRecognizer recognizer,
            IToken token,
            int lineNumber,
            int letterIndex,
            string message,
            RecognitionException exception)
        {
            throw new KvasirGrammarException(lineNumber - 1, letterIndex);
        }
    }

    private sealed class KvasirGrammarException : Exception
    {
        public KvasirGrammarException(int lineIndex, int letterIndex)
        {
            this.LineIndex = lineIndex;
            this.LetterIndex = letterIndex;
        }

        public int LineIndex { get; }

        public int LetterIndex { get; }
    }
}