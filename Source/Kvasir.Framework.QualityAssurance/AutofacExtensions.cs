// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutofacExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 27, 2023 6:56:23 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Autofac;

using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.Framework;

public static class AutofacExtensions
{
    public static ContainerBuilder RegisterTestingInfrastructure(this ContainerBuilder containerBuilder)
    {
        containerBuilder
            .Register(_ => VoidLogger.Instance)
            .InstancePerLifetimeScope()
            .As<ILogger>();

        return containerBuilder;
    }
}