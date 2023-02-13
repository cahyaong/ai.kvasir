// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ValidationResult.cs" company="nGratis">
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
// <creation_timestamp>Thursday, November 11, 2021 11:51:24 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System.Collections.Generic;
using System.Linq;

public class ValidationResult
{
    private ValidationResult()
    {
        this.Reasons = Enumerable.Empty<ValidationReason>();
    }

    public static ValidationResult Successful { get; } = new();

    public bool HasError => this.Reasons.Any();

    public IEnumerable<ValidationReason> Reasons { get; private init; }

    public IEnumerable<string> Messages => this
        .Reasons
        .Select(reason => reason.CreateDetailedMessage());

    public static ValidationResult Create(IReadOnlyCollection<ValidationReason> reasons)
    {
        return reasons.Any()
            ? new ValidationResult { Reasons = reasons }
            : ValidationResult.Successful;
    }
}