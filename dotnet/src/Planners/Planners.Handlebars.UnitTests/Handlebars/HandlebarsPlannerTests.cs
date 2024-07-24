﻿// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Microsoft.SemanticKernel.Text;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Planners.UnitTests.Handlebars;

public sealed class HandlebarsPlannerTests
{
    private const string PlanString = """
        ```handlebars
        {{!-- Step 1: Call Summarize function --}}  
        {{set "summary" (SummarizePlugin-Summarize)}}  

        {{!-- Step 2: Call Translate function with the language set to French --}}  
        {{set "translatedSummary" (WriterPlugin-Translate language="French" input=(get "summary"))}}  

        {{!-- Step 3: Call GetEmailAddress function with input set to John Doe --}}  
        {{set "emailAddress" (email-GetEmailAddress input="John Doe")}}  

        {{!-- Step 4: Call SendEmail function with input set to the translated summary and email_address set to the retrieved email address --}}  
        {{email-SendEmail input=(get "translatedSummary") email_address=(get "emailAddress")}}
        ```
        """;

    [Theory]
    [InlineData("Summarize this text, translate it to French and send it to John Doe.")]
    public async Task ItCanCreatePlanAsync(string goal)
    {
        // Arrange
        var plugins = this.CreatePluginCollection();
        var kernel = this.CreateKernelWithMockCompletionResult(PlanString, plugins);
        var planner = new HandlebarsPlanner();

        // Act
        HandlebarsPlan plan = await planner.CreatePlanAsync(kernel, goal);

        // Assert
        Assert.False(string.IsNullOrEmpty(plan.Prompt));
        Assert.False(string.IsNullOrEmpty(plan.ToString()));
    }

    [Fact]
    public async Task EmptyGoalThrowsAsync()
    {
        // Arrange
        var kernel = this.CreateKernelWithMockCompletionResult(PlanString);

        var planner = new HandlebarsPlanner();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await planner.CreatePlanAsync(kernel, string.Empty));
    }

    [Fact]
    public async Task InvalidHandlebarsTemplateThrowsAsync()
    {
        // Arrange
        var invalidPlan = "<plan>notvalid<</plan>";
        var kernel = this.CreateKernelWithMockCompletionResult(invalidPlan);

        var planner = new HandlebarsPlanner();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PlanCreationException>(async () => await planner.CreatePlanAsync(kernel, "goal"));

        Assert.True(exception?.Message?.Contains("CreatePlan failed. See inner exception for details.", StringComparison.InvariantCulture));
        Assert.True(exception?.InnerException?.Message?.Contains("Could not find the plan in the results", StringComparison.InvariantCulture));
        Assert.Equal(exception?.ModelResults?.Content, invalidPlan);
        Assert.NotNull(exception?.CreatePlanPrompt);
    }

    [Fact]
    public void ItDefinesAllPartialsInlinePrompt()
    {
        // Arrange
        var assemply = Assembly.GetExecutingAssembly();
        var planner = new HandlebarsPlanner();

        var promptName = "CreatePlan";
        var actualPartialsNamespace = $"{planner.GetType().Namespace}.{promptName}PromptPartials";
        var resourceNames = assemply.GetManifestResourceNames()
            .Where(name => name.Contains($"{promptName}PromptPartials", StringComparison.CurrentCulture));

        // Act  
        var actualContent = planner.ReadAllPromptPartials(promptName);

        // Assert
        foreach (var resourceName in resourceNames)
        {
            var expectedInlinePartialHeader = $"{{{{#*inline \"{resourceName}\"}}}}";
            Assert.Contains(expectedInlinePartialHeader, actualContent, StringComparison.CurrentCulture);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ItInjectsPredefinedVariablesAsync(bool containsPredefinedVariables)
    {
        // Arrange
        var kernel = this.CreateKernelWithMockCompletionResult(PlanString);

        var planner = new HandlebarsPlanner();

        KernelArguments? mockArguments = containsPredefinedVariables ? new(){
            { "test", new List<string>(){ "test", "test1" } },
            { "testNumber", 1 },
            { "testObject", new Dictionary<string, string>()
                {
                    {"test", "John Doe" },
                    { "testInfo", "testing" },
                }
            }
        } : null;

        // Act
        var plan = await planner.CreatePlanAsync(kernel, "goal", mockArguments);

        // Assert
        var sectionHeader = "### Predefined Variables";
        if (containsPredefinedVariables)
        {
            Assert.Contains(sectionHeader, plan.Prompt, StringComparison.CurrentCulture);
            foreach (var variable in mockArguments!)
            {
                Assert.Contains($"- \"{variable.Key}\" ({variable.Value?.GetType().GetFriendlyTypeName()})", plan.Prompt, StringComparison.CurrentCulture);
                Assert.Contains(JsonSerializer.Serialize(variable.Value, JsonOptionsCache.WriteIndented), plan.Prompt, StringComparison.InvariantCulture);
            }
        }
        else
        {
            Assert.DoesNotContain(sectionHeader, plan.Prompt, StringComparison.CurrentCulture);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ItInjectsAdditionalContextAsync(bool hasAdditionalContext)
    {
        // Arrange
        var kernel = this.CreateKernelWithMockCompletionResult(PlanString);
        var mockContext = "Mock context";

        var planner = new HandlebarsPlanner(
            new HandlebarsPlannerOptions()
            {
                GetAdditionalPromptContext = hasAdditionalContext ? () => Task.FromResult(mockContext) : null
            });

        // Act
        var plan = await planner.CreatePlanAsync(kernel, "goal");

        // Assert
        var sectionHeader = "### Additional Context";
        if (hasAdditionalContext)
        {
            Assert.Contains("### Additional Context", plan.Prompt, StringComparison.CurrentCulture);
            Assert.Contains(mockContext, plan.Prompt, StringComparison.CurrentCulture);
        }
        else
        {
            Assert.DoesNotContain(sectionHeader, plan.Prompt, StringComparison.CurrentCulture);
        }
    }

    [Fact]
    public async Task ItOverridesPromptAsync()
    {
        // Arrange
        var kernel = this.CreateKernelWithMockCompletionResult(PlanString);
        var mockPromptOverride = "Help me fulfill my goal!";

        var planner = new HandlebarsPlanner(
            new HandlebarsPlannerOptions()
            {
                CreatePlanPromptHandler = () => $"{mockPromptOverride} {{{{> UserGoal }}}}"
            });

        // Act
        var plan = await planner.CreatePlanAsync(kernel, "goal");

        // Assert
        Assert.Contains(mockPromptOverride, plan.Prompt, StringComparison.CurrentCulture);
        Assert.Contains("## Goal", plan.Prompt, StringComparison.CurrentCulture);
        Assert.DoesNotContain("## Tips and reminders", plan.Prompt, StringComparison.CurrentCulture);
    }

    [Fact]
    public async Task ItThrowsIfStrictlyOnePlanCantBeIdentifiedAsync()
    {
        // Arrange
        var ResponseWithMultipleHbTemplates = """
            ```handlebars
            {{!-- Step 1: Call Summarize function --}}  
            {{set "summary" (SummarizePlugin-Summarize)}}  
            ```

            ```handlebars
            {{!-- Step 2: Call Translate function with the language set to French --}}  
            {{set "translatedSummary" (WriterPlugin-Translate language="French" input=(get "summary"))}}  
            ```

            ```handlebars
            {{!-- Step 3: Call GetEmailAddress function with input set to John Doe --}}  
            {{set "emailAddress" (email-GetEmailAddress input="John Doe")}}  

            {{!-- Step 4: Call SendEmail function with input set to the translated summary and email_address set to the retrieved email address --}}  
            {{email-SendEmail input=(get "translatedSummary") email_address=(get "emailAddress")}}
            ```

            ```handlebars
            {{!-- Step 4: Call SendEmail function with input set to the translated summary and email_address set to the retrieved email address --}}  
            {{email-SendEmail input=(get "translatedSummary") email_address=(get "emailAddress")}}
            ```
            """;
        var kernel = this.CreateKernelWithMockCompletionResult(ResponseWithMultipleHbTemplates);
        var planner = new HandlebarsPlanner();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<PlanCreationException>(async () => await planner.CreatePlanAsync(kernel, "goal"));
        Assert.True(exception?.InnerException?.Message?.Contains("Identified multiple Handlebars templates in model response", StringComparison.InvariantCulture));
    }

    private Kernel CreateKernelWithMockCompletionResult(string testPlanString, KernelPluginCollection? plugins = null)
    {
        plugins ??= [];

        var chatMessage = new ChatMessageContent(AuthorRole.Assistant, testPlanString);

        var chatCompletion = new Mock<IChatCompletionService>();
        chatCompletion
            .Setup(cc => cc.GetChatMessageContentsAsync(It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([chatMessage]);

        var serviceSelector = new Mock<IAIServiceSelector>();
        IChatCompletionService resultService = chatCompletion.Object;
        PromptExecutionSettings? resultSettings = new();
        serviceSelector
            .Setup(ss => ss.TrySelectAIService<IChatCompletionService>(It.IsAny<Kernel>(), It.IsAny<KernelFunction>(), It.IsAny<KernelArguments>(), out resultService!, out resultSettings))
            .Returns(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAIServiceSelector>(serviceSelector.Object);
        serviceCollection.AddSingleton<IChatCompletionService>(chatCompletion.Object);

        return new Kernel(serviceCollection.BuildServiceProvider(), plugins);
    }

    private KernelPluginCollection CreatePluginCollection() =>
        [
            KernelPluginFactory.CreateFromFunctions("email", "Email functions",
            [
                KernelFunctionFactory.CreateFromMethod(() => "MOCK FUNCTION CALLED", "SendEmail", "Send an e-mail"),
                KernelFunctionFactory.CreateFromMethod(() => "MOCK FUNCTION CALLED", "GetEmailAddress", "Get an e-mail address")
            ]),
            KernelPluginFactory.CreateFromFunctions("WriterPlugin", "Writer functions",
            [
                KernelFunctionFactory.CreateFromMethod(() => "MOCK FUNCTION CALLED", "Translate", "Translate something"),
            ]),
            KernelPluginFactory.CreateFromFunctions("SummarizePlugin", "Summarize functions",
            [
                KernelFunctionFactory.CreateFromMethod(() => "MOCK FUNCTION CALLED", "Summarize", "Summarize something"),
            ])
        ];
}
