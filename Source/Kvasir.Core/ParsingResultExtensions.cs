// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParsingResultExtensions.cs" company="nGratis">
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
// <creation_timestamp>Friday, 28 December 2018 1:22:40 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    internal static class ParsingResultExtensions
    {
        public static ParsingResult BindToCardInfo(
            this ParsingResult parsingResult,
            Expression<Func<CardInfo, object>> bindingExpression)
        {
            Guard
                .Require(parsingResult, nameof(parsingResult))
                .Is.Not.Null();

            Guard
                .Require(bindingExpression, nameof(bindingExpression))
                .Is.Not.Null();

            return parsingResult.BindTo(bindingExpression.GetPropertyInfo());
        }

        private static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> expression)
        {
            var unaryExpression = expression.Body as UnaryExpression;

            var memberExpression =
                unaryExpression?.Operand as MemberExpression ??
                expression.Body as MemberExpression;

            var propertyInfo = memberExpression?.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                throw new KvasirException("Binding expression must return <Property> value!");
            }

            return propertyInfo;
        }
    }
}