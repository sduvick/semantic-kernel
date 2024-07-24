﻿// Copyright (c) Microsoft. All rights reserved.

using System.Globalization;
using Microsoft.SemanticKernel;
using Xunit;

namespace SemanticKernel.UnitTests.Events;

#pragma warning disable CS0618 // Events are deprecated

public class FunctionInvokedEventArgsTests
{
    [Fact]
    public void ResultValuePropertyShouldBeInitializedByOriginalOne()
    {
        //Arrange
        var originalResults = new FunctionResult(KernelFunctionFactory.CreateFromMethod(() => { }), 36, CultureInfo.InvariantCulture);

        var sut = new FunctionInvokedEventArgs(KernelFunctionFactory.CreateFromMethod(() => { }), [], originalResults);

        //Assert
        Assert.Equal(36, sut.ResultValue);
    }

    [Fact]
    public void ResultValuePropertyShouldBeUpdated()
    {
        //Arrange
        var originalResults = new FunctionResult(KernelFunctionFactory.CreateFromMethod(() => { }), 36, CultureInfo.InvariantCulture);

        var sut = new FunctionInvokedEventArgs(KernelFunctionFactory.CreateFromMethod(() => { }), [], originalResults);

        //Act
        sut.SetResultValue(72);

        //Assert
        Assert.Equal(72, sut.ResultValue);
    }
}
