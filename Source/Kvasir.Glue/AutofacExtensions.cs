// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutofacExtensions.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, May 27, 2023 4:53:03 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace

namespace Autofac;

using System.Reflection;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Engine;

public static class AutofacExtensions
{
    public static ContainerBuilder RegisterJudge(this ContainerBuilder containerBuilder)
    {
        var engineAssembly = Assembly.Load("nGratis.AI.Kvasir.Engine");

        containerBuilder
            .RegisterAssemblyTypes(engineAssembly)
            .PublicOnly()
            .Where(type => type is { IsInterface: false, IsAbstract: false })
            .Where(type => type.IsAssignableTo<IActionHandler>())
            .SingleInstance()
            .As<IActionHandler>();

        containerBuilder
            .RegisterAssemblyTypes(engineAssembly)
            .PublicOnly()
            .Where(type => type is { IsInterface: false, IsAbstract: false })
            .Where(type => type.IsAssignableTo<ICostHandler>())
            .SingleInstance()
            .As<ICostHandler>();

        containerBuilder
            .RegisterType<ExecutionManager>()
            .SingleInstance()
            .As<IExecutionManager>()
            .OnActivated(activatedEvent =>
            {
                activatedEvent.Instance.RegisterCostHandler(activatedEvent.Context.Resolve<ICostHandler[]>());
                activatedEvent.Instance.RegisterActionHandler(activatedEvent.Context.Resolve<IActionHandler[]>());
            });

        containerBuilder
            .RegisterType<JudicialAssistant>()
            .SingleInstance()
            .As<IJudicialAssistant>();

        containerBuilder
            .RegisterType<ActionJudge>()
            .SingleInstance()
            .As<IActionJudge>();

        containerBuilder
            .RegisterType<RoundJudge>()
            .SingleInstance()
            .As<IRoundJudge>();

        return containerBuilder;
    }
}