﻿// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.SemanticKernel;

/// <summary>
/// Class for extensions methods to define functions using prompt markdown format.
/// </summary>
public static class MarkdownKernelExtensions
{
    /// <summary>
    /// Creates a <see cref="KernelFunction"/> instance for a prompt function using the specified markdown text.
    /// </summary>
    /// <param name="kernel">The <see cref="Kernel"/> containing services, plugins, and other state for use throughout the operation.</param>
    /// <param name="text">YAML representation of the <see cref="PromptTemplateConfig"/> to use to create the prompt function</param>
    /// <param name="functionName">The name of the function.</param>
    /// <param name="promptTemplateFactory">
    /// The <see cref="IPromptTemplateFactory"/> to use when interpreting the prompt template configuration into a <see cref="IPromptTemplate"/>.
    /// If null, a default factory will be used.
    /// </param>
    /// <returns>The created <see cref="KernelFunction"/>.</returns>
    public static KernelFunction CreateFunctionFromMarkdown(
        this Kernel kernel,
        string text,
        string functionName,
        IPromptTemplateFactory? promptTemplateFactory = null)
    {
        Verify.NotNull(kernel);
        Verify.NotNull(text);
        Verify.NotNull(functionName);

        return KernelFunctionMarkdown.FromPromptMarkdown(text, functionName, promptTemplateFactory, kernel.LoggerFactory);
    }
}
