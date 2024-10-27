// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NoopObserver.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Friday, May 17, 2024 4:08:45 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Engine;

using nGratis.AI.Kvasir.Contract;

public class NoopObserver : IObserver
{
    private NoopObserver()
    {
    }

    public static IObserver Instance { get; } = new NoopObserver();

    public void ObserveStateChanged(ITabletop _)
    {
    }
}