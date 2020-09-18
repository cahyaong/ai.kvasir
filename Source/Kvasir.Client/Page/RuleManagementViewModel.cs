// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleManagementViewModel.cs" company="nGratis">
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
// <creation_timestamp>Saturday, January 18, 2020 7:03:06 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using nGratis.AI.Kvasir.Contract;
    using nGratis.AI.Kvasir.Core;
    using nGratis.Cop.Olympus.Contract;
    using nGratis.Cop.Olympus.Wpf;
    using nGratis.Cop.Olympus.Wpf.Glue;
    using ReactiveUI;

    [PageDefinition("Rule", Ordering = 3)]
    public sealed class RuleManagementViewModel : ReactiveScreen, IDisposable
    {
        private readonly IMagicRepository _repository;

        private IDisposableCollection<RuleViewModel> _ruleViewModels;

        private bool _isDisposed;

        public RuleManagementViewModel(IMagicRepository repository)
        {
            Guard
                .Require(repository, nameof(repository))
                .Is.Not.Null();

            this._repository = repository;
        }

        ~RuleManagementViewModel()
        {
            this.Dispose(false);
        }

        public IDisposableCollection<RuleViewModel> RuleViewModels
        {
            get => this._ruleViewModels;
            private set => this.RaiseAndSetIfChanged(ref this._ruleViewModels, value);
        }

        protected override async Task ActivateCoreAsync(CancellationToken cancellationToken)
        {
            var virtualizingProvider = new RuleViewModelProvider(this._repository);

            this.RuleViewModels?.Dispose();
            this.RuleViewModels = new AsyncVirtualizingCollection<RuleViewModel>(virtualizingProvider);

            await Task.CompletedTask;
        }

        protected override async Task DeactivateCoreAysnc(bool isClosed, CancellationToken cancellationToken)
        {
            this.RuleViewModels?.Dispose();
            this.RuleViewModels = null;

            await Task.CompletedTask;
        }

        private sealed class RuleViewModelProvider : IPagingDataProvider<RuleViewModel>
        {
            private readonly IMagicRepository _repository;

            public RuleViewModelProvider(IMagicRepository repository)
            {
                Guard
                    .Require(repository, nameof(repository))
                    .Is.Not.Null();

                this._repository = repository;
            }

            public RuleViewModel DefaultItem
            {
                get
                {
                    var unparsedRule = new UnparsedBlob.Rule
                    {
                        Id = "000.0",
                        Text = "Loading..."
                    };

                    return new RuleViewModel(unparsedRule);
                }
            }

            public async Task<int> GetCountAsync()
            {
                return await this._repository.GetRuleCountAsync();
            }

            public async Task<IReadOnlyCollection<RuleViewModel>> GetItemsAsync(int pagingIndex, int itemCount)
            {
                var unparsedRules = await this._repository.GetRulesAsync(pagingIndex, itemCount);

                return unparsedRules
                    .Select(unparsedRule => new RuleViewModel(unparsedRule))
                    .ToArray();
            }
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
                this.RuleViewModels?.Dispose();
            }

            this._isDisposed = true;
        }
    }
}