﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Orchestration;
using Microsoft.SemanticKernel.Experimental.Orchestration.Abstractions;
using Xunit;

namespace SemanticKernel.Experimental.Orchestration.Flow.UnitTests;

public class FlowExtensionsTests
{
    [Fact]
    public async Task TestBuildReferenceStepAsync()
    {
        // Arrange
        var flow1 = CreateFlowWithReferenceStep("flow2");

        var flow2 = new Microsoft.SemanticKernel.Experimental.Orchestration.Flow("flow2", "test flow goal 2")
        {
            CompletionType = CompletionType.Optional
        };
        var step5 = new FlowStep("step1");
        step5.AddRequires("a");
        step5.AddProvides("b");
        flow2.AddProvides("b");
        flow2.AddStep(step5);

        // Act
        var catalog = new InMemoryFlowCatalog([flow1, flow2]);
        var flow1InCatalog = await catalog.GetFlowAsync("flow1");
        Assert.NotNull(flow1InCatalog);

        // Assert
        Assert.DoesNotContain(flow1InCatalog.Steps, step => step is ReferenceFlowStep);
        var flow2Step = flow1InCatalog.Steps.OfType<Microsoft.SemanticKernel.Experimental.Orchestration.Flow>().SingleOrDefault();
        Assert.NotNull(flow2Step);
        Assert.Equal("flow2", flow2Step.Name);
        Assert.Equal(CompletionType.Optional, flow2Step.CompletionType);
        Assert.Equal("a", flow2Step.Requires.SingleOrDefault());
        Assert.Equal("b", flow2Step.Provides.SingleOrDefault());
    }

    [Fact]
    public void TestBuildNonExistReferenceStep()
    {
        // Arrange
        var flow1 = CreateFlowWithReferenceStep("flow2");

        var flow2 = new Microsoft.SemanticKernel.Experimental.Orchestration.Flow("flow3", "test flow goal 2");
        var step5 = new FlowStep("step1");
        step5.AddProvides("a");
        flow2.AddProvides("a");
        flow2.AddStep(step5);

        // Act and assert
        Assert.Throws<AggregateException>(() => new InMemoryFlowCatalog([flow1, flow2]));
    }

    private static Microsoft.SemanticKernel.Experimental.Orchestration.Flow CreateFlowWithReferenceStep(string referenceFlowName)
    {
        var flow = new Microsoft.SemanticKernel.Experimental.Orchestration.Flow("flow1", "test flow goal");
        var step1 = new FlowStep("step1");
        step1.AddProvides("a");
        var step2 = new FlowStep("step2");
        step2.AddRequires("a");
        step2.AddProvides("b");
        var step3 = new FlowStep("step3");
        step3.AddRequires("a", "b");
        step3.AddProvides("c");
        var step4 = new ReferenceFlowStep(referenceFlowName)
        {
            CompletionType = CompletionType.Optional
        };
        flow.AddStep(step1);
        flow.AddStep(step2);
        flow.AddStep(step3);
        flow.AddStep(step4);

        return flow;
    }

    private sealed class InMemoryFlowCatalog : IFlowCatalog
    {
        private readonly Dictionary<string, Microsoft.SemanticKernel.Experimental.Orchestration.Flow> _flows = [];

        internal InMemoryFlowCatalog()
        {
        }

        internal InMemoryFlowCatalog(IReadOnlyList<Microsoft.SemanticKernel.Experimental.Orchestration.Flow> flows)
        {
            // phase 1: register original flows
            foreach (var flow in flows)
            {
                this._flows.Add(flow.Name, flow);
            }

            // phase 2: build references
            foreach (var flow in flows)
            {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                flow.BuildReferenceAsync(this).Wait();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
            }
        }

        public Task<IEnumerable<Microsoft.SemanticKernel.Experimental.Orchestration.Flow>> GetFlowsAsync()
        {
            return Task.FromResult(this._flows.Select(_ => _.Value));
        }

        public Task<Microsoft.SemanticKernel.Experimental.Orchestration.Flow?> GetFlowAsync(string flowName)
        {
            return Task.FromResult(this._flows.TryGetValue(flowName, out var flow) ? flow : null);
        }

        public Task<bool> RegisterFlowAsync(Microsoft.SemanticKernel.Experimental.Orchestration.Flow flow)
        {
            this._flows.Add(flow.Name, flow);

            return Task.FromResult(true);
        }
    }
}
