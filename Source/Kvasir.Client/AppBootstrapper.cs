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
// <creation_timestamp>Tuesday, 23 October 2018 9:32:04 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows;
    using Autofac;
    using Autofac.Core;
    using Caliburn.Micro;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;
    using nGratis.Cop.Olympus.Wpf;

    internal sealed class AppBootstrapper : BootstrapperBase
    {
        private IContainer _container;

        public AppBootstrapper()
        {
            this.Initialize();
        }

        protected override void Configure()
        {
            var dataFolderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NGRATIS",
                "ai.kvasir");

            if (!Directory.Exists(dataFolderPath))
            {
                Directory.CreateDirectory(dataFolderPath);
            }

            var dataFolderUri = new Uri(dataFolderPath);

            this._container = new ContainerBuilder()
                .RegisterInfrastructure()
                .RegisterRepository(dataFolderUri)
                .RegisterViewModels()
                .Build();
        }

        protected override object GetInstance(Type type, string key)
        {
            return this._container.Resolve(type);
        }

        protected override IEnumerable<object> GetAllInstances(Type type)
        {
            var service = new TypedService(typeof(IEnumerable<>).MakeGenericType(type));

            return (object[])this._container.ResolveService(service);
        }

        protected override void OnStartup(object sender, StartupEventArgs args)
        {
            if (sender is Application app)
            {
                var theme = ControlzEx.Theming.ThemeManager.Current.ChangeTheme(app, "Dark.Green");
                app.AdjustAccentColor(theme.PrimaryAccentColor);
            }

            this.DisplayRootViewFor<AppViewModel>();
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
                .RegisterType<WindowManager>()
                .As<IWindowManager>();

            return containerBuilder;
        }

        public static ContainerBuilder RegisterRepository(this ContainerBuilder containerBuilder, Uri dataFolderUri)
        {
            Guard
                .Require(containerBuilder, nameof(containerBuilder))
                .Is.Not.Null();

            Guard
                .Require(dataFolderUri, nameof(dataFolderUri))
                .Is.Not.Null()
                .Is.Folder()
                .Is.Exist();

            containerBuilder
                .Register(_ => new FileStorageManager(dataFolderUri))
                .As<IStorageManager>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterType<ScryfallFetcher>()
                .As<IMagicFetcher>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterType<WizardFetcher>()
                .As<IMagicFetcher>()
                .InstancePerLifetimeScope();

            containerBuilder
                .Register(_ => new IndexManager(dataFolderUri))
                .As<IIndexManager>()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterType<UnprocessedMagicRepository>()
                .As<IUnprocessedMagicRepository>()
                .InstancePerLifetimeScope();

            return containerBuilder;
        }

        public static ContainerBuilder RegisterViewModels(this ContainerBuilder containerBuilder)
        {
            Guard
                .Require(containerBuilder, nameof(containerBuilder))
                .Is.Not.Null();

            containerBuilder
                .RegisterType<AppViewModel>()
                .AsSelf()
                .InstancePerLifetimeScope();

            containerBuilder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(type => typeof(IScreen).IsAssignableFrom(type))
                .Where(type => type.FullName?.EndsWith("ViewModel") == true)
                .Where(type => type != typeof(AppViewModel))
                .As<IScreen>()
                .InstancePerLifetimeScope();

            return containerBuilder;
        }
    }
}