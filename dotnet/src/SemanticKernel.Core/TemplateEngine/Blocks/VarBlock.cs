﻿// Copyright (c) Microsoft. All rights reserved.

using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Microsoft.SemanticKernel.TemplateEngine;

internal sealed partial class VarBlock : Block, ITextRendering
{
    internal override BlockTypes Type => BlockTypes.Variable;

    internal string Name { get; } = string.Empty;

    public VarBlock(string? content, ILoggerFactory? loggerFactory = null) : base(content?.Trim(), loggerFactory)
    {
        if (this.Content.Length < 2)
        {
            this.Logger.LogError("The variable name is empty");
            return;
        }

        this.Name = this.Content.Substring(1);
    }

#pragma warning disable CA2254 // error strings are used also internally, not just for logging
    // ReSharper disable TemplateIsNotCompileTimeConstantProblem
    public override bool IsValid(out string errorMsg)
    {
        errorMsg = string.Empty;

        if (string.IsNullOrEmpty(this.Content))
        {
            errorMsg = $"A variable must start with the symbol {Symbols.VarPrefix} and have a name";
            this.Logger.LogError(errorMsg);
            return false;
        }

        if (this.Content[0] != Symbols.VarPrefix)
        {
            errorMsg = $"A variable must start with the symbol {Symbols.VarPrefix}";
            this.Logger.LogError(errorMsg);
            return false;
        }

        if (this.Content.Length < 2)
        {
            errorMsg = "The variable name is empty";
            this.Logger.LogError(errorMsg);
            return false;
        }

        if (!ValidNameRegex().IsMatch(this.Name))
        {
            errorMsg = $"The variable name '{this.Name}' contains invalid characters. " +
                       "Only alphanumeric chars and underscore are allowed.";
            this.Logger.LogError(errorMsg);
            return false;
        }

        return true;
    }
#pragma warning restore CA2254

    /// <inheritdoc/>
    public object? Render(KernelArguments? arguments)
    {
        if (arguments is null) { return null; }

        if (string.IsNullOrEmpty(this.Name))
        {
            const string ErrMsg = "Variable rendering failed, the variable name is empty";
            this.Logger.LogError(ErrMsg);
            throw new KernelException(ErrMsg);
        }

        if (arguments.TryGetValue(this.Name, out object? value))
        {
            return value;
        }

        this.Logger.LogWarning("Variable `{0}{1}` not found", Symbols.VarPrefix, this.Name);

        return null;
    }

#if NET
    [GeneratedRegex("^[a-zA-Z0-9_]*$")]
    private static partial Regex ValidNameRegex();
#else
    private static Regex ValidNameRegex() => s_validNameRegex;
    private static readonly Regex s_validNameRegex = new("^[a-zA-Z0-9_]*$", RegexOptions.Compiled);
#endif
}
