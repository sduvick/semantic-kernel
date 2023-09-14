using System.Text;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;

var configJson = @"{
  ""Logging"": {
    ""LogLevel"": { // No provider, LogLevel applies to all the enabled providers.
      ""Default"": ""Trace"", // Default, application level if no other level applies
      ""Microsoft"": ""Trace"", // Log level for log category which starts with text 'Microsoft' (i.e. 'Microsoft.*')
      ""Microsoft.Graph.GraphServiceClient"": ""Trace"",
      ""Microsoft.SemanticKernel.MsGraph.Skills"": ""Trace""
    }
  },
  ""MsGraph"": {
    ""ClientId"": ""<Your App Client ID>"",
    ""TenantId"": ""<tenant ID>"", // MSA/Consumer/Personal tenant,  https://learn.microsoft.com/azure/active-directory/develop/accounts-overview
    ""Scopes"": [
      ""User.Read"",
      ""Files.ReadWrite"",
      ""Tasks.ReadWrite"",
      ""Mail.Send""
    ],
    ""RedirectUri"": ""http://localhost""
  },
  ""OneDrivePathToFile"": ""<path to a text file in your OneDrive>"", // e.g. ""Documents/MyFile.txt""
  ""DefaultCompletionServiceId"": ""gpt-35-turbo"", // ""gpt-3.5-turbo"" (note the '.' between 3 and 5) for OpenAI
  ""OpenAI"": {
      ""serviceId"": ""gpt-3.5-turbo"",
      ""modelId"": ""gpt-3.5-turbo"",
    //  ""ApiKey"": """"
  },
    ""OpenAIEmbeddings"": {
    ""ServiceId"": ""text-embedding-ada-002"",
    ""ModelId"": ""text-embedding-ada-002"",
  },
  ""Bing"": {
    ""Endpoint"" : """"

  },
  ""AzureOpenAIEmbeddings"": {
    ""ServiceId"": ""azure-text-embedding-ada-002"",
    ""DeploymentName"": ""text-embedding-ada-002"",
    ""Endpoint"": ""https://eastus-shared-prd-cs.openai.azure.com""
  },
  ""ACS"": {
    ""ServiceId"": ""azure-text-embedding-ada-002"",
    ""DeploymentName"": ""text-embedding-ada-002"",
    ""Endpoint"": ""https://eastus-shared-prd-cs.openai.azure.com""
  },
  ""AzureOpenAI"": {
    ""ServiceId"": ""gpt-35-turbo"",
    ""DeploymentName"": ""gpt-35-turbo"",
    ""ChatDeploymentName"": ""gpt-35-turbo"",
    ""ModelId"": ""0613"",
    ""Endpoint"": ""https://eastus-shared-prd-cs.openai.azure.com""
  }
}";

IConfigurationRoot configRoot = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(configJson)))
    .Build();



TestConfiguration.Initialize(configRoot);


//await Example01_NativeFunctions.RunAsync();
//await Example02_Pipeline.RunAsync();
//await Example03_Variables.RunAsync();
//await Example04_CombineLLMPromptsAndNativeCode.RunAsync();
await Example05_InlineFunctionDefinition.RunAsync();
await Example06_TemplateLanguage.RunAsync();
await Example07_BingAndGoogleSkills.RunAsync();
await Example08_RetryHandler.RunAsync();
await Example09_FunctionTypes.RunAsync();
await Example10_DescribeAllSkillsAndFunctions.RunAsync();
await Example11_WebSearchQueries.RunAsync();
await Example12_SequentialPlanner.RunAsync();
await Example13_ConversationSummarySkill.RunAsync();
await Example14_SemanticMemory.RunAsync();
await Example15_TextMemorySkill.RunAsync();
await Example16_CustomLLM.RunAsync();
await Example17_ChatGPT.RunAsync();
await Example18_DallE.RunAsync();
await Example20_HuggingFace.RunAsync();
await Example21_ChatGptPlugins.RunAsync();
await Example22_OpenApiSkill_AzureKeyVault.RunAsync();
await Example23_OpenApiSkill_GitHub.RunAsync();
await Example24_OpenApiSkill_Jira.RunAsync();
await Example25_ReadOnlyMemoryStore.RunAsync();
await Example26_AADAuth.RunAsync();
await Example27_SemanticFunctionsUsingChatGPT.RunAsync();
await Example28_ActionPlanner.RunAsync();
await Example30_ChatWithPrompts.RunAsync();
await Example31_CustomPlanner.RunAsync();
await Example32_StreamingCompletion.RunAsync();
await Example33_StreamingChat.RunAsync();
await Example34_CustomChatModel.RunAsync();
await Example35_GrpcSkills.RunAsync();
await Example36_MultiCompletion.RunAsync();
await Example37_MultiStreamingCompletion.RunAsync();
await Example40_DIContainer.RunAsync();
await Example41_HttpClientUsage.RunAsync();
await Example42_KernelBuilder.RunAsync();
await Example43_GetModelResult.RunAsync();
await Example44_MultiChatCompletion.RunAsync();
await Example45_MultiStreamingChatCompletion.RunAsync();
await Example48_GroundednessChecks.RunAsync();
await Example49_LogitBias.RunAsync();
await Example51_StepwisePlanner.RunAsync();
await Example52_ApimAuth.RunAsync();
await Example54_AzureChatCompletionWithData.RunAsync();
await Example55_TextChunker.RunAsync();
await Example56_TemplateNativeFunctionsWithMultipleArguments.RunAsync();
await Example57_FunctionEventHandlers.RunAsync();
