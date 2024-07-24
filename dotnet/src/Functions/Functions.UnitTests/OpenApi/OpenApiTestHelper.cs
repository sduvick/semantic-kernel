﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SemanticKernel.Functions.UnitTests.OpenApi;

/// <summary>
/// Contains helper methods for OpenAPI related tests.
/// </summary>
internal static class OpenApiTestHelper
{
    /// <summary>
    /// Modifies OpenAPI document for testing different scenarios.
    /// </summary>
    /// <param name="openApiDocument">The OpenAPI document content.</param>
    /// <param name="transformer">Delegate with document modifications.</param>
    internal static MemoryStream ModifyOpenApiDocument(Stream openApiDocument, Action<JsonObject> transformer)
    {
        var json = JsonSerializer.Deserialize<JsonObject>(openApiDocument);

        transformer(json!);

        var stream = new MemoryStream();

        JsonSerializer.Serialize(stream, json);

        stream.Seek(0, SeekOrigin.Begin);

        return stream;
    }
}
