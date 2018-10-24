// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppBootstrapper.cs" company="nGratis">
//  The MIT License (MIT)
//
//  Copyright (c) 2014 - 2018 Cahya Ong
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
    using System.Linq;
    using System.Windows;
    using Caliburn.Micro;
    using Lamar;
    using Lamar.Scanning;
    using Lamar.Scanning.Conventions;
    using Microsoft.Extensions.DependencyInjection;
    using nGratis.Cop.Core.Contract;

    internal class AppBootstrapper : BootstrapperBase
    {
        private Container container;

        public AppBootstrapper()
        {
            this.Initialize();
        }

        protected override void Configure()
        {
            this.container = new Container(registry =>
            {
                registry.AddSingleton<IWindowManager, WindowManager>();

                registry.Scan(scanner =>
                {
                    scanner.TheCallingAssembly();
                    scanner.Convention<ViewModelConvention>();
                });
            });
        }

        protected override object GetInstance(Type type, string key)
        {
            return !string.IsNullOrEmpty(key)
                ? this.container.GetInstance(type, key)
                : this.container.GetInstance(type);
        }

        protected override IEnumerable<object> GetAllInstances(Type type)
        {
            return this
                .container
                .GetAllInstances(type)
                .Cast<object>();
        }

        protected override void OnStartup(object sender, StartupEventArgs args)
        {
            this.DisplayRootViewFor<AppViewModel>();
        }

        private class ViewModelConvention : IRegistrationConvention
        {
            public void ScanTypes(TypeSet typeSet, ServiceRegistry serviceRegistry)
            {
                Guard
                    .Require(typeSet, nameof(typeSet))
                    .Is.Not.Null();

                Guard
                    .Require(serviceRegistry, nameof(serviceRegistry))
                    .Is.Not.Null();

                typeSet
                    .FindTypes(TypeClassification.Concretes)
                    .Where(type => !string.IsNullOrEmpty(type.Name))
                    .Where(type => type.Name.EndsWith("ViewModel"))
                    .ForEach(type => serviceRegistry.AddSingleton(type));
            }
        }
    }
}