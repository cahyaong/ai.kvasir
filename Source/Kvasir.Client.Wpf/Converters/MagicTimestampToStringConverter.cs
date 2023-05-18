// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MagicTimestampToStringConverter.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, 9 November 2018 9:38:11 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using System;
using System.Globalization;
using System.Windows.Data;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;

[ValueConversion(typeof(DateTime), typeof(string))]
internal class MagicTimestampToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is DateTime timestamp)
        {
            return !timestamp.IsDated()
                ? "-"
                : timestamp
                    .ToString("yyyy-MMM-dd")
                    .ToUpperInvariant();
        }

        return DefinedText.Unknown;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}