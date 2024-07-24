﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Services;
using Microsoft.SemanticKernel.TextToImage;

namespace Microsoft.SemanticKernel.Connectors.OpenAI;

/// <summary>
/// Azure OpenAI Image generation
/// <see herf="https://learn.microsoft.com/en-us/azure/cognitive-services/openai/reference#image-generation" />
/// </summary>
[Experimental("SKEXP0010")]
public sealed class AzureOpenAITextToImageService : ITextToImageService
{
    private readonly OpenAIClient _client;
    private readonly ILogger _logger;
    private readonly string _deploymentName;
    private readonly Dictionary<string, object?> _attributes = [];

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, object?> Attributes => this._attributes;

    /// <summary>
    /// Gets the key used to store the deployment name in the <see cref="IAIService.Attributes"/> dictionary.
    /// </summary>
    public static string DeploymentNameKey => "DeploymentName";

    /// <summary>
    /// Create a new instance of Azure OpenAI image generation service
    /// </summary>
    /// <param name="deploymentName">Deployment name identifier</param>
    /// <param name="endpoint">Azure OpenAI deployment URL</param>
    /// <param name="apiKey">Azure OpenAI API key</param>
    /// <param name="modelId">Model identifier</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The ILoggerFactory used to create a logger for logging. If null, no logging will be performed.</param>
    /// <param name="apiVersion">Azure OpenAI Endpoint ApiVersion</param>
    public AzureOpenAITextToImageService(
        string deploymentName,
        string endpoint,
        string apiKey,
        string? modelId,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null,
        string? apiVersion = null)
    {
        Verify.NotNullOrWhiteSpace(apiKey);
        Verify.NotNullOrWhiteSpace(deploymentName);

        this._deploymentName = deploymentName;

        if (modelId is not null)
        {
            this.AddAttribute(AIServiceExtensions.ModelIdKey, modelId);
        }
        this.AddAttribute(DeploymentNameKey, deploymentName);

        this._logger = loggerFactory?.CreateLogger(typeof(AzureOpenAITextToImageService)) ?? NullLogger.Instance;

        var connectorEndpoint = (!string.IsNullOrWhiteSpace(endpoint) ? endpoint! : httpClient?.BaseAddress?.AbsoluteUri) ??
            throw new ArgumentException($"The {nameof(httpClient)}.{nameof(HttpClient.BaseAddress)} and {nameof(endpoint)} are both null or empty. Please ensure at least one is provided.");

        this._client = new(new Uri(connectorEndpoint),
            new AzureKeyCredential(apiKey),
            GetClientOptions(httpClient, apiVersion));
    }

    /// <summary>
    /// Create a new instance of Azure OpenAI image generation service
    /// </summary>
    /// <param name="deploymentName">Deployment name identifier</param>
    /// <param name="endpoint">Azure OpenAI deployment URL</param>
    /// <param name="credential">Token credentials, e.g. DefaultAzureCredential, ManagedIdentityCredential, EnvironmentCredential, etc.</param>
    /// <param name="modelId">Model identifier</param>
    /// <param name="httpClient">Custom <see cref="HttpClient"/> for HTTP requests.</param>
    /// <param name="loggerFactory">The ILoggerFactory used to create a logger for logging. If null, no logging will be performed.</param>
    /// <param name="apiVersion">Azure OpenAI Endpoint ApiVersion</param>
    public AzureOpenAITextToImageService(
        string deploymentName,
        string endpoint,
        TokenCredential credential,
        string? modelId,
        HttpClient? httpClient = null,
        ILoggerFactory? loggerFactory = null,
        string? apiVersion = null)
    {
        Verify.NotNull(credential);
        Verify.NotNullOrWhiteSpace(deploymentName);

        this._deploymentName = deploymentName;

        if (modelId is not null)
        {
            this.AddAttribute(AIServiceExtensions.ModelIdKey, modelId);
        }
        this.AddAttribute(DeploymentNameKey, deploymentName);

        this._logger = loggerFactory?.CreateLogger(typeof(AzureOpenAITextToImageService)) ?? NullLogger.Instance;

        var connectorEndpoint = !string.IsNullOrWhiteSpace(endpoint) ? endpoint! : httpClient?.BaseAddress?.AbsoluteUri;
        if (connectorEndpoint is null)
        {
            throw new ArgumentException($"The {nameof(httpClient)}.{nameof(HttpClient.BaseAddress)} and {nameof(endpoint)} are both null or empty. Please ensure at least one is provided.");
        }

        this._client = new(new Uri(connectorEndpoint),
            credential,
            GetClientOptions(httpClient, apiVersion));
    }

    /// <summary>
    /// Create a new instance of Azure OpenAI image generation service
    /// </summary>
    /// <param name="deploymentName">Deployment name identifier</param>
    /// <param name="openAIClient"><see cref="OpenAIClient"/> to use for the service.</param>
    /// <param name="modelId">Model identifier</param>
    /// <param name="loggerFactory">The ILoggerFactory used to create a logger for logging. If null, no logging will be performed.</param>
    public AzureOpenAITextToImageService(
        string deploymentName,
        OpenAIClient openAIClient,
        string? modelId,
        ILoggerFactory? loggerFactory = null)
    {
        Verify.NotNull(openAIClient);
        Verify.NotNullOrWhiteSpace(deploymentName);

        this._deploymentName = deploymentName;

        if (modelId is not null)
        {
            this.AddAttribute(AIServiceExtensions.ModelIdKey, modelId);
        }
        this.AddAttribute(DeploymentNameKey, deploymentName);

        this._logger = loggerFactory?.CreateLogger(typeof(AzureOpenAITextToImageService)) ?? NullLogger.Instance;

        this._client = openAIClient;
    }

    /// <inheritdoc/>
    public async Task<string> GenerateImageAsync(
        string description,
        int width,
        int height,
        Kernel? kernel = null,
        CancellationToken cancellationToken = default)
    {
        Verify.NotNull(description);

        var size = (width, height) switch
        {
            (1024, 1024) => ImageSize.Size1024x1024,
            (1792, 1024) => ImageSize.Size1792x1024,
            (1024, 1792) => ImageSize.Size1024x1792,
            _ => throw new NotSupportedException("Dall-E 3 can only generate images of the following sizes 1024x1024, 1792x1024, or 1024x1792")
        };

        Response<ImageGenerations> imageGenerations;
        try
        {
            imageGenerations = await this._client.GetImageGenerationsAsync(
                new ImageGenerationOptions
                {
                    DeploymentName = this._deploymentName,
                    Prompt = description,
                    Size = size,
                }, cancellationToken).ConfigureAwait(false);
        }
        catch (RequestFailedException e)
        {
            throw e.ToHttpOperationException();
        }

        if (!imageGenerations.HasValue)
        {
            throw new KernelException("The response does not contain an image result");
        }

        if (imageGenerations.Value.Data.Count == 0)
        {
            throw new KernelException("The response does not contain any image");
        }

        return imageGenerations.Value.Data[0].Url.AbsoluteUri;
    }

    private static OpenAIClientOptions GetClientOptions(HttpClient? httpClient, string? apiVersion) =>
        ClientCore.GetOpenAIClientOptions(httpClient, apiVersion switch
        {
            // DALL-E 3 is supported in the latest API releases
            _ => OpenAIClientOptions.ServiceVersion.V2024_02_15_Preview
        });

    internal void AddAttribute(string key, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            this._attributes.Add(key, value);
        }
    }
}
