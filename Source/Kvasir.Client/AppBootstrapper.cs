// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppBootstrapper.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2020 Cahya Ong
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
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using Caliburn.Micro;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Framework;
    using nGratis.Cop.Olympus.Wpf;
    using Unity;
    using Unity.Injection;
    using Unity.Lifetime;
    using Unity.RegistrationByConvention;

    internal sealed class AppBootstrapper : BootstrapperBase, IDisposable
    {
        private readonly IUnityContainer _unityContainer;

        private bool _isDisposed;

        public AppBootstrapper()
        {
            this._unityContainer = new UnityContainer();

            this.Initialize();
        }

        ~AppBootstrapper()
        {
            this.Dispose(false);
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

            this._unityContainer
                .RegisterInfrastructure()
                .RegisterRepository(dataFolderUri)
                .RegisterViewModels();
        }

        protected override object GetInstance(Type type, string key)
        {
            return this._unityContainer.Resolve(type, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type type)
        {
            return this._unityContainer.ResolveAll(type);
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
                this._unityContainer?.Dispose();
            }

            this._isDisposed = true;
        }
    }

    internal static class UnityExtensions
    {
        public static IUnityContainer RegisterInfrastructure(this IUnityContainer unityContainer)
        {
            Guard
                .Require(unityContainer, nameof(unityContainer))
                .Is.Not.Null();

            unityContainer.RegisterType<IWindowManager, WindowManager>();

            return unityContainer;
        }

        public static IUnityContainer RegisterRepository(this IUnityContainer unityContainer, Uri dataFolderUri)
        {
            Guard
                .Require(unityContainer, nameof(unityContainer))
                .Is.Not.Null();

            Guard
                .Require(dataFolderUri, nameof(dataFolderUri))
                .Is.Not.Null()
                .Is.Folder()
                .Is.Exist();

            unityContainer.RegisterType<IStorageManager, FileStorageManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(dataFolderUri));

            unityContainer.RegisterType<IMagicFetcher, ScryfallFetcher>(
                "SCRYFALL",
                new ContainerControlledLifetimeManager());

            unityContainer.RegisterType<IMagicFetcher, WizardFetcher>(
                "WIZARD",
                new ContainerControlledLifetimeManager());

            unityContainer.RegisterType<IIndexManager, IndexManager>(
                new ContainerControlledLifetimeManager(),
                new InjectionConstructor(dataFolderUri));

            unityContainer.RegisterType<IMagicRepository, MagicRepository>(
                new ContainerControlledLifetimeManager());

            return unityContainer;
        }

        public static IUnityContainer RegisterViewModels(this IUnityContainer unityContainer)
        {
            Guard
                .Require(unityContainer, nameof(unityContainer))
                .Is.Not.Null();

            AllClasses
                .FromAssemblies(false, Assembly.GetEntryAssembly())
                .Where(type => typeof(IScreen).IsAssignableFrom(type))
                .Where(type => type.FullName?.EndsWith("ViewModel") == true)
                .Where(type => type.Name != nameof(AppViewModel))
                .ForEach(type => unityContainer.RegisterType(
                    typeof(IScreen),
                    type,
                    $"auto.{type.Name}",
                    new ContainerControlledLifetimeManager()));

            return unityContainer;
        }
    }
}