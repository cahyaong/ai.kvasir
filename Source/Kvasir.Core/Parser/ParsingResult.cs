namespace nGratis.AI.Kvasir.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using nGratis.Cop.Core.Contract;

    public class ParsingResult<TValue>
    {
        private readonly List<string> _messages;

        private ParsingResult(TValue value, IEnumerable<string> messages)
        {
            this._messages = new List<string>(messages);
            this.Value = value;
        }

        public bool IsValid =>
            !object.Equals(this.Value, default(TValue)) &&
            !this.Messages.Any();

        public TValue Value { get; }

        public IEnumerable<string> Messages => this._messages;

        public static ParsingResult<TValue> CreateValid(TValue value)
        {
            Guard
                .Require(value, nameof(value))
                .Is.Not.Default();

            return new ParsingResult<TValue>(value, Enumerable.Empty<string>());
        }

        public static ParsingResult<TValue> CreateInvalid(params string[] messages)
        {
            return new ParsingResult<TValue>(
                default(TValue),
                messages.Where(message => !string.IsNullOrEmpty(message)));
        }

        public ParsingResult<TValue> WithMessage(string message)
        {
            Guard
                .Require(message, nameof(message))
                .Is.Not.Empty();

            this._messages.Add(message);

            return this;
        }

        public ParsingResult<TValue> WithChildResult<TChildValue>(
            ParsingResult<TChildValue> childResult,
            Action<TValue, TChildValue> merge)
        {
            Guard
                .Require(childResult, nameof(childResult))
                .Is.Not.Null();

            var shouldMerge =
                merge != null &&
                childResult.IsValid;

            if (shouldMerge)
            {
                merge(this.Value, childResult.Value);
            }

            if (!childResult.IsValid)
            {
                var filteredMessages = childResult
                    .Messages
                    .Where(message => !string.IsNullOrEmpty(message))
                    .ToArray();

                this._messages.AddRange(filteredMessages);
            }

            return this;
        }
    }
}