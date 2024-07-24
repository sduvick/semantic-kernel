﻿// Copyright (c) Microsoft. All rights reserved.

using System.Threading.Tasks;

namespace Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

/// <summary>
/// Flow executor interface
/// </summary>
public interface IFlowExecutor
{
    /// <summary>
    /// Execute the <see cref="Flow"/>
    /// </summary>
    /// <param name="flow">Flow</param>
    /// <param name="sessionId">Session id, which is used to track the execution status.</param>
    /// <param name="input">The input from client to continue the execution.</param>
    /// <param name="kernelArguments">The request kernel arguments </param>
    /// <returns>The execution context</returns>
    Task<FunctionResult> ExecuteFlowAsync(Flow flow, string sessionId, string input, KernelArguments kernelArguments);
}
