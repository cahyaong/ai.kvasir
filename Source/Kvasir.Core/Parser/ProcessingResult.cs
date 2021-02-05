// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessingResult.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2021 Cahya Ong
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
// <creation_timestamp>Sunday, December 8, 2019 6:22:17 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Core.Parser
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Olympus.Contract;

    public sealed class ValidProcessingResult : ProcessingResult
    {
        private readonly object _value;

        private object _childValue;

        private ValidProcessingResult(object value)
            : base(Enumerable.Empty<string>())
        {
            Guard
                .Require(value, nameof(value))
                .Is.Not.Default();

            this._value = value;
        }

        public static ProcessingResult Create(object value)
        {
            return new ValidProcessingResult(value);
        }

        public override TValue GetValue<TValue>()
        {
            return (TValue)this._value;
        }

        protected override ProcessingResult WithChildResultCore(ProcessingResult childResult)
        {
            if (childResult.IsValid && childResult is ValidProcessingResult validChildResult)
            {
                this._childValue = validChildResult._value;
            }
            else
            {
                this._childValue = default;
            }

            return this;
        }

        protected override ProcessingResult BindToCore(PropertyInfo propertyInfo)
        {
            propertyInfo.SetValue(this._value, this._childValue);
            this._childValue = default;

            return this;
        }
    }

    public sealed class InvalidProcessingResult : ProcessingResult
    {
        private InvalidProcessingResult(IEnumerable<string> messages)
            : base(messages)
        {
        }

        public static ProcessingResult Create(params string[] messages)
        {
            return new InvalidProcessingResult(messages);
        }

        public override TValue GetValue<TValue>()
        {
            throw new KvasirException("No valid value for this parsing result!");
        }
    }

    // TODO: Instead of valid and invalid parsing results, we need to support card and field parsing results!

    public abstract class ProcessingResult
    {
        // TODO: Add category to messages, e.g. ability, kind, etc.!
        // TODO: Refactor this class to extend from <ExecutionResult>!

        private readonly List<string> _messages;

        protected ProcessingResult(IEnumerable<string> messages)
        {
            Guard
                .Require(messages, nameof(messages))
                .Is.Not.Null();

            this._messages = messages
                .Where(message => !string.IsNullOrEmpty(message))
                .ToList();
        }

        public bool IsValid =>
            this.IsValidCore &&
            !this.Messages.Any();

        public IEnumerable<string> Messages => this._messages;

        protected virtual bool IsValidCore => true;

        internal ProcessingResult WithMessage(string message)
        {
            Guard
                .Require(message, nameof(message))
                .Is.Not.Empty();

            this._messages.Add(message);

            return this;
        }

        internal ProcessingResult WithChildResult(ProcessingResult childResult)
        {
            Guard
                .Require(childResult, nameof(childResult))
                .Is.Not.Null();

            if (!childResult.IsValid)
            {
                var filteredMessages = childResult
                    .Messages
                    .Where(message => !string.IsNullOrEmpty(message))
                    .ToArray();

                this._messages.AddRange(filteredMessages);
            }

            return this.WithChildResultCore(childResult);
        }

        internal ProcessingResult BindTo(PropertyInfo propertyInfo)
        {
            Guard
                .Require(propertyInfo, nameof(propertyInfo))
                .Is.Not.Null();

            return this.BindToCore(propertyInfo);
        }

        public abstract TValue GetValue<TValue>();

        protected virtual ProcessingResult WithChildResultCore(ProcessingResult childResult)
        {
            return this;
        }

        protected virtual ProcessingResult BindToCore(PropertyInfo propertyInfo)
        {
            return this;
        }
    }
}