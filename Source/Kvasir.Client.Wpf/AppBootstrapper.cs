// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppBootstrapper.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Tuesday, 23 October 2018 9:32:04 AM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Windows;
using Autofac;
using Autofac.Core;
using Caliburn.Micro;
using nGratis.AI.Kvasir.Contract;
using nGratis.AI.Kvasir.Core;
using nGratis.Cop.Olympus.Contract;
using nGratis.Cop.Olympus.Framework;
using nGratis.Cop.Olympus.Wpf;

[SuppressMessage(
    "Interoperability", "CA1416:Validate platform compatibility",
    Justification = "We support only Windows OS for now!")]
internal sealed class AppBootstrapper : BootstrapperBase, IDisposable
{
    private IContainer? _container;

    private bool _isDisposed;

    public AppBootstrapper()
    {
        this.Initialize();
    }

    ~AppBootstrapper()
    {
        this.Dispose(false);
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
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
        if (this._container == null)
        {
            throw new KvasirException("Container should have been initialized during configuration?!");
        }

        return this._container.Resolve(type);
    }

    protected override IEnumerable<object> GetAllInstances(Type type)
    {
        if (this._container == null)
        {
            throw new KvasirException("Container should have been initialized during configuration?!");
        }

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

    private void Dispose(bool isDisposing)
    {
        if (this._isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            this._container?.Dispose();
        }

        this._isDisposed = true;
    }
}

[SuppressMessage(
    "Interoperability", "CA1416:Validate platform compatibility",
    Justification = "We support only Windows OS for now!")]
internal static class AutofacExtensions
{
    public static ContainerBuilder RegisterInfrastructure(this ContainerBuilder containerBuilder)
    {
        containerBuilder
            .RegisterType<WindowManager>()
            .As<IWindowManager>();

        return containerBuilder;
    }

    public static ContainerBuilder RegisterRepository(this ContainerBuilder containerBuilder, Uri dataFolderUri)
    {
        Guard
            .Require(dataFolderUri, nameof(dataFolderUri))
            .Is.Folder()
            .Is.Exist();

        containerBuilder
            .Register(_ => new FileStorageManager(dataFolderUri))
            .InstancePerLifetimeScope()
            .As<IStorageManager>();

        containerBuilder
            .RegisterType<ScryfallFetcher>()
            .InstancePerLifetimeScope()
            .As<IMagicFetcher>();

        containerBuilder
            .RegisterType<WizardFetcher>()
            .InstancePerLifetimeScope()
            .As<IMagicFetcher>();

        containerBuilder
            .Register(_ => new IndexManager(dataFolderUri))
            .InstancePerLifetimeScope()
            .As<IIndexManager>();

        containerBuilder
            .RegisterType<UnprocessedMagicRepository>()
            .InstancePerLifetimeScope()
            .As<IUnprocessedMagicRepository>();

        return containerBuilder;
    }

    public static ContainerBuilder RegisterViewModels(this ContainerBuilder containerBuilder)
    {
        containerBuilder
            .RegisterType<AppViewModel>()
            .InstancePerLifetimeScope()
            .AsSelf();

        containerBuilder
            .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .Where(type => typeof(IScreen).IsAssignableFrom(type))
            .Where(type => type.FullName?.EndsWith("ViewModel") == true)
            .Where(type => type != typeof(AppViewModel))
            .InstancePerLifetimeScope()
            .As<IScreen>();

        return containerBuilder;
    }
}