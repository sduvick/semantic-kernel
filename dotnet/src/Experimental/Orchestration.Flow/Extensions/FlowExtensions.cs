﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;

namespace Microsoft.SemanticKernel.Experimental.Orchestration;

/// <summary>
/// Extension methods for <see cref="Flow"/>.
/// </summary>
public static class FlowExtensions
{
    internal static List<FlowStep> SortSteps(this Flow flow)
    {
        var sortedSteps = new List<FlowStep>();
        var remainingSteps = new List<FlowStep>(flow.Steps);

        while (remainingSteps.Count > 0)
        {
            var independentStep = remainingSteps.FirstOrDefault(step => !remainingSteps.Any(step.DependsOn)) ??
                throw new KernelException("The plan contains circular dependencies.");

            sortedSteps.Add(independentStep);
            remainingSteps.Remove(independentStep);
        }

        return sortedSteps;
    }

    /// <summary>
    /// Hydrate the reference steps in the flow.
    /// </summary>
    /// <param name="flow">the flow</param>
    /// <param name="flowRepository">the flow repository</param>
    /// <returns>The flow with hydrated steps</returns>
    /// <exception cref="ArgumentException">if referenced flow cannot be found in the repository</exception>
    public static async Task<Flow> BuildReferenceAsync(this Flow flow, IFlowCatalog flowRepository)
    {
        var referenceSteps = flow.Steps.OfType<ReferenceFlowStep>().ToList();

        foreach (var step in referenceSteps)
        {
            flow.Steps.Remove(step);
            var referencedFlow = await flowRepository.GetFlowAsync(step.FlowName).ConfigureAwait(false) ??
                throw new ArgumentException($"Referenced flow {step.FlowName} is not found");

            referencedFlow.CompletionType = step.CompletionType;
            referencedFlow.AddPassthrough(step.Passthrough.ToArray());
            referencedFlow.StartingMessage = step.StartingMessage;
            referencedFlow.TransitionMessage = step.TransitionMessage;

            foreach (var referencedFlowStep in referencedFlow.Steps)
            {
                referencedFlowStep.AddPassthrough(step.Passthrough.ToArray(), isReferencedFlow: true);
            }

            flow.Steps.Add(referencedFlow);
        }

        return flow;
    }
}
