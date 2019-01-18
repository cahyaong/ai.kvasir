// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicCardAbility.g4.parser.cs" company="nGratis">
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
// <creation_timestamp>Saturday, 12 January 2019 10:06:41 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System.IO;
    using Antlr4.Runtime;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    public partial class MagicCardAbilityParser
    {
        public static ParsingResult Parse(string rawAbility)
        {
            Guard
                .Require(rawAbility, nameof(rawAbility))
                .Is.Not.Empty();

            using (var reader = new StringReader(rawAbility))
            {
                var stream = new AntlrInputStream(reader);
                var lexer = new MagicCardAbilityLexer(stream);
                var tokens = new CommonTokenStream(lexer);
                var parser = new MagicCardAbilityParser(tokens);

                var ability = Visitor.Instance.VisitAbility(parser.ability());

                return ValidParsingResult.Create(ability);
            }
        }

        private sealed class Visitor : MagicCardAbilityBaseVisitor<AbilityDefinition>
        {
            private Visitor()
            {
            }

            public static Visitor Instance { get; } = new Visitor();

            public override AbilityDefinition VisitAbility(AbilityContext context)
            {
                Guard
                    .Require(context, nameof(context))
                    .Is.Not.Null();

                return this.Visit(context.GetChild(0));
            }

            public override AbilityDefinition VisitProducingManaAbility(ProducingManaAbilityContext context)
            {
                Guard
                    .Require(context, nameof(context))
                    .Is.Not.Null();

                return new AbilityDefinition
                {
                    Kind = AbilityKind.Activated,
                    CostDefinitions = new[]
                    {
                        new CostDefinition
                        {
                            Kind = CostKind.Tapping,
                            Amount = string.Empty
                        }
                    },
                    EffectDefinitions = new[]
                    {
                        new EffectDefinition
                        {
                            Kind = EffectKind.ProducingMana,
                            Amount = context.MANA_SYMBOL().GetText()
                        }
                    }
                };
            }
        }
    }
}