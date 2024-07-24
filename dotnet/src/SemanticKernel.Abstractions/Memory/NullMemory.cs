﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Memory;

/// <summary>
/// Implementation of <see cref="ISemanticTextMemory"/> that stores nothing.
/// </summary>
[Experimental("SKEXP0001")]
public sealed class NullMemory : ISemanticTextMemory
{
    private static readonly Task<string> s_emptyStringTask = Task.FromResult(string.Empty);

    /// <summary>
    /// Singleton instance
    /// </summary>
    public static NullMemory Instance { get; } = new();

    /// <inheritdoc/>
    public Task<string> SaveInformationAsync(
        string collection,
        string text,
        string id,
        string? description = null,
        string? additionalMetadata = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return s_emptyStringTask;
    }

    /// <inheritdoc/>
    public Task<string> SaveReferenceAsync(
        string collection,
        string text,
        string externalId,
        string externalSourceName,
        string? description = null,
        string? additionalMetadata = null,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return s_emptyStringTask;
    }

    /// <inheritdoc/>
    public Task<MemoryQueryResult?> GetAsync(
        string collection,
        string key,
        bool withEmbedding = false,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<MemoryQueryResult?>(null);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(
        string collection,
        string key,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<MemoryQueryResult> SearchAsync(
        string collection,
        string query,
        int limit = 1,
        double minRelevanceScore = 0.0,
        bool withEmbeddings = false,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return AsyncEnumerable.Empty<MemoryQueryResult>();
    }

    /// <inheritdoc/>
    public Task<IList<string>> GetCollectionsAsync(
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IList<string>>([]);
    }

    private NullMemory()
    {
    }
}
