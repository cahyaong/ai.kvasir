// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IZone.cs" company="nGratis">
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
// <creation_timestamp>Friday, April 15, 2022 2:24:33 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using System;
using System.Collections.Generic;
using nGratis.AI.Kvasir.Contract;

public interface IZone<TEntity>
{
    ZoneKind Kind { get; }

    Visibility Visibility { get; }

    int Quantity { get; }

    bool IsEmpty => this.Quantity <= 0;

    void AddToTop(TEntity entity);

    TEntity FindFromTop();

    void RemoveFromTop();

    IEnumerable<TEntity> FindAll();

    void RemoveAll();

    void MoveToZone(TEntity entity, IZone<TEntity> zone);
}

internal class UnknownZone<TEntity> : IZone<TEntity>
{
    private UnknownZone()
    {
    }

    internal static IZone<TEntity> Instance { get; } = new UnknownZone<TEntity>();

    public ZoneKind Kind => ZoneKind.Unknown;

    public Visibility Visibility => Visibility.Unknown;

    public int Quantity => -42;

    public void AddToTop(TEntity _)
    {
        throw new NotSupportedException("Adding entity to top is not allowed!");
    }

    public TEntity FindFromTop()
    {
        throw new NotSupportedException("Finding entity from top is not allowed!");
    }

    public void RemoveFromTop()
    {
        throw new NotSupportedException("Removing entity from top is not allowed!");
    }

    public IEnumerable<TEntity> FindAll()
    {
        throw new NotSupportedException("Finding all entities is not allowed!");
    }

    public void RemoveAll()
    {
        throw new NotImplementedException("Removing all entities is not allowed!");
    }

    public void MoveToZone(TEntity _, IZone<TEntity> __)
    {
        throw new NotSupportedException("Moving entity to zone is not allowed!");
    }
}