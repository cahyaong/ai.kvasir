﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppBootstrapper.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, February 20, 2021 7:58:00 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Cmd;

using System;
using System.Reflection;
using Autofac;
using Autofac.Core;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.AI.Kvasir.Core.Parser;
using nGratis.AI.Kvasir.Engine;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.Framework;

internal class AppBootstrapper : IInfrastructureFactory, IDisposable
{
    private readonly IContainer _container;

    private bool _isDisposed;

    public AppBootstrapper()
    {
        this._container = new ContainerBuilder()
            .RegisterInfrastructure(this)
            .RegisterStorageManager()
            .RegisterRepository()
            .RegisterJudge()
            .RegisterExecution()
            .RegisterSimulator()
            .Build();
    }

    ~AppBootstrapper()
    {
        this.Dispose(false);
    }

    public IJob CreateJob<T>()
        where T : class, IJob
    {
        return this._container.Resolve<T>();
    }

    public IMagicLogger CreateMagicLogger()
    {
        return this._container.Resolve<IMagicLogger>();
    }

    public ISimulator<GameConfig, GameResult> CreateGameSimulator(string id, int seed)
    {
        return this._container.Resolve<ISimulator<GameConfig, GameResult>>(new ResolvedParameter(
            (parameterInfo, _) => parameterInfo.ParameterType == typeof(IRandomGenerator),
            (_, _) => new RandomGenerator(id, seed)));
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool isDisposing)
    {
        if (this._isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            this._container.Dispose();
        }

        this._isDisposed = true;
    }
}

internal static class AutofacExtensions
{
    public static ContainerBuilder RegisterInfrastructure(this ContainerBuilder containerBuilder, IInfrastructureFactory infrastructureFactory)
    {
        containerBuilder
            .Register(_ => infrastructureFactory)
            .SingleInstance()
            .As<IInfrastructureFactory>();

        containerBuilder
            .Register(_ => new ConsoleLogger("Main"))
            .InstancePerLifetimeScope()
            .As<ILogger>();

        containerBuilder
            .RegisterType<ConsoleMagicLogger>()
            .InstancePerLifetimeScope()
            .As<IMagicLogger>();

        // TODO (SHOULD): Implement flag to enable/disable debugging observer!

        containerBuilder
            .Register(_ => NoopObserver.Instance)
            .SingleInstance()
            .As<IObserver>();

        return containerBuilder;
    }

    public static ContainerBuilder RegisterStorageManager(this ContainerBuilder containerBuilder)
    {
        var dataFolderUri = Config.FindDataFolderUri();

        containerBuilder
            .Register(_ => new IndexManager(dataFolderUri))
            .InstancePerLifetimeScope()
            .As<IIndexManager>();

        containerBuilder
            .Register(_ => new FileStorageManager(dataFolderUri))
            .InstancePerLifetimeScope()
            .Named<IStorageManager>(Name.StorageManager.Data);

        containerBuilder
            .Register(context => new CompressedStorageManager(
                Config.ProcessedContentSpec,
                context.ResolveNamed<IStorageManager>(Name.StorageManager.Data)))
            .InstancePerLifetimeScope()
            .Named<IStorageManager>(Name.StorageManager.Processed);

        return containerBuilder;
    }

    public static ContainerBuilder RegisterRepository(this ContainerBuilder containerBuilder)
    {
        containerBuilder
            .RegisterType<NopFetcher>()
            .InstancePerLifetimeScope()
            .As<IMagicFetcher>();

        containerBuilder
            .RegisterType<UnprocessedMagicRepository>()
            .As<IUnprocessedMagicRepository>()
            .InstancePerLifetimeScope();

        containerBuilder
            .Register(context => new ProcessedMagicRepository(
                context.ResolveNamed<IStorageManager>(Name.StorageManager.Processed)))
            .InstancePerLifetimeScope()
            .As<IProcessedMagicRepository>();

        return containerBuilder;
    }

    public static ContainerBuilder RegisterExecution(this ContainerBuilder containerBuilder)
    {
        containerBuilder
            .RegisterType<MagicCardProcessor>()
            .InstancePerLifetimeScope()
            .As<IMagicCardProcessor>();

        containerBuilder
            .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(type => typeof(IJob).IsAssignableFrom(type))
            .InstancePerLifetimeScope();

        return containerBuilder;
    }

    public static ContainerBuilder RegisterSimulator(this ContainerBuilder containerBuilder)
    {
        containerBuilder
            .RegisterType<EntityFactory>()
            .InstancePerLifetimeScope()
            .As<IEntityFactory>();

        containerBuilder
            .Register(_ => RandomGenerator.Default)
            .InstancePerLifetimeScope()
            .As<IRandomGenerator>();

        containerBuilder
            .RegisterType<GameSimulator>()
            .InstancePerDependency()
            .As<ISimulator<GameConfig, GameResult>>();

        containerBuilder
            .RegisterType<ExperimentSimulator>()
            .InstancePerDependency()
            .As<ISimulator<ExperimentConfig, ExperimentResult>>();

        return containerBuilder;
    }

    private static class Name
    {
        public static class StorageManager
        {
            public static readonly string Data = "StorageManager.Data";

            public static readonly string Processed = "StorageManager.Processed";
        }
    }
}