﻿// Copyright (c) Microsoft. All rights reserved.

using System.IO;
using System.Resources;

namespace SemanticKernel.Functions.UnitTests.OpenApi.TestResponses;

internal static class ResourceResponseProvider
{
    /// <summary>
    /// Loads OpenAPI response schema and content from assembly resource.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <returns>The OpenAPI response schema or content resource stream.</returns>
    public static string LoadFromResource(string resourceName)
    {
        var type = typeof(ResourceResponseProvider);

        var stream = type.Assembly.GetManifestResourceStream(type, resourceName) ??
            throw new MissingManifestResourceException($"Unable to load OpenAPI response from assembly resource '{resourceName}'.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
