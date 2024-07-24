﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SemanticKernel.Experimental.Agents.Models;

namespace Microsoft.SemanticKernel.Experimental.Agents;

internal static class AssistantsKernelFunctionExtensions
{
    /// <summary>
    /// Produce a fully qualified toolname.
    /// </summary>
    public static string GetQualifiedName(this KernelFunction function, string pluginName)
    {
        return $"{pluginName}-{function.Name}";
    }

    /// <summary>
    /// Convert <see cref="KernelFunction"/> to an OpenAI tool model.
    /// </summary>
    /// <param name="function">The source function</param>
    /// <param name="pluginName">The plugin name</param>
    /// <returns>An OpenAI tool model</returns>
    public static ToolModel ToToolModel(this KernelFunction function, string pluginName)
    {
        var metadata = function.Metadata;
        var required = new List<string>(metadata.Parameters.Count);
        var properties =
            metadata.Parameters.ToDictionary(
                p => p.Name,
                p =>
                {
                    if (p.IsRequired)
                    {
                        required.Add(p.Name);
                    }

                    return
                        new OpenAIParameter
                        {
                            Type = ConvertType(p.ParameterType),
                            Description = p.Description,
                        };
                });

        var payload =
            new ToolModel
            {
                Type = "function",
                Function =
                    new()
                    {
                        Name = function.GetQualifiedName(pluginName),
                        Description = function.Description,
                        Parameters =
                                new OpenAIParameters
                                {
                                    Properties = properties,
                                    Required = required,
                                },
                    },
            };

        return payload;
    }

    private static string ConvertType(Type? type)
    {
        if (type is null || type == typeof(string))
        {
            return "string";
        }

        if (type.IsNumber())
        {
            return "number";
        }

        if (type == typeof(bool))
        {
            return "boolean";
        }

        if (type.IsEnum)
        {
            return "enum";
        }

        if (type.IsArray)
        {
            return "array";
        }

        return "object";
    }

    private static bool IsNumber(this Type type) =>
        type == typeof(byte) ||
        type == typeof(sbyte) ||
        type == typeof(short) ||
        type == typeof(ushort) ||
        type == typeof(int) ||
        type == typeof(uint) ||
        type == typeof(long) ||
        type == typeof(ulong) ||
        type == typeof(float) ||
        type == typeof(double) ||
        type == typeof(decimal);
}
