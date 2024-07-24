﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Microsoft.SemanticKernel.Experimental.Orchestration;

/// <summary>
/// Serializer for <see cref="Flow"/>
/// </summary>
public static class FlowSerializer
{
    /// <summary>Options for <see cref="DeserializeFromJson"/>.</summary>
    private static readonly JsonSerializerOptions s_deserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    /// <summary>
    /// Deserialize flow from yaml
    /// </summary>
    /// <param name="yaml">the yaml string</param>
    /// <returns>the <see cref="Flow"/> instance</returns>
    public static Flow DeserializeFromYaml(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var flow = deserializer.Deserialize<FlowModel>(new StringReader(yaml));

        return UpCast(flow);
    }

    /// <summary>
    /// Deserialize flow from json
    /// </summary>
    /// <param name="json">the json string</param>
    /// <returns>the <see cref="Flow"/> instance</returns>
    public static Flow? DeserializeFromJson(string json)
    {
        var flow = JsonSerializer.Deserialize<FlowModel>(json, s_deserializeOptions) ??
            throw new JsonException("Failed to deserialize flow");

        return UpCast(flow);
    }

    private static Flow UpCast(FlowModel flow)
    {
        Flow result = new(flow.Name, flow.Goal);

        foreach (var step in flow.Steps)
        {
            result.AddStep(UpCast(step));
        }

        PopulateVariables(result, flow);

        return result;
    }

    private static FlowStep UpCast(FlowStepModel step)
    {
        FlowStep result = string.IsNullOrEmpty(step.FlowName) ? new FlowStep(step.Goal) : new ReferenceFlowStep(step.FlowName!);

        result.CompletionType = step.CompletionType;
        result.StartingMessage = step.StartingMessage;
        result.TransitionMessage = step.TransitionMessage;
        result.Plugins = step.Plugins;

        PopulateVariables(result, step);

        return result;
    }

    private static void PopulateVariables(FlowStep step, FlowStepModel model)
    {
        step.AddProvides(model.Provides.ToArray());
        step.AddRequires(model.Requires.ToArray());
        step.AddPassthrough(model.Passthrough.ToArray());
    }

    private class FlowStepModel
    {
        public string Goal { get; set; } = string.Empty;

        public List<string> Requires { get; set; } = [];

        public List<string> Provides { get; set; } = [];

        public List<string> Passthrough { get; set; } = [];

        public CompletionType CompletionType { get; set; } = CompletionType.Once;

        public string? StartingMessage { get; set; }

        public string? TransitionMessage { get; set; }

        public List<string> Plugins { get; set; } = [];

        public string? FlowName { get; set; }
    }

    private sealed class FlowModel : FlowStepModel
    {
        public string Name { get; set; } = string.Empty;

        public List<FlowStepModel> Steps { get; set; } = [];
    }
}
