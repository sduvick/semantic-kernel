﻿// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;
using System.Threading;
using Moq;
using Moq.Protected;

namespace SemanticKernel.Experimental.Agents.UnitTests;

internal static class MockExtensions
{
    public static void VerifyMock(this Mock<HttpMessageHandler> mockHandler, HttpMethod method, int times, string? uri = null)
    {
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Exactly(times),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == method && (uri == null || req.RequestUri!.AbsoluteUri.StartsWith(uri))),
            ItExpr.IsAny<CancellationToken>());
    }
}
