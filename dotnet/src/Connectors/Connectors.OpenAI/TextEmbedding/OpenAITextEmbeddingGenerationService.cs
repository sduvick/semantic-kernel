﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Services;

namespace Microsoft.SemanticKernel.Connectors.OpenAI;

/// <summary>
/// OpenAI text embedding service.
/// </summary>
[Experimental("SKEXP0010")]
public sealed class OpenAITextEmbeddingGenerationService : ITextEmbeddingGenerationService
{
    private readonly OpenAIClientCore _core;
    private readonly int? _dimensions;

    /// <summary>
    /// Create an instance of the OpenAI text embedding connector
    /// </summary>
    /// <param name="modelId">Model name</param>
    /// <param name="apiKey">OpenAI API Key</param>
    /// <param name="organization">OpenAI Organization Id (usually optional)</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    /// <param name="dimensions">The number of dimensions the resulting output embeddings should have. Only supported in "text-embedding-3" and later models.</param>
    public OpenAITextEmbeddingGenerationService(
        string modelId,
        string apiKey,
        string? organization = null,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null,
        int? dimensions = null)
    {
        this._core = new(
            modelId: modelId,
            apiKey: apiKey,
            organization: organization,
            httpClient: httpClient,
            logger: loggerFactory?.CreateLogger(typeof(OpenAITextEmbeddingGenerationService)));

        this._core.AddAttribute(AIServiceExtensions.ModelIdKey, modelId);

        this._dimensions = dimensions;
    }

    /// <summary>
    /// Create an instance of the OpenAI text embedding connector
    /// </summary>
    /// <param name="modelId">Model name</param>
    /// <param name="openAIClient">Custom <see cref="OpenAIClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use for logging. If null, no logging will be performed.</param>
    /// <param name="dimensions">The number of dimensions the resulting output embeddings should have. Only supported in "text-embedding-3" and later models.</param>
    public OpenAITextEmbeddingGenerationService(
        string modelId,
        OpenAIClient openAIClient,
        ILoggerFactory? loggerFactory = null,
        int? dimensions = null)
    {
        this._core = new(modelId, openAIClient, loggerFactory?.CreateLogger(typeof(OpenAITextEmbeddingGenerationService)));
        this._core.AddAttribute(AIServiceExtensions.ModelIdKey, modelId);

        this._dimensions = dimensions;
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Attributes => this._core.Attributes;

    /// <inheritdoc/>
    public Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IList<string> data,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        this._core.LogActionDetails();
        return this._core.GetEmbeddingsAsync(data, kernel, this._dimensions, cancellationToken);
    }
}
