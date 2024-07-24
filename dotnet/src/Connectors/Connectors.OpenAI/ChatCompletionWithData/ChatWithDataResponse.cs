﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Microsoft.SemanticKernel.Connectors.OpenAI;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes

[Experimental("SKEXP0010")]
[Obsolete("This class is deprecated in favor of OpenAIPromptExecutionSettings.AzureChatExtensionsOptions")]
[method: JsonConstructor]
internal sealed class ChatWithDataResponse(ChatWithDataUsage usage)
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("created")]
    public int Created { get; set; } = default;

    [JsonPropertyName("choices")]
    public IList<ChatWithDataChoice> Choices { get; set; } = Array.Empty<ChatWithDataChoice>();

    [JsonPropertyName("usage")]
    public ChatWithDataUsage Usage { get; set; } = usage;

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;
}

[Experimental("SKEXP0010")]
[Obsolete("This class is deprecated in favor of OpenAIPromptExecutionSettings.AzureChatExtensionsOptions")]
[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Used for JSON deserialization")]
internal sealed class ChatWithDataChoice
{
    [JsonPropertyName("messages")]
    public IList<ChatWithDataMessage> Messages { get; set; } = Array.Empty<ChatWithDataMessage>();
}

[Experimental("SKEXP0010")]
[Obsolete("This class is deprecated in favor of OpenAIPromptExecutionSettings.AzureChatExtensionsOptions")]
internal sealed class ChatWithDataUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}
