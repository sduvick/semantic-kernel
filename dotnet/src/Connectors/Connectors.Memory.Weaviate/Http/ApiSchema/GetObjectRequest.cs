﻿// Copyright (c) Microsoft. All rights reserved.

using System.Net.Http;

namespace Microsoft.SemanticKernel.Connectors.Weaviate;

internal sealed class GetObjectRequest
{
    public string? Id { get; set; }
    public string[]? Additional { get; set; }

    public HttpRequestMessage Build()
    {
        return HttpRequest.CreateGetRequest($"objects/{this.Id}{(this.Additional is null ? string.Empty : $"?include={string.Join(",", this.Additional)}")}");
    }
}
