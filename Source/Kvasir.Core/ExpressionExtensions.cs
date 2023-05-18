// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExpressionExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 28 December 2018 1:22:40 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace System.Linq.Expressions;

using System;
using System.Reflection;
using nGratis.AI.Kvasir.Contract;

internal static class ExpressionExtensions
{
    public static PropertyInfo GetPropertyInfo<T>(this Expression<Func<T, object>> expression)
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