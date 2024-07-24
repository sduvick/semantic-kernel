﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;

namespace Microsoft.SemanticKernel;

/// <summary>Provides an <see cref="EventArgs"/> for operations related to <see cref="Kernel"/>-based operations.</summary>
[Obsolete("Events are deprecated in favor of filters. Example in dotnet/samples/GettingStarted/Step7_Observability.cs of Semantic Kernel repository.")]
public abstract class KernelEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KernelEventArgs"/> class.
    /// </summary>
    /// <param name="function">The <see cref="KernelFunction"/> with which this event is associated.</param>
    /// <param name="arguments">The arguments associated with the operation.</param>
    /// <param name="metadata">A dictionary of metadata associated with the operation.</param>
    internal KernelEventArgs(KernelFunction function, KernelArguments arguments, IReadOnlyDictionary<string, object?>? metadata)
    {
        Verify.NotNull(function);
        Verify.NotNull(arguments);

        this.Function = function;
        this.Arguments = arguments;
        this.Metadata = metadata;
    }

    /// <summary>
    /// Gets the <see cref="KernelFunction"/> with which this event is associated.
    /// </summary>
    public KernelFunction Function { get; }

    /// <summary>
    /// Gets the arguments associated with the operation.
    /// </summary>
    public KernelArguments Arguments { get; }

    /// <summary>
    /// Gets a dictionary of metadata related to the event.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Metadata { get; }
}
