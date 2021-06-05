// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppBootstrapper.cs" company="nGratis">
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
// <creation_timestamp>Saturday, February 20, 2021 7:58:00 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Console
{
    using System;
    using System.Reflection;
    using Autofac;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.AI.Kvasir.Core.Parser;
    using nGratis.AI.Kvasir.Engine;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;

    internal class AppBootstrapper : IDisposable
    {
        private readonly IContainer _container;

        private bool _isDisposed;

        public AppBootstrapper()
        {
            this._container = new ContainerBuilder()
                .RegisterInfrastructure()
                .RegisterStorageManager()
                .RegisterRepository()
                .RegisterExecution()
                .RegisterSimulator()
                .Build();
        }

        ~AppBootstrapper()
        {
            this.Dispose(false);
        }

        public IExecution CreateExecution<T>()
            where T : class, IExecution
        {
            return this._container.Resolve<T>();
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
        public static ContainerBuilder RegisterInfrastructure(this ContainerBuilder containerBuilder)
        {
            Guard
                .Require(containerBuilder, nameof(containerBuilder))
                .Is.Not.Null();

            containerBuilder
                .Register(_ => new ConsoleLogger("Main"))
                .InstancePerLifetimeScope()
                .As<ILogger>();

            return containerBuilder;
        }

        public static ContainerBuilder RegisterStorageManager(this ContainerBuilder containerBuilder)
        {
            Guard
                .Require(containerBuilder, nameof(containerBuilder))
                .Is.Not.Null();

            var dataFolderUri = Configuration.FindDataFolderUri();

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
                    Configuration.ProcessedContentSpec,
                    context.ResolveNamed<IStorageManager>(Name.StorageManager.Data)))
                .InstancePerLifetimeScope()
                .Named<IStorageManager>(Name.StorageManager.Processed);

            return containerBuilder;
        }

        public static ContainerBuilder RegisterRepository(this ContainerBuilder containerBuilder)
        {
            Guard
                .Require(containerBuilder, nameof(containerBuilder))
                .Is.Not.Null();

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
            Guard
                .Require(containerBuilder, nameof(containerBuilder))
                .Is.Not.Null();

            containerBuilder
                .RegisterType<MagicCardProcessor>()
                .InstancePerLifetimeScope()
                .As<IMagicCardProcessor>();

            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(type => typeof(IExecution).IsAssignableFrom(type))
                .InstancePerLifetimeScope();

            return containerBuilder;
        }

        public static ContainerBuilder RegisterSimulator(this ContainerBuilder containerBuilder)
        {
            Guard
                .Require(containerBuilder, nameof(containerBuilder))
                .Is.Not.Null();

            containerBuilder
                .Register(_ => RandomGenerator.Default)
                .InstancePerLifetimeScope()
                .As<IRandomGenerator>();

            containerBuilder
                .RegisterType<MagicEntityFactory>()
                .InstancePerLifetimeScope()
                .As<IMagicEntityFactory>();

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
}