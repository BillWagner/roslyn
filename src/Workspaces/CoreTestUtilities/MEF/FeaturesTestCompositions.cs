﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Remote;
using Microsoft.CodeAnalysis.Remote.Testing;
using Microsoft.CodeAnalysis.UnitTests.Remote;

namespace Microsoft.CodeAnalysis.Test.Utilities;

public static class FeaturesTestCompositions
{
    public static readonly TestComposition Features = TestComposition.Empty
        .AddAssemblies(MefHostServices.DefaultAssemblies)
        .AddParts(
            typeof(TestSerializerService.Factory),
            typeof(MockWorkspaceEventListenerProvider),  // by default, avoid running Solution Crawler and other services that start in workspace event listeners
            typeof(TestErrorReportingService),           // mocks the info-bar error reporting                                
            typeof(MockFallbackAnalyzerConfigOptionsProvider)); // Prevent initialization for fallback options on TestWorkspace from global options.
                                                                // We need to update tests to handle the options correctly before enabling the default provider.

    public static readonly TestComposition RemoteHost = TestComposition.Empty
        .AddAssemblies(MefHostServicesHelpers.LoadNearbyAssemblies(RemoteExportProviderBuilder.RemoteHostAssemblyNames))
        .AddParts(typeof(TestSerializerService.Factory));

    public static TestComposition WithTestHostParts(this TestComposition composition, TestHost host)
        => (host == TestHost.InProcess) ? composition : composition.AddAssemblies(typeof(RemoteWorkspacesResources).Assembly).AddParts(typeof(InProcRemoteHostClientProvider.Factory));
}
