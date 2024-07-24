﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TemplateEngine;
using Xunit;

namespace SemanticKernel.UnitTests.TemplateEngine;

public class NamedArgBlockTests
{
    [Fact]
    public void ItHasTheCorrectType()
    {
        // Act
        var target = new NamedArgBlock("a=$b");

        // Assert
        Assert.Equal(BlockTypes.NamedArg, target.Type);
    }

    [Theory]
    [InlineData("  a=$b  ", "a=$b")]
    [InlineData(" a =  $b ", "a=$b")]
    [InlineData(" a=\"b\" ", "a=\"b\"")]
    [InlineData(" a =  \"b\" ", "a=\"b\"")]
    [InlineData("  a='b'  ", "a='b'")]
    [InlineData("a =  'b' ", "a='b'")]
    public void ItTrimsSpaces(string input, string expected)
    {
        // Act + Assert
        Assert.Equal(expected, new NamedArgBlock(input).Content);
    }

    [Theory]
    [InlineData("0='val'", true)]
    [InlineData("1='val'", true)]
    [InlineData("a='val'", true)]
    [InlineData("_='val'", true)]
    [InlineData("01='val'", true)]
    [InlineData("01a='val'", true)]
    [InlineData("a01='val'", true)]
    [InlineData("_0='val'", true)]
    [InlineData("a01_='val'", true)]
    [InlineData("_a01='val'", true)]
    [InlineData(".='val'", false)]
    [InlineData("-='val'", false)]
    [InlineData("a b='val'", false)]
    [InlineData("a\nb='val'", false)]
    [InlineData("a\tb='val'", false)]
    [InlineData("a\rb='val'", false)]
    [InlineData("a.b='val'", false)]
    [InlineData("a,b='val'", false)]
    [InlineData("a-b='val'", false)]
    [InlineData("a+b='val'", false)]
    [InlineData("a~b='val'", false)]
    [InlineData("a`b='val'", false)]
    [InlineData("a!b='val'", false)]
    [InlineData("a@b='val'", false)]
    [InlineData("a#b='val'", false)]
    [InlineData("a$b='val'", false)]
    [InlineData("a%b='val'", false)]
    [InlineData("a^b='val'", false)]
    [InlineData("a*b='val'", false)]
    [InlineData("a(b='val'", false)]
    [InlineData("a)b='val'", false)]
    [InlineData("a|b='val'", false)]
    [InlineData("a{b='val'", false)]
    [InlineData("a}b='val'", false)]
    [InlineData("a[b='val'", false)]
    [InlineData("a]b='val'", false)]
    [InlineData("a:b='val'", false)]
    [InlineData("a;b='val'", false)]
    [InlineData("a'b='val'", false)]
    [InlineData("a\"b='val'", false)]
    [InlineData("a<b='val'", false)]
    [InlineData("a>b='val'", false)]
    [InlineData("a/b='val'", false)]
    [InlineData("a\\b='val'", false)]
    [InlineData("a ='val'", true)]
    public void ArgNameAllowsUnderscoreLettersAndDigits(string name, bool isValid)
    {
        // Arrange
        var target = new NamedArgBlock($" {name} ");

        // Act + Assert
        Assert.Equal(isValid, target.IsValid(out _));
    }

    [Theory]
    [InlineData("name   ='value'")]
    [InlineData("name=   'value'")]
    public void AllowsAnyNumberOfSpacesBeforeAndAfterEqualSign(string input)
    {
        // Arrange
        var target = new NamedArgBlock(input);

        // Act + Assert
        Assert.True(target.IsValid(out _));
        Assert.Equal("name", target.Name);
        Assert.Equal("value", target.GetValue(null));
    }

    [Fact]
    public void ArgValueNeedsQuoteOrDollarSignPrefix()
    {
        // Arrange
        var target = new NamedArgBlock("a=b");

        // Act + Assert
        Assert.False(target.IsValid(out var error));
        Assert.Equal("There was an issue with the named argument value for 'a': A value must have single quotes or double quotes on both sides", error);
    }

    [Fact]
    public void ArgNameShouldBeNonEmpty()
    {
        // Arrange
        static NamedArgBlock funcToTest() => new("='b'");

        // Act + Assert
        KernelException exception = Assert.Throws<KernelException>(funcToTest);
        Assert.Equal("A function named argument must contain a name and value separated by a '=' character.", exception.Message);
    }

    [Fact]
    public void ArgValueShouldBeNonEmpty()
    {
        Assert.Throws<KernelException>(() => new NamedArgBlock("a="));
    }

    [Theory]
    [InlineData("!@#^='b'", "The argument name '!@#^' contains invalid characters. Only alphanumeric chars and underscore are allowed.")]
    [InlineData("a=$!@#^", "There was an issue with the named argument value for 'a': The variable name '!@#^' contains invalid characters. Only alphanumeric chars and underscore are allowed.")]
    public void ArgNameAndVariableShouldBeAValidVariableName(string content, string expectedError)
    {
        // Arrange
        var target = new NamedArgBlock(content);

        // Act + Assert
        Assert.False(target.IsValid(out var error));
        Assert.Equal(expectedError, error);
    }

    [Theory]
    [InlineData("0='val'", true)]
    [InlineData("0=\"val\"", true)]
    [InlineData("0='val\"", false)]
    [InlineData("0=\"val'", false)]
    [InlineData("0= 'val'", true)]
    public void ArgValueAllowsConsistentlyQuotedValues(string name, bool isValid)
    {
        // Arrange
        var target = new NamedArgBlock($" {name} ");

        // Act + Assert
        Assert.Equal(isValid, target.IsValid(out _));
    }

    [Theory]
    [InlineData("0", true)]
    [InlineData("1", true)]
    [InlineData("a", true)]
    [InlineData("_", true)]
    [InlineData("01", true)]
    [InlineData("01a", true)]
    [InlineData("a01", true)]
    [InlineData("_0", true)]
    [InlineData("a01_", true)]
    [InlineData("_a01", true)]
    [InlineData(".", false)]
    [InlineData("-", false)]
    [InlineData("a b", false)]
    [InlineData("a\nb", false)]
    [InlineData("a\tb", false)]
    [InlineData("a\rb", false)]
    [InlineData("a.b", false)]
    [InlineData("a,b", false)]
    [InlineData("a-b", false)]
    [InlineData("a+b", false)]
    [InlineData("a~b", false)]
    [InlineData("a`b", false)]
    [InlineData("a!b", false)]
    [InlineData("a@b", false)]
    [InlineData("a#b", false)]
    [InlineData("a$b", false)]
    [InlineData("a%b", false)]
    [InlineData("a^b", false)]
    [InlineData("a*b", false)]
    [InlineData("a(b", false)]
    [InlineData("a)b", false)]
    [InlineData("a|b", false)]
    [InlineData("a{b", false)]
    [InlineData("a}b", false)]
    [InlineData("a[b", false)]
    [InlineData("a]b", false)]
    [InlineData("a:b", false)]
    [InlineData("a;b", false)]
    [InlineData("a'b", false)]
    [InlineData("a\"b", false)]
    [InlineData("a<b", false)]
    [InlineData("a>b", false)]
    [InlineData("a/b", false)]
    [InlineData("a\\b", false)]
    public void ArgValueAllowsVariablesWithUnderscoreLettersAndDigits(string name, bool isValid)
    {
        // Arrange
        var target = new NamedArgBlock($"a=${name}");

        // Act + Assert
        Assert.Equal(isValid, target.IsValid(out _));
    }

    [Fact]
    public void ItRequiresOneEquals()
    {
        // Arrange
        var target1 = new NamedArgBlock("a='b'");
        var target2 = new NamedArgBlock("a=$b");
        var target3 = new NamedArgBlock("a=\"b\"");
        Assert.Throws<KernelException>(() => new NamedArgBlock("foo"));
        Assert.Throws<KernelException>(() => new NamedArgBlock("foo=$bar=$baz"));

        // Act + Assert
        Assert.True(target1.IsValid(out _));
        Assert.True(target2.IsValid(out _));
        Assert.True(target3.IsValid(out _));
    }

    [Fact]
    public void ItReturnsArgumentsValueAndType()
    {
        // Arrange
        var target = new NamedArgBlock("a=$var");
        var arguments = new KernelArguments()
        {
            ["var"] = (double)28.2,
        };

        // Act
        var result = target.GetValue(arguments);

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal(28.2, result);
    }

    [Fact]
    public void ItRendersToNullWithNoArgument()
    {
        // Arrange
        var target = new NamedArgBlock("a=$var");

        // Act
        var result = target.GetValue([]);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ItRendersToNullIfArgumentIsNull()
    {
        // Arrange
        var target = new NamedArgBlock("a=$var");
        var arguments = new KernelArguments()
        {
            ["var"] = null
        };

        // Act
        var result = target.GetValue(arguments);

        // Assert
        Assert.Null(result);
    }
}
