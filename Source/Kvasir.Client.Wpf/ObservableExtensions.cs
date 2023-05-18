// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObservableExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, 8 January 2019 9:20:11 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace System.Reactive.Linq;

using System.ComponentModel;
using ReactiveUI;

internal static class ObservableExtensions
{
    public static IObservable<EventPattern<PropertyChangedEventArgs>> WhenPropertyChanged(
        this ReactiveObject reactiveObject)
    {
        // FIXME: Find another way to create observable of events because issue encountered after upgrading to
        // .NET 5 related to System.Runtime.InteropServices.WindowsRuntime binding failure!

        return Observable.Empty<EventPattern<PropertyChangedEventArgs>>();
    }
}