﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel.Connectors.Google.Core;
using Xunit;

namespace SemanticKernel.Connectors.Google.UnitTests.Core.GoogleAI;

public sealed class GoogleAIEmbeddingRequestTests
{
    [Fact]
    public void FromDataReturnsValidRequestWithData()
    {
        // Arrange
        string[] data = ["text1", "text2"];
        var modelId = "modelId";

        // Act
        var request = GoogleAIEmbeddingRequest.FromData(data, modelId);

        // Assert
        Assert.Equal(2, request.Requests.Count);
        Assert.Equal(data[0], request.Requests[0].Content.Parts![0].Text);
        Assert.Equal(data[1], request.Requests[1].Content.Parts![0].Text);
    }

    [Fact]
    public void FromDataReturnsValidRequestWithModelId()
    {
        // Arrange
        string[] data = ["text1", "text2"];
        var modelId = "modelId";

        // Act
        var request = GoogleAIEmbeddingRequest.FromData(data, modelId);

        // Assert
        Assert.Equal(2, request.Requests.Count);
        Assert.Equal($"models/{modelId}", request.Requests[0].Model);
        Assert.Equal($"models/{modelId}", request.Requests[1].Model);
    }
}
