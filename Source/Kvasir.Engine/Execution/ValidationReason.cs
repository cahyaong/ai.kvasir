// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationReason.cs" company="nGratis">
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
// <creation_timestamp>Friday, November 12, 2021 12:09:03 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.Cop.Olympus.Contract;

public class ValidationReason
{
    private ValidationReason()
    {
        this.Id = -42;
        this.Cause = DefinedText.Unknown;
        this.Reference = DefinedText.Unknown;
        this.Message = DefinedText.Unknown;
    }

    public int Id { get; init; }

    public string Cause { get; init; }

    public string Reference { get; init; }

    public string Message { get; init; }

    public static ValidationReason Create(string message, Creature creature)
    {
        return ValidationReason.Create(message, creature.Permanent);
    }

    public static ValidationReason Create(string message, IPermanent permanent)
    {
        Guard
            .Require(message, nameof(message))
            .Is.Not.Empty();

        return new ValidationReason
        {
            Id = permanent.Id,
            Cause = $"Permanent_{permanent.Card.Kind}",
            Reference = permanent.Name,
            Message = message
        };
    }
}