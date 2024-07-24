﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.SemanticKernel.Experimental.Agents.Models;
using static Microsoft.SemanticKernel.Experimental.Agents.IChatMessage;

namespace Microsoft.SemanticKernel.Experimental.Agents.Internal;

/// <summary>
/// Represents a message that is part of an agent thread.
/// </summary>
internal sealed class ChatMessage : IChatMessage
{
    /// <inheritdoc/>
    public string Id { get; }

    /// <inheritdoc/>
    public string? AgentId { get; }

    /// <inheritdoc/>
    public ChatMessageType ContentType { get; }

    /// <inheritdoc/>
    public string Content { get; }

    /// <inheritdoc/>
    public string Role { get; }

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, object> Properties { get; }

    public IList<IAnnotation> Annotations { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatMessage"/> class.
    /// </summary>
    internal ChatMessage(ThreadMessageModel model)
    {
        var content = model.Content.First();

        this.Annotations =
            content.Text is null ?
                Array.Empty<IAnnotation>() :
                content.Text.Annotations.Select(a => new Annotation(a.Text, a.StartIndex, a.EndIndex, a.FileCitation?.FileId ?? a.FilePath!.FileId, a.FileCitation?.Quote)).ToArray();

        this.Id = model.Id;
        this.AgentId = string.IsNullOrWhiteSpace(model.AssistantId) ? null : model.AssistantId;
        this.Role = model.Role;
        this.ContentType = content.Text is null ? ChatMessageType.Image : ChatMessageType.Text;
        this.Content = content.Text?.Value ?? content.Image?.FileId ?? string.Empty;
        this.Properties = new ReadOnlyDictionary<string, object>(model.Metadata);
    }

    private sealed class Annotation(string label, int startIndex, int endIndex, string fileId, string? quote) : IAnnotation
    {
        /// <inheritdoc/>
        public string FileId { get; } = fileId;

        /// <inheritdoc/>
        public string Label { get; } = label;

        /// <inheritdoc/>
        public string? Quote { get; } = quote;

        /// <inheritdoc/>
        public int StartIndex { get; } = startIndex;

        /// <inheritdoc/>
        public int EndIndex { get; } = endIndex;
    }
}
