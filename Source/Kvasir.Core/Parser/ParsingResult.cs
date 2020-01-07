namespace nGratis.AI.Kvasir.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.Cop.Core.Contract;

    public sealed class ValidParsingResult : ParsingResult
    {
        private readonly object _value;

        private object _childValue;

        private ValidParsingResult(object value)
            : base(Enumerable.Empty<string>())
        {
            Guard
                .Require(value, nameof(value))
                .Is.Not.Default();

            this._value = value;
        }

        public static ParsingResult Create(object value)
        {
            return new ValidParsingResult(value);
        }

        public override TValue GetValue<TValue>()
        {
            return (TValue)this._value;
        }

        protected override ParsingResult WithChildResultCore(ParsingResult childResult)
        {
            if (childResult.IsValid && childResult is ValidParsingResult validChildResult)
            {
                this._childValue = validChildResult._value;
            }
            else
            {
                this._childValue = default;
            }

            return this;
        }

        protected override ParsingResult BindToCore(PropertyInfo propertyInfo)
        {
            propertyInfo.SetValue(this._value, this._childValue);
            this._childValue = default;

            return this;
        }
    }

    public sealed class InvalidParsingResult : ParsingResult
    {
        private InvalidParsingResult(IEnumerable<string> messages)
            : base(messages)
        {
        }

        public static ParsingResult Create(params string[] messages)
        {
            return new InvalidParsingResult(messages);
        }

        public override TValue GetValue<TValue>()
        {
            throw new KvasirException("No valid value for this parsing result!");
        }
    }

    public abstract class ParsingResult
    {
        private readonly List<string> _messages;

        protected ParsingResult(IEnumerable<string> messages)
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

        public ParsingResult WithMessage(string message)
        {
            Guard
                .Require(message, nameof(message))
                .Is.Not.Empty();

            this._messages.Add(message);

            return this;
        }

        public ParsingResult WithChildResult(ParsingResult childResult)
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

        public ParsingResult BindTo(PropertyInfo propertyInfo)
        {
            Guard
                .Require(propertyInfo, nameof(propertyInfo))
                .Is.Not.Null();

            return this.BindToCore(propertyInfo);
        }

        public abstract TValue GetValue<TValue>();

        protected virtual ParsingResult WithChildResultCore(ParsingResult childResult)
        {
            return this;
        }

        protected virtual ParsingResult BindToCore(PropertyInfo propertyInfo)
        {
            return this;
        }
    }
}