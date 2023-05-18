// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RuleManagementViewModel.cs" company="nGratis">
//  The MIT License — Copyright (c) Cahya Ong
//  See the LICENSE file in the project root for more information.
// </copyright>
// <author>Cahya Ong — cahya.ong@gmail.com</author>
// <creation_timestamp>Saturday, January 18, 2020 7:03:06 PM UTC</creation_timestamp>
// --------------------------------------------------------------------------------------------------------------------

namespace nGratis.AI.Kvasir.Client.Wpf;

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
    private readonly IUnprocessedMagicRepository _unprocessedRepository;

    private IDisposableCollection<RuleViewModel>? _ruleViewModels;

    private bool _isDisposed;

    public RuleManagementViewModel(IUnprocessedMagicRepository unprocessedRepository)
    {
        this._unprocessedRepository = unprocessedRepository;
    }

    ~RuleManagementViewModel()
    {
        this.Dispose(false);
    }

    public IDisposableCollection<RuleViewModel>? RuleViewModels
    {
        get => this._ruleViewModels;
        private set => this.RaiseAndSetIfChanged(ref this._ruleViewModels, value);
    }

    protected override async Task ActivateCoreAsync(CancellationToken cancellationToken)
    {
        var virtualizingProvider = new RuleViewModelProvider(this._unprocessedRepository);

        this.RuleViewModels?.Dispose();
        this.RuleViewModels = new AsyncVirtualizingCollection<RuleViewModel>(virtualizingProvider);

        await Task.CompletedTask;
    }

    protected override async Task DeactivateCoreAsync(bool isClosed, CancellationToken cancellationToken)
    {
        this.RuleViewModels?.Dispose();
        this.RuleViewModels = default;

        await Task.CompletedTask;
    }

    private sealed class RuleViewModelProvider : IPagingDataProvider<RuleViewModel>
    {
        private readonly IUnprocessedMagicRepository _repository;

        public RuleViewModelProvider(IUnprocessedMagicRepository repository)
        {
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