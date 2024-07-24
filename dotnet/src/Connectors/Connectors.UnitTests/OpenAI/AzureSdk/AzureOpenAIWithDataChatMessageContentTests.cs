﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Xunit;

namespace SemanticKernel.Connectors.UnitTests.OpenAI.AzureSdk;

#pragma warning disable CS0618 // AzureOpenAIChatCompletionWithData is deprecated in favor of OpenAIPromptExecutionSettings.AzureChatExtensionsOptions

/// <summary>
/// Unit tests for <see cref="AzureOpenAIWithDataChatMessageContent"/> class.
/// </summary>
public sealed class AzureOpenAIWithDataChatMessageContentTests
{
    [Fact]
    public void ConstructorThrowsExceptionWhenAssistantMessageIsNotProvided()
    {
        // Arrange
        var choice = new ChatWithDataChoice();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new AzureOpenAIWithDataChatMessageContent(choice, "model-id"));

        Assert.Contains("Chat is not valid", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConstructorReturnsInstanceWithNullToolContent()
    {
        // Arrange
        var choice = new ChatWithDataChoice { Messages = [new() { Content = "Assistant content", Role = "assistant" }] };

        // Act
        var content = new AzureOpenAIWithDataChatMessageContent(choice, "model-id");

        // Assert
        Assert.Equal("Assistant content", content.Content);
        Assert.Equal(AuthorRole.Assistant, content.Role);

        Assert.Null(content.ToolContent);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ConstructorReturnsInstanceWithNonNullToolContent(bool includeMetadata)
    {
        // Arrange
        var choice = new ChatWithDataChoice
        {
            Messages = [
                new() { Content = "Assistant content", Role = "assistant" },
                new() { Content = "Tool content", Role = "tool" }]
        };

        // Act
        var content = includeMetadata ?
            new AzureOpenAIWithDataChatMessageContent(choice, "model-id", new Dictionary<string, object?>()) :
            new AzureOpenAIWithDataChatMessageContent(choice, "model-id");

        // Assert
        Assert.Equal("Assistant content", content.Content);
        Assert.Equal("Tool content", content.ToolContent);
        Assert.Equal(AuthorRole.Assistant, content.Role);

        Assert.NotNull(content.Metadata);
        Assert.Equal("Tool content", content.Metadata["ToolContent"]);
    }

    [Fact]
    public void ConstructorCloneReadOnlyMetadataDictionary()
    {
        // Arrange
        var choice = new ChatWithDataChoice
        {
            Messages = [new() { Content = "Assistant content", Role = "assistant" }]
        };

        var metadata = new ReadOnlyInternalDictionary(new Dictionary<string, object?>() { ["Extra"] = "Data" });

        // Act
        var content = new AzureOpenAIWithDataChatMessageContent(choice, "model-id", metadata);

        // Assert
        Assert.Equal("Assistant content", content.Content);
        Assert.Equal(AuthorRole.Assistant, content.Role);

        Assert.NotNull(content.Metadata);
        Assert.Equal("Data", content.Metadata["Extra"]);
    }

    private sealed class ReadOnlyInternalDictionary : IReadOnlyDictionary<string, object?>
    {
        public ReadOnlyInternalDictionary(IDictionary<string, object?> initializingData)
        {
            this._internalDictionary = new Dictionary<string, object?>(initializingData);
        }
        private readonly Dictionary<string, object?> _internalDictionary;

        public object? this[string key] => this._internalDictionary[key];

        public IEnumerable<string> Keys => this._internalDictionary.Keys;

        public IEnumerable<object?> Values => this._internalDictionary.Values;

        public int Count => this._internalDictionary.Count;

        public bool ContainsKey(string key) => this._internalDictionary.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator() => this._internalDictionary.GetEnumerator();

        public bool TryGetValue(string key, out object? value) => this._internalDictionary.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => this._internalDictionary.GetEnumerator();
    }
}
