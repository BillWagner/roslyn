﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable disable

using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGeneration;
using Microsoft.CodeAnalysis.Editor.Shared.Extensions;
using Microsoft.CodeAnalysis.Editor.Shared.Utilities;
using Microsoft.CodeAnalysis.ExtractInterface;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageService;
using Microsoft.CodeAnalysis.Notification;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.LanguageServices.Implementation.CommonControls;
using Microsoft.VisualStudio.LanguageServices.Utilities;
using Microsoft.VisualStudio.Utilities;
using Roslyn.Utilities;

namespace Microsoft.VisualStudio.LanguageServices.Implementation.ExtractInterface;

[ExportWorkspaceService(typeof(IExtractInterfaceOptionsService), ServiceLayer.Host), Shared]
[method: ImportingConstructor]
[method: Obsolete(MefConstruction.ImportingConstructorMessage, error: true)]
internal sealed class VisualStudioExtractInterfaceOptionsService(IGlyphService glyphService, IThreadingContext threadingContext, IUIThreadOperationExecutor uiThreadOperationExecutor) : IExtractInterfaceOptionsService
{
    private readonly IGlyphService _glyphService = glyphService;
    private readonly IThreadingContext _threadingContext = threadingContext;
    private readonly IUIThreadOperationExecutor _uiThreadOperationExecutor = uiThreadOperationExecutor;

    public ExtractInterfaceOptionsResult GetExtractInterfaceOptions(
        ISyntaxFactsService syntaxFactsService,
        INotificationService notificationService,
        List<ISymbol> extractableMembers,
        string defaultInterfaceName,
        List<string> allTypeNames,
        string defaultNamespace,
        string generatedNameTypeParameterSuffix,
        string languageName,
        CancellationToken cancellationToken)
    {
        _threadingContext.ThrowIfNotOnUIThread();

        using var cancellationTokenSource = new CancellationTokenSource();

        var memberViewModels = extractableMembers
            .SelectAsArray(member =>
                new MemberSymbolViewModel(member, _glyphService)
                {
                    IsChecked = true,
                    MakeAbstract = false,
                    IsMakeAbstractCheckable = false,
                    IsCheckable = true
                });

        var viewModel = new ExtractInterfaceDialogViewModel(
            syntaxFactsService,
            _uiThreadOperationExecutor,
            notificationService,
            defaultInterfaceName,
            allTypeNames,
            memberViewModels,
            defaultNamespace,
            generatedNameTypeParameterSuffix,
            languageName);

        var dialog = new ExtractInterfaceDialog(viewModel);
        var result = dialog.ShowModal();

        if (result.HasValue && result.Value)
        {
            var includedMembers = viewModel.MemberContainers.Where(c => c.IsChecked).Select(c => c.Symbol);

            return new ExtractInterfaceOptionsResult(
                isCancelled: false,
                includedMembers: includedMembers.AsImmutable(),
                interfaceName: viewModel.DestinationViewModel.TypeName.Trim(),
                fileName: viewModel.DestinationViewModel.FileName.Trim(),
                location: GetLocation(viewModel.DestinationViewModel.Destination));
        }
        else
        {
            return ExtractInterfaceOptionsResult.Cancelled;
        }
    }

    private static ExtractInterfaceOptionsResult.ExtractLocation GetLocation(NewTypeDestination destination)
        => destination switch
        {
            NewTypeDestination.CurrentFile => ExtractInterfaceOptionsResult.ExtractLocation.SameFile,
            NewTypeDestination.NewFile => ExtractInterfaceOptionsResult.ExtractLocation.NewFile,
            _ => throw ExceptionUtilities.UnexpectedValue(destination),
        };
}
